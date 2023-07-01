using UnityEngine;


#if !ODIN_INSPECTOR
    public class DummyStringAttribute : PropertyAttribute
    {
        public DummyStringAttribute(string v)
        {
            
        }
    }
    public class ValueDropdownAttribute : PropertyAttribute
    {
        public ValueDropdownAttribute(string s)
        {
            
        }
    }
    public class RequireAttribute : PropertyAttribute
    {
         
    }
    public class DisableIfAttribute : DummyStringAttribute
    {
        public DisableIfAttribute(string v) : base(v)
        {
        }
    }
    public class EnableIfAttribute : DummyStringAttribute
    {
        public EnableIfAttribute(string v) : base(v)
        {
        }
    }
    public class HideIfAttribute : DummyStringAttribute
    {
        public HideIfAttribute(string v) : base(v)
        {
        }
    }
    public class ShowIfAttribute : DummyStringAttribute
    {
        public ShowIfAttribute(string v) : base(v)
        {
        }
    }
    public class ButtonAttribute : PropertyAttribute
    {
        public ButtonAttribute(string label = "")
        {
            
        }
    }

    public class DisableInPlayModeAttribute : PropertyAttribute
    {
        
    }

    public class HideInPlayModeAttribute : PropertyAttribute
    {
            
    }
#endif


   
