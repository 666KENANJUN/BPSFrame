using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

[CreateAssetMenu(fileName = "AddressableConfig", menuName = "KzhTools/CreateAddressableConfig", order = 0)]
public class AddressableConfigs : ScriptableObject
{
    [Header("这里放置所有需要动态加载的场景资源")] public List<SceneAsset> SceneList;

    [Header("eg:Assets/HotRes")] [Header("注意：这里的路径采用以下形式")]
    public List<string> HotResPathList;

    [Header("Ignore Asset Group")] public List<string> IgnoreAssetGroupList = new List<string>()
        {"Built In Data", "Default Local Group", "Config Assets", "Code Assets"};

    // Schema Group NamingStyle
    [HideInInspector]
    public BundledAssetGroupSchema.BundleNamingStyle localNamingStyle = BundledAssetGroupSchema.BundleNamingStyle.OnlyHash;
    [HideInInspector]
    public BundledAssetGroupSchema.BundleNamingStyle remoteNamingStyle = BundledAssetGroupSchema.BundleNamingStyle.OnlyHash;
}