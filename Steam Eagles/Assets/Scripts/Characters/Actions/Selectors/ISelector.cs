using System.Collections;
using System.Collections.Generic;
using Buildings.Tiles;
using CoreLib;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Characters.Actions.Selectors
{
    
    
    
    /// <summary>
    /// interface used by abilities to dynamically find target resources
    /// <para>I am basing this off my CellSelector which abstracts the process of finding available cells,
    /// I would like to be able to chain dynamic selectable resources together allowing for way more gameplay dyanmics
    /// and much less reliance on hard coded values or inspector assigned references</para>
    /// <para>One of the main goals is to allow multiple tilemaps in the scene, and make it much easier to add
    /// new kinds of tiles, new base building mechanics, ect. none of which should rely on hard coded values, instead treats all
    /// values as dymamic.  All dependencies (excluding the ones that are internal to the ability system) should be fulfilled by a selector
    /// instead of a direct reference.   </para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISelector<T> : IEnumerable<T>
    {
        
        bool IsSelectorValid { get; }
        
        int SelectionOptionCount { get; }
        
    }


    public class Cell
    {
        public TilemapTypes TilemapType;
        public Grid Grid { get; }
        public Tilemap Tilemap { get; }
        public Vector3Int Position { get; }
        public Vector3 WorldPosition { get; }
        
        public Cell(Tilemap tilemap, Vector3Int position)
        {
            Grid = tilemap.layoutGrid;
            Tilemap = tilemap;
            Position = position;
            WorldPosition = Grid.CellToWorld(Position);
        }
    }




    public interface IPositionSelector : ISelector<Vector3> { }
    public interface ICellSelector2 : ISelector<Vector3Int> {}
    public interface IPipeSelector : ITileSelector<PipeTile> { }
    public interface ISolidSelector : ITileSelector<SolidTile> { }
    public interface ITileSelector<T> : ISelector<T> where T : PuzzleTile { }
    public interface ITilemapSelector : ISelector<Tilemap> { }
    public interface IGridSelector : ISelector<Grid> { }
    
    

    public abstract class AbilityResourceSelector<T> : ISelector<T>
    {
        private readonly AbilityUser _user;

        protected AbilityUser User { get; }

        public AbilityResourceSelector(AbilityUser user)
        {
            _user = user;
        }
        
        protected abstract bool IsResourceSelectorValid();


        public bool IsSelectorValid => _user != null && IsResourceSelectorValid();
        public abstract int SelectionOptionCount { get; }


        
        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    public abstract class AbilityResourceSelector<T, TResource> : AbilityResourceSelector<T>
    {
        private readonly ISelector<TResource> _resourceSelector;


        protected AbilityResourceSelector(AbilityUser user, ISelector<TResource> resourceSelector) : base(user)
        {
            _resourceSelector = resourceSelector;
        }

        protected override bool IsResourceSelectorValid()
        {
            return this._resourceSelector.IsSelectorValid && 
                   this._resourceSelector.SelectionOptionCount > 0;
        }
    }
    
    
}