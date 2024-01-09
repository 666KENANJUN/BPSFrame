using System;

//*****************************************
//创建人：BPS
//创建时间：2023.12.29
//功能说明：事件管理中心拓展方法
//*****************************************
namespace BPSFrame
{
    public static class EventTriggerExt
    {
        public static void TriggerEvent(this object sender, string eventName)
        {
            EventManager.Instance.TriggerEvent(eventName, sender);
        }

        public static void TriggerEvent(this object serder, string eventName, EventArgs args)
        {
            EventManager.Instance.TriggerEvent(eventName, serder, args);
        }
    }
}
