using CoreLib;

public struct SlowTickAddRequest
{
    public ISlowTickable SlowTickable { get; }

    public SlowTickAddRequest(ISlowTickable slowTickable)
    {
        SlowTickable = slowTickable;
    }
    public override string ToString() => $"Add request for {SlowTickable}";
}

public struct ExtraSlowTickAddRequest
{
    public IExtraSlowTickable ExtraSlowTickable { get; }


    public ExtraSlowTickAddRequest(IExtraSlowTickable extraSlowTickable)
    {
        ExtraSlowTickable = extraSlowTickable;
    }
    public override string ToString()
    {
        return $"Add request for {ExtraSlowTickable}";
    }
}
public struct SlowTickRemovalRequest
{
    public ISlowTickable SlowTickable { get; }

    public SlowTickRemovalRequest(ISlowTickable slowTickable)
    {
        SlowTickable = slowTickable;
    }

    public override string ToString() => $"Removal request for {SlowTickable}";
}
public struct ExtraSlowTickRemovalRequest
{
    public IExtraSlowTickable ExtraSlowTickable { get; }


    public ExtraSlowTickRemovalRequest(IExtraSlowTickable extraSlowTickable)
    {
        ExtraSlowTickable = extraSlowTickable;
    }

    public override string ToString()
    {
        return $"Removal request for {ExtraSlowTickable}";
    }
}