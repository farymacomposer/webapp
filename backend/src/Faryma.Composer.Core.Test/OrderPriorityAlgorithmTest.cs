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
        public void Donat()
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

            Check([
                (1, "Nick1"),
                (2, "Nick2"),
                (3, "Nick3"),
            ], orders, orderPositions);
        }

        [Fact]
        public void Donat_Alt1()
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

            Check([
                (1, "Nick1"),
                (3, "Nick2"),
                (2, "Nick1"),
            ], orders, orderPositions);
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

            Check([
                (1, "Nick1"),
                (4, "Nick4"), // долг x1
                (2, "Nick2"),
                (5, "Nick5"), // долг x1
                (3, "Nick3"),
                (6, "Nick6"), // долг x1
            ], orders, orderPositions);
        }

        [Fact]
        public void Donat_Debt_Alt1()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonation("10.01.2000", 1, "Nick1", 900),
                GetDonation("10.01.2000", 2, "Nick1", 800),
                GetDonation("10.01.2000", 3, "Nick2", 700),

                GetDonation("09.01.2000", 4, "Nick1", 900), // долг x1
                GetDonation("09.01.2000", 5, "Nick1", 800), // долг x1
                GetDonation("09.01.2000", 6, "Nick2", 700), // долг x1
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderQueuePosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderQueuePosition());
            Algorithm.RefreshOrderPositions(currentStreamDate, orders, orderPositions);

            Check([
                (1, "Nick1"),
                (6, "Nick2"), // долг x1
                (2, "Nick1"),
                (3, "Nick2"),
                (4, "Nick1"), // долг x1
                (5, "Nick1"), // долг x1
            ], orders, orderPositions);
        }

        [Fact]
        public void Donat_Debt_Alt2()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonation("10.01.2000", 1, "Nick2", 900),
                GetDonation("10.01.2000", 2, "Nick1", 800),
                GetDonation("10.01.2000", 3, "Nick1", 700),

                GetDonation("09.01.2000", 4, "Nick2", 900), // долг x1
                GetDonation("09.01.2000", 5, "Nick1", 800), // долг x1
                GetDonation("09.01.2000", 6, "Nick1", 700), // долг x1
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderQueuePosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderQueuePosition());
            Algorithm.RefreshOrderPositions(currentStreamDate, orders, orderPositions);

            Check([
                (1, "Nick2"),
                (5, "Nick1"), // долг x1
                (4, "Nick2"), // долг x1
                (2, "Nick1"),
                (3, "Nick1"),
                (6, "Nick1"), // долг x1
            ], orders, orderPositions);
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

            Check([
                (1, "Nick1"),
                (2, "Nick1"),
                (3, "Nick1"),
                (4, "Nick1"), // долг x1
                (5, "Nick1"), // долг x1
                (6, "Nick1"), // долг x1
            ], orders, orderPositions);
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

            Check([
                (1, "Nick1"),
                (7, "Nick7"), // долг x2
                (2, "Nick2"),
                (4, "Nick4"), // долг x1
                (3, "Nick3"),
                (8, "Nick8"), // долг x2
                (5, "Nick5"), // долг x1
                (9, "Nick9"), // долг x2
                (6, "Nick6"), // долг x1
            ], orders, orderPositions);
        }

        [Fact]
        public void Donat_Debt_Debt_Alt1()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonation("10.01.2000", 1, "Nick1", 900),
                GetDonation("10.01.2000", 2, "Nick1", 800),
                GetDonation("10.01.2000", 3, "Nick2", 700),

                GetDonation("09.01.2000", 4, "Nick1", 900), // долг x1
                GetDonation("09.01.2000", 5, "Nick1", 800), // долг x1
                GetDonation("09.01.2000", 6, "Nick2", 700), // долг x1

                GetDonation("08.01.2000", 7, "Nick1", 900), // долг x2
                GetDonation("08.01.2000", 8, "Nick1", 800), // долг x2
                GetDonation("08.01.2000", 9, "Nick2", 700), // долг x2
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderQueuePosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderQueuePosition());
            Algorithm.RefreshOrderPositions(currentStreamDate, orders, orderPositions);

            Check([
                (1, "Nick1"),
                (9, "Nick2"), // долг x2
                (2, "Nick1"),
                (6, "Nick2"), // долг x1
                (7, "Nick1"), // долг x2
                (3, "Nick2"),
                (4, "Nick1"), // долг x1
                (8, "Nick1"), // долг x2
                (5, "Nick1"), // долг x1
            ], orders, orderPositions);
        }

        [Fact]
        public void Donat_Debt_Debt_Alt2()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonation("10.01.2000", 1, "Nick2", 900),
                GetDonation("10.01.2000", 2, "Nick1", 800),
                GetDonation("10.01.2000", 3, "Nick1", 700),

                GetDonation("09.01.2000", 4, "Nick2", 900), // долг x1
                GetDonation("09.01.2000", 5, "Nick1", 800), // долг x1
                GetDonation("09.01.2000", 6, "Nick1", 700), // долг x1

                GetDonation("08.01.2000", 7, "Nick2", 900), // долг x2
                GetDonation("08.01.2000", 8, "Nick1", 800), // долг x2
                GetDonation("08.01.2000", 9, "Nick1", 700), // долг x2
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderQueuePosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderQueuePosition());
            Algorithm.RefreshOrderPositions(currentStreamDate, orders, orderPositions);

            Check([
                (1, "Nick2"),
                (8, "Nick1"), // долг x2
                (4, "Nick2"), // долг x1
                (2, "Nick1"),
                (7, "Nick2"), // долг x2
                (3, "Nick1"),
                (5, "Nick1"), // долг x1
                (9, "Nick1"), // долг x2
                (6, "Nick1"), // долг x1
            ], orders, orderPositions);
        }

        [Fact]
        public void Donat_Debt_Debt_Alt3()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonation("10.01.2000", 1, "Nick1", 900),
                GetDonation("10.01.2000", 2, "Nick1", 800),
                GetDonation("10.01.2000", 3, "Nick1", 700),
                GetDonation("10.01.2000", 4, "Nick1", 600),
                GetDonation("10.01.2000", 5, "Nick1", 500),
                GetDonation("10.01.2000", 6, "Nick1", 400),

                GetDonation("09.01.2000", 7, "Nick1", 900), // долг x1
                GetDonation("09.01.2000", 8, "Nick2", 800), // долг x1
                GetDonation("09.01.2000", 9, "Nick3", 700), // долг x1

                GetDonation("08.01.2000", 10, "Nick1", 900), // долг x2
                GetDonation("08.01.2000", 11, "Nick4", 800), // долг x2
                GetDonation("08.01.2000", 12, "Nick5", 700), // долг x2
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderQueuePosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderQueuePosition());
            Algorithm.RefreshOrderPositions(currentStreamDate, orders, orderPositions);

            Check([
                (1,  "Nick1"),
                (11, "Nick4"), // долг x2
                (2,  "Nick1"),
                (8,  "Nick2"), // долг x1
                (3,  "Nick1"),
                (12, "Nick5"), // долг x2
                (4,  "Nick1"),
                (9,  "Nick3"), // долг x1
                (5,  "Nick1"),
                (6,  "Nick1"),
                (10, "Nick1"), // долг x2
                (7,  "Nick1"), // долг x1
            ], orders, orderPositions);
        }

        [Fact]
        public void Donat_Debt_Debt_Debt()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonation("10.01.2000", 1, "Nick1", 900),
                GetDonation("10.01.2000", 2, "Nick1", 800),
                GetDonation("10.01.2000", 3, "Nick1", 700),
                GetDonation("10.01.2000", 4, "Nick1", 600),
                GetDonation("10.01.2000", 5, "Nick1", 500),
                GetDonation("10.01.2000", 6, "Nick1", 400),

                GetDonation("09.01.2000", 7, "Nick2", 900), // долг x1
                GetDonation("09.01.2000", 8, "Nick3", 800), // долг x1
                GetDonation("09.01.2000", 9, "Nick4", 700), // долг x1

                GetDonation("08.01.2000", 10, "Nick5", 900), // долг x2
                GetDonation("08.01.2000", 11, "Nick6", 800), // долг x2
                GetDonation("08.01.2000", 12, "Nick7", 700), // долг x2

                GetDonation("07.01.2000", 13, "Nick8", 900), // долг x3
                GetDonation("07.01.2000", 14, "Nick9", 800), // долг x3
                GetDonation("07.01.2000", 15, "Nick10", 700), // долг x3
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderQueuePosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderQueuePosition());
            Algorithm.RefreshOrderPositions(currentStreamDate, orders, orderPositions);

            Check([
                (1,  "Nick1"),
                (13, "Nick8"), // долг x3
                (2,  "Nick1"),
                (10, "Nick5"), // долг x2
                (3,  "Nick1"),
                (7,  "Nick2"), // долг x1

                (4,  "Nick1"),
                (14, "Nick9"), // долг x3
                (5,  "Nick1"),
                (11, "Nick6"), // долг x2
                (6,  "Nick1"),
                (8,  "Nick3"), // долг x1

                (15, "Nick10"), // долг x3
                (12, "Nick7"), // долг x2
                (9,  "Nick4"), // долг x1
            ], orders, orderPositions);
        }

        [Fact]
        public void Donat_Debt_Debt_Debt_Alt1()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetDonation("10.01.2000", 1, "Nick10", 900),
                GetDonation("10.01.2000", 2, "Nick10", 800),
                GetDonation("10.01.2000", 3, "Nick11", 700),
                GetDonation("10.01.2000", 4, "Nick11", 600),
                GetDonation("10.01.2000", 5, "Nick12", 500),
                GetDonation("10.01.2000", 6, "Nick12", 400),

                GetDonation("09.01.2000", 7, "Nick1", 900), // долг x1
                GetDonation("09.01.2000", 8, "Nick2", 800), // долг x1
                GetDonation("09.01.2000", 9, "Nick3", 700), // долг x1

                GetDonation("08.01.2000", 10, "Nick4", 900), // долг x2
                GetDonation("08.01.2000", 11, "Nick5", 800), // долг x2
                GetDonation("08.01.2000", 12, "Nick6", 700), // долг x2

                GetDonation("07.01.2000", 13, "Nick7", 900), // долг x3
                GetDonation("07.01.2000", 14, "Nick8", 800), // долг x3
                GetDonation("07.01.2000", 15, "Nick9", 700), // долг x3
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderQueuePosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderQueuePosition());
            Algorithm.RefreshOrderPositions(currentStreamDate, orders, orderPositions);

            Check([
                (1,  "Nick10"),
                (13, "Nick7"), // долг x3
                (3,  "Nick11"),
                (10, "Nick4"), // долг x2
                (2,  "Nick10"),
                (7,  "Nick1"), // долг x1
                (4,  "Nick11"),
                (14, "Nick8"), // долг x3
                (5,  "Nick12"),
                (11, "Nick5"), // долг x2
                (6,  "Nick12"),
                (8,  "Nick2"), // долг x1
                (15, "Nick9"), // долг x3
                (12, "Nick6"), // долг x2
                (9,  "Nick3"), // долг x1
            ], orders, orderPositions);
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

            Check([
                (7, "Nick7"), // долг x2
                (4, "Nick4"), // долг x1

                (8, "Nick8"), // долг x2
                (5, "Nick5"), // долг x1

                (9, "Nick9"), // долг x2
                (6, "Nick6"), // долг x1
            ], orders, orderPositions);
        }

        [Fact]
        public void OutOfQueue()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetOutOfQueue("01.01.2000", 1, "Nick1"), // ВНЕ ОЧЕРЕДИ
                GetOutOfQueue("01.01.2000", 2, "Nick1"), // ВНЕ ОЧЕРЕДИ
                GetOutOfQueue("01.01.2000", 3, "Nick1"), // ВНЕ ОЧЕРЕДИ
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderQueuePosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderQueuePosition());
            Algorithm.RefreshOrderPositions(currentStreamDate, orders, orderPositions);

            Check([
                (1, "Nick1"), // ВНЕ ОЧЕРЕДИ
                (2, "Nick1"), // ВНЕ ОЧЕРЕДИ
                (3, "Nick1"), // ВНЕ ОЧЕРЕДИ
            ], orders, orderPositions);
        }

        [Fact]
        public void OutOfQueue_Donat()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetOutOfQueue("01.01.2000", 1, "Nick1"), // ВНЕ ОЧЕРЕДИ
                GetOutOfQueue("01.01.2000", 2, "Nick1"), // ВНЕ ОЧЕРЕДИ
                GetOutOfQueue("01.01.2000", 3, "Nick2"), // ВНЕ ОЧЕРЕДИ

                GetDonation("10.01.2000", 4, "Nick1", 900),
                GetDonation("10.01.2000", 5, "Nick1", 800),
                GetDonation("10.01.2000", 6, "Nick2", 700),
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderQueuePosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderQueuePosition());
            Algorithm.RefreshOrderPositions(currentStreamDate, orders, orderPositions);

            Check([
                (1, "Nick1"), // ВНЕ ОЧЕРЕДИ
                (3, "Nick2"), // ВНЕ ОЧЕРЕДИ
                (2, "Nick1"), // ВНЕ ОЧЕРЕДИ
                (6, "Nick2"),
                (4, "Nick1"),
                (5, "Nick1"),
            ], orders, orderPositions);
        }

        [Fact]
        public void OutOfQueue_Donat_Alt1()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetOutOfQueue("01.01.2000", 1, "Nick1"), // ВНЕ ОЧЕРЕДИ
                GetOutOfQueue("01.01.2000", 2, "Nick1"), // ВНЕ ОЧЕРЕДИ
                GetOutOfQueue("01.01.2000", 3, "Nick1"), // ВНЕ ОЧЕРЕДИ

                GetDonation("10.01.2000", 4, "Nick2", 900),
                GetDonation("10.01.2000", 5, "Nick3", 800),
                GetDonation("10.01.2000", 6, "Nick4", 700),
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderQueuePosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderQueuePosition());
            Algorithm.RefreshOrderPositions(currentStreamDate, orders, orderPositions);

            Check([
                (1, "Nick1"), // ВНЕ ОЧЕРЕДИ
                (4, "Nick2"),
                (2, "Nick1"), // ВНЕ ОЧЕРЕДИ
                (5, "Nick3"),
                (3, "Nick1"), // ВНЕ ОЧЕРЕДИ
                (6, "Nick4"),
            ], orders, orderPositions);
        }

        [Fact]
        public void OutOfQueue_Debt_Debt_Debt()
        {
            var currentStreamDate = DateOnly.Parse("20.01.2000");
            ReviewOrder[] items =
            [
                GetOutOfQueue("10.01.2000", 1, "Nick1"), // ВНЕ ОЧЕРЕДИ
                GetOutOfQueue("10.01.2000", 2, "Nick1"), // ВНЕ ОЧЕРЕДИ
                GetOutOfQueue("10.01.2000", 3, "Nick1"), // ВНЕ ОЧЕРЕДИ
                GetOutOfQueue("10.01.2000", 4, "Nick1"), // ВНЕ ОЧЕРЕДИ
                GetOutOfQueue("10.01.2000", 5, "Nick1"), // ВНЕ ОЧЕРЕДИ
                GetOutOfQueue("10.01.2000", 6, "Nick1"), // ВНЕ ОЧЕРЕДИ

                GetDonation("09.01.2000", 7, "Nick2", 900), // долг x1
                GetDonation("09.01.2000", 8, "Nick3", 800), // долг x1
                GetDonation("09.01.2000", 9, "Nick4", 700), // долг x1

                GetDonation("08.01.2000", 10, "Nick5", 900), // долг x2
                GetDonation("08.01.2000", 11, "Nick6", 800), // долг x2
                GetDonation("08.01.2000", 12, "Nick7", 700), // долг x2

                GetDonation("07.01.2000", 13, "Nick8", 900), // долг x3
                GetDonation("07.01.2000", 14, "Nick9", 800), // долг x3
                GetDonation("07.01.2000", 15, "Nick10", 700), // долг x3
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderQueuePosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderQueuePosition());
            Algorithm.RefreshOrderPositions(currentStreamDate, orders, orderPositions);

            Check([
                (1,  "Nick1"),
                (13, "Nick8"), // долг x3
                (2,  "Nick1"),
                (10, "Nick5"), // долг x2
                (3,  "Nick1"),
                (7,  "Nick2"), // долг x1

                (4,  "Nick1"),
                (14, "Nick9"), // долг x3
                (5,  "Nick1"),
                (11, "Nick6"), // долг x2
                (6,  "Nick1"),
                (8,  "Nick3"), // долг x1

                (15, "Nick10"), // долг x3
                (12, "Nick7"), // долг x2
                (9,  "Nick4"), // долг x1

            ], orders, orderPositions);
        }

        [Fact]
        public void OutOfQueue_Debt_Debt_Debt_Alt1()
        {
            var currentStreamDate = DateOnly.Parse("20.01.2000");
            ReviewOrder[] items =
            [
                GetOutOfQueue("10.01.2000", 1, "Nick10"), // ВНЕ ОЧЕРЕДИ
                GetOutOfQueue("10.01.2000", 2, "Nick10"), // ВНЕ ОЧЕРЕДИ
                GetOutOfQueue("10.01.2000", 3, "Nick11"), // ВНЕ ОЧЕРЕДИ
                GetOutOfQueue("10.01.2000", 4, "Nick11"), // ВНЕ ОЧЕРЕДИ
                GetOutOfQueue("10.01.2000", 5, "Nick12"), // ВНЕ ОЧЕРЕДИ
                GetOutOfQueue("10.01.2000", 6, "Nick12"), // ВНЕ ОЧЕРЕДИ

                GetDonation("09.01.2000", 7, "Nick1", 900), // долг x1
                GetDonation("09.01.2000", 8, "Nick2", 800), // долг x1
                GetDonation("09.01.2000", 9, "Nick3", 700), // долг x1

                GetDonation("08.01.2000", 10, "Nick4", 900), // долг x2
                GetDonation("08.01.2000", 11, "Nick5", 800), // долг x2
                GetDonation("08.01.2000", 12, "Nick6", 700), // долг x2

                GetDonation("07.01.2000", 13, "Nick7", 900), // долг x3
                GetDonation("07.01.2000", 14, "Nick8", 800), // долг x3
                GetDonation("07.01.2000", 15, "Nick9", 700), // долг x3
            ];

            Dictionary<long, ReviewOrder> orders = items.ToDictionary(k => k.Id);
            Dictionary<long, OrderQueuePosition> orderPositions = orders.ToDictionary(k => k.Key, _ => new OrderQueuePosition());
            Algorithm.RefreshOrderPositions(currentStreamDate, orders, orderPositions);

            Check([
                (1,  "Nick10"),
                (3,  "Nick11"),
                (2,  "Nick10"),
                (4,  "Nick11"),

                (5,  "Nick12"),
                (13, "Nick7"), // долг x3
                (6,  "Nick12"),
                (10, "Nick4"), // долг x2
                (7,  "Nick1"), // долг x1

                (14, "Nick8"), // долг x3
                (11, "Nick5"), // долг x2
                (8,  "Nick2"), // долг x1

                (15, "Nick9"), // долг x3
                (12, "Nick6"), // долг x2
                (9,  "Nick3"), // долг x1
            ], orders, orderPositions);
        }

        [Fact]
        public void OutOfQueue_FutureDonat_Debt_Debt_Inactive()
        {
            var currentStreamDate = DateOnly.Parse("10.01.2000");
            ReviewOrder[] items =
            [
                GetOutOfQueue("01.01.2000", 1, "Nick1"), // ВНЕ ОЧЕРЕДИ
                GetOutOfQueue("01.01.2000", 2, "Nick1"), // ВНЕ ОЧЕРЕДИ
                GetOutOfQueue("01.01.2000", 3, "Nick2"), // ВНЕ ОЧЕРЕДИ

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

            Check([
                (0, 4, OrderActivityStatus.Future,    "Nick1"),
                (1, 5, OrderActivityStatus.Future,    "Nick1"),
                (2, 6, OrderActivityStatus.Future,    "Nick2"),

                (0, 1, OrderActivityStatus.Active,    "Nick1"),
                (1, 3, OrderActivityStatus.Active,    "Nick2"),
                (2, 2, OrderActivityStatus.Active,    "Nick1"),

                (3, 11, OrderActivityStatus.Active,   "Nick2"), // долг x2
                (4, 9, OrderActivityStatus.Active,    "Nick6"), // долг x1

                (5, 10, OrderActivityStatus.Active,   "Nick1"), // долг x2
                (6, 7, OrderActivityStatus.Active,    "Nick2"), // долг x1

                (7, 12, OrderActivityStatus.Active,   "Nick9"), // долг x2
                (8, 8, OrderActivityStatus.Active,    "Nick2"), // долг x1

                (0, 13, OrderActivityStatus.Inactive, "Nick2"),
                (1, 14, OrderActivityStatus.Inactive, "Nick2"),
                (2, 15, OrderActivityStatus.Inactive, "Nick9"),
            ], orders, orderPositions);
        }

        private void Check((long id, string nick)[] values, Dictionary<long, ReviewOrder> orders, Dictionary<long, OrderQueuePosition> orderPositions)
        {
            int index = 0;
            foreach ((long id, string nick) in values)
            {
                Assert.Equal((index, id, nick), (orderPositions[id].CurrentIndex, id, orders[id].UserNickname.Nickname));
                index++;
            }
        }

        private void Check((int index, long id, OrderActivityStatus status, string nick)[] values, Dictionary<long, ReviewOrder> orders, Dictionary<long, OrderQueuePosition> orderPositions)
        {
            foreach ((int index, long id, OrderActivityStatus status, string nick) in values)
            {
                Assert.Equal((index, id, status, nick), (orderPositions[id].CurrentIndex, id, orderPositions[id].CurrentActivityStatus, orders[id].UserNickname.Nickname));
            }
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