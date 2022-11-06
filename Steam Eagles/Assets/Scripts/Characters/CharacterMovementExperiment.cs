using System;
using UniRx;
using UnityEngine;

namespace Characters
{
    public class CharacterMovementExperiment : MonoBehaviour
    {
        private CharacterState state;
        private Collider2D collider2D;

        private void Awake()
        {
            state = GetComponent<CharacterState>();
            state.OnCharacterJumped.TakeUntilDestroy(this).Subscribe(_ => CharacterJumped());
            state.OnCharacterLanded.TakeUntilDestroy(this).Subscribe(_ => CharacterLanded());
            state.IsGroundedEventStream.TakeUntilDestroy(this).Subscribe(CharacterGrounded);
        }


        void CharacterGrounded(bool grounded)
        {
            if (grounded)
            {
                Debug.Log($"{name} became grounded!", this);
            }
            else
            {
                Debug.Log($"{name} became not grounded!", this);
            }
        }

        void CharacterJumped()
        {
            Debug.Log($"{name} Jumped!", this);
        }

        void CharacterLanded()
        {
            Debug.Log($"{name} Landed!", this);
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            var pnt = col.contacts[0].point;
            var normal = col.contacts[0].normal;
            Debug.DrawRay(pnt, normal * 3, Color.magenta, 1);
            Debug.Log($"{name} Collision Occured between {col.collider.name} and {col.otherCollider.name}!");
        }

        private void OnCollisionExit2D(Collision2D col)
        {
            Debug.Log($"Contact Count = {col.contactCount}");
            Debug.Log($"{name} Collision Occured between {col.collider.name} and {col.otherCollider.name}!");
        }
    }
}