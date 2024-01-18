using BPSFrame;
using Cysharp.Threading.Tasks;
using JK.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using static UnityEditor.Experimental.GraphView.GraphView;
//*****************************************
//创建人：BPS
//创建时间：
//功能说明：
//*****************************************
[UIWindowData(typeof(UI_Login), true, "UI_Login", 0)]
public class UI_Login : UI_WindowBase
{
    private VideoPlayer _player;
    private Double _videoLenth;

    public override async void InitAsync()
    {
        base.InitAsync();
        _player = GetComponent<VideoPlayer>();
        _videoLenth = _player.length;
        await UniTask.Delay((int)(_videoLenth * 1000));
        SceneSystem.LoadSceneAsync("Level Main", GotoNext, LoadSceneMode.Single);
        
    }
    private void GotoNext(float obj)
    {
        UISystem.Close<UI_Login>();
        UISystem.Show<UI_GameWindow>();
    }
    public override void OnClose()
    {
        base.OnClose();
    }

    public override void OnShowAsync()
    {
        base.OnShowAsync();

    }

}
