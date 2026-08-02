using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application;
using MyWerehouse.Application.ViewModels.AddressModels;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Persistence;
using MyWerehouse.Infrastructure.Persistence.Seeding;
using MyWerehouse.Server.Middleware;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<WerehouseDbContext>(options =>
options.UseSqlServer(
	builder.Configuration.GetConnectionString("DefaultConnection"),
	sqlOptions =>
	{
		sqlOptions.EnableRetryOnFailure(
			maxRetryCount: 5,
			maxRetryDelay: TimeSpan.FromSeconds(10),
			errorNumbersToAdd: null);
	})
);
// Add services to the container.
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
		.AddEntityFrameworkStores<WerehouseDbContext>()
		.AddDefaultTokenProviders();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddControllers();

builder.Services.AddValidatorsFromAssemblyContaining<AddAddressDTOValidation>();
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

if (builder.Configuration.GetValue<bool>("DemoData:Enabled"))
{
	using var scope = app.Services.CreateScope();
	var dbContext = scope.ServiceProvider.GetRequiredService<WerehouseDbContext>();
	await dbContext.Database.MigrateAsync();
	await DemoDataSeeder.SeedAsync(dbContext);
}

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
