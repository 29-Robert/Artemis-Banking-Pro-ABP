using ArtemisBankingPro.Application.DTOs.CreditCard;
using ArtemisBankingPro.Application.DTOs.Loan;
using ArtemisBankingPro.Application.DTOs.Commerces;
using ArtemisBankingPro.Domain.Entities;
using AutoMapper;
using System.Linq;

namespace ArtemisBankingPro.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            // LOAN
            CreateMap<Loan, LoanResponseDto>()
                .ForMember(d => d.ClientId,
                           o => o.MapFrom(s => s.ClientId.ToString()))
                .ForMember(d => d.ClientFullName,
                           o => o.MapFrom(s => s.Client != null ? $"{s.Client.FirstName} {s.Client.LastName}" : string.Empty))
                .ForMember(d => d.MonthlyInstallment,
                           o => o.MapFrom(s => s.Installments != null && s.Installments.Any()
                               ? s.Installments.OrderBy(i => i.InstallmentNumber).First().InstallmentAmount
                               : 0))
                .ForMember(d => d.PaidInstallments,
                           o => o.MapFrom(s => s.Installments != null
                               ? s.Installments.Count(i => i.PaymentStatus == "Pagada") : 0))
                .ForMember(d => d.PendingAmount,
                           o => o.MapFrom(s => s.Installments != null
                               ? s.Installments.Sum(i => i.PendingInstallmentAmount) : 0))
                .ForMember(d => d.TotalAmountToPay,
                           o => o.MapFrom(s => s.Installments != null
                               ? s.Installments.Sum(i => i.InstallmentAmount) : 0))
                .ForMember(d => d.ClientPaymentStatus,
                           o => o.MapFrom(s => s.Installments != null &&
                               s.Installments.Any(i => i.IsLate && i.PaymentStatus != "Pagada") ? "En mora" : "Al día"))
                .ForMember(d => d.Amortization,
                           o => o.MapFrom(s => s.Installments != null
                               ? s.Installments.OrderBy(i => i.InstallmentNumber) : null))
                
                .ForMember(d => d.EmailNotificationFailed, o => o.Ignore());

            CreateMap<LoanInstallment, LoanInstallmentDto>();
           


            // CREDIT CARD
            CreateMap<CreditCard, CreditCardResponseDto>()
                .ForMember(d => d.MaskedCardNumber,
                           o => o.MapFrom(s => "************" + s.CardNumber.Substring(12)))
                .ForMember(d => d.LastFourDigits,
                           o => o.MapFrom(s => s.CardNumber.Substring(12)))
                .ForMember(d => d.AvailableCredit,
                           o => o.MapFrom(s => s.CreditLimit - s.CurrentDebt))
                .ForMember(d => d.ExpirationDate,
                           o => o.MapFrom(s => $"{s.ExpirationMonth}/{s.ExpirationYear.Substring(2)}"))
                .ForMember(d => d.Consumptions,
                           o => o.MapFrom(s => s.Consumptions))
                .ForMember(d => d.ClientFullName,
                           o => o.MapFrom(s => s.Client != null ? $"{s.Client.FirstName} {s.Client.LastName}" : string.Empty))
                .ForMember(d => d.ClientId,
                           o => o.MapFrom(s => s.ClientId.ToString()));

            CreateMap<CreditCard, CreditCardCreatedResponseDto>()
                .IncludeBase<CreditCard, CreditCardResponseDto>()
                .ForMember(d => d.Cvc, o => o.Ignore());

            CreateMap<CreditCardConsumption, CreditCardConsumptionDto>()
                .ForMember(d => d.Date, o => o.MapFrom(s => s.TransactionDate));

            // COMMERCE
            CreateMap<Commerce, CommerceListItemDto>();
            CreateMap<Commerce, CommerceDetailDto>();
            CreateMap<CreateCommerceDto, Commerce>()
                .ForMember(d => d.BusinessName, o => o.MapFrom(s => s.BusinessName))
                .ForMember(d => d.RNC, o => o.MapFrom(s => s.RNC))
                .ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
                .ForMember(d => d.Phone, o => o.MapFrom(s => s.Phone))
                .ForMember(d => d.Address, o => o.MapFrom(s => s.Address))
                .ForMember(d => d.IsActive, o => o.MapFrom(s => true))
                .ForMember(d => d.PrincipalAccountNumber, o => o.Ignore())
                .ForMember(d => d.User, o => o.Ignore())
                .ForMember(d => d.Consumptions, o => o.Ignore())
                .ForMember(d => d.Id, o => o.Ignore());

            CreateMap<UpdateCommerceDto, Commerce>()
                .ForMember(d => d.BusinessName, o => o.MapFrom(s => s.BusinessName))
                .ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
                .ForMember(d => d.Phone, o => o.MapFrom(s => s.Phone))
                .ForMember(d => d.RNC, o => o.Ignore())
                .ForMember(d => d.Address, o => o.Ignore())
                .ForMember(d => d.IsActive, o => o.Ignore())
                .ForMember(d => d.PrincipalAccountNumber, o => o.Ignore())
                .ForMember(d => d.User, o => o.Ignore())
                .ForMember(d => d.Consumptions, o => o.Ignore())
                .ForMember(d => d.Id, o => o.Ignore());
        }
    }
}
    
