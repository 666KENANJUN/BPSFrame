using BPSFrame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
//*****************************************
//创建人：BPS
//创建时间：
//功能说明：
//*****************************************
public class TestAddressable : MonoBehaviour
{
    async void Start()
    {
        ResSystem.LoadAssetAsync<GameObject>("Test", OnLoadComplete);
    }

    private void OnLoadComplete(GameObject @object)
    {
        Debug.Log("加载完成！现在实例化");
        Instantiate(@object);
        Debug.Log("实例化完成！");
        UISystem.Show<TestUI>(0);
    }

    void Update()
    {
        
    }
}
