using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Betlln.Data.Integration.Collections
{
    public class ListOf<T> : List<T>
    {
        public ListOf()
        {
        }

        private ListOf(IEnumerable<T> enumerable)
            : base(enumerable)
        {
        }

        public bool HasAnyItems()
        {
            return this.Any();
        }

        public T FirstElement()
        {
            return this.First();
        }

        public static implicit operator ListOf<T>(EnumerableRowCollection<T> enumerable)
        {
            return new ListOf<T>(enumerable);
        }
    }
}