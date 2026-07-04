using System;
using Systems;
using UnityEngine;

namespace Launchers
{
    public static class GameLauncher
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void GameInit()
        {
            ServiceLocator.Reset();
            
            //全局缓存区
            var global = new Global();
            global.Reset();
            ServiceLocator.Register(global);

            var anchorGlobal = new AnchorGlobal();
            anchorGlobal.Reset();
            ServiceLocator.Register(anchorGlobal);
        }

        
    }
}
