using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems
{
    public sealed class PhysicsUpdater : MonoBehaviour
    {
        private Global _global;
        [ShowInInspector] private float _timeScale;
        
        private void Awake()
        {
            Physics.simulationMode = SimulationMode.Script;
            _global = ServiceLocator.GetService<Global>();
        }
        private void FixedUpdate()
        {
            //固定更新
            _timeScale = _global.TimeScale.Value;
            Physics.Simulate(_global.TimeDelta);
        }
    }
}
