using UnityEngine;

namespace Toolkits
{
    public static class GeometryUtility
    {
        #region Vector3
        public static Vector3 ConvertXZ(this Vector2 src) => new(src.x, 0, src.y);
        public static Vector3 SetY(this Vector3 src, float y) => new(src.x, y, src.z);
        
        #endregion
    }
}
