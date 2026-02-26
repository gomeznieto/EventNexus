using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EventNexus.Application.DTOs;
using EventNexus.Application.Interfaces;
using EventNexus.Domain.Entities;
using EventNexus.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EventNexus.Infrastructure.Services;

public class AuthService : IAuthService
{  
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AppDbContext _appDbContext;
    private readonly IEmailService _emailService;
    private readonly ITokenService _tokenService;
    public AuthService(
            UserManager<IdentityUser> userManager, 
            AppDbContext appDbContext,
            IEmailService emailService,
            ITokenService tokenService
            )
    {
        _userManager = userManager;
        _appDbContext = appDbContext;
        _emailService = emailService;
        _tokenService = tokenService;
    }

    // --- LOGIN --- //
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
    {
        // Find if user mail is already registed 
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if(user is null) return new LoginResponseDto {Message = "If that email exists, a code was sent(not)"};

        // Find if user got an active login code
        var activeCode = await _appDbContext .UserLoginCodes.FirstOrDefaultAsync( u => u.UserId == Guid.Parse(user.Id) && DateTime.UtcNow < u.Expiration);

        if (activeCode is not null){
            activeCode.UsedAt = DateTime.UtcNow;
        }
        
        // Generate a login code
        var code = GenerateLoginCode();
        int expiresInMinutes = 15;

        var newLoginCode = new UserLoginCode {
            UserId = Guid.Parse(user.Id),
            Code = code,
            Expiration = DateTime.UtcNow.AddMinutes(expiresInMinutes),
            CreatedAt = DateTime.UtcNow
        };

        _appDbContext.UserLoginCodes.Add(newLoginCode);
        await _appDbContext.SaveChangesAsync();
        
        // Sending email with the login code
        var newMsg = new EmailDetailsDto {
           Destination = user.Email!,
           Subject = "Sign In Verification Code",
           Body = $"Please enter the following code to complete sign in: {code}.\nCode will expire in 15 minutes."
        };

        await _emailService.SendEmailAsync(newMsg);

        return new LoginResponseDto {Message = "If that email exists, a code was sent"};
    }

    
    private string GenerateLoginCode(){
        var MIN_CODE = 100000; // TODO: Move to settings after it works!
        var MAX_CODE = 999999;
        return Random.Shared.Next(MIN_CODE, MAX_CODE).ToString(); 
    }

    // --- REGISTER CUSTOMER --- //
    public async Task<Guid> RegisterCustomerAsync(RegisterCustomerRequestDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto); 

        // Find if the user email already exists
        var userExists = await _userManager.FindByEmailAsync(dto.Email);

        if(userExists != null) throw new ArgumentException("The email address is already registered.");

        var transaction = await _appDbContext.Database.BeginTransactionAsync();

        try
        {
            // Create and Save Identity and User
            var baseUser = await CreateBaseUserAsync(dto.Email, dto.FirstName, dto.LastName, "Customer");
            await _appDbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return baseUser.Id;
        }
        catch (System.Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    // --- REGISTER ORGANIZER --- //
    public async Task<Guid> RegisterOrganizerAsync(RegisterOrganizerRequestDto dto){

        ArgumentNullException.ThrowIfNull(dto); 

        // Find if the user email already exists
        var userExists = await _userManager.FindByEmailAsync(dto.Email);

        if(userExists != null) throw new ArgumentException("The email address is already registered.");

        var transaction = await _appDbContext.Database.BeginTransactionAsync();

        try
        {
            // Crate and Save IdentityUser and User
            var baseUser = await CreateBaseUserAsync(dto.Email, dto.FirstName, dto.LastName, "Organizer");

            // Create Organizer
            var newOrganizer = new Organizer{
                UserId = baseUser.Id,
                CompanyName = dto.CompanyName,
                BusinessPhone = dto.BusinessPhone
            };

            _appDbContext.Organizers.Add(newOrganizer);
            await _appDbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return baseUser.Id;

        }
        catch (System.Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    // --- CREATE BASE USER --- //
    private async Task<User> CreateBaseUserAsync(string email, string firstName, string lastName, string roleName){

        // Create Identity
        var newIdentityUser = new IdentityUser {
            Email = email,
            UserName = email
        };

        var result = await _userManager.CreateAsync(newIdentityUser);

        if(!result.Succeeded){
            var errors = string.Join(",", result.Errors.Select( e => e.Description));
            throw new ArgumentException($"An Error ocurred while register the user : {errors}");
        }
        
        // Create role
        await _userManager.AddToRoleAsync(newIdentityUser, roleName);

        // Create user
        var newUser = new User {
            Id = Guid.Parse(newIdentityUser.Id),
            FirstName = firstName,
            LastName = lastName,
            Email = email
        };

        _appDbContext.Users.Add(newUser);

        return newUser;
    }

    // TODO: Revoke token refresh
    // -- VERIFY CODE -- //
    public async Task<AuthResponseDto> VerifyLoginCodeAsync(VerifyRequestDto dto)
    {

        var transaction = await _appDbContext.Database.BeginTransactionAsync();

        try {
            // Verify if user mail exists
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if(user is null) throw new ArgumentException("The Email or de Code are not valid");

            // Verify Code from same user
            var code = await _appDbContext.UserLoginCodes.FirstOrDefaultAsync(c => c.UserId == Guid.Parse(user.Id) && c.Code == dto.Code && c.UsedAt == null);

            if(code is null) throw new ArgumentException("The code is not valid");
            if(code.Expiration < DateTime.UtcNow) throw new ArgumentException("Code has expired");

            // Update table login code
            code.UsedAt = DateTime.UtcNow;

            // Make Token and Refresh Toekn to response
            var roles =  await _userManager.GetRolesAsync(user);

            var ctxUser = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == Guid.Parse(user.Id));
            if (ctxUser is null) throw new InvalidOperationException("The User information is missing or corrupt");

            var jti = Guid.NewGuid().ToString();
            var token = _tokenService.CreateToken(ctxUser, user.SecurityStamp!, roles, jti);
            var refreshTokenString = GenerateRefreshToken(); 

            // Refresh token
            var refreshTokenEntity = new RefreshToken {
                Token = refreshTokenString,
                UserId = Guid.Parse(user.Id),
                JwtId = jti,
                CreationDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false, 
                IsUsed = false,
            };

            _appDbContext.RefreshTokens.Add(refreshTokenEntity);

            await _appDbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return new AuthResponseDto {
                Token = token,
                RefreshToken = refreshTokenString
            };

        } catch (Exception){
            await transaction.RollbackAsync();
            throw;
        }
    }

    private string GenerateRefreshToken(){
        var randomNumber = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    // -- REVOKE TOKEN --- //
    public async Task RevokeTokenAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null) throw new ArgumentException("The User is not correct");

        var result = await _userManager.UpdateSecurityStampAsync(user);

        if(!result.Succeeded){
            var errors = string.Join(",", result.Errors.Select( e => e.Description));
            throw new ArgumentException($"An error occurred while the token was revoking: {errors} ");
        } 
    }

    // -- REFRESH TOKEN -- //
    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto){
        
        // Valid expired Token
        var tokenPrincipal = _tokenService.GetPrincipalFromExpiredToken(dto.ExpiredToken);  
        var expiryClaim = tokenPrincipal.FindFirstValue(JwtRegisteredClaimNames.Exp);
        var userId = tokenPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        var jti = tokenPrincipal.FindFirstValue(JwtRegisteredClaimNames.Jti);

        if(userId is null || jti is null || expiryClaim is null) throw new SecurityTokenException("Invalid Token Payload");

        var expiryDateUnix = long.Parse(expiryClaim); 
        var expiryDateTimeUtc = DateTimeOffset.FromUnixTimeSeconds(expiryDateUnix).UtcDateTime;
        if(DateTime.UtcNow < expiryDateTimeUtc) throw new SecurityTokenException("This Token has not expired yet");


        // Valid Refresh Token
        var storedRefreshToken = await _appDbContext.RefreshTokens
            .FirstOrDefaultAsync( r => r.Token == dto.RefreshToken);

        if(storedRefreshToken is null ||
                DateTime.UtcNow > storedRefreshToken.ExpiryDate || 
                storedRefreshToken.IsUsed || 
                storedRefreshToken.IsRevoked ||
                storedRefreshToken.JwtId != jti
          ) throw new SecurityTokenException("The Refresh Token is not valid");

        // Fetch User
        var user = await _userManager.FindByIdAsync(userId);
        if(user is null) throw new SecurityTokenException("The User doesn't exists");

        var userGuid = Guid.Parse(user.Id);
        var ctxUser = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == userGuid);
        if(ctxUser is null) throw new ArgumentException("The user data is corrupted or missing");

        var roles = await _userManager.GetRolesAsync(user);

        // Revoking Refresh Token
        storedRefreshToken.IsRevoked = true;
        storedRefreshToken.IsUsed =  true;

        // Create new Token and Refresh Token
        var newJti = Guid.NewGuid().ToString();
        var newToken = _tokenService.CreateToken(ctxUser, user.SecurityStamp!, roles, newJti);
        var newRefreshTokenString = GenerateRefreshToken(); 

        // Refresh token
        var refreshTokenEntity = new RefreshToken {
            Token = newRefreshTokenString,
            UserId = Guid.Parse(user.Id),
            JwtId = newJti,
            CreationDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false, 
            IsUsed = false,
        };

        // Save new Refresh Token
        _appDbContext.RefreshTokens.Add(refreshTokenEntity);

        await _appDbContext.SaveChangesAsync();

        return new AuthResponseDto {
            Token = newToken,
            RefreshToken = newRefreshTokenString
        };
    }
}
