using BPSFrame;
using Cysharp.Threading.Tasks;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
//*****************************************
//创建人：BPS
//创建时间：
//功能说明：
//*****************************************
public class InitManager : MonoBehaviour
{
    void Start()
    {
        Init();
    }

    void Init()
    {
        LoadAsset();
        
        SceneSystem.LoadSceneAsync("Level 1");
        EventSystem.AddEventListener<float>("LoadingSceneProgress", LoadProgress);
        EventSystem.AddEventListener("LoadSceneSucceed",LoadSceneSucceed);
    }

    private void LoadAsset()
    {
        
    }

    private void LoadSceneSucceed()
    {
        Debug.Log("加载完成");
    }

    private void LoadProgress(float progress)
    {
        Debug.Log("加载进度:"+progress);
    }
}
