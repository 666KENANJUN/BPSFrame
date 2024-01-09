using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace BPSFrame
{
    /// <summary>
    /// 事件类型,枚举本质上是int，所以您可以自定义事件，只需要枚举的值和下方不重复即可
    /// </summary>
    public enum BPSEventType
    {
        OnMouseEnter = -10001,
        OnMouseExit = -10002,
        OnClick = -10003,
        OnClickDown = -10004,
        OnClickUp = -10005,
        OnDrag = -10006,
        OnBeginDrag = -10007,
        OnEndDrag = -10008,
        OnCollisionEnter = -10009,
        OnCollisionStay = -10010,
        OnCollisionExit = -10011,
        OnCollisionEnter2D = -10012,
        OnCollisionStay2D = -10013,
        OnCollisionExit2D = -10014,
        OnTriggerEnter = -10015,
        OnTriggerStay = -10016,
        OnTriggerExit = -10017,
        OnTriggerEnter2D = -10018,
        OnTriggerStay2D = -10019,
        OnTriggerExit2D = -10020,
        OnReleaseAddressableAsset = -10021,
        OnDestroy = -10022,
    }

    public interface IMouseEvent : IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    { }

    /// <summary>
    /// 事件工具
    /// 可以添加 鼠标、碰撞、触发等事件
    /// </summary>
    public class BPSEventListener : MonoBehaviour, IMouseEvent
    {
        private static ObjectPoolModule poolModul = new ObjectPoolModule();
        #region 内部类、接口等
        /// <summary>
        /// 持有关键字典数据，主要用于将这个引用放入对象池中
        /// </summary>
        private class BPSEventListenerData
        {
            public Dictionary<int, IBPSEventListenerEventInfos> eventInfoDic = new Dictionary<int, BPSEventListener.IBPSEventListenerEventInfos>();
        }


        private interface IBPSEventListenerEventInfo<T>
        {
            void TriggerEvent(T eventData);
            void Destory();
        }

        /// <summary>
        /// 某个事件中一个事件的数据包装类
        /// </summary>
        private class BPSEventListenerEventInfo<T, TEventArg> : IBPSEventListenerEventInfo<T>
        {
            // T：事件本身的参数（PointerEventData、Collision）
            // object[]:事件的参数
            public Action<T, TEventArg> action;
            public TEventArg arg;
            public void Init(Action<T, TEventArg> action, TEventArg args = default(TEventArg))
            {
                this.action = action;
                this.arg = args;
            }
            public void Destory()
            {
                this.action = null;
                this.arg = default(TEventArg);
                poolModul.PushObject(this);
            }
            public void TriggerEvent(T eventData)
            {
                action?.Invoke(eventData, arg);
            }
        }

        interface IBPSEventListenerEventInfos
        {
            void RemoveAll();

        }

        /// <summary>
        /// 一类事件的数据包装类型：包含多个JKEventListenerEventInfo
        /// </summary>
        private class BPSEventListenerEventInfos<T> : IBPSEventListenerEventInfos
        {
            // 所有的事件
            private List<IBPSEventListenerEventInfo<T>> eventList = new List<IBPSEventListenerEventInfo<T>>();

            /// <summary>
            /// 添加事件
            /// </summary>
            public void AddListener<TEventArg>(Action<T, TEventArg> action, TEventArg args = default(TEventArg))
            {
                BPSEventListenerEventInfo<T, TEventArg> info = poolModul.GetObject<BPSEventListenerEventInfo<T, TEventArg>>();
                if (info == null) info = new BPSEventListenerEventInfo<T, TEventArg>();
                info.Init(action, args);
                eventList.Add(info);
            }

            public void TriggerEvent(T evetData)
            {
                for (int i = 0; i < eventList.Count; i++)
                {
                    eventList[i].TriggerEvent(evetData);
                }
            }

            /// <summary>
            /// 移除事件
            /// 即时同一个函数+参数注册过多次，无论如何该方法只会移除一个事件
            /// </summary>
            public void RemoveListener<TEventArg>(Action<T, TEventArg> action, TEventArg args = default(TEventArg))
            {
                for (int i = 0; i < eventList.Count; i++)
                {
                    BPSEventListenerEventInfo<T, TEventArg> eventInfo = eventList[i] as BPSEventListenerEventInfo<T, TEventArg>;
                    if (eventInfo == null) continue; // 类型不符

                    // 找到这个事件，查看是否相等
                    if (eventInfo.action.Equals(action))
                    {
                        // 移除
                        eventInfo.Destory();
                        eventList.RemoveAt(i);
                        return;
                    }
                }
            }

            /// <summary>
            /// 移除全部，全部放进对象池
            /// </summary>
            public void RemoveAll()
            {
                for (int i = 0; i < eventList.Count; i++)
                {
                    eventList[i].Destory();
                }
                eventList.Clear();
                poolModul.PushObject(this);
            }
        }

        #endregion

        private BPSEventListenerData data;
        private BPSEventListenerData Data
        {
            get
            {
                if (data == null)
                {
                    data = poolModul.GetObject<BPSEventListenerData>();
                    if (data == null) data = new BPSEventListenerData();
                }
                return data;
            }
        }

        #region 外部的访问
        /// <summary>
        /// 添加事件
        /// </summary>
        public void AddListener<T, TEventArg>(int eventTypeInt, Action<T, TEventArg> action, TEventArg args)
        {
            if (Data.eventInfoDic.TryGetValue(eventTypeInt, out IBPSEventListenerEventInfos info))
            {
                ((BPSEventListenerEventInfos<T>)info).AddListener(action, args);
            }
            else
            {
                BPSEventListenerEventInfos<T> infos = poolModul.GetObject<BPSEventListenerEventInfos<T>>();
                if (infos == null) infos = new BPSEventListenerEventInfos<T>();
                infos.AddListener(action, args);
                Data.eventInfoDic.Add(eventTypeInt, infos);
            }
        }
        /// <summary>
        /// 添加事件
        /// </summary>
        public void AddListener<T, TEventArg>(BPSEventType eventType, Action<T, TEventArg> action, TEventArg args)
        {
            AddListener((int)eventType, action, args);
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        public void RemoveListener<T, TEventArg>(int eventTypeInt, Action<T, TEventArg> action)
        {
            if (Data.eventInfoDic.TryGetValue(eventTypeInt, out IBPSEventListenerEventInfos info))
            {
                ((BPSEventListenerEventInfos<T>)info).RemoveListener(action);
            }
        }
        /// <summary>
        /// 移除事件
        /// </summary>
        public void RemoveListener<T, TEventArg>(BPSEventType eventType, Action<T, TEventArg> action)
        {
            RemoveListener((int)eventType, action);
        }

        /// <summary>
        /// 移除某一个事件类型下的全部事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventType"></param>
        public void RemoveAllListener(BPSEventType eventType)
        {
            if (Data.eventInfoDic.TryGetValue((int)eventType, out IBPSEventListenerEventInfos infos))
            {
                infos.RemoveAll();
                Data.eventInfoDic.Remove((int)eventType);
            }
        }
        /// <summary>
        /// 移除全部事件
        /// </summary>
        public void RemoveAllListener()
        {
            foreach (IBPSEventListenerEventInfos infos in Data.eventInfoDic.Values)
            {
                infos.RemoveAll();
            }

            data.eventInfoDic.Clear();
            // 将整个数据容器放入对象池
            poolModul.PushObject(data);
            data = null;
        }

        #endregion
        /// <summary>
        /// 触发事件
        /// </summary>
        public void TriggerAction<T>(int eventTypeInt, T eventData)
        {
            if (Data.eventInfoDic.TryGetValue(eventTypeInt, out IBPSEventListenerEventInfos infos))
            {
                (infos as BPSEventListenerEventInfos<T>).TriggerEvent(eventData);
            }
        }
        /// <summary>
        /// 触发事件
        /// </summary>
        public void TriggerAction<T>(BPSEventType eventType, T eventData)
        {
            TriggerAction<T>((int)eventType, eventData);
        }

        #region 鼠标事件
        public void OnPointerEnter(PointerEventData eventData)
        {
            TriggerAction(BPSEventType.OnMouseEnter, eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TriggerAction(BPSEventType.OnMouseExit, eventData);
        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            TriggerAction(BPSEventType.OnBeginDrag, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            TriggerAction(BPSEventType.OnDrag, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            TriggerAction(BPSEventType.OnEndDrag, eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            TriggerAction(BPSEventType.OnClick, eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            TriggerAction(BPSEventType.OnClickDown, eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            TriggerAction(BPSEventType.OnClickUp, eventData);
        }
        #endregion

        #region 碰撞事件
        private void OnCollisionEnter(Collision collision)
        {
            TriggerAction(BPSEventType.OnCollisionEnter, collision);
        }
        private void OnCollisionStay(Collision collision)
        {
            TriggerAction(BPSEventType.OnCollisionStay, collision);
        }
        private void OnCollisionExit(Collision collision)
        {
            TriggerAction(BPSEventType.OnCollisionExit, collision);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TriggerAction(BPSEventType.OnCollisionEnter2D, collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TriggerAction(BPSEventType.OnCollisionStay2D, collision);
        }
        private void OnCollisionExit2D(Collision2D collision)
        {
            TriggerAction(BPSEventType.OnCollisionExit2D, collision);
        }
        #endregion

        #region 触发事件
        private void OnTriggerEnter(Collider other)
        {
            TriggerAction(BPSEventType.OnTriggerEnter, other);
        }
        private void OnTriggerStay(Collider other)
        {
            TriggerAction(BPSEventType.OnTriggerStay, other);
        }
        private void OnTriggerExit(Collider other)
        {
            TriggerAction(BPSEventType.OnTriggerExit, other);
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            TriggerAction(BPSEventType.OnTriggerEnter2D, collision);
        }
        private void OnTriggerStay2D(Collider2D collision)
        {
            TriggerAction(BPSEventType.OnTriggerStay2D, collision);
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            TriggerAction(BPSEventType.OnTriggerExit2D, collision);
        }
        #endregion

        #region 销毁事件
        private void OnDestroy()
        {
            TriggerAction(BPSEventType.OnReleaseAddressableAsset, gameObject);
            TriggerAction(BPSEventType.OnDestroy, gameObject);

            // 销毁所有数据，并将一些数据放回对象池中
            RemoveAllListener();
        }
        #endregion
    }
}