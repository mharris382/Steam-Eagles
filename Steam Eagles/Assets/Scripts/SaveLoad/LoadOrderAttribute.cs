using System;

namespace SaveLoad
{
    public class LoadOrderAttribute : Attribute
    {
        public int Order { get; }

        public LoadOrderAttribute(int order)
        {
            Order = order;
        }
    }
}