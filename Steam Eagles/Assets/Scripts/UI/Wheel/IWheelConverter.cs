namespace UI.Wheel
{
    public interface IWheelConverter<in T>
    {
        UIWheelSelectable GetSelectableFor(T item);
    }
    
    
    public class StringConverter : IWheelConverter<string>
    {
        public UIWheelSelectable GetSelectableFor(string item)
        {
            var selectable = new UIWheelSelectable
            {
                name = item
            };
            return selectable;
        }
    }
}