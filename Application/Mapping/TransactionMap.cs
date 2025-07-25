using AutoMapper;
using Domain.Entities;
using Shared.Contracts.Response.Transaction;

namespace Application.Mapping
{
    public class TransactionMap : Profile
    {
        public TransactionMap()
        {
            CreateMap<TransactionEntity, AdminTopUpTransactionResponse>()
                .IncludeBase<TransactionEntity, AdminTransactionResponse>()
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.payment_method));

            CreateMap<TransactionEntity, AdminWithdrawTransactionResponse>()
                .IncludeBase<TransactionEntity, AdminTransactionResponse>()
                .ForMember(dest => dest.BankAccountName, opt => opt.MapFrom(src => src.bank_account_name))
                .ForMember(dest => dest.BankAccountNumber, opt => opt.MapFrom(src => src.bank_account_number))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.payment_method))
                .ForMember(dest => dest.ActionById, opt => opt.Ignore())
                .ForMember(dest => dest.ActionType, opt => opt.Ignore())
                .ForMember(dest => dest.Message, opt => opt.Ignore());

            CreateMap<TransactionEntity, AdminBuyNovelTransactionResponse>()
                .IncludeBase<TransactionEntity, AdminTransactionResponse>()
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.novel_id));

            CreateMap<TransactionEntity, AdminBuyChapterTransactionResponse>()
                .IncludeBase<TransactionEntity, AdminTransactionResponse>()
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.novel_id))
                .ForMember(dest => dest.ChapterId, opt => opt.MapFrom(src => src.chapter_id));

            CreateMap<TransactionEntity, AdminTransactionResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.RequesterId, opt => opt.MapFrom(src => src.requester_id))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.type))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.amount))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.status))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.created_at))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.updated_at))
                .ForMember(dest => dest.CompletedAt, opt => opt.MapFrom(src => src.completed_at));

            CreateMap<TransactionEntity, TopUpTransactionResponse>()
                .IncludeBase<TransactionEntity, UserTransactionResponse>()
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.payment_method))
                .ForMember(dest => dest.CompletedAt, opt => opt.MapFrom(src => src.completed_at));

            CreateMap<TransactionEntity, WithdrawTransactionResponse>()
                .IncludeBase<TransactionEntity, UserTransactionResponse>()
                .ForMember(dest => dest.BankAccountName, opt => opt.MapFrom(src => src.bank_account_name))
                .ForMember(dest => dest.BankAccountNumber, opt => opt.MapFrom(src => src.bank_account_number))
                .ForMember(dest => dest.CompletedAt, opt => opt.MapFrom(src => src.completed_at))
                .ForMember(dest => dest.Message, opt => opt.Ignore());

            CreateMap<TransactionEntity, BuyNovelTransactionResponse>()
                .IncludeBase<TransactionEntity, UserTransactionResponse>()
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.novel_id));

            CreateMap<TransactionEntity, BuyChapterTransactionResponse>()
                .IncludeBase<TransactionEntity, UserTransactionResponse>()
                .ForMember(dest => dest.NovelId, opt => opt.MapFrom(src => src.novel_id))
                .ForMember(dest => dest.ChapterId, opt => opt.MapFrom(src => src.chapter_id));

            CreateMap<TransactionEntity, UserTransactionResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.type))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.amount))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.status))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.created_at));
        }
    }
}
