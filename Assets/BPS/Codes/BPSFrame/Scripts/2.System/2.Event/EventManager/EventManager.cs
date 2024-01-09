

//*****************************************
//创建人：BPS
//创建时间：2023.12.29
//功能说明：事件管理中心
//*****************************************

using System;
using System.Collections.Generic;

namespace BPSFrame
{
    public class EventManager : Singleton<EventManager>
    {
        private Dictionary<string, EventHandler> handlerDic = new Dictionary<string, EventHandler>();

        /// <summary>
        /// 添加一个事件的监听者
        /// </summary>
        /// <param name="eventName">事件名</param>
        /// <param name="handler">事件处理函数（即回调函数）</param>
        public void AddListener(string eventName, EventHandler handler)
        {
            if (handlerDic.ContainsKey(eventName))
            {
                handlerDic[eventName] += handler;
            }
            else
            {
                handlerDic.Add(eventName,handler);
            }
        }
        
        /// <summary>
        /// 移除一个事件的监听者
        /// </summary>
        /// <param name="eventName">事件名</param>
        /// <param name="handler">事件处理函数（回调函数）</param>
        public void RemoveListener(string eventName, EventHandler handler)
        {
            if (handlerDic.ContainsKey(eventName))
            {
                handlerDic[eventName] -= handler;
            }
        }

        /// <summary>
        /// 无参数 触发事件
        /// </summary>
        /// <param name="eventName">触发事件名</param>
        /// <param name="sender">触发源</param>
        public void TriggerEvent(string eventName,object sender)
        {
            if (handlerDic.ContainsKey(eventName))
            {
                handlerDic[eventName]?.Invoke(sender,EventArgs.Empty);
            }
        }

        /// <summary>
        /// 有参数 触发事件
        /// </summary>
        /// <param name="eventName">事件名</param>
        /// <param name="serder">触发源</param>
        /// <param name="args">事件参数</param>
        public void TriggerEvent(string eventName, object serder, EventArgs args)
        {
            if (handlerDic.ContainsKey(eventName))
            {
                handlerDic[eventName]?.Invoke(serder,args);
            }
        }

        /// <summary>
        /// 清空所有事件
        /// </summary>
        public void Clear()
        {
            handlerDic.Clear();
        }
    }
}
