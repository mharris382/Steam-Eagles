using UnityEngine;
using Zenject;

[RequireComponent(typeof(Grid))]
public class SimGrid : MonoBehaviour
{
    private Grid _grid;

    public Grid Grid => _grid ??= GetComponent<Grid>();


    [Inject] void Install(SimConfig config)
    {
        Grid.cellSize = config.GetGridCellSize();
    }

}