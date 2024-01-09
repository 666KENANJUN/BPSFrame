using System;
using UnityEngine;
using UnityEngine.EventSystems;
namespace BPSFrame
{
    public static class BPSEventListenerExtend
    {
        #region 工具函数
        private static BPSEventListener GetOrAddJKEventListener(Component com)
        {
            BPSEventListener lis = com.GetComponent<BPSEventListener>();
            if (lis == null) return com.gameObject.AddComponent<BPSEventListener>();
            else return lis;
        }
        public static void AddEventListener<T, TEventArg>(this Component com, BPSEventType eventType, Action<T, TEventArg> action, TEventArg args = default(TEventArg))
        {
            AddEventListener(com, (int)eventType, action, args);
        }
        public static void AddEventListener<T, TEventArg>(this Component com, int customEventTypeInt, Action<T, TEventArg> action, TEventArg args = default(TEventArg))
        {
            BPSEventListener lis = GetOrAddJKEventListener(com);
            lis.AddListener(customEventTypeInt, action, args);
        }
        public static void RemoveEventListener<T, TEventArg>(this Component com, int customEventTypeInt, Action<T, TEventArg> action)
        {
            BPSEventListener lis = com.GetComponent<BPSEventListener>();
            if (lis != null) lis.RemoveListener(customEventTypeInt, action);
        }
        public static void RemoveEventListener<T, TEventArg>(this Component com, BPSEventType eventType, Action<T, TEventArg> action)
        {
            RemoveEventListener(com, (int)eventType, action);
        }
        public static void RemoveAllListener(this Component com, int customEventTypeInt)
        {
            BPSEventListener lis = com.GetComponent<BPSEventListener>();
            if (lis != null) lis.RemoveAllListener(customEventTypeInt);
        }
        public static void RemoveAllListener(this Component com, BPSEventType eventType)
        {
            RemoveAllListener(com, (int)eventType);
        }
        public static void RemoveAllListener(this Component com)
        {
            BPSEventListener lis = com.GetComponent<BPSEventListener>();
            if (lis != null) lis.RemoveAllListener();
        }
        public static void TriggerCustomEvent<T>(this Component com, int customEventTypeInt, T eventData)
        {
            BPSEventListener lis = GetOrAddJKEventListener(com);
            lis.TriggerAction<T>(customEventTypeInt, eventData);
        }
        #endregion

        #region 鼠标相关事件
        public static void OnMouseEnter<TEventArg>(this Component com, Action<PointerEventData, TEventArg> action, TEventArg args = default(TEventArg))
        {
            AddEventListener(com, BPSEventType.OnMouseEnter, action, args);
        }
        public static void OnMouseExit<TEventArg>(this Component com, Action<PointerEventData, TEventArg> action, TEventArg args = default(TEventArg))
        {
            AddEventListener(com, BPSEventType.OnMouseExit, action, args);
        }
        public static void OnClick<TEventArg>(this Component com, Action<PointerEventData, TEventArg> action, TEventArg args = default(TEventArg))
        {
            AddEventListener(com, BPSEventType.OnClick, action, args);
        }
        public static void OnClickDown<TEventArg>(this Component com, Action<PointerEventData, TEventArg> action, TEventArg args = default(TEventArg))
        {
            AddEventListener(com, BPSEventType.OnClickDown, action, args);
        }
        public static void OnClickUp<TEventArg>(this Component com, Action<PointerEventData, TEventArg> action, TEventArg args = default(TEventArg))
        {
            AddEventListener(com, BPSEventType.OnClickUp, action, args);
        }
        public static void OnDrag<TEventArg>(this Component com, Action<PointerEventData, TEventArg> action, TEventArg args = default(TEventArg))
        {
            AddEventListener(com, BPSEventType.OnDrag, action, args);
        }
        public static void OnBeginDrag<TEventArg>(this Component com, Action<PointerEventData, TEventArg> action, TEventArg args = default(TEventArg))
        {
            AddEventListener(com, BPSEventType.OnBeginDrag, action, args);
        }
        public static void OnEndDrag<TEventArg>(this Component com, Action<PointerEventData, TEventArg> action, TEventArg args = default(TEventArg))
        {
            AddEventListener(com, BPSEventType.OnEndDrag, action, args);
        }

        public static void RemoveOnClick<TEventArg>(this Component com, Action<PointerEventData, TEventArg> action)
        {
            RemoveEventListener(com, BPSEventType.OnClick, action);
        }
        public static void RemoveOnClickDown<TEventArg>(this Component com, Action<PointerEventData, TEventArg> action)
        {
            RemoveEventListener(com, BPSEventType.OnClickDown, action);
        }

        public static void RemoveOnMouseEnter<TEventArg>(this Component com, Action<PointerEventData, TEventArg> action)
        {
            RemoveEventListener(com, BPSEventType.OnMouseEnter, action);
        }
        public static void RemoveOnMouseExit<TEventArg>(this Component com, Action<PointerEventData, TEventArg> action)
        {
            RemoveEventListener(com, BPSEventType.OnMouseExit, action);
        }
        public static void RemoveOnClickUp<TEventArg>(this Component com, Action<PointerEventData, TEventArg> action)
        {
            RemoveEventListener(com, BPSEventType.OnClickUp, action);
        }
        public static void RemoveOnDrag<TEventArg>(this Component com, Action<PointerEventData, TEventArg> action)
        {
            RemoveEventListener(com, BPSEventType.OnDrag, action);
        }
        public static void RemoveOnBeginDrag<TEventArg>(this Component com, Action<PointerEventData, TEventArg> action)
        {
            RemoveEventListener(com, BPSEventType.OnBeginDrag, action);
        }
        public static void RemoveOnEndDrag<TEventArg>(this Component com, Action<PointerEventData, TEventArg> action)
        {
            RemoveEventListener(com, BPSEventType.OnEndDrag, action);
        }


        #endregion

        #region 碰撞相关事件

        public static void OnCollisionEnter<TEventArg>(this Component com, Action<Collision, TEventArg> action, TEventArg args = default(TEventArg))
        {
            com.AddEventListener(BPSEventType.OnCollisionEnter, action, args);
        }


        public static void OnCollisionStay<TEventArg>(this Component com, Action<Collision, TEventArg> action, TEventArg args = default(TEventArg))
        {
            AddEventListener(com, BPSEventType.OnCollisionStay, action, args);
        }
        public static void OnCollisionExit<TEventArg>(this Component com, Action<Collision, TEventArg> action, TEventArg args = default(TEventArg))
        {
            AddEventListener(com, BPSEventType.OnCollisionExit, action, args);
        }
        public static void OnCollisionEnter2D<TEventArg>(this Component com, Action<Collision2D, TEventArg> action, TEventArg args = default(TEventArg))
        {
            AddEventListener(com, BPSEventType.OnCollisionEnter2D, action, args);
        }
        public static void OnCollisionStay2D<TEventArg>(this Component com, Action<Collision2D, TEventArg> action, TEventArg args = default(TEventArg))
        {
            AddEventListener(com, BPSEventType.OnCollisionStay2D, action, args);
        }
        public static void OnCollisionExit2D<TEventArg>(this Component com, Action<Collision2D, TEventArg> action, TEventArg args = default(TEventArg))
        {
            AddEventListener(com, BPSEventType.OnCollisionExit2D, action, args);
        }
        public static void RemoveOnCollisionEnter<TEventArg>(this Component com, Action<Collision, TEventArg> action)
        {
            RemoveEventListener(com, BPSEventType.OnCollisionEnter, action);
        }
        public static void RemoveOnCollisionStay<TEventArg>(this Component com, Action<Collision, TEventArg> action)
        {
            RemoveEventListener(com, BPSEventType.OnCollisionStay, action);
        }
        public static void RemoveOnCollisionExit<TEventArg>(this Component com, Action<Collision, TEventArg> action)
        {
            RemoveEventListener(com, BPSEventType.OnCollisionExit, action);
        }
        public static void RemoveOnCollisionEnter2D<TEventArg>(this Component com, Action<Collision2D, TEventArg> action)
        {
            RemoveEventListener(com, BPSEventType.OnCollisionEnter2D, action);
        }
        public static void RemoveOnCollisionStay2D<TEventArg>(this Component com, Action<Collision2D, TEventArg> action)
        {
            RemoveEventListener(com, BPSEventType.OnCollisionStay2D, action);
        }
        public static void RemoveOnCollisionExit2D<TEventArg>(this Component com, Action<Collision2D, TEventArg> action)
        {
            RemoveEventListener(com, BPSEventType.OnCollisionExit2D, action);
        }
        #endregion

        #region 触发相关事件
        public static void OnTriggerEnter<TEventArg>(this Component com, Action<Collider, TEventArg> action, TEventArg args = default(TEventArg))
        {
            AddEventListener(com, BPSEventType.OnTriggerEnter, action, args);
        }
        public static void OnTriggerStay<TEventArg>(this Component com, Action<Collider, TEventArg> action, TEventArg args = default(TEventArg))
        {
            AddEventListener(com, BPSEventType.OnTriggerStay, action, args);
        }
        public static void OnTriggerExit<TEventArg>(this Component com, Action<Collider, TEventArg> action, TEventArg args = default(TEventArg))
        {
            AddEventListener(com, BPSEventType.OnTriggerExit, action, args);
        }
        public static void OnTriggerEnter2D<TEventArg>(this Component com, Action<Collider2D, TEventArg> action, TEventArg args = default(TEventArg))
        {
            AddEventListener(com, BPSEventType.OnTriggerEnter2D, action, args);
        }
        public static void OnTriggerStay2D<TEventArg>(this Component com, Action<Collider2D, TEventArg> action, TEventArg args = default(TEventArg))
        {
            AddEventListener(com, BPSEventType.OnTriggerStay2D, action, args);
        }
        public static void OnTriggerExit2D<TEventArg>(this Component com, Action<Collider2D, TEventArg> action, TEventArg args = default(TEventArg))
        {
            AddEventListener(com, BPSEventType.OnTriggerExit2D, action, args);
        }
        public static void RemoveOnTriggerEnter<TEventArg>(this Component com, Action<Collider, TEventArg> action)
        {
            RemoveEventListener(com, BPSEventType.OnTriggerEnter, action);
        }
        public static void RemoveOnTriggerStay<TEventArg>(this Component com, Action<Collider, TEventArg> action)
        {
            RemoveEventListener(com, BPSEventType.OnTriggerStay, action);
        }
        public static void RemoveOnTriggerExit<TEventArg>(this Component com, Action<Collider, TEventArg> action)
        {
            RemoveEventListener(com, BPSEventType.OnTriggerExit, action);
        }
        public static void RemoveOnTriggerEnter2D<TEventArg>(this Component com, Action<Collider2D, TEventArg> action)
        {
            RemoveEventListener(com, BPSEventType.OnTriggerEnter2D, action);
        }
        public static void RemoveOnTriggerStay2D<TEventArg>(this Component com, Action<Collider2D, TEventArg> action)
        {
            RemoveEventListener(com, BPSEventType.OnTriggerStay2D, action);
        }
        public static void RemoveOnTriggerExit2D<TEventArg>(this Component com, Action<Collider2D, TEventArg> action)
        {
            RemoveEventListener(com, BPSEventType.OnTriggerExit2D, action);
        }
        #endregion

        #region 资源相关事件
        public static void OnReleaseAddressableAsset<TEventArg>(this Component com, Action<GameObject, TEventArg> action, TEventArg args = default(TEventArg))
        {
            AddEventListener(com, BPSEventType.OnReleaseAddressableAsset, action, args);
        }
        public static void RemoveOnReleaseAddressableAsset<TEventArg>(this Component com, Action<GameObject, TEventArg> action)
        {
            RemoveEventListener(com, BPSEventType.OnReleaseAddressableAsset, action);
        }

        public static void OnDestroy<TEventArg>(this Component com, Action<GameObject, TEventArg> action, TEventArg args = default(TEventArg))
        {
            AddEventListener(com, BPSEventType.OnDestroy, action, args);
        }
        public static void RemoveOnDestroy<TEventArg>(this Component com, Action<GameObject, TEventArg> action)
        {
            RemoveEventListener(com, BPSEventType.OnDestroy, action);
        }

        #endregion
    }
}
