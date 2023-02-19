using StripeExample.Extensions;
using StripeExample.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<IStripeService, StripeService>();
builder.Services.AddStripeInfrastructure(builder.Configuration);
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();




app.MapControllers();
app.Run();

