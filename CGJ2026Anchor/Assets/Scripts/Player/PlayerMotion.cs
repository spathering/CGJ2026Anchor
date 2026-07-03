using System;
using Sirenix.OdinInspector;
using Systems;
using Toolkits;
using UnityEngine;

namespace Player
{
    public sealed class PlayerMotion : MonoBehaviour
    {
        [FoldoutGroup("Comp")] public Rigidbody rigid;
        
        [FoldoutGroup("Setting")] public float moveSpeed = 9;
        [FoldoutGroup("Setting")] public float jumpStrength = 15;
        
        [FoldoutGroup("Runtime")] public Vector2 input;

        [FoldoutGroup("Runtime")] [ShowInInspector, ReadOnly] private Vector3 _collisionVel;
        [FoldoutGroup("Runtime")] public Vector3 output;
        
        private Global _global;
        
        #region Mono

        private void Awake()
        {
            _global = ServiceLocator.GetService<Global>();
        }

        #endregion
        
        private void OnCollisionStay(Collision other)
        {
            var related = other.relativeVelocity;
            _collisionVel += related;
        }
        private void FixedUpdate()
        {
            output = Vector3.zero;
            output += DealingInput(output);
            
            rigid.velocity = output;
        }
        

        #region Motion Dealing
        private Vector3 DealingInput(Vector3 vel)
        {
            var inputVel = (input.normalized * moveSpeed).ConvertXZ();
            return vel + inputVel;
        }

        private Vector3 DealingRelated(Vector3 vel)
        {
            _collisionVel = Vector3.zero;
            return vel + _collisionVel;
        }
        #endregion
        
    }
}
