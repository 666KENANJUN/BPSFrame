

//*****************************************
//创建人：BPS
//创建时间：2023.12.29
//功能说明：事件参数类管理中心
//*****************************************

using System;

namespace BPSFrame
{
    public class PlayerDeadEvenArgs : EventArgs
    {
        public string PlayerName;

        public PlayerDeadEvenArgs(string playerName)
        {
            PlayerName = playerName;
        }
    }
}
