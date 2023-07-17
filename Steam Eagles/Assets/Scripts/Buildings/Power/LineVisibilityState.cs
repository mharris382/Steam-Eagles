using System;
using UniRx;

namespace Buildings
{
    
    public class LineVisibilityState
    {
        public BoolReactiveProperty lineVisibility = new BoolReactiveProperty(true);
    }
}