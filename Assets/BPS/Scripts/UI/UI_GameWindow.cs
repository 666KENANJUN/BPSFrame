using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BPSFrame;
using UnityEngine.UI;
using System;
using Sirenix.OdinInspector;
//*****************************************
//创建人：BPS
//创建时间：
//功能说明：
//*****************************************
[UIWindowData(typeof(UI_GameWindow), true, "UI_GameWindow", 0)]
public class UI_GameWindow : UI_WindowBase
{
    Animator animator;

    [SerializeField] private Slider _sliderGlobalVolume;
    [SerializeField] private Slider _sliderBGVolume;
    [SerializeField] private Slider _sliderEffectVolume;
    [SerializeField] private Dropdown _dropdownImageSetting;
    [SerializeField] private Button _exitGame;


    public override void Init()
    {
        base.Init();
        animator = GetComponent<Animator>();
    }


    public override void OnShow()
    {
        base.OnShow();
        _sliderGlobalVolume.onValueChanged.AddListener(SetGlobalVolume);
        _sliderBGVolume.onValueChanged.AddListener(SetBGVolume);
        _sliderEffectVolume.onValueChanged.AddListener(SetEffectVolume);
        _exitGame.onClick.AddListener(ExitGame);

    }

    private void ExitGame()
    {
#if UNITY_EDITOR//在编辑器模式退出
        UnityEditor.EditorApplication.isPlaying = false;
#else//发布后退出
        Application.Quit();
#endif
    }

    private void SetEffectVolume(float arg0)
    {
        AudioSystem.EffectVolume = arg0;
    }

    private void SetBGVolume(float arg0)
    {
        AudioSystem.BGVolume = arg0;
    }

    private void SetGlobalVolume(float arg0)
    {
        AudioSystem.GlobalVolume = arg0;
    }


    public override void OnClose()
    {
        base.OnClose();
        _sliderGlobalVolume.RemoveAllListener();
        _sliderBGVolume.RemoveAllListener();
        _sliderEffectVolume.RemoveAllListener();
        _exitGame.RemoveAllListener();
    }


}
