using System.Collections.Generic;
using Characters;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Zenject;

namespace Interactions
{
    public class ElevatorPassengers : IInitializable
    {
        public List<Rigidbody2D> Passengers { get; } = new List<Rigidbody2D>();

        public ElevatorPassengers(ElevatorController controller)
        {
            var trigger = controller.GetComponent<Collider2D>();
            
            trigger.OnTriggerEnter2DAsObservable().Select(t => t.GetComponent<StructureState>()).Where(t => t != null).Subscribe(t => Passengers.Add(t.GetComponent<Rigidbody2D>())).AddTo(controller);
            trigger.OnTriggerExit2DAsObservable().Select(t => t.GetComponent<StructureState>()).Where(t => t != null).Subscribe(t => Passengers.Remove(t.GetComponent<Rigidbody2D>())).AddTo(controller);
        }

        public void Initialize()
        {
            
        }
    }
}