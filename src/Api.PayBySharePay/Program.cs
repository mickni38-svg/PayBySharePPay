using Api.PayBySharePay.Middleware;
using DataStorage.PayBySharePay.Extensions;
using Service.PayBySharePay.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "PayBySharePay API",
        Version = "v1",
        Description = "API til fælles bestillinger og delt betaling"
    });
});

var connectionString = builder.Configuration.GetConnectionString("PayBySharePayDb")
    ?? throw new InvalidOperationException("Connection string 'PayBySharePayDb' er ikke konfigureret.");

builder.Services.AddDataStorage(connectionString);
builder.Services.AddServiceLayer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "PayBySharePay API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
