using System;
using Sirenix.OdinInspector;
using Systems;
using Toolkits;
using UnityEngine;

namespace Player
{
    public sealed class PlayerMotion : MonoBehaviour
    {
        [FoldoutGroup("Comp")] public Transform cam;
        [FoldoutGroup("Comp")] public Rigidbody rigid;

        [FoldoutGroup("Setting")] public float camXRate = 5;
        [FoldoutGroup("Setting")] public float camYRate = 5;
        [FoldoutGroup("Setting")] public Vector2 camYRange = new(-80, 80);
        [PropertySpace]
        [FoldoutGroup("Setting")] public float moveSpeed = 9;
        [FoldoutGroup("Setting")] public float declineOnGnd = 120;
        [FoldoutGroup("Setting")] public float declineOpposite = 150;
        [FoldoutGroup("Setting")] public float declineOnAir = 5;
        [PropertySpace]
        [FoldoutGroup("Setting")] public float jumpStrength = 15;
        [FoldoutGroup("Setting")] public ConsumeCounterFloat jumpCounter = new();
        [PropertySpace]
        [FoldoutGroup("Setting")] public float gravity = 9.81f;
        [FoldoutGroup("Setting")] public ConsumeCounterFloat gravityCounter = new();
        [FoldoutGroup("Setting")] public float onGroundDistance = .5f;
        [FoldoutGroup("Setting")] public float prejumpDistance = .1f;
        [FoldoutGroup("Setting")] public float flyTimeMax = 5;
        
        [FoldoutGroup("Runtime")] public Vector2 input;
        [FoldoutGroup("Runtime")] public Vector2 camInput;
        
        [FoldoutGroup("Runtime")] [ShowInInspector, ReadOnly] private Vector3 _declineVel;
        [FoldoutGroup("Runtime")] [ShowInInspector, ReadOnly] private Vector3 _collisionVel;
        [FoldoutGroup("Runtime")] [ShowInInspector, ReadOnly] private float _flyTime;
        [FoldoutGroup("Runtime")] [ShowInInspector, ReadOnly] private bool _onGround;
        
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
            var related = other.impulse;
            if (related.sqrMagnitude < 1) return;
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

        private void LateUpdate()
        {
            DealingCam();
        }


        #region Motion Dealing
        private void DealingCam()
        {
            if (_global.TimeScale.Value == 0) return;
            var rotX = camInput.x * camXRate * Time.deltaTime;
            var rotY = -camInput.y * camYRate * Time.deltaTime;

            var playerEuler = new Vector3(0, transform.eulerAngles.y + rotX, 0);
            rigid.MoveRotation(Quaternion.Euler(playerEuler));  

            var curCamEulerX = cam.localEulerAngles.x < 180 ? 
                cam.localEulerAngles.x : cam.localEulerAngles.x - 360;
            var afterRotX = curCamEulerX + rotY;
            var endX = afterRotX >= 0 ? 
                Mathf.Min(afterRotX, camYRange.y) : Mathf.Max(afterRotX, camYRange.x);
            cam.localEulerAngles = cam.localEulerAngles.SetX(endX); 
        }
        private Vector3 DealingInput(Vector3 vel)
        {
            var delta = _global.TimeDelta;
            //根据方向变换运动向量
            var inputVel = (input.normalized * moveSpeed).ConvertXZ();
            inputVel = inputVel.Rotate(Vector3.up, transform.eulerAngles.y);
            
            //应对冲量进行分解 对应方向上处理为衰减效果
            var decHorizon = _declineVel.ConvertXZ();
            var projDot = Vector2.Dot(inputVel.ConvertXZ(), decHorizon);
            var projOnDec = Vector3.Project(inputVel, decHorizon);
            var otherDec = inputVel - projOnDec;
            Debug.Log($"{_declineVel.sqrMagnitude} & {projDot}");
            if (_declineVel.sqrMagnitude > 1 && projDot < 0)
            {
                var decValue = _declineVel.magnitude - delta * declineOpposite;
                _declineVel = Vector3.ClampMagnitude(_declineVel, decValue);    
                inputVel = otherDec;
                Debug.Log($"Dealing Input to Decline");
            }
            
            //斜坡适应
            var checkPos = transform.position + inputVel * delta;
            if (_onGround 
                && Physics.Raycast(transform.position, Vector3.down, out var pInfo) 
                && Physics.Raycast(checkPos, Vector3.down, out var info))
            {
                var curPos = pInfo.point;
                var nextPos = info.point;
                var dir = nextPos - curPos;
                inputVel = dir.normalized * moveSpeed;
            }
            
            return vel + inputVel;
        }
        
        private Vector3 DealingGravity(Vector3 vel)
        {
            //滞空时间累计
            var delta = _global.TimeDelta;
            gravityCounter.Use(delta);
            jumpCounter.Use(delta);
            _flyTime += delta;
            
            //判断是否着地
            _onGround = false;
            var hit = Physics.Raycast(transform.position, Vector3.down, out var info);
            if (hit && gravityCounter.CanUse())
            {
                var checkDis = onGroundDistance + _global.TimeDelta * _flyTime * gravity;
                if (info.distance <= checkDis)
                {
                    _flyTime = 0;
                    _declineVel = _declineVel.SetY(_declineVel.y * .1f);
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
            var opposite = Vector2.Dot(input, _declineVel.ConvertXZ()) < 0;
            var declineRate = _onGround || opposite ? declineOnGnd : declineOnAir;
            var delta = _global.TimeDelta;
            
            var decXZ = _declineVel.ConvertXZ();
            var decY = _declineVel.y;
            
            var maxSpeedXZ = Mathf.Max(decXZ.magnitude - declineRate * delta, 0);
            var endXZ = Vector2.ClampMagnitude(decXZ, maxSpeedXZ);

            var maxSpeedY = Mathf.Max(decY - declineOnAir * delta, 0);
            var endY = Mathf.Clamp(maxSpeedY, -maxSpeedY, maxSpeedY);

            _declineVel = endXZ.ConvertXZ().SetY(endY); 
            return vel;
        }
        #endregion

        #region Jump
        public bool DoJump()
        {
            if (!jumpCounter.CanUse()) return false;
            if (Physics.Raycast(transform.position, Vector3.down, out var info)
                && info.distance < onGroundDistance + prejumpDistance
                )
            {
                jumpCounter.Refresh();
                gravityCounter.Refresh();
                _declineVel += jumpStrength * Vector3.up;

                var checkDis = onGroundDistance + _global.TimeDelta * (_flyTime + .1f) * gravity;
                if (info.distance <= checkDis)
                    transform.position = info.point + (checkDis + float.Epsilon) * Vector3.up;
                return true;
            }   
            else
            {
                return true;
            }
        }

        #endregion
        
    }
}
