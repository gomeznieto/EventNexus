using EventNexus.Application.DTOs;
using EventNexus.Application.Interfaces;
using EventNexus.Domain.Entities;
using EventNexus.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventNexus.Infrastructure.Services;

public class AuthService : IAuthService
{   private readonly UserManager<IdentityUser> _userManager;
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
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if(user is null) return new LoginResponseDto {Message = "If that email exists, a code was sent(not)"};

        var activeCode = await _appDbContext
            .UserLoginCodes.FirstOrDefaultAsync( u => u.UserId == Guid.Parse(user.Id) && DateTime.UtcNow < u.Expiration);

        if (activeCode is not null){
            activeCode.UsedAt = DateTime.UtcNow;
        }

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

    // --- REGISTER --- //
    public async Task<Guid> RegisterAsync(RegisterRequestDtos dto)
    {
        ArgumentNullException.ThrowIfNull(dto); 

        var userExists = await _userManager.FindByEmailAsync(dto.Email);

        if(userExists != null) throw new ArgumentException("La cuenta de E-mail que intenta registrar ya existe");

        var newIdentityUser = new IdentityUser {
            Email = dto.Email,
            UserName = dto.Email
        };

        var transaction = await _appDbContext.Database.BeginTransactionAsync();

        try
        {
            var result = await _userManager.CreateAsync(newIdentityUser);

            if(!result.Succeeded){
                var errors = string.Join(",", result.Errors.Select( e => e.Description));
                throw new ArgumentException($"Ha ocurrido un error al intentar registrarse: {errors}");
            }

            var newUser = new User {
                Id = Guid.Parse(newIdentityUser.Id),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email
            };

            _appDbContext.Users.Add(newUser);
            await _appDbContext.SaveChangesAsync();
           
            var userManager = await _userManager.FindByIdAsync(newUser.Id.ToString());

            await _userManager.AddToRoleAsync(userManager!, "Customer");
            await transaction.CommitAsync();

            return newUser.Id;

        }
        catch (System.Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }

    }

    // -- VERIFY CODE -- //
    public async Task<AuthResponseDto> VerifyLoginCodeAsync(VerifyRequestDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if(user is null) throw new ArgumentException("Verifique que tanto el código como el mail sean correctos");

        var code = await _appDbContext.UserLoginCodes.FirstOrDefaultAsync(c => c.UserId == Guid.Parse(user.Id) && c.Code == dto.Code && c.UsedAt == null);
        if(code is null) throw new ArgumentException("El código es inválido");
        if(code.Expiration < DateTime.UtcNow) throw new ArgumentException("El código ha expirado");

        code.UsedAt = DateTime.UtcNow;
        await _appDbContext.SaveChangesAsync();
        
        var roles =  await _userManager.GetRolesAsync(user);
        
        var ctxUser = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == Guid.Parse(user.Id));
        if (ctxUser is null) throw new InvalidOperationException("La información del usuario está perdida o corrompida.");
        var token = _tokenService.CreateToken(ctxUser, roles);

        return new AuthResponseDto {
            Token = token
        };
    }
}
