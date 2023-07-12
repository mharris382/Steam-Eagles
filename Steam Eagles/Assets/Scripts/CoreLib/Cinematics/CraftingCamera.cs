using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CoreLib.Cinematics
{
    public class CraftingCamera : MonoBehaviour
    {
        [Required]
        public Transform followTarget;
        public float minBlendDistance = 1f;
        public float maxBlendDistance = 5f;
        public AnimationCurve blendCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public bool separateAxes = true;
        
        [EnumPaging]
        public CameraFollowMode followMode = CameraFollowMode.MIXED;
        
        public enum CameraFollowMode
        {
            CHARACTER,
            AIM,
            MIXED
        }
        
        public Transform CharacterFollowPosition
        {
            get;
            set;
        }
        public Transform AimFollowPosition
        {
            get;
            set;
        }


        bool HasResources()
        {
            if (!followTarget)
            {
                Debug.LogError("Follow target is not set!",this);
                return false;
            }
            switch (followMode)
            {
                case CameraFollowMode.CHARACTER:
                    return CharacterFollowPosition != null;
                    break;
                case CameraFollowMode.AIM:
                    return AimFollowPosition != null;
                    break;
                case CameraFollowMode.MIXED:
                    return CharacterFollowPosition != null && AimFollowPosition != null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Update()
        {
            if (!HasResources()) return;
            switch (followMode)
            {
                case CameraFollowMode.CHARACTER:
                    followTarget.position = CharacterFollowPosition.position;
                    break;
                case CameraFollowMode.AIM:
                    followTarget.position = AimFollowPosition.position;
                    break;
                case CameraFollowMode.MIXED:
                    followTarget.position =
                        GetMixedPosition(CharacterFollowPosition.position, AimFollowPosition.position);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Vector3 GetMixedPosition(Vector3 characterPos, Vector3 aimPos)
        {
            if (separateAxes)
            {
                float x = GetMixedPosition(characterPos.x, aimPos.x);
                float y = GetMixedPosition(characterPos.y, aimPos.y);
                return new Vector3(x, y, characterPos.z);
            }
            else
            {
                float distance = Vector2.Distance(characterPos, aimPos);
                if (distance < minBlendDistance)
                    return characterPos;
                distance = Mathf.Min(maxBlendDistance, distance);
                float t = Mathf.InverseLerp(minBlendDistance, maxBlendDistance, distance);
                float tWeighted = blendCurve.Evaluate(t);
                return Vector2.Lerp(characterPos, aimPos, tWeighted);
            }
        }

        private float GetMixedPosition(float characterPos, float aimPosition)
        {
            float distance = Mathf.Abs(characterPos - aimPosition);
            if(distance < minBlendDistance)
                return characterPos;
            distance = Mathf.Min(maxBlendDistance, distance);
            float t = Mathf.InverseLerp(minBlendDistance, maxBlendDistance, distance);
            float tWeighted = blendCurve.Evaluate(t);
            return Mathf.Lerp(characterPos, aimPosition, tWeighted);
        }

        private void OnDrawGizmos()
        {
            Vector3 position = transform.position;
            
            if (CharacterFollowPosition != null)
            {
                position = CharacterFollowPosition.position;   
            }

            var rMin = minBlendDistance;
            var rMax = maxBlendDistance;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(position, rMin);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, rMax);
        }
    }
}