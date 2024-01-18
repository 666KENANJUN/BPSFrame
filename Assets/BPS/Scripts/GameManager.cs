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
        UISystem.Show<UI_Login>();
    }


}
