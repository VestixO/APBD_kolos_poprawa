using Kolokwium_s28657.Properties;
 
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IDbService, DbService>();

var app = builder.Build();
app.MapControllers();
app.Run();