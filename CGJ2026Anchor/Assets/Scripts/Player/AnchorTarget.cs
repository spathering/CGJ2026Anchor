using Sirenix.OdinInspector;
using Systems;
using UnityEngine;

namespace Player
{
    public sealed class AnchorTarget : MonoBehaviour, IAnchor
    {
        #region Comps

        [FoldoutGroup("Comp")] [SerializeField] private OutlineFx.OutlineFx outline;
        

        #endregion
        
        #region Anchor
        public Transform AnchorTrans => transform;
        public void OnVisual()
        {
            outline.enabled = true;
        }

        public void OffVisual()
        {
            outline.enabled = false;
        }

        #endregion
        
        #region Mono
        private void OnEnable()
        {
            var global = ServiceLocator.GetService<AnchorGlobal>();
            global?.RegisterAnchor(this);
        }

        private void OnDisable()
        {
            var global = ServiceLocator.GetService<AnchorGlobal>();
            global?.UnregisterAnchor(this);
        }

        #endregion

    }
}
