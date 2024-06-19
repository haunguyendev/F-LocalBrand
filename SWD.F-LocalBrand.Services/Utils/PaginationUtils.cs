using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.Utils
{
    public static class PaginationUtils
    {
        public static IEnumerable<T> Paginate<T>(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (pageNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than or equal to 1.");
            }

            return source.Skip((pageNumber - 1) * pageSize)
                         .Take(pageSize)
                         .ToList();
        }
    }
}
