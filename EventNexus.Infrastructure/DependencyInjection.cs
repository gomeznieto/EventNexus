using Resend;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

public static class DependencyInjection {
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration){
        services.AddOptions();
        services.Configure<ResendClientOptions>(configuration.GetSection("Resend"));

        services.AddHttpClient<IResend, ResendClient>();
        services.AddTransient<IResend, ResendClient>();
        return services;
    }
}
