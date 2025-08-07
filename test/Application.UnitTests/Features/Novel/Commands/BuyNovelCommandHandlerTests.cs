using Application.Features.Novel.Commands;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories.Interfaces;
using Moq;
using Shouldly;

namespace Application.UnitTests.Features.Novel.Commands
{
    public class BuyNovelCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IPurchaserRepository> _purchaserRepoMock;
        private readonly Mock<ITransactionRepository> _transactionRepoMock;
        private readonly Mock<INovelRepository> _novelRepoMock;
        private readonly Mock<IChapterRepository> _chapterRepoMock;
        private readonly Mock<IAuthorEarningRepository> _authorEarningRepoMock;
        private readonly BuyNovelCommandHandler _handler;

        public BuyNovelCommandHandlerTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _purchaserRepoMock = new Mock<IPurchaserRepository>();
            _transactionRepoMock = new Mock<ITransactionRepository>();
            _novelRepoMock = new Mock<INovelRepository>();
            _chapterRepoMock = new Mock<IChapterRepository>();
            _authorEarningRepoMock = new Mock<IAuthorEarningRepository>();

            _handler = new BuyNovelCommandHandler(
                _userRepoMock.Object,
                _purchaserRepoMock.Object,
                _transactionRepoMock.Object,
                _novelRepoMock.Object,
                _chapterRepoMock.Object,
                _authorEarningRepoMock.Object
            );
        }

        // ---------- FAILURE CASES ----------

        [Fact]
        public async Task Handle_Should_Fail_When_Missing_User_Or_Novel_Id()
        {
            var command = new BuyNovelCommand { UserId = null, NovelId = null };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Missing user or novel ID.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Novel_Not_Found()
        {
            var command = new BuyNovelCommand { UserId = "u1", NovelId = "n1", CoinCost = 100 };

            _novelRepoMock.Setup(n => n.GetByNovelIdAsync("n1"))
                .ReturnsAsync((NovelEntity?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Novel not found.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Novel_Is_Free()
        {
            var command = new BuyNovelCommand { UserId = "u1", NovelId = "n1", CoinCost = 100 };

            _novelRepoMock.Setup(n => n.GetByNovelIdAsync("n1"))
                .ReturnsAsync(new NovelEntity { id = "n1", is_paid = false });

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("This novel is free and does not need to be purchased.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_Novel_Not_Completed()
        {
            var command = new BuyNovelCommand { UserId = "u1", NovelId = "n1", CoinCost = 100 };

            _novelRepoMock.Setup(n => n.GetByNovelIdAsync("n1"))
                .ReturnsAsync(new NovelEntity { id = "n1", is_paid = true, status = NovelStatus.Ongoing });

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Only completed novels can be purchased in full.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_User_Already_Owns_Novel()
        {
            var command = new BuyNovelCommand { UserId = "u1", NovelId = "n1", CoinCost = 100 };

            _novelRepoMock.Setup(n => n.GetByNovelIdAsync("n1"))
                .ReturnsAsync(new NovelEntity { id = "n1", is_paid = true, status = NovelStatus.Completed });

            _purchaserRepoMock.Setup(p => p.GetByUserAndNovelAsync("u1", "n1"))
                .ReturnsAsync(new PurchaserEntity { is_full = true });

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("User already owns this novel.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_User_Not_Found()
        {
            var command = new BuyNovelCommand { UserId = "u1", NovelId = "n1", CoinCost = 100 };

            _novelRepoMock.Setup(n => n.GetByNovelIdAsync("n1"))
                .ReturnsAsync(new NovelEntity { id = "n1", is_paid = true, status = NovelStatus.Completed });

            _purchaserRepoMock.Setup(p => p.GetByUserAndNovelAsync("u1", "n1"))
                .ReturnsAsync((PurchaserEntity?)null);

            _userRepoMock.Setup(u => u.GetById("u1"))
                .ReturnsAsync((UserEntity?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("User not found.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_User_Insufficient_Coins()
        {
            var command = new BuyNovelCommand { UserId = "u1", NovelId = "n1", CoinCost = 100 };

            _novelRepoMock.Setup(n => n.GetByNovelIdAsync("n1"))
                .ReturnsAsync(new NovelEntity { id = "n1", is_paid = true, status = NovelStatus.Completed });

            _purchaserRepoMock.Setup(p => p.GetByUserAndNovelAsync("u1", "n1"))
                .ReturnsAsync((PurchaserEntity?)null);

            _userRepoMock.Setup(u => u.GetById("u1"))
                .ReturnsAsync(new UserEntity { id = "u1", coin = 50 });

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Insufficient coins.");
        }

        [Fact]
        public async Task Handle_Should_Fail_When_DecreaseCoin_Fails()
        {
            var command = new BuyNovelCommand { UserId = "u1", NovelId = "n1", CoinCost = 100 };

            _novelRepoMock.Setup(n => n.GetByNovelIdAsync("n1"))
                .ReturnsAsync(new NovelEntity { id = "n1", is_paid = true, status = NovelStatus.Completed, author_id = "a1" });

            _purchaserRepoMock.Setup(p => p.GetByUserAndNovelAsync("u1", "n1"))
                .ReturnsAsync((PurchaserEntity?)null);

            _userRepoMock.Setup(u => u.GetById("u1"))
                .ReturnsAsync(new UserEntity { id = "u1", coin = 200 });

            _chapterRepoMock.Setup(c => c.GetIdsByNovelIdAsync("n1"))
                .ReturnsAsync(new List<string> { "ch1", "ch2" });

            _userRepoMock.Setup(u => u.DecreaseCoinAsync("u1", 100))
                .ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeFalse();
            result.Message.ShouldBe("Failed to deduct coins.");
        }

        // ---------- SUCCESS CASES ----------

        [Fact]
        public async Task Handle_Should_Create_New_Purchase_Successfully()
        {
            var command = new BuyNovelCommand { UserId = "u1", NovelId = "n1", CoinCost = 100 };

            _novelRepoMock.Setup(n => n.GetByNovelIdAsync("n1"))
                .ReturnsAsync(new NovelEntity { id = "n1", is_paid = true, status = NovelStatus.Completed, author_id = "a1" });

            _purchaserRepoMock.Setup(p => p.GetByUserAndNovelAsync("u1", "n1"))
                .ReturnsAsync((PurchaserEntity?)null);

            _userRepoMock.Setup(u => u.GetById("u1"))
                .ReturnsAsync(new UserEntity { id = "u1", coin = 200 });

            _chapterRepoMock.Setup(c => c.GetIdsByNovelIdAsync("n1"))
                .ReturnsAsync(new List<string> { "ch1", "ch2" });

            _userRepoMock.Setup(u => u.DecreaseCoinAsync("u1", 100))
                .ReturnsAsync(true);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Purchase novel successfully.");
            _transactionRepoMock.Verify(t => t.AddAsync(It.IsAny<TransactionEntity>()), Times.Once);
            _purchaserRepoMock.Verify(p => p.CreateAsync(It.IsAny<PurchaserEntity>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Upgrade_Existing_Purchase_To_Full_Successfully()
        {
            var command = new BuyNovelCommand { UserId = "u1", NovelId = "n1", CoinCost = 100 };

            var existingPurchaser = new PurchaserEntity
            {
                id = "pr1",
                user_id = "u1",
                novel_id = "n1",
                is_full = false
            };

            _novelRepoMock.Setup(n => n.GetByNovelIdAsync("n1"))
                .ReturnsAsync(new NovelEntity { id = "n1", is_paid = true, status = NovelStatus.Completed, author_id = "a1" });

            _purchaserRepoMock.Setup(p => p.GetByUserAndNovelAsync("u1", "n1"))
                .ReturnsAsync(existingPurchaser);

            _userRepoMock.Setup(u => u.GetById("u1"))
                .ReturnsAsync(new UserEntity { id = "u1", coin = 200 });

            _chapterRepoMock.Setup(c => c.GetIdsByNovelIdAsync("n1"))
                .ReturnsAsync(new List<string> { "ch1", "ch2" });

            _userRepoMock.Setup(u => u.DecreaseCoinAsync("u1", 100))
                .ReturnsAsync(true);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.ShouldBeTrue();
            result.Message.ShouldBe("Upgraded to full novel purchase.");
            _purchaserRepoMock.Verify(p => p.UpdateAsync(existingPurchaser.id, existingPurchaser), Times.Once);
        }
    }
}
