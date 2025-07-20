using Faryma.Composer.Core.Features.OrderQueueFeature.Enums;
using Faryma.Composer.Core.Features.OrderQueueFeature.Models;
using Faryma.Composer.Core.Features.OrderQueueFeature.PriorityAlgorithm;
using Faryma.Composer.Infrastructure.Entities;
using Faryma.Composer.Infrastructure.Enums;
using Microsoft.AspNetCore.Identity;

namespace Faryma.Composer.Core.Test
{
    public class OrderPriorityAlgorithmTest
    {
        private readonly UpperInvariantLookupNormalizer _normalizer = new();

        [Fact]
        public void Basic_Sequence()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonation("10.01.2000", 1, "Nick1", 900),
                GetDonation("10.01.2000", 2, "Nick2", 800),
                GetDonation("10.01.2000", 3, "Nick3", 700),
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderQueuePosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderQueuePosition());

            Algorithm.RefreshOrderPositions(currentStreamDate, orders, orderPositions);

            Assert.Equal(0, orderPositions[1].CurrentIndex);
            Assert.Equal(1, orderPositions[2].CurrentIndex);
            Assert.Equal(2, orderPositions[3].CurrentIndex);
        }

        [Fact]
        public void Basic_Alternation()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonation("10.01.2000", 1, "Nick1", 900),
                GetDonation("10.01.2000", 2, "Nick1", 800),
                GetDonation("10.01.2000", 3, "Nick2", 700),
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderQueuePosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderQueuePosition());

            Algorithm.RefreshOrderPositions(currentStreamDate, orders, orderPositions);

            Assert.Equal(0, orderPositions[1].CurrentIndex);
            Assert.Equal(1, orderPositions[3].CurrentIndex);
            Assert.Equal(2, orderPositions[2].CurrentIndex);
        }

        [Fact]
        public void Donat_Debt()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonation("10.01.2000", 1, "Nick1", 900),
                GetDonation("10.01.2000", 2, "Nick2", 800),
                GetDonation("10.01.2000", 3, "Nick3", 700),

                GetDonation("09.01.2000", 4, "Nick4", 900), // долг x1
                GetDonation("09.01.2000", 5, "Nick5", 800), // долг x1
                GetDonation("09.01.2000", 6, "Nick6", 700), // долг x1
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderQueuePosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderQueuePosition());

            Algorithm.RefreshOrderPositions(currentStreamDate, orders, orderPositions);

            Assert.Equal(0, orderPositions[1].CurrentIndex);
            Assert.Equal(1, orderPositions[4].CurrentIndex);
            Assert.Equal(2, orderPositions[2].CurrentIndex);
            Assert.Equal(3, orderPositions[5].CurrentIndex);
            Assert.Equal(4, orderPositions[3].CurrentIndex);
            Assert.Equal(5, orderPositions[6].CurrentIndex);
        }

        [Fact]
        public void Donat_Debt_Alt1()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonation("10.01.2000", 1, "Nick1", 900),
                GetDonation("10.01.2000", 2, "Nick1", 800),
                GetDonation("10.01.2000", 3, "Nick3", 700),

                GetDonation("09.01.2000", 4, "Nick4", 900), // долг x1
                GetDonation("09.01.2000", 5, "Nick1", 800), // долг x1
                GetDonation("09.01.2000", 6, "Nick6", 700), // долг x1
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderQueuePosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderQueuePosition());

            Algorithm.RefreshOrderPositions(currentStreamDate, orders, orderPositions);

            Assert.Equal((0, "Nick1"), (orderPositions[1].CurrentIndex, orders[1].UserNickname.Nickname));
            Assert.Equal((1, "Nick4"), (orderPositions[4].CurrentIndex, orders[4].UserNickname.Nickname));
            Assert.Equal((2, "Nick1"), (orderPositions[2].CurrentIndex, orders[2].UserNickname.Nickname));
            Assert.Equal((3, "Nick6"), (orderPositions[6].CurrentIndex, orders[6].UserNickname.Nickname));
            Assert.Equal((4, "Nick3"), (orderPositions[3].CurrentIndex, orders[3].UserNickname.Nickname));
            Assert.Equal((5, "Nick1"), (orderPositions[5].CurrentIndex, orders[5].UserNickname.Nickname));
        }

        [Fact]
        public void Donat_Debt_IsOnlyNicknameLeft()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonation("10.01.2000", 1, "Nick1", 900),
                GetDonation("10.01.2000", 2, "Nick1", 800),
                GetDonation("10.01.2000", 3, "Nick1", 700),

                GetDonation("09.01.2000", 4, "Nick1", 900), // долг x1
                GetDonation("09.01.2000", 5, "Nick1", 800), // долг x1
                GetDonation("09.01.2000", 6, "Nick1", 700), // долг x1
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderQueuePosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderQueuePosition());

            Algorithm.RefreshOrderPositions(currentStreamDate, orders, orderPositions);

            (int Current, string Nickname)[] list = items
                .Select(x => (orderPositions[x.Id].CurrentIndex, orders[x.Id].UserNickname.Nickname))
                .OrderBy(x => x.CurrentIndex)
                .ToArray();

            Assert.Equal((0, "Nick1"), (orderPositions[1].CurrentIndex, orders[1].UserNickname.Nickname));
            Assert.Equal((1, "Nick1"), (orderPositions[2].CurrentIndex, orders[2].UserNickname.Nickname));
            Assert.Equal((2, "Nick1"), (orderPositions[3].CurrentIndex, orders[3].UserNickname.Nickname));
            Assert.Equal((3, "Nick1"), (orderPositions[4].CurrentIndex, orders[4].UserNickname.Nickname));
            Assert.Equal((4, "Nick1"), (orderPositions[5].CurrentIndex, orders[5].UserNickname.Nickname));
            Assert.Equal((5, "Nick1"), (orderPositions[6].CurrentIndex, orders[6].UserNickname.Nickname));
        }

        [Fact]
        public void Donat_Debt_Debt()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonation("10.01.2000", 1, "Nick1", 900),
                GetDonation("10.01.2000", 2, "Nick2", 800),
                GetDonation("10.01.2000", 3, "Nick3", 700),

                GetDonation("09.01.2000", 4, "Nick4", 900), // долг x1
                GetDonation("09.01.2000", 5, "Nick5", 800), // долг x1
                GetDonation("09.01.2000", 6, "Nick6", 700), // долг x1

                GetDonation("08.01.2000", 7, "Nick7", 900), // долг x2
                GetDonation("08.01.2000", 8, "Nick8", 800), // долг x2
                GetDonation("08.01.2000", 9, "Nick9", 700), // долг x2
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderQueuePosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderQueuePosition());

            Algorithm.RefreshOrderPositions(currentStreamDate, orders, orderPositions);

            Assert.Equal((0, "Nick1"), (orderPositions[1].CurrentIndex, orders[1].UserNickname.Nickname));
            Assert.Equal((1, "Nick7"), (orderPositions[7].CurrentIndex, orders[7].UserNickname.Nickname));
            Assert.Equal((2, "Nick2"), (orderPositions[2].CurrentIndex, orders[2].UserNickname.Nickname));

            Assert.Equal((3, "Nick4"), (orderPositions[4].CurrentIndex, orders[4].UserNickname.Nickname));
            Assert.Equal((4, "Nick3"), (orderPositions[3].CurrentIndex, orders[3].UserNickname.Nickname));
            Assert.Equal((5, "Nick8"), (orderPositions[8].CurrentIndex, orders[8].UserNickname.Nickname));

            Assert.Equal((6, "Nick5"), (orderPositions[5].CurrentIndex, orders[5].UserNickname.Nickname));
            Assert.Equal((7, "Nick9"), (orderPositions[9].CurrentIndex, orders[9].UserNickname.Nickname));
            Assert.Equal((8, "Nick6"), (orderPositions[6].CurrentIndex, orders[6].UserNickname.Nickname));
        }

        [Fact]
        public void Debt_Debt()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonation("09.01.2000", 4, "Nick4", 900), // долг x1
                GetDonation("09.01.2000", 5, "Nick5", 800), // долг x1
                GetDonation("09.01.2000", 6, "Nick6", 700), // долг x1

                GetDonation("08.01.2000", 7, "Nick7", 900), // долг x2
                GetDonation("08.01.2000", 8, "Nick8", 800), // долг x2
                GetDonation("08.01.2000", 9, "Nick9", 700), // долг x2
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderQueuePosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderQueuePosition());

            Algorithm.RefreshOrderPositions(currentStreamDate, orders, orderPositions);

            Assert.Equal((0, "Nick7"), (orderPositions[7].CurrentIndex, orders[7].UserNickname.Nickname));
            Assert.Equal((1, "Nick4"), (orderPositions[4].CurrentIndex, orders[4].UserNickname.Nickname));
            Assert.Equal((2, "Nick8"), (orderPositions[8].CurrentIndex, orders[8].UserNickname.Nickname));

            Assert.Equal((3, "Nick5"), (orderPositions[5].CurrentIndex, orders[5].UserNickname.Nickname));
            Assert.Equal((4, "Nick9"), (orderPositions[9].CurrentIndex, orders[9].UserNickname.Nickname));
            Assert.Equal((5, "Nick6"), (orderPositions[6].CurrentIndex, orders[6].UserNickname.Nickname));
        }

        [Fact]
        public void OutOfQueue_Donat()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetOutOfQueue("01.01.2000", 1, "Nick1"),
                GetOutOfQueue("01.01.2000", 2, "Nick1"),
                GetOutOfQueue("01.01.2000", 3, "Nick2"),

                GetDonation("10.01.2000", 4, "Nick1", 900),
                GetDonation("10.01.2000", 5, "Nick1", 800),
                GetDonation("10.01.2000", 6, "Nick2", 700),
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderQueuePosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderQueuePosition());

            Algorithm.RefreshOrderPositions(currentStreamDate, orders, orderPositions);

            Assert.Equal((0, "Nick1"), (orderPositions[1].CurrentIndex, orders[1].UserNickname.Nickname));
            Assert.Equal((1, "Nick2"), (orderPositions[3].CurrentIndex, orders[3].UserNickname.Nickname));
            Assert.Equal((2, "Nick1"), (orderPositions[2].CurrentIndex, orders[2].UserNickname.Nickname));

            Assert.Equal((3, "Nick2"), (orderPositions[6].CurrentIndex, orders[6].UserNickname.Nickname));
            Assert.Equal((4, "Nick1"), (orderPositions[4].CurrentIndex, orders[4].UserNickname.Nickname));
            Assert.Equal((5, "Nick1"), (orderPositions[5].CurrentIndex, orders[5].UserNickname.Nickname));
        }

        [Fact]
        public void OutOfQueue_Donat_Debt_Debt()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetOutOfQueue("01.01.2000", 1, "Nick1"),
                GetOutOfQueue("01.01.2000", 2, "Nick1"),
                GetOutOfQueue("01.01.2000", 3, "Nick2"),

                GetDonation("10.01.2000", 4, "Nick1", 900),
                GetDonation("10.01.2000", 5, "Nick1", 800),
                GetDonation("10.01.2000", 6, "Nick2", 700),

                GetDonation("09.01.2000", 7, "Nick4", 900), // долг x1
                GetDonation("09.01.2000", 8, "Nick5", 800), // долг x1
                GetDonation("09.01.2000", 9, "Nick6", 700), // долг x1

                GetDonation("08.01.2000", 10, "Nick7", 900), // долг x2
                GetDonation("08.01.2000", 11, "Nick8", 800), // долг x2
                GetDonation("08.01.2000", 12, "Nick9", 700), // долг x2
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderQueuePosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderQueuePosition());

            Algorithm.RefreshOrderPositions(currentStreamDate, orders, orderPositions);

            Assert.Equal(0, orderPositions[1].CurrentIndex);
            Assert.Equal(1, orderPositions[3].CurrentIndex);
            Assert.Equal(2, orderPositions[2].CurrentIndex);
        }

        [Fact]
        public void OutOfQueue_FutureDonat_Debt_Debt_Inactive()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetOutOfQueue("01.01.2000", 1, "Nick1"),
                GetOutOfQueue("01.01.2000", 2, "Nick1"),
                GetOutOfQueue("01.01.2000", 3, "Nick2"),

                GetDonation("20.01.2000", 4, "Nick1", 900),
                GetDonation("20.01.2000", 5, "Nick1", 800),
                GetDonation("20.01.2000", 6, "Nick2", 700),

                GetDonation("09.01.2000", 7, "Nick2", 900), // долг x1
                GetDonation("09.01.2000", 8, "Nick2", 800), // долг x1
                GetDonation("09.01.2000", 9, "Nick6", 700), // долг x1

                GetDonation("08.01.2000", 10, "Nick1", 900), // долг x2
                GetDonation("08.01.2000", 11, "Nick2", 800), // долг x2
                GetDonation("08.01.2000", 12, "Nick9", 700), // долг x2

                GetDonation("20.01.2000", 13, "Nick2", 900, false),
                GetDonation("10.01.2000", 14, "Nick2", 800, false),
                GetDonation("08.01.2000", 15, "Nick9", 700, false),
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderQueuePosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderQueuePosition());

            Algorithm.RefreshOrderPositions(currentStreamDate, orders, orderPositions);

            Assert.Equal((0, OrderActivityStatus.Future, "Nick1"), (orderPositions[4].CurrentIndex, orderPositions[4].CurrentActivityStatus, orders[4].UserNickname.Nickname));
            Assert.Equal((1, OrderActivityStatus.Future, "Nick1"), (orderPositions[5].CurrentIndex, orderPositions[5].CurrentActivityStatus, orders[5].UserNickname.Nickname));
            Assert.Equal((2, OrderActivityStatus.Future, "Nick2"), (orderPositions[6].CurrentIndex, orderPositions[6].CurrentActivityStatus, orders[6].UserNickname.Nickname));

            Assert.Equal((0, OrderActivityStatus.Active, "Nick1"), (orderPositions[1].CurrentIndex, orderPositions[1].CurrentActivityStatus, orders[1].UserNickname.Nickname));
            Assert.Equal((1, OrderActivityStatus.Active, "Nick2"), (orderPositions[3].CurrentIndex, orderPositions[3].CurrentActivityStatus, orders[3].UserNickname.Nickname));
            Assert.Equal((2, OrderActivityStatus.Active, "Nick1"), (orderPositions[2].CurrentIndex, orderPositions[2].CurrentActivityStatus, orders[2].UserNickname.Nickname));

            Assert.Equal((3, OrderActivityStatus.Active, "Nick2"), (orderPositions[11].CurrentIndex, orderPositions[11].CurrentActivityStatus, orders[11].UserNickname.Nickname));
            Assert.Equal((4, OrderActivityStatus.Active, "Nick6"), (orderPositions[9].CurrentIndex, orderPositions[9].CurrentActivityStatus, orders[9].UserNickname.Nickname));
            Assert.Equal((5, OrderActivityStatus.Active, "Nick1"), (orderPositions[10].CurrentIndex, orderPositions[10].CurrentActivityStatus, orders[10].UserNickname.Nickname));

            Assert.Equal((6, OrderActivityStatus.Active, "Nick2"), (orderPositions[7].CurrentIndex, orderPositions[7].CurrentActivityStatus, orders[7].UserNickname.Nickname));
            Assert.Equal((7, OrderActivityStatus.Active, "Nick9"), (orderPositions[12].CurrentIndex, orderPositions[12].CurrentActivityStatus, orders[12].UserNickname.Nickname));
            Assert.Equal((8, OrderActivityStatus.Active, "Nick2"), (orderPositions[8].CurrentIndex, orderPositions[8].CurrentActivityStatus, orders[8].UserNickname.Nickname));

            Assert.Equal((0, OrderActivityStatus.Inactive, "Nick2"), (orderPositions[13].CurrentIndex, orderPositions[13].CurrentActivityStatus, orders[13].UserNickname.Nickname));
            Assert.Equal((1, OrderActivityStatus.Inactive, "Nick2"), (orderPositions[14].CurrentIndex, orderPositions[14].CurrentActivityStatus, orders[14].UserNickname.Nickname));
            Assert.Equal((2, OrderActivityStatus.Inactive, "Nick9"), (orderPositions[15].CurrentIndex, orderPositions[15].CurrentActivityStatus, orders[15].UserNickname.Nickname));
        }

        private ReviewOrder GetDonation(string eventDate, long id, string name, int amount, bool isActive = true)
        {
            return new()
            {
                Id = id,
                CreatedAt = DateTime.Now,
                IsActive = isActive,
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

        private ReviewOrder GetOutOfQueue(string eventDate, long id, string name)
        {
            return new()
            {
                Id = id,
                CreatedAt = DateTime.Now,
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