using System;
using UnityEngine;

namespace BPSFrame
{
    /// <summary>
    /// 窗口基类
    /// </summary>
    public class UI_WindowBase : MonoBehaviour
    {
        protected bool uiEnable;
        public bool UIEnable { get => uiEnable; }
        protected int currentLayer;
        public int CurrentLayer { get => currentLayer; }

        // 窗口类型
        public Type Type { get { return this.GetType(); } }

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void InitAsync() { }

        public void ShowGeneralLogic()
        {
            uiEnable = true;
            OnUpdateLanguage();
            RegisterEventListener();
            OnShowAsync();
        }

        /// <summary>
        /// 显示
        /// </summary>
        public virtual void OnShowAsync() { }


        public void CloseGeneralLogic()
        {
            uiEnable = false;
            CancelEventListener();
            OnClose();
        }

        /// <summary>
        /// 关闭时额外执行的内容
        /// </summary>
        public virtual void OnClose() { }

        /// <summary>
        /// 注册事件
        /// </summary>
        protected virtual void RegisterEventListener() { }

        /// <summary>
        /// 取消事件
        /// </summary>
        protected virtual void CancelEventListener() { }
        protected virtual void OnUpdateLanguage() { }
    }
}