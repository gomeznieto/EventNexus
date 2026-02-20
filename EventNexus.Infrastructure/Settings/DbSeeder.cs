using EventNexus.Domain.Entities;
using EventNexus.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

public static class DbSeeder {

    public static async Task Seed(IApplicationBuilder applicationBuilder)
    {
        using (var scope = applicationBuilder.ApplicationServices.CreateScope()){

            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            if(dbContext is null || roleManager is null || userManager is null) return;

            // Crear Roles
            List<string> roles = ["Admin", "Patient", "Doctor"];
            foreach(var role in roles){
                if(!await roleManager!.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Creamos usuario si no existe
            var email = "admin@test.com";
            var firstName = "Super";
            var lastName = "Admin";

            var adminExist = await userManager.FindByEmailAsync(email);

            if(adminExist != null) return; // Si existe retornamos

            // Si no existe el admin iniciamos transacción
            var transaction = await dbContext.Database.BeginTransactionAsync();

            try {
                var newAdmin = new IdentityUser{
                    Email = email,
                    UserName = email
                };

                var result = await userManager.CreateAsync(newAdmin);
                if(!result.Succeeded){
                    var errors = string.Join(",", result.Errors.Select(e => e.Description));
                    throw new Exception($"Error al crear el admin {errors}");
                }

                await userManager.AddToRoleAsync(newAdmin, roles[0]);

                var newUser = new User {
                    Id = Guid.Parse(newAdmin.Id),
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email 
                };

                dbContext.Users.Add(newUser);
                await dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

            } catch(Exception){
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}
