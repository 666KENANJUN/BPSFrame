using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.AddressableAssets;
using BPSFrame;
using UnityEngine.ResourceManagement.AsyncOperations;
//*****************************************
//创建人：BPS
//创建时间：
//功能说明：
//*****************************************
public class NewBehaviourScript :MonoBehaviour
{
    private async void Awake()
    {
        var cube = await ResManager.LoadAssetAsync<GameObject>("Cube",callBack1);
        Instantiate(cube);
        var list =  await ResManager.LoadAssetsAsync<ArrayList>("1", callBac2);
    }

    private void callBack1(GameObject @object)
    {
        throw new NotImplementedException();
    }

    private void callBac2(AsyncOperationHandle<IList<ArrayList>> handle)
    {
        throw new NotImplementedException();
    }
}
