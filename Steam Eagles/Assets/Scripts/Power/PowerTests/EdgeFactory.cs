using System;
using QuikGraph;

public class EdgeFactory<T>
{
    public TaggedUndirectedEdge<GridNode, T> Create(GridNode source, GridNode target)
    {
        try
        {
            return new TaggedUndirectedEdge<GridNode, T>(source, target, default);
        }
        catch (Exception e)
        {
            return new TaggedUndirectedEdge<GridNode, T>(target, source, default);
        }
    }
}