using System.Reflection;
using ArtemisBankingPro.Application.Behaviors;
using ArtemisBankingPro.Application.Interfaces.Services; 
using ArtemisBankingPro.Application.Services;           
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ArtemisBankingPro.Application
{
    public static class ServiceRegistration
    {
        public static void AddApplicationLayer(this IServiceCollection services)
        {
            services.AddAutoMapper(config => config.AddMaps(Assembly.GetExecutingAssembly()));

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            services.AddTransient<ISavingsAccountService, SavingsAccountService>();
            services.AddTransient<ITransactionService, TransactionService>();
            services.AddTransient<ICreditCardService, CreditCardService>();

            services.AddTransient<ILoanService, LoanService>();

        }
    }
}