using EventNexus.Application.DTOs;
using EventNexus.Application.Interfaces;
using EventNexus.Domain.Entities;
using EventNexus.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace EventNexus.Infrastructure.Services;

public class AuthService : IAuthService
{   private readonly UserManager<IdentityUser> _userManager;
    private readonly AppDbContext _appDbContext;

    public AuthService(UserManager<IdentityUser> userManager, AppDbContext appDbContext)
    {
        _userManager = userManager;
        _appDbContext = appDbContext;
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
            await transaction.CommitAsync();

            return newUser.Id;

        }
        catch (System.Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }

    }

    // -- LOGIN -- //
    // public async Task<LoginResponseDto>LoginAsync(LoginRequestDto dto){
    //   throw NotImplementedException(); 
    // }
}
