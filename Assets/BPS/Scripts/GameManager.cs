using BPSFrame;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//*****************************************
//创建人：BPS
//创建时间：
//功能说明：
//*****************************************
public class GameManager : MonoBehaviour
{
    AudioClip BGClip;
    void Start()
    {
        BGClip = ResSystem.LoadAsset<AudioClip>("BGMusic");
        SceneSystem.LoadSceneAsync("Level Main",Init,LoadSceneMode.Single);
    }

    private async void Init(float obj)
    {
        AudioSystem.PlayBGAudio(BGClip);
        await UniTask.Delay(500);
        UISystem.Show<UI_GameWindow>();
    }
}
