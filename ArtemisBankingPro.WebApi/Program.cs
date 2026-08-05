using ArtemisBankingPro.Application;
using ArtemisBankingPro.Application.Interfaces.Services;
using ArtemisBankingPro.Application.Services;
using ArtemisBankingPro.Domain.Interfaces.Repositories;
using ArtemisBankingPro.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
// Application Layer 
builder.Services.AddApplicationLayer();

// Repositorios (Persistence)
builder.Services.AddScoped<ILoanRepository, LoanRepository>();
builder.Services.AddScoped<ICreditCardRepository, CreditCardRepository>();
builder.Services.AddScoped<ILoanInstallmentRepository, LoanInstallmentRepository>();
builder.Services.AddScoped<ICreditCardConsumptionRepository, CreditCardConsumptionRepository>();

// Servicios (Application)
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<ICreditCardService, CreditCardService>();
builder.Services.AddScoped<ICashierService, CashierService>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
