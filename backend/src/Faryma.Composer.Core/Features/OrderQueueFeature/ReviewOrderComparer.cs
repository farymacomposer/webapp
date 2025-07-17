# nullable disable
using Faryma.Composer.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Faryma.Composer.Core.Features.OrderQueueFeature
{
    public sealed class ReviewOrderComparer : IComparer<ReviewOrder>
    {
        public int Compare(ReviewOrder x, ReviewOrder y)
        {
            decimal xAmount = x.NominalAmount + x.Payments.Sum(x => x.Amount);
            decimal yAmount = y.NominalAmount + y.Payments.Sum(y => y.Amount);

            int result = decimal.Compare(xAmount, yAmount) * (-1);
            if (result != 0)
            {
                return result;
            }

            return DateTime.Compare(x.CreatedAt, y.CreatedAt);
        }
    }
}