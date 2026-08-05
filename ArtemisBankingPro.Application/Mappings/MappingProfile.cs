using ArtemisBankingPro.Application.DTOs.CreditCard;
using ArtemisBankingPro.Application.DTOs.Loan;
using ArtemisBankingPro.Domain.Entities;
using AutoMapper;

namespace ArtemisBankingPro.Application.Mappings
{
    public class MappingProfile : Profile
    {
        // registrar sus mapeos aquí. Ejemplo:
        // CreateMap<User, UserDto>().ReverseMap();


        public MappingProfile()
        {
            CreateMap<Loan, LoanResponseDto>()
               .ForMember(d => d.Amortization,
                          o => o.MapFrom(s => s.Installments))
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
                              s.Installments.Any(i => i.IsLate) ? "En mora" : "Al día"));
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
                           o => o.MapFrom(s => s.Consumptions));
            CreateMap<CreditCardConsumption, CreditCardConsumptionDto>()
                .ForMember(d => d.Date,
                           o => o.MapFrom(s => s.TransactionDate));
        }
    }
}
    
