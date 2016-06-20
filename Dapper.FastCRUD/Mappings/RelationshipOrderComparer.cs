namespace Dapper.FastCrud.SqlBuilders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Comparer for the optional order used in relationships. (e.g. 1 2 NULL NULL)
    /// </summary>
    internal class RelationshipOrderComparer:IComparer<int?>
    {
        /// <summary>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</summary>
        /// 
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.
        /// Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.
        /// Zero<paramref name="x" /> equals <paramref name="y" />.
        /// Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.
        /// </returns>
        /// 
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        public int Compare(int? x, int? y)
        {
            if (x.HasValue && y.HasValue)
            {
                return x.Value.CompareTo(y.Value);
            }

            if (x.HasValue)
            {
                return -1;
            }

            if (y.HasValue)
            {
                return 1;
            }

            return 0;
        }
    }
}
