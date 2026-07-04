using System;
using Rewired;
using Toolkits;
using UnityEngine;

namespace Player
{
    public sealed class PlayerInput : MonoBehaviour
    {
        [SerializeField] private int playerId = 0;
        private Rewired.Player _player;
        
        #region Mono
        private void OnEnable()
        {
            if (!ReInput.isReady) return;
            _player = ReInput.players.GetPlayer(playerId);
            
            _onJump = DoJump;
            _player.AddInputEventDelegate(_onJump, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, RewiredConsts.Action.jump);
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDisable()
        {
            if (!ReInput.isReady || _player == null) return;

            _player.RemoveInputEventDelegate(_onJump, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, RewiredConsts.Action.jump);
            
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        
        #endregion

        #region Updater

        private void Update()
        {
            OnCam();
            OnMotion();
            DealingJumpInst(Time.deltaTime);
        }

        #endregion
        
        #region Motion
        [SerializeField] private PlayerMotion motion;
        private Action<InputActionEventData> _onJump;

        [SerializeField] private ConsumeCounterFloat jumpInst = new();
        
        private void OnMotion()
        {
            var input = _player.GetAxis2D(RewiredConsts.Action.move_x, RewiredConsts.Action.move_y);
            motion.input = input;   
        }

        private void OnCam()
        {
            var input = _player.GetAxis2D(RewiredConsts.Action.cam_x, RewiredConsts.Action.cam_y);
            motion.camInput = input;
        }

        private void DealingJumpInst(float deltaTime)
        {
            jumpInst.Use(deltaTime);
            if (jumpInst.CanUse()) return;
            if (motion.DoJump()) jumpInst.Use(999);
        }
        private void DoJump(InputActionEventData data)
        {
            jumpInst.Refresh();
        }
        #endregion

    }
}
