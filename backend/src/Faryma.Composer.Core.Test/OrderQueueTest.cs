using Faryma.Composer.Core.Features.OrderQueueFeature;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Microsoft.AspNetCore.Identity;

namespace Faryma.Composer.Core.Test
{
    public class OrderQueueTest
    {
        private readonly UpperInvariantLookupNormalizer _normalizer = new();

        [Fact]
        public void Basic_Sequence()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonat("10.01.2000", 1, "Nick1", 900),
                GetDonat("10.01.2000", 2, "Nick2", 800),
                GetDonat("10.01.2000", 3, "Nick3", 700),
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderPosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderPosition());

            OrderQueueService.Refresh(currentStreamDate, orders, orderPositions);

            Assert.Equal(0, orderPositions[1].Current);
            Assert.Equal(1, orderPositions[2].Current);
            Assert.Equal(2, orderPositions[3].Current);
        }

        [Fact]
        public void Basic_Alternation()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonat("10.01.2000", 1, "Nick1", 900),
                GetDonat("10.01.2000", 2, "Nick1", 800),
                GetDonat("10.01.2000", 3, "Nick2", 700),
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderPosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderPosition());

            OrderQueueService.Refresh(currentStreamDate, orders, orderPositions);

            Assert.Equal(0, orderPositions[1].Current);
            Assert.Equal(1, orderPositions[3].Current);
            Assert.Equal(2, orderPositions[2].Current);
        }

        [Fact]
        public void Alternation_Donat_Debt()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonat("10.01.2000", 1, "Nick1", 900),
                GetDonat("10.01.2000", 2, "Nick2", 800),
                GetDonat("10.01.2000", 3, "Nick3", 700),

                GetDonat("09.01.2000", 4, "Nick4", 900), // долг x1
                GetDonat("09.01.2000", 5, "Nick5", 800), // долг x1
                GetDonat("09.01.2000", 6, "Nick6", 700), // долг x1
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderPosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderPosition());

            OrderQueueService.Refresh(currentStreamDate, orders, orderPositions);

            Assert.Equal(0, orderPositions[1].Current);
            Assert.Equal(1, orderPositions[4].Current);
            Assert.Equal(2, orderPositions[2].Current);
            Assert.Equal(3, orderPositions[5].Current);
            Assert.Equal(4, orderPositions[3].Current);
            Assert.Equal(5, orderPositions[6].Current);
        }

        [Fact]
        public void Alternation_Donat_Debt_Alt1()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonat("10.01.2000", 1, "Nick1", 900),
                GetDonat("10.01.2000", 2, "Nick1", 800),
                GetDonat("10.01.2000", 3, "Nick3", 700),

                GetDonat("09.01.2000", 4, "Nick4", 900), // долг x1
                GetDonat("09.01.2000", 5, "Nick1", 800), // долг x1
                GetDonat("09.01.2000", 6, "Nick6", 700), // долг x1
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderPosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderPosition());

            OrderQueueService.Refresh(currentStreamDate, orders, orderPositions);

            Assert.Equal((0, "Nick1"), (orderPositions[1].Current, orders[1].UserNickname.Nickname));
            Assert.Equal((1, "Nick4"), (orderPositions[4].Current, orders[4].UserNickname.Nickname));
            Assert.Equal((2, "Nick3"), (orderPositions[3].Current, orders[3].UserNickname.Nickname));
            Assert.Equal((3, "Nick1"), (orderPositions[5].Current, orders[5].UserNickname.Nickname));
            Assert.Equal((4, "Nick1"), (orderPositions[2].Current, orders[2].UserNickname.Nickname));
            Assert.Equal((5, "Nick6"), (orderPositions[6].Current, orders[6].UserNickname.Nickname));
        }

        [Fact]
        public void Alternation_Donat_Debt_Alt2()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonat("10.01.2000", 1, "Nick1", 900),
                GetDonat("10.01.2000", 2, "Nick1", 800),
                GetDonat("10.01.2000", 3, "Nick3", 700),

                GetDonat("09.01.2000", 4, "Nick3", 900), // долг x1
                GetDonat("09.01.2000", 5, "Nick1", 800), // долг x1
                GetDonat("09.01.2000", 6, "Nick6", 700), // долг x1
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderPosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderPosition());

            OrderQueueService.Refresh(currentStreamDate, orders, orderPositions);

            (int Current, string Nickname)[] list = items
                .Select(x => (orderPositions[x.Id].Current, orders[x.Id].UserNickname.Nickname))
                .OrderBy(x => x.Current)
                .ToArray();

            Assert.Equal((0, "Nick1"), (orderPositions[1].Current, orders[1].UserNickname.Nickname));
            Assert.Equal((1, "Nick3"), (orderPositions[4].Current, orders[4].UserNickname.Nickname));
            Assert.Equal((2, "Nick3"), (orderPositions[3].Current, orders[3].UserNickname.Nickname));
            Assert.Equal((3, "Nick1"), (orderPositions[5].Current, orders[5].UserNickname.Nickname));
            Assert.Equal((4, "Nick1"), (orderPositions[2].Current, orders[2].UserNickname.Nickname));
            Assert.Equal((5, "Nick6"), (orderPositions[6].Current, orders[6].UserNickname.Nickname));
        }

        [Fact]
        public void Alternation_Donat_Debt_Alt3()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonat("10.01.2000", 1, "Nick1", 900),
                GetDonat("10.01.2000", 2, "Nick1", 800),
                GetDonat("10.01.2000", 3, "Nick3", 700),

                GetDonat("09.01.2000", 4, "Nick1", 900), // долг x1
                GetDonat("09.01.2000", 5, "Nick3", 800), // долг x1
                GetDonat("09.01.2000", 6, "Nick6", 700), // долг x1
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderPosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderPosition());

            OrderQueueService.Refresh(currentStreamDate, orders, orderPositions);

            Assert.Equal((0, "Nick1"), (orderPositions[1].Current, orders[1].UserNickname.Nickname));
            Assert.Equal((1, "Nick3"), (orderPositions[5].Current, orders[5].UserNickname.Nickname));
            Assert.Equal((2, "Nick3"), (orderPositions[3].Current, orders[3].UserNickname.Nickname));
            Assert.Equal((3, "Nick1"), (orderPositions[4].Current, orders[4].UserNickname.Nickname));
            Assert.Equal((4, "Nick1"), (orderPositions[2].Current, orders[2].UserNickname.Nickname));
            Assert.Equal((5, "Nick6"), (orderPositions[6].Current, orders[6].UserNickname.Nickname));
        }

        [Fact]
        public void Alternation_Donat_Debt_Debt()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonat("10.01.2000", 1, "Nick1", 900),
                GetDonat("10.01.2000", 2, "Nick1", 800),
                GetDonat("10.01.2000", 3, "Nick2", 700),
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderPosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderPosition());

            OrderQueueService.Refresh(currentStreamDate, orders, orderPositions);

            Assert.Equal(0, orderPositions[1].Current);
            Assert.Equal(1, orderPositions[3].Current);
            Assert.Equal(2, orderPositions[2].Current);
        }

        [Fact]
        public void Alternation_Debt_Debt()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonat("10.01.2000", 1, "Nick1", 900),
                GetDonat("10.01.2000", 2, "Nick1", 800),
                GetDonat("10.01.2000", 3, "Nick2", 700),
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderPosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderPosition());

            OrderQueueService.Refresh(currentStreamDate, orders, orderPositions);

            Assert.Equal(0, orderPositions[1].Current);
            Assert.Equal(1, orderPositions[3].Current);
            Assert.Equal(2, orderPositions[2].Current);
        }

        [Fact]
        public void Alternation_OutOfQueue_Donat()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonat("10.01.2000", 1, "Nick1", 900),
                GetDonat("10.01.2000", 2, "Nick1", 800),
                GetDonat("10.01.2000", 3, "Nick2", 700),
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderPosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderPosition());

            OrderQueueService.Refresh(currentStreamDate, orders, orderPositions);

            Assert.Equal(0, orderPositions[1].Current);
            Assert.Equal(1, orderPositions[3].Current);
            Assert.Equal(2, orderPositions[2].Current);
        }

        [Fact]
        public void Alternation_OutOfQueue_Debt()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonat("10.01.2000", 1, "Nick1", 900),
                GetDonat("10.01.2000", 2, "Nick1", 800),
                GetDonat("10.01.2000", 3, "Nick2", 700),
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderPosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderPosition());

            OrderQueueService.Refresh(currentStreamDate, orders, orderPositions);

            Assert.Equal(0, orderPositions[1].Current);
            Assert.Equal(1, orderPositions[3].Current);
            Assert.Equal(2, orderPositions[2].Current);
        }

        [Fact]
        public void Alternation_OutOfQueue_Donat_Debt()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonat("10.01.2000", 1, "Nick1", 900),
                GetDonat("10.01.2000", 2, "Nick1", 800),
                GetDonat("10.01.2000", 3, "Nick2", 700),
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderPosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderPosition());

            OrderQueueService.Refresh(currentStreamDate, orders, orderPositions);

            Assert.Equal(0, orderPositions[1].Current);
            Assert.Equal(1, orderPositions[3].Current);
            Assert.Equal(2, orderPositions[2].Current);
        }

        [Fact]
        public void Test()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonat("10.01.2000", 1, "Nick1", 900),
                GetDonat("10.01.2000", 2, "Nick2", 800),
                GetDonat("10.01.2000", 3, "Nick3", 700),

                GetDonat("09.01.2000", 4, "Nick4", 900), // долг x1
                GetDonat("09.01.2000", 5, "Nick5", 800), // долг x1
                GetDonat("09.01.2000", 6, "Nick6", 700), // долг x1

                GetDonat("08.01.2000", 7, "Nick7", 900), // долг x2
                GetDonat("08.01.2000", 8, "Nick8", 800), // долг x2
                GetDonat("08.01.2000", 9, "Nick9", 700), // долг x2

                GetDonat("07.01.2000", 10, "Nick10", 900), // долг x3
                GetDonat("07.01.2000", 11, "Nick11", 800), // долг x3
                GetDonat("07.01.2000", 12, "Nick12", 700), // долг x3
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderPosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderPosition());

            OrderQueueService.Refresh(currentStreamDate, orders, orderPositions);
        }

        private ReviewOrder GetDonat(string eventDate, long id, string name, int amount)
        {
            return new()
            {
                Id = id,
                CreatedAt = DateTime.Now,
                IsActive = true,
                Status = ReviewOrderStatus.Pending,
                Type = ReviewOrderType.Donation,
                NominalAmount = amount,
                UserNickname = new UserNickname
                {
                    Nickname = name,
                    NormalizedNickname = _normalizer.NormalizeName(name),
                },
                ComposerStream = new ComposerStream
                {
                    EventDate = DateOnly.Parse(eventDate),
                    Type = ComposerStreamType.Regular,
                    Status = ComposerStreamStatus.Planned,
                }
            };
        }

        private ReviewOrder GetOutOfQueue(string eventDate, long id, string name, string createdAt)
        {
            return new()
            {
                Id = id,
                CreatedAt = DateTime.Parse(createdAt),
                IsActive = true,
                Status = ReviewOrderStatus.Pending,
                Type = ReviewOrderType.OutOfQueue,
                UserNickname = new UserNickname
                {
                    Nickname = name,
                    NormalizedNickname = _normalizer.NormalizeName(name),
                },
                ComposerStream = new ComposerStream
                {
                    EventDate = DateOnly.Parse(eventDate),
                    Type = ComposerStreamType.Regular,
                    Status = ComposerStreamStatus.Planned,
                }
            };
        }
    }
}