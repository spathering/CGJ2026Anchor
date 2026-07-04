using System;
using System.Collections.Generic;
using UnityEngine;

namespace Systems
{
    public sealed class AnchorGlobal : IDisposable
    {
        private readonly List<IAnchor> _iAnchors = new();

        public void Reset()
        {
            _iAnchors.Clear();
        }
        public void Dispose()
        {
            Reset();
        }

        public IReadOnlyList<IAnchor> Anchors => _iAnchors;
        
        public void RegisterAnchor(IAnchor iAnchor)
        {
            _iAnchors.Add(iAnchor);
        }
        public void UnregisterAnchor(IAnchor iAnchor)
        {
            _iAnchors.Remove(iAnchor);
        }
    }

    public interface IAnchor
    {
        public Transform AnchorTrans { get; }

        public virtual void OnVisual(){}
        public virtual void OffVisual(){}
        
    }
}
