using System;
using Toolkits;
using UnityEngine;

namespace Systems
{
    public sealed class Global : IDisposable
    {
        public DecoratedValue<float> TimeScale;
        public float TimeDelta = .02f;
        
        public OnTimeScaleDelegate OnTimeScale;
        public OnTimeDeltaDelegate OnTimeDelta;
        
        public Collider[] Colliders = new Collider[256];
        public void Reset()
        {
            OnTimeScale = scale => { };
            OnTimeDelta = delta => { };
            var onTimeScale = new ValueChanged<float>(scale =>
            {
                TimeDelta = 0.02f * scale;
                OnTimeScale?.Invoke(scale);
                OnTimeDelta?.Invoke(TimeDelta);
            });
            
            Colliders = new Collider[256];
            TimeScale = new DecoratedValue<float>(1f, onTimeScale);
        }

        public void Dispose()
        {
            
        }
    }
}
