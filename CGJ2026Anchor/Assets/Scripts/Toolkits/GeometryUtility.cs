using UnityEngine;

namespace Toolkits
{
    public static class GeometryUtility
    {
        #region Vector3
        public static Vector3 ConvertXZ(this Vector2 src) => new(src.x, 0, src.y);
        public static Vector3 SetY(this Vector3 src, float y) => new(src.x, y, src.z);
        public static Vector3 SetX(this Vector3 src, float x) => new(x, src.y, src.z);
        public static Vector3 SetZ(this Vector3 src, float z) => new(src.x, src.y, z);

        public static Vector3 Rotate(this Vector3 src, Vector3 axis, float angle)
        {
            return Quaternion.AngleAxis(angle, axis) * src;
        }
        #endregion

        #region Vector2

        public static Vector2 ConvertXZ(this Vector3 src) => new Vector2(src.x, src.z);

        #endregion
    }
}
