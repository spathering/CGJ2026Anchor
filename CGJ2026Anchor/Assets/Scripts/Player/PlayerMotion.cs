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
        [FoldoutGroup("Setting")] public float decline = 5;
        [FoldoutGroup("Setting")] public float jumpStrength = 15;
        [PropertySpace]
        [FoldoutGroup("Setting")] public float gravity = 9.81f;
        [FoldoutGroup("Setting")] public float onGroundDistance = .1f;
        [FoldoutGroup("Setting")] public float flyTimeMax = 5;
        
        [FoldoutGroup("Runtime")] public Vector2 input;

        [FoldoutGroup("Runtime")] [ShowInInspector, ReadOnly] private Vector3 _collisionVel;
        [FoldoutGroup("Runtime")] [ShowInInspector, ReadOnly] private float _flyTime;
        [FoldoutGroup("Runtime")] [ShowInInspector, ReadOnly] private bool _onGround;
        [FoldoutGroup("Runtime")] public Vector3 output;

        
        private Vector3 _declineVel;
        private Global _global;
        
        #region Mono

        private void Awake()
        {
            _global = ServiceLocator.GetService<Global>();
        }

        #endregion
        
        private void OnCollisionStay(Collision other)
        {
            var related = other.impulse;
            _collisionVel += related;
        }
        private void FixedUpdate()
        {
            output = Vector3.zero;
            output += DealingInput(output);
            output += DealingGravity(output);
            output += DealingRelated(output);
            output += DealingDecline(output);
            rigid.velocity = output;
        }
        

        #region Motion Dealing
        private Vector3 DealingInput(Vector3 vel)
        {
            var inputVel = (input.normalized * moveSpeed).ConvertXZ();
            return vel + inputVel;
        }
        
        private Vector3 DealingGravity(Vector3 vel)
        {
            //滞空时间累计
            _flyTime += _global.TimeDelta;
            
            //判断是否着地
            _onGround = false;
            var hit = Physics.Raycast(transform.position, Vector3.down, out var info);
            if (hit)
            {
                var checkDis = onGroundDistance + _global.TimeDelta * _flyTime * gravity;
                if (info.distance <= checkDis)
                {
                    _flyTime = 0;
                    transform.position = info.point + Vector3.up * onGroundDistance;
                    _onGround = true;
                }
            }

            var validFlyTime = Mathf.Clamp(_flyTime, 0, flyTimeMax);
            vel += validFlyTime * gravity * Vector3.down;
            
            return vel;
        }
        
        
        private Vector3 DealingRelated(Vector3 vel)
        {
            _declineVel += _collisionVel;
            _collisionVel = Vector3.zero;
            return vel;
        }
        private Vector3 DealingDecline(Vector3 vel)
        {
            vel += _declineVel;
            var maxSpeed = Mathf.Max(_declineVel.magnitude - decline * _global.TimeDelta, 0);
            _declineVel = Vector3.ClampMagnitude(_declineVel, maxSpeed);
            return vel;
        }
        #endregion

        #region Jump
        public void DoJump()
        {
            if (_onGround)
            {
                // _declineVel += 
            }   
            else
            {
                
            }
        }

        #endregion
        
    }
}
