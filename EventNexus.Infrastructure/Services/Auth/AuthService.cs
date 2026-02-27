using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EventNexus.Application.DTOs;
using EventNexus.Application.Interfaces;
using EventNexus.Application.Settings;
using EventNexus.Domain.Entities;
using EventNexus.Domain.Enums;
using EventNexus.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EventNexus.Infrastructure.Services;

public class AuthService : IAuthService
{  
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AppDbContext _appDbContext;
    private readonly IEmailService _emailService;
    private readonly ITokenService _tokenService;
    private readonly TicketSettings _ticketSetting;
    private readonly IVerificationCodeService _codeService;

    public AuthService(
            UserManager<IdentityUser> userManager, 
            AppDbContext appDbContext,
            IEmailService emailService,
            IVerificationCodeService verificationCodeService,
            ITokenService tokenService,
            IOptions<TicketSettings> options
            )
    {
        _userManager = userManager;
        _appDbContext = appDbContext;
        _emailService = emailService;
        _tokenService = tokenService;
        _ticketSetting = options.Value;
        _codeService = verificationCodeService;
    }

    // --- LOGIN --- //
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
    {
        // Find if user mail is already registed 
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if(user is null) return new LoginResponseDto {Message = "If that email exists, a code was sent(not)"};
        
        // Create code
        var code = await _codeService.GenerateCodeAsync(Guid.Parse(user.Id), ActionType.Login);
        
        // Sending email with the login code
        var newMsg = new EmailDetailsDto {
           Destination = user.Email!,
           Subject = "Sign In Verification Code",
           Body = $"Please enter the following code to complete sign in: {code}.\nCode will expire in 15 minutes."
        };

        await _emailService.SendEmailAsync(newMsg);

        await _appDbContext.SaveChangesAsync();

        return new LoginResponseDto {Message = "If that email exists, a code was sent"};
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

    // -- VERIFY CODE -- //
    public async Task<AuthResponseDto> VerifyLoginCodeAsync(VerifyRequestDto dto)
    {
        var transaction = await _appDbContext.Database.BeginTransactionAsync();

        try {
            // Verify if user mail exists
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if(user is null) throw new ArgumentException("The Email or de Code are not valid");

            // Verify Code from same user
            await _codeService.ValidateCodeAsync(Guid.Parse(user.Id), dto.Code, ActionType.Login);

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

    // -- LOGOUT -- //
    public async Task LogoutAsync(LogoutRequestDto dto){

        var storedRefreshToken = await _appDbContext.RefreshTokens
            .FirstOrDefaultAsync( r => r.Token == dto.RefreshToken);

        if(storedRefreshToken is null ||
                storedRefreshToken.IsUsed || 
                storedRefreshToken.IsRevoked
          ) return; 

        storedRefreshToken.IsUsed =  true;
        storedRefreshToken.IsRevoked = true;

        await _appDbContext.SaveChangesAsync();
    }

    // -- REFRESH TOKEN -- //
    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto){

        // Valid expired Token
        var tokenPrincipal = _tokenService.GetPrincipalFromExpiredToken(dto.ExpiredToken);  
        var expiryClaim = tokenPrincipal.FindFirstValue(JwtRegisteredClaimNames.Exp);
        var userId = tokenPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        var jti = tokenPrincipal.FindFirstValue(JwtRegisteredClaimNames.Jti);

        if(userId is null || jti is null || expiryClaim is null) 
            throw new SecurityTokenException("Invalid Token Payload");

        // Valid Refresh Token
        var storedRefreshToken = await _appDbContext.RefreshTokens
            .FirstOrDefaultAsync( r => r.Token == dto.RefreshToken);

        if(storedRefreshToken is null ||
                DateTime.UtcNow > storedRefreshToken.ExpiryDate || 
                storedRefreshToken.IsUsed || 
                storedRefreshToken.IsRevoked ||
                storedRefreshToken.JwtId != jti
          ) throw new SecurityTokenException("The Refresh Token is not valid");

        // Validate expiracy token date
        var expiryDateUnix = long.Parse(expiryClaim); 
        var expiryDateTimeUtc = DateTimeOffset.FromUnixTimeSeconds(expiryDateUnix).UtcDateTime;
        if(DateTime.UtcNow < expiryDateTimeUtc)
            throw new SecurityTokenException("This Token has not expired yet");

        // Revoking Refresh Token
        storedRefreshToken.IsUsed =  true;
        storedRefreshToken.IsRevoked = true;

        // Fetch User
        var user = await _userManager.FindByIdAsync(userId);
        if(user is null)
            throw new SecurityTokenException("The User doesn't exists");

        var userGuid = Guid.Parse(user.Id);
        var ctxUser = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == userGuid);
        if(ctxUser is null) 
            throw new ArgumentException("The user data is corrupted or missing");

        var roles = await _userManager.GetRolesAsync(user);

        // Create new Token and Refresh Token
        var newJti = Guid.NewGuid().ToString();
        var newToken = _tokenService.CreateToken(ctxUser, user.SecurityStamp!, roles, newJti);
        var newRefreshTokenString = GenerateRefreshToken(); 

        // Refresh token
        var refreshTokenEntity = new RefreshToken {
            Token = newRefreshTokenString,
            UserId = userGuid,
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
