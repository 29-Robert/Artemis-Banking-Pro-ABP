using ArtemisBankingPro.Application.Features.Accounts.Commands;
using ArtemisBankingPro.Application.Features.Commerces.Commands;
using ArtemisBankingPro.WebApp.ViewModels;
using ArtemisBankingPro.WebApp.Models;
using ArtemisBankingPro.Domain.Entities;
using ArtemisBankingPro.Application.DTOs.Account;
using AutoMapper;

namespace ArtemisBankingPro.WebApp.Mappings
{
    public class WebAppMappingProfile : Profile
    {
        public WebAppMappingProfile()
        {
            CreateMap<CreateSecondaryAccountViewModel, CreateSecondaryAccountCommand>();

            CreateMap<OwnAccountTransferViewModel, TransferCommand>()
                .ForMember(dest => dest.IsOwnAccount, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.IsThirdParty, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.IsBeneficiary, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CashierId, opt => opt.Ignore())
                .ForMember(dest => dest.ClientId, opt => opt.Ignore());

            CreateMap<ExpressTransactionViewModel, TransferCommand>()
                .ForMember(dest => dest.IsOwnAccount, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.IsThirdParty, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.IsBeneficiary, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CashierId, opt => opt.Ignore())
                .ForMember(dest => dest.ClientId, opt => opt.Ignore());

            CreateMap<ThirdTransferViewModel, TransferCommand>()
                .ForMember(dest => dest.SourceAccountNumber, opt => opt.MapFrom(src => src.SourceAccountNumber))
                .ForMember(dest => dest.DestinationAccountNumber, opt => opt.MapFrom(src => src.DestinationAccountNumber))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.IsOwnAccount, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.IsThirdParty, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.IsBeneficiary, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CashierId, opt => opt.Ignore())
                .ForMember(dest => dest.ClientId, opt => opt.Ignore());

            CreateMap<DepositViewModel, DepositCommand>()
                .ForMember(dest => dest.DestinationAccountNumber, opt => opt.MapFrom(src => src.DestinationAccountNumber))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.CashierId, opt => opt.Ignore());

            CreateMap<WithdrawalViewModel, WithdrawCommand>()
                .ForMember(dest => dest.SourceAccountNumber, opt => opt.MapFrom(src => src.SourceAccountNumber))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.CashierId, opt => opt.Ignore());

            CreateMap<CreateCommerceViewModel, CreateCommerceCommand>();
            CreateMap<UpdateCommerceViewModel, UpdateCommerceCommand>();
            CreateMap<CreateBeneficiaryViewModel, CreateBeneficiaryCommand>();
            CreateMap<CreditCardPaymentViewModel, PayCreditCardOwnAccountCommand>()
                .ForMember(dest => dest.SourceAccountNumber, opt => opt.MapFrom(src => src.SourceAccountNumber))
                .ForMember(dest => dest.CardNumber, opt => opt.MapFrom(src => src.CardNumber))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.UserId, opt => opt.Ignore());
            CreateMap<LoanPaymentViewModel, PayLoanOwnAccountCommand>()
                .ForMember(dest => dest.SourceAccountNumber, opt => opt.MapFrom(src => src.SourceAccountNumber))
                .ForMember(dest => dest.LoanNumber, opt => opt.MapFrom(src => src.LoanNumber))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.UserId, opt => opt.Ignore());

            CreateMap<SavingsAccount, SavingsAccountListItemDto>()
                .ForMember(dest => dest.ClientFullName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
                .ForMember(dest => dest.ClientCedula, opt => opt.MapFrom(src => src.User != null ? src.User.Cedula : string.Empty));

            CreateMap<CashAdvanceViewModel, CashAdvanceCommand>()
                .ForMember(dest => dest.SourceAccountNumber, opt => opt.MapFrom(src => src.SourceAccountNumber))
                .ForMember(dest => dest.CardNumber, opt => opt.MapFrom(src => src.CardNumber))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.UserId, opt => opt.Ignore());
        }
    }
}
