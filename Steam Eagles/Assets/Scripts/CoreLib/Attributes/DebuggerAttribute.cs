using System;

[AttributeUsage(AttributeTargets.Class)]
public class DebuggerAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Class)]
public class PreviewAttribute : Attribute
{
    public bool isPreview;

    public PreviewAttribute()
    {
        isPreview = true;
    }
}