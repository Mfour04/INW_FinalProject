using Application.Features.Transaction.Commands;
using Infrastructure.Repositories.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Helpers;

namespace Application.Services.Implements
{
    public class TransactionCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public TransactionCleanupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    var now = TimeHelper.NowVN;
                    var expiredTicks = now.Ticks - TimeSpan.FromMinutes(15).Ticks;

                    var expiredTransactions = await repo.GetExpiredPendingTransactionsAsync(expiredTicks);

                    Console.WriteLine($"[Auto-Cancel] Found {expiredTransactions.Count} expired transactions at {now}");

                    foreach (var tx in expiredTransactions)
                    {
                        if (long.TryParse(tx.id, out var orderCode))
                        {
                            await mediator.Send(new CancelPaymentLinkCommand
                            {
                                OrderCode = orderCode
                            });

                            Console.WriteLine($"[Auto-Cancel] Cancelled OrderCode: {orderCode}");
                        }
                        else
                        {
                            Console.WriteLine($"[Auto-Cancel] Invalid OrderCode (tx.id): {tx.id}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Auto-Cancel] Error: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
