using Buildings;
using Buildings.Tiles;
using Items;
using UI.Crafting.Sampling;
using UnityEngine;
using Zenject;

public class TileSampler : ISampler
{
    [Inject] LadderTileSampler _LadderTileSampler;
    [Inject] WireTileSampler _WireTileSampler;
    [Inject] WallTileSampler _WallTileSampler;
    [Inject] PipeTileSampler _PipeTileSampler;
    [Inject] SolidTileSampler _SolidTileSampler;

    public bool TryGetRecipe(Building building, Vector3 aimPosition, out Recipe recipe)
    {
        if (_LadderTileSampler.TryGetRecipe(building, aimPosition, out recipe))
        {
            return true;
        }
        if (_WireTileSampler.TryGetRecipe(building, aimPosition, out recipe))
        {
            return true;
        }
        else if (_WallTileSampler.TryGetRecipe(building, aimPosition, out recipe))
        {
            return true;
        }
        else if (_PipeTileSampler.TryGetRecipe(building, aimPosition, out recipe))
        {
            return true;
        }
        else if (_SolidTileSampler.TryGetRecipe(building, aimPosition, out recipe))
        {
            return true;
        }
        return false;
    }

    #region [Nested Classes]

    public class LadderTileSampler : LayerTileSampler
    {
        public override BuildingLayers TargetLayer => BuildingLayers.LADDERS;
    }

    public class WireTileSampler : LayerTileSampler
    {
        public override BuildingLayers TargetLayer => BuildingLayers.WIRES;
    }

    public class WallTileSampler : LayerTileSampler
    {
        public override BuildingLayers TargetLayer => BuildingLayers.WALL;
    }

    public class PipeTileSampler : LayerTileSampler
    {
        public override BuildingLayers TargetLayer => BuildingLayers.PIPE;
    }

    public class SolidTileSampler : LayerTileSampler
    {
        public override BuildingLayers TargetLayer => BuildingLayers.SOLID;
    }

    #endregion
    #region [Nested Abstract Classes]

    public abstract class LayerTileSampler : ISampler
    {
        public abstract BuildingLayers TargetLayer { get; }
        [Inject] public Recipes recipes;

        public bool TryGetRecipe(Building building, Vector3 aimPosition, out Recipe recipe)
        {
            var cell = building.Map.WorldToBCell(aimPosition, TargetLayer);
            var tile = building.Map.GetTile<EditableTile>(cell);
            if (tile == null)
            {
                recipe = null;
                return false;
            }

            var recipeName = tile.recipeName;
            if (string.IsNullOrEmpty(recipeName))
            {
                Debug.LogError($"recipeName is null or empty: {tile.name}", tile);
                recipe = null;
                return false;
            }

            return (recipe = recipes.FindRecipeWithName(recipeName)) != null;
        }
    }
    

    #endregion

}