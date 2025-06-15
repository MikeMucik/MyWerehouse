using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MyWerehouse.Application;
using MyWerehouse.Application.ViewModels.AddressModels;
using MyWerehouse.Infrastructure;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MyWerehouse.Infrastructure.WerehouseDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Add services to the container.
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
		.AddEntityFrameworkStores<MyWerehouse.Infrastructure.WerehouseDbContext>()
		.AddDefaultTokenProviders();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddValidatorsFromAssemblyContaining<AddressDTOValidation>();

//builder.Services.AddSwaggerGen();
var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
