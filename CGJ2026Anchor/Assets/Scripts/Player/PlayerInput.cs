using System;
using Rewired;
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
            
        }

        private void OnDisable()
        {
            if (!ReInput.isReady || _player == null) return;

        }

        
        #endregion

        #region Updater

        private void Update()
        {
            OnMotion();
        }

        #endregion
        
        #region Motion
        [SerializeField] private PlayerMotion motion;
        private void OnMotion()
        {
            var input = _player.GetAxis2D("move-x","move-y");
            motion.input = input;   
        }

        #endregion

    }
}
