using BPSFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************************
//创建人：BPS
//创建时间：
//功能说明：
//*****************************************
[UIWindowData(typeof(TestUI),true,"TestUI",1)]
public class TestUI : UI_WindowBase
{
    public override void Init()
    {
        base.Init();
    }

    public override void OnClose()
    {
        base.OnClose();
        
    }

    public override void OnShow()
    {
        base.OnShow();
        
    }

    protected override void RegisterEventListener()
    {
        base.RegisterEventListener();
        UISystem.AddTips("打开了窗口！");
    }

    protected override void CancelEventListener()
    {
        base.CancelEventListener();
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
