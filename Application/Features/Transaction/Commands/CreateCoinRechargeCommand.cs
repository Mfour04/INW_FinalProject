using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;
using Shared.Helpers;

namespace Application.Features.Transaction.Commands
{
    public class CreateCoinRechargeCommand : IRequest<string>
    {
        public string? UserId { get; set; }
        public int CoinAmount { get; set; }
    }

    public class CreateCoinRechargeCommandHandler : IRequestHandler<CreateCoinRechargeCommand, string>
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly PayOS _payOS;
        private readonly string _baseUrl;

        public CreateCoinRechargeCommandHandler(ITransactionRepository transactionRepo, PayOS payOS, IConfiguration config)
        {
            _transactionRepo = transactionRepo;
            _payOS = payOS;
            _baseUrl = config["BeUrl"] ?? throw new ArgumentNullException("BaseUrl is missing in config");
        }

        public async Task<string> Handle(CreateCoinRechargeCommand request, CancellationToken cancellationToken)
        {
            var orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + new Random().Next(0, 999);
            var amountVND = request.CoinAmount * 1000;

            var items = new List<ItemData>
            {
                new ItemData($"Recharge {request.CoinAmount} coins", 1, amountVND)
            };

            var paymentData = new PaymentData(
                orderCode,
                amountVND,
                "Recharge Coins",
                items,
                cancelUrl: $"{_baseUrl}/api/transactions/recharges/cancel-url",
                returnUrl: $"{_baseUrl}/api/transactions/recharges/return-url"
            );

            var paymentLink = await _payOS.createPaymentLink(paymentData);

            var transaction = new TransactionEntity
            {
                id = orderCode.ToString(),
                requester_id = request.UserId,
                type = PaymentType.TopUp,
                amount = request.CoinAmount,
                payment_method = "PayOS",
                status = PaymentStatus.Pending,
                created_at = TimeHelper.NowTicks
            };

            await _transactionRepo.AddAsync(transaction);

            return paymentLink.checkoutUrl;
        }
    }
}