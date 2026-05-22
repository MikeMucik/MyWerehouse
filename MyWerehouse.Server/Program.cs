using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application;
using MyWerehouse.Application.ViewModels.AddressModels;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Persistence;
using MyWerehouse.Server.Middleware;

Environment.SetEnvironmentVariable("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES", "");

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<WerehouseDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
//.LogTo(Console.WriteLine, LogLevel.Information)//
);
// Add services to the container.
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
		.AddEntityFrameworkStores<WerehouseDbContext>()
		.AddDefaultTokenProviders();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddControllers();

builder.Services.AddValidatorsFromAssemblyContaining<AddressDTOValidation>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.MapType<DateOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema
	{
		Type = "string",
		Format = "date"
	});
});

WebApplication app;

app = builder.Build();
Console.WriteLine($"ENV: {app.Environment.EnvironmentName}");
app.UseMiddleware<ExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
	c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyWerehouse API v1");
	c.RoutePrefix = "swagger";
});
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();