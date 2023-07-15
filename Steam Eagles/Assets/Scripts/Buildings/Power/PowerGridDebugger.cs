using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Buildings
{
    public class PowerGridDebugger : MonoBehaviour
    {

        public SpriteRenderer productionSpritePrefab;
        public SpriteRenderer consumptionSpritePrefab;
        private BuildingPowerGrid _powerGrid;
        private Building _building;


        List<SpriteRenderer> producers = new();
        List<SpriteRenderer> consumers = new();


        void Clear()
        {
            producers.ForEach(t => Object.Destroy(t.gameObject));
            consumers.ForEach(t => Object.Destroy(t.gameObject));
            producers.Clear();
            consumers.Clear();
        }

        void CreateProducerAt(Vector3 wsPos)
        {
            var sr = Object.Instantiate(productionSpritePrefab);
            sr.transform.position = wsPos;
            producers.Add(sr);
        }
        
        void CreateConsumerAt(Vector3 wsPos)
        {
            var sr = Object.Instantiate(consumptionSpritePrefab);
            sr.transform.position = wsPos;
            producers.Add(sr);
        }
        [Inject] void Install(BuildingPowerGrid powerGrid, Building building)
        {
            _powerGrid = powerGrid;
            _building = building;
        }


        
        [Button, DisableInEditorMode, ButtonGroup()]
        void DebugAll()
        {
            Clear();
            foreach (var valueTuple in _powerGrid.GetSuppliers())
            {
                var wsPos = _building.Map.CellToWorldCentered(valueTuple.cell);
                CreateProducerAt(wsPos);
            }   
            foreach (var valueTuple in _powerGrid.GetConsumers())
            {
                var wsPos = _building.Map.CellToWorldCentered(valueTuple.cell);
                CreateConsumerAt(wsPos);
            }
        }

        [Button, DisableInEditorMode, ButtonGroup()]
       void DebugProducers()
       {
           Clear();
           foreach (var valueTuple in _powerGrid.GetSuppliers())
           {
               var wsPos = _building.Map.CellToWorldCentered(valueTuple.cell);
                CreateProducerAt(wsPos);
           }   
       }

       [Button, DisableInEditorMode, ButtonGroup()]
       void DebugConsumers()
       {
           Clear();
              foreach (var valueTuple in _powerGrid.GetConsumers())
              {
                var wsPos = _building.Map.CellToWorldCentered(valueTuple.cell);
                CreateConsumerAt(wsPos);
              }
       }
    }
}