using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Toolkits
{
    public delegate void DefaultDelegate();
    
    #region 计数器
    [Serializable] public sealed class ConsumeCounter
    {
        [ShowInInspector, ReadOnly] private int _remain;
        public int refreshTo = 1;

        public void Refresh()
        {
            _remain = refreshTo;
            OnCounterUpdate();
        }

        public void Refresh(int count)
        {
            _remain = count; 
            OnCounterUpdate();
        }

        public void SetRefreshTo(int count)
        {
            refreshTo = count; 
            OnCounterUpdate();
        }
        public bool CanUse() => _remain > 0;
        /// <summary>
        /// 剩余使用机制，够用则返回true
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool Use(int count = 1)
        {
            if (_remain < count) return false;
            
            _remain -= count;
            OnCounterUpdate();
            
            return true;
        }
        public float GetPercent() { return 1f - Mathf.Clamp01(1f * _remain / refreshTo) ; }
        public float GetValue() => _remain;
        
        //回调
        public DefaultDelegate OnCounterUpdate = () => { };
    }
    [Serializable] public class ConsumeCounterFloat
    {
        [ShowInInspector, ReadOnly] private float _remain;
        public float refreshTo = 1;
        public void Refresh() { _remain = refreshTo; }
        public void Refresh(float count) { _remain = count; }
        public void SetRefreshTo(float count) { refreshTo = count; }
        /// <summary>
        /// 累计使用机制，本次累计后达标，则可使用
        /// </summary>
        /// <param name="count">累计值</param>
        /// <returns></returns>
        public bool Use(float count)
        {
            if (_remain <= 0) return true;
            _remain -= count;
            return _remain <= 0;
        }
        public bool CanUse(){return _remain <= 0;}
        public float GetPercent()
        {
            return 1f - Mathf.Clamp01(_remain / refreshTo) ;
        }

        public float GetValue() => _remain;
    }
    [Serializable] public class LoopCounter
    {
        [ShowInInspector, ReadOnly, HorizontalGroup("0"), HideLabel] private int _curCount;
        [HorizontalGroup("0"), HideLabel] public int loopCount = 1;
        public void Reset() { _curCount = 0; }
        public int GetCurrent() => _curCount;
        public void DoCount(int count) { _curCount = (_curCount + count + loopCount) % loopCount; }
    }
    [Serializable] public class ConsumeCounterFloat_RefreshAdd
    {
        [ShowInInspector, ReadOnly] private float _remain;
        public float refreshTo = 1;
        public void Refresh() { _remain = refreshTo; }
        public void Refresh(float count) { _remain += count; }
        public void SetRefreshTo(float count) { refreshTo = count; }
        /// <summary>
        /// 无上限累计使用机制，会不断累计。每次刷新时从总累计值中扣除
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool Use(float count)
        {
            var result = _remain <= 0;
            _remain -= count;
            return result;
        }
        public bool CanUse(){return _remain <= 0;}
        public float GetPercent()
        {
            return 1f - Mathf.Clamp01(_remain / refreshTo) ;
        }
    }

    [Serializable]
    public class RangeCounterFloat
    {
        public Vector2 limit;
        [ShowInInspector] private float _value;
        public float Value
        {
            get => _value;
            set => _value = Mathf.Clamp(value, limit.x, limit.y);
        }
        
        /// <summary>
        /// 扣除使用机制，本次够用则返回true
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool UseCheck(float count)
        {
            return Value >= count;
        }
        
        /// <summary>
        /// 扣除使用机制，本次够用则返回true
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool Use(float count)
        {
            if (Value < count) return false;
            Value -= count;
            return true;
        }
        public float GetPercent()
        {
            return (Value - limit.x) / (limit.y - limit.x);
        }
        public void Fill(float percentage = 1)
        {
            Value = limit.x + (limit.y - limit.x) * percentage;
        }
        public void FillAbs(float value)
        {
            Value = value;
        }
    }
    #endregion
    
    #region Unity扩展工具
    public static class UIUtility
    {
        /// <summary>
        /// 将世界坐标转换为 Canvas 上某个 RectTransform 下的局部坐标
        /// </summary>
        /// <param name="worldPos">世界坐标</param>
        /// <param name="camera">用于渲染该 Canvas 的摄像机</param>
        /// <param name="canvas">目标 Canvas</param>
        /// <param name="parent">Canvas 下的目标父 RectTransform</param>
        /// <returns>父节点下的本地坐标</returns>
        public static Vector2 WorldToUILocalPosition(Vector3 worldPos, Camera camera, Canvas canvas, RectTransform parent)
        {
            Vector3 screenPos;
            if (camera)
            {
                var viewPos = camera.WorldToViewportPoint(worldPos);
                // z <= 0 表示目标在相机背后
                if (viewPos.z <= 0f)
                {
                    // 翻转 x/y，避免背后目标投影到反方向
                    viewPos.x = 1f - viewPos.x;
                    viewPos.y = 1f - viewPos.y;

                    // 或者按你的需求直接返回边缘方向
                }
                screenPos = new Vector2(
                    viewPos.x * Screen.width,
                    viewPos.y * Screen.height
                );
            }
            else
            {
                screenPos = worldPos;
            }
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent,
                screenPos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : camera,
                out var localPos);

            return localPos;
        }
        
    }
    public static class TransformExtensions
    {
        public static void SelectChild(this Transform src, Func<Transform, bool> func,
            Action<Transform> action)
        {
            for (var i = 0; i < src.childCount; i++)
                src.GetChild(i).SelectChild(func, action);
            if (!func(src)) return;
            action(src);
        }
    }
    
    [Serializable] public sealed class TagFilter
    {
        [ValueDropdown("@UnityEditorInternal.InternalEditorUtility.tags")] [SerializeReference]
        private string[] tags;

        public bool HasTag(string tag)
        {
            for (var index = tags.Length - 1; index >= 0; index--)
            {
                var t = tags[index];
                if (t.Equals(tag)) return true;
            }
            return false;
        }
        
        public TagFilter (string[] tags){
            this.tags = tags;
        }
    }
    #endregion

    #region 容器工具
    public static class ListExtensions
    {
        /// <summary>
        /// 将 source 中满足 predicate 的元素添加到 target 中（不会清空 target）
        /// </summary>
        public static void AddWhere<T>(this List<T> target, List<T> source, Predicate<T> predicate)
        {
            foreach (var item in source)
                if (predicate(item)) target.Add(item);
        }
        /// <summary>
        /// 对目标List进行原地插入排序 List-int (a,b)=>(a-b)为降序
        /// </summary>
        public static void InsertionSort<T>(this List<T> list, Func<T,T,int> comparer)
        {
            var count = list.Count;
            for (var i = 1; i < count; i++)
            {
                var current = list[i];
                var j = i - 1;
                while (j >= 0 && comparer(current, list[j]) > 0)
                {
                    list[j + 1] = list[j]; // 后移
                    j--;
                }
                list[j + 1] = current;
            }
        }
        public static void InsertSortedFrom<T>(this List<T> target, ICollection<T> source, Comparer<T> comparer)
        {
            foreach (var item in source)
            {
                var index = target.BinarySearch(item, comparer);
                if (index < 0)
                    index = ~index;
                target.Insert(index, item);
            }
        }
        /// <summary>
        /// 将单个元素插入到指定的有序List
        /// </summary>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <param name="comparer"></param>
        /// <typeparam name="T"></typeparam>
        public static void InsertSorted<T>(this List<T> list, T item, Func<T, T, int> comparer)
        {
            var low = 0;
            var high = list.Count - 1;

            while (low <= high)
            {
                var mid = (low + high) >> 1;
                var cmp = comparer(item, list[mid]);
                if (cmp > 0) high = mid - 1;
                else low = mid + 1;
            }
            list.Insert(low, item);
        }

        public static bool RangeValid<T>(this List<T> container, int idx)
        {
            return idx >= 0 && idx < container.Count;
        }
    }
    public static class ArrayTools
    {
        /// <summary>
        /// 创建并填充一个数组，每个元素通过构造函数生成
        /// </summary>
        public static T[] CreateFilled<T>(int count, Func<T> generator)
        {
            var arr = new T[count];
            for (var i = 0; i < count; i++) arr[i] = generator();
            return arr;
        }
    }
    
    #endregion
    
    
    public static class FastMath
    {
        /// <summary>
        /// return value clamp to [min,max]
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float value, float min, float max)
        {
            return value < min ? min : value > max ? max : value;
        }
    }
    public static class InterfaceFinder
    {
        /// <summary>
        /// 查找场景中所有实现了接口 T 的组件
        /// </summary>
        public static void FindObjectsOfInterface<T>(ref List<T> results) where T : class
        {
            // 遍历场景中所有 MonoBehaviour
            foreach (var mono in Object.FindObjectsOfType<MonoBehaviour>(true)) // true = 包含 inactive
            {
                if (mono is T t) // 判断是否实现接口
                {
                    results.Add(t);
                }
            }
        }
    }
#if UNITY_EDITOR
    public static class EditorExtensions
    {
        public static bool IsMouseLeftDown(){return Event.current.type == UnityEngine.EventType.MouseDown && Event.current.button == 0;}
        public static bool IsMouseRightDown(){return Event.current.type == UnityEngine.EventType.MouseDown && Event.current.button == 1;}
        public static bool IsMouseLeftUp(){return Event.current.type == UnityEngine.EventType.MouseUp && Event.current.button == 0;}
        public static bool IsMouseRightUp(){return Event.current.type == UnityEngine.EventType.MouseUp && Event.current.button == 1;}
        public static Vector3 GetMouseWorldPoint(float maxDistance)
        {
            // 获取鼠标射线
            var ray = UnityEditor.HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            // 如果射线命中场景物体，返回碰撞点
            return Physics.Raycast(ray, out var hitInfo, maxDistance) ? hitInfo.point :
                // 否则，返回射线最大距离处的点
                ray.GetPoint(maxDistance);
        } 
    }
    public static class MAssetTool
    {
        public static List<T> FindAllAssetsOfType<T>() where T : ScriptableObject
        {
            var results = new List<T>();
            var guids = UnityEditor.AssetDatabase.FindAssets($"t:{typeof(T).Name}");

            foreach (var guid in guids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var so = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
                if (so != null)
                    results.Add(so);
            }
            return results;
        }

    }
#endif
}
