using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application;
using MyWerehouse.Application.ViewModels.AddressModels;
using MyWerehouse.Infrastructure;
using MyWerehouse.Server.Middleware;

Environment.SetEnvironmentVariable("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES", "");

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<MyWerehouse.Infrastructure.WerehouseDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Add services to the container.
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
		.AddEntityFrameworkStores<MyWerehouse.Infrastructure.WerehouseDbContext>()
		.AddDefaultTokenProviders();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddControllers();

builder.Services.AddValidatorsFromAssemblyContaining<AddressDTOValidation>();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
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

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	//app.UseSwaggerUI(c =>
	//{
	//	c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyWerehouse API v1");
	//	c.RoutePrefix = "swagger";
	//	c.HeadContent = "<!-- Swagger UI clean -->";
	//	//c.EnableTryItOutByDefault();
	//	//c.ConfigObject = new Swashbuckle.AspNetCore.SwaggerUI.SwaggerUIOptions
	//	//{
	//	//	DocExpansion = Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None
	//	//};
	//});
}
app.UseStaticFiles();

app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

//WebApplication app;

//try
//{
//	 app = builder.Build();
//	if (app.Environment.IsDevelopment())
//	{
//		app.UseSwagger();
//		app.UseSwaggerUI();
//	}
//	app.UseMiddleware<ExceptionMiddleware>();
//	app.UseDefaultFiles();
//	app.MapStaticAssets();

//	// Configure the HTTP request pipeline.
//	if (app.Environment.IsDevelopment())
//	{
//		app.MapOpenApi();
//	}

//	app.UseHttpsRedirection();

//	app.UseAuthorization();

//	app.MapControllers();

//	app.MapFallbackToFile("/index.html");

//	app.Run();

//}
//catch (ReflectionTypeLoadException ex)
//{
//	foreach (var loaderException in ex.LoaderExceptions)
//	{
//		Console.WriteLine(loaderException?.Message);
//	}
//	throw;
//}
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
//builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSingleton<MediatR.Licensing.ILicense, MediatR.Licensing.OpenSourceLicense>();
//builder.Services.AddMediatR(cfg =>
//cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));