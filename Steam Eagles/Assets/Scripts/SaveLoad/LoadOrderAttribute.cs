using System;

namespace SaveLoad
{
    public class LoadOrderAttribute : Attribute, IComparable<LoadOrderAttribute>
    {
        public int Order { get; }

        public LoadOrderAttribute(int order)
        {
            Order = order;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int CompareTo(LoadOrderAttribute other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Order.CompareTo(other.Order);
        }
    }
}