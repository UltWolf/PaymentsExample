using Stripe;
using StripeExample.Services;

namespace StripeExample.Extensions
{
    public static class ServiceCollection
    {
        public static IServiceCollection AddStripeInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            StripeConfiguration.ApiKey = configuration.GetValue<string>("Stripe:SecretKey");

            return services
                .AddScoped<CustomerService>()
                .AddScoped<ChargeService>()
                .AddScoped<TokenService>()
                .AddScoped<IStripeService, StripeService>();
        }
    }
}
