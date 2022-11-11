using System;
using UniRx;
using UnityEngine;
#if UNITY_EDITOR
using System.Text;
using UnityEditor;
#endif
namespace Characters
{
    public class PickupFinder : MonoBehaviour
    {
        [SerializeField] internal TriggerArea inRangePickups;

        public ReadOnlyReactiveProperty<bool> hasFoundTarget;

        private ReactiveProperty<Rigidbody2D> _selectedPickup;

        private void Awake()
        {
            inRangePickups.onTargetAdded.AsObservable().TakeUntilDisable(this).Subscribe(OnTargetAdded);
            inRangePickups.onTargetRemoved.AsObservable().TakeUntilDestroy(this).Subscribe(OnTargetRemoved);
            var targetCountStream =inRangePickups.onTargetCountChanged.AsObservable().TakeUntilDestroy(this);
            targetCountStream.Subscribe(OnTargetCountChanged);
            hasFoundTarget = new ReadOnlyReactiveProperty<bool>(targetCountStream.Select(t => t > 0));
        }
        
        
        
        void OnTargetAdded(Rigidbody2D rigidbody2D)
        {
            
        }
        
        void OnTargetRemoved(Rigidbody2D rigidbody2D)
        {
            
        }

        void OnTargetCountChanged(int count)
        {
            
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(PickupFinder))]
    public class TriggerAreaTesterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            //if application is playing, display the list of pickups in range
            StringBuilder sb = new StringBuilder();
            
            if (Application.isPlaying)
            {
                PickupFinder tester = (PickupFinder)target;
                var pickup = tester.inRangePickups;
                //display the number of objects in range
                sb.AppendLine("Number of objects in range: " + pickup.GetTargetCount());
                //display the nearest object
                sb.AppendLine("Nearest object: " + pickup.GetNearestTarget());
                //display the list of objects
                for (int i = 0; i < pickup.GetTargetCount(); i++)
                {
                    var target = pickup.GetTarget(i);
                    sb.AppendLine($"\t{target.name} : {target.transform.position.x:F2}, {target.transform.position.y:F2}");
                }
                //Draw info box to contain info
                EditorGUILayout.HelpBox(sb.ToString(), MessageType.Info);
            }
            base.OnInspectorGUI();
        }
    }
#endif
}