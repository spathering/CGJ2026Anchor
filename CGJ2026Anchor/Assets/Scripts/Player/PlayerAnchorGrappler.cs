using System;
using Systems;
using UnityEngine;

namespace Player
{
    public sealed class PlayerAnchorGrappler : MonoBehaviour
    {
        private Camera _camera;
        private AnchorGlobal _anchorGlobal;
        #region Mono
        private void OnEnable()
        {
            _camera = Camera.main;
            _anchorGlobal = ServiceLocator.GetService<AnchorGlobal>();
        }

        #endregion
        
        #region Updater

        private void FixedUpdate()
        {
            
        }

        #endregion

        #region Data Process

        private void CheckingAnchors()
        {
            var anchors = _anchorGlobal.Anchors;
            for (var i = 0; i < anchors.Count; i++)
            {
                var anchor = anchors[i];
                // var anchorScreenPos = 
            }
        }

        #endregion

        #region Ctrl

        public void DoGrapple()
        {
            
        }

        public void ExitGrapple()
        {
            
        }

        #endregion
    }
}
