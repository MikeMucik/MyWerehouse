using System.Reflection;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.AddressModels;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Receiving.Models;
using MyWerehouse.Domain.ReversePickings.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Persistence;
using MyWerehouse.Infrastructure.Persistence.Seeding;
using MyWerehouse.Server.Middleware;
using MyWerehouse.Server.ServicesToInfrastructure;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<WerehouseDbContext>(options =>
options.UseSqlServer(
	builder.Configuration.GetConnectionString("DefaultConnection"),
	sqlOptions =>
	{
		sqlOptions.EnableRetryOnFailure(
			maxRetryCount: 10,
			maxRetryDelay: TimeSpan.FromSeconds(30),
			errorNumbersToAdd: null);
	})
);
// Add services to the container.
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
		.AddEntityFrameworkStores<WerehouseDbContext>()
		.AddDefaultTokenProviders();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

//refaktor clean architecture
builder.Services.AddScoped<IUnitOfWork, MyWerehouse.Server.UnitOfWork>();
builder.Services.AddScoped<ICategoryReadService, CategoryReadService>();
builder.Services.AddScoped<IClientReadService, ClientReadService>();
builder.Services.AddScoped<ILocationReadService, LocationReadService>();
builder.Services.AddScoped<IProductReadService, ProductReadService>();
builder.Services.AddScoped<IPalletReadService, PalletReadService>();
builder.Services.AddScoped<IIssueReadService, IssueReadService>();
builder.Services.AddScoped<IReceiptReadService, ReceiptReadService>();
builder.Services.AddScoped<IPickingReadService, PickingReadService>();
builder.Services.AddScoped<IReversePickingReadService, ReversePickingReadService>();
builder.Services.AddScoped<IInventoryReadService, InventoryReadService>();
builder.Services.AddScoped<IHistoryReadService, HistoryReadService>();

//enum string swagger
builder.Services.AddControllers()
	.AddJsonOptions(options =>
	{
		var converters = options.JsonSerializerOptions.Converters;
		converters.Add(new JsonStringEnumConverter<PalletStatus>());
		converters.Add(new JsonStringEnumConverter<IssueStatus>());
		converters.Add(new JsonStringEnumConverter<ReceiptStatus>());
		converters.Add(new JsonStringEnumConverter<PickingStatus>());
		converters.Add(new JsonStringEnumConverter<ReversePickingStatus>());
		converters.Add(new JsonStringEnumConverter<ReversePickingStrategy>());
		converters.Add(new JsonStringEnumConverter<ReasonForPallet>());
	});

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
//base
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
