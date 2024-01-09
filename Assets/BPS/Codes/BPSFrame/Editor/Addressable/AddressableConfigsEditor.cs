using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

[CustomEditor(typeof(AddressableConfigs))]
public class AddressableConfigsEditor : Editor
{
    private enum AssetType
    {
        None = 0, // 未定义类型
        Packages = 1, // Package内的资源，默认添加到Default Local Group中
        Script = 2, // 脚本代码，忽略的资源
        Code = 3, // 热更代码二进制资源
        Config = 4, // 配置文件资源
        Audio = 5, // 音频文件资源
        Shader = 6, // 着色器
        Mat = 7, // 材质
        Model = 8, // 模型
        Prefab = 9, // 预制体
        HotRes = 10, // 热更资源
        SceneData = 11, // 场景数据
        Scene = 12, // 场景
        SpriteAtlas = 13, // 图集
    }

    private class AssetItem
    {
        public string GroupName; // 资源组名字
        public string AssetPath; // 资源路径
        public bool Address = false; // 是否已经标记Flag
        public bool HotRes = false; // 是否为自定义资源Flag
        public AssetType AssetType; // 资源类型
        public List<AssetItem> Assets; // 引用资源

        public AssetItem()
        {
            Assets = new List<AssetItem>();
        }

        public AssetItem(string name, AssetType type)
        {
            GroupName = name;
            AssetType = type;
            Assets = new List<AssetItem>();
        }

        public void Add(AssetItem assetItem)
        {
            Assets.Add(assetItem);
        }
    }

    private AddressableConfigs configs;

    private Dictionary<string, AssetItem> allAssetItem =
        new Dictionary<string, AssetItem>();

    private Dictionary<AssetType, List<AssetItem>> assetItemDic =
        new Dictionary<AssetType, List<AssetItem>>();

    private Dictionary<AssetType, List<AssetItem>> customAssetItemDic =
        new Dictionary<AssetType, List<AssetItem>>();

    private List<string> sceneList = new List<string>();

    // 缓存标记中存在的所有
    private List<SpriteAtlas> spriteAtlas = new List<SpriteAtlas>();

    private bool isRemote = false;

    private BundledAssetGroupSchema.BundleNamingStyle _localNamingStyle;
    private BundledAssetGroupSchema.BundleNamingStyle _remoteNamingStyle;

    private void OnEnable()
    {
        configs = target as AddressableConfigs;

        if (configs is { })
        {
            _localNamingStyle = configs.localNamingStyle;
            _remoteNamingStyle = configs.remoteNamingStyle;
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // 清除AssetBundles的标记
        if (GUILayout.Button("Clean Legacy AssetBundles"))
        {
            AssetDatabase.StartAssetEditing();
            CleanLegacyAssetBundles();
            AssetDatabase.StopAssetEditing();
        }

        // 清除Null和MissReference的AssetGroup
        // if (GUILayout.Button("Fix Null Or MissReference AssetGroup"))
        // {
        //     AssetDatabase.StartAssetEditing();
        //     ClearMissReferenceGroup();
        //     AssetDatabase.StopAssetEditing();
        // }

        // 解决Default AssetGroup缺失或丢失Schemas造成的问题
        // if (GUILayout.Button("Fix Default AssetGroup Schemas"))
        // {
        //     AssetDatabase.StartAssetEditing();
        //     FixDefaultAssetGroupSchemas();
        //     AssetDatabase.StopAssetEditing();
        // }

        // 移除所有自动生成的Asset Group
        if (GUILayout.Button("Remove All Generate AssetGroup"))
        {
            AssetDatabase.StartAssetEditing();
            RemoveAllGenerateAssetGroup();
            AssetDatabase.StopAssetEditing();
        }

        // 标记Addressable
        EditorGUILayout.BeginHorizontal();
        _localNamingStyle = (BundledAssetGroupSchema.BundleNamingStyle) EditorGUILayout.EnumPopup(_localNamingStyle);
        if (GUILayout.Button("标记 Addressable"))
        {
            AssetDatabase.StartAssetEditing();
            FixDefaultAssetGroupSchemas();
            ClearMissReferenceGroup();
            SignLocalAddressable();
            AssetDatabase.StopAssetEditing();
        }

        EditorGUILayout.EndHorizontal();

        // 标记远程Addressable
        EditorGUILayout.BeginHorizontal();
        _remoteNamingStyle = (BundledAssetGroupSchema.BundleNamingStyle) EditorGUILayout.EnumPopup(_remoteNamingStyle);
        if (GUILayout.Button("标记远程 Addressable"))
        {
            AssetDatabase.StartAssetEditing();
            FixDefaultAssetGroupSchemas();
            ClearMissReferenceGroup();
            SignRemoteAddressable();
            AssetDatabase.StopAssetEditing();
        }

        EditorGUILayout.EndHorizontal();

        // 设置BundleName
        // if (GUILayout.Button("BundleName Use Hash Of File"))
        // {
        //     AssetDatabase.StartAssetEditing();
        //     BundleNameUseHashOfFile();
        //     AssetDatabase.StopAssetEditing();
        // }
    }

    #region Clean Legacy AssetBundles

    private void CleanLegacyAssetBundles()
    {
        Debug.Log("开始Clean Legacy AssetBundles");
        string[] bundles = AssetDatabase.GetAllAssetBundleNames();
        EditorUtility.DisplayProgressBar("Clean Legacy AssetBundles", "", 0);
        for (int i = 0; i < bundles.Length; i++)
        {
            EditorUtility.DisplayProgressBar("Clean Legacy AssetBundles", bundles[i], (float) i / bundles.Length);
            AssetDatabase.RemoveAssetBundleName(bundles[i], true);
        }

        EditorUtility.ClearProgressBar();
        Debug.Log("Clean Legacy AssetBundles完毕");
    }

    #endregion

    #region Fix Null Or MissReference AssetGroup

    private void ClearMissReferenceGroup()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        string groupFolder = settings.GroupFolder;
        string[] files = System.IO.Directory.GetFiles(groupFolder, "*.asset");
        EditorUtility.DisplayProgressBar("Fix Null Or MissReference AssetGroup", "", 0);
        int index = 0;

        foreach (var item in files)
        {
            index++;
            EditorUtility.DisplayProgressBar("Fix Null Or MissReference AssetGroup", item,
                (float) index / files.Length);
            string groupName = System.IO.Path.GetFileNameWithoutExtension(item);

            // 过滤忽略AssetGroup
            if (IsIgnoreAssetGroup(groupName)) continue;

            AddressableAssetGroup group = settings.FindGroup(groupName);
            if (group == null || !group.ReadOnly) continue;
            if (group.entries.Count <= 0)
            {
                string temp = group.Name;
                RemoveSchema(group);
                settings.RemoveGroup(group);
                Debug.Log($"清理Group：{temp}");
            }
            else
            {
                List<AddressableAssetEntry> removeList = new List<AddressableAssetEntry>();
                foreach (var entry in group.entries)
                {
                    if (entry.MainAsset == null)
                    {
                        removeList.Add(entry);
                    }
                }

                foreach (var entry in removeList)
                {
                    group.RemoveAssetEntry(entry);
                }

                if (group.entries.Count <= 0)
                {
                    string temp = group.Name;
                    RemoveSchema(group);
                    settings.RemoveGroup(group);
                    Debug.Log($"清理Group：{temp}");
                }
            }
        }

        // AddressableAssetSettings.cs internal bool RemoveMissingGroupReferences()
        MethodInfo methodInfo = settings.GetType()
            .GetMethod("RemoveMissingGroupReferences", BindingFlags.NonPublic | BindingFlags.Instance);
        var b = (bool) methodInfo.Invoke(settings, null);

        // 刷新界面
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupRemoved, null, true, true);

        EditorUtility.ClearProgressBar();
    }

    #endregion

    #region Fix Default AssetGroup Schemas

    private void FixDefaultAssetGroupSchemas()
    {
        if (AddressableAssetSettingsDefaultObject.Settings == null)
        {
            AddressableAssetSettingsDefaultObject.Settings = AddressableAssetSettings.Create(
                AddressableAssetSettingsDefaultObject.kDefaultConfigFolder,
                AddressableAssetSettingsDefaultObject.kDefaultConfigAssetName, true, true);
        }

        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        AddressableAssetGroupTemplate groupTemplate =
            (AddressableAssetGroupTemplate) settings.GetGroupTemplateObject(0);
        AddressableAssetGroup group = settings.FindGroup("Default Local Group");
        if (group == null)
        {
            group = settings.CreateGroup("Default Local Group", true, false, true, groupTemplate.SchemaObjects);
        }
        else
        {
            foreach (var item in groupTemplate.SchemaObjects)
            {
                if (!group.HasSchema(item.GetType()))
                {
                    group.AddSchema(item);
                }
            }
        }

        if (group.CanBeSetAsDefault())
        {
            if (settings.DefaultGroup != group)
            {
                settings.DefaultGroup = group;
                Debug.Log("Set Default Local Group As Default.");
            }
        }
        else
        {
            Debug.LogWarning("Default Local Group Can Not Set As Default.");
        }
    }

    #endregion

    #region Remove All Generate AssetGroup

    /// <summary>
    /// 移除所有自动生成的AssetGroup
    /// </summary>
    private void RemoveAllGenerateAssetGroup()
    {
        Debug.Log("开始移除所有自动生成的AssetGroup");
        string groupFolder = AddressableAssetSettingsDefaultObject.Settings.GroupFolder;
        string[] files = System.IO.Directory.GetFiles(groupFolder, "*.asset");
        List<string> deleteList = new List<string>();
        EditorUtility.DisplayProgressBar("移除AssetGroup", "", 0);
        int index = 0;
        foreach (var item in files)
        {
            index++;
            string groupName = System.IO.Path.GetFileNameWithoutExtension(item);

            if (IsIgnoreAssetGroup(groupName)) continue;

            AddressableAssetGroup group = AddressableAssetSettingsDefaultObject.Settings.FindGroup(groupName);
            if (group == null || !group.ReadOnly) continue;

            RemoveSchema(group);

            EditorUtility.DisplayProgressBar("移除AssetGroup", group.name, (float) index / files.Length);
            // 直接删文件较快
            deleteList.Add(item);
            //AddressableAssetSettingsDefaultObject.Settings.RemoveGroup(group);
        }

        for (int i = 0; i < deleteList.Count; i++)
        {
            EditorUtility.DisplayProgressBar("Delete File", deleteList[i], (float) i / files.Length);
            System.IO.File.Delete(deleteList[i]);
        }

        EditorUtility.ClearProgressBar();
        Debug.Log("移除所有自动生成的AssetGroup完毕");

        // 关于Schemas的处理这个地方不对

        //string schemaFolder = groupFolder + "/Schemas";
        //if (System.IO.Directory.Exists(schemaFolder))
        //{
        //    System.IO.Directory.Delete(schemaFolder, true);
        //}

        AssetDatabase.Refresh();

        // 自动清理一遍
        ClearMissReferenceGroup();
    }

    #endregion

    #region Sign Addressable

    private void SignAddressable()
    {
        Debug.Log("Sign Addressable");
        spriteAtlas.Clear();

        // 获取基础定义数据
        sceneList.Clear();
        List<string> baseDefinitionData = new List<string>();

        # region 收集打包资源

        // 获取场景数据
        if (configs.SceneList != null && configs.SceneList.Count > 0)
        {
            for (int i = 0; i < configs.SceneList.Count; i++)
            {
                string path = AssetDatabase.GetAssetPath(configs.SceneList[i]);
                if (!baseDefinitionData.Contains(path))
                {
                    // 添加场景资源到缓存中
                    baseDefinitionData.Add(path);
                    sceneList.Add(configs.SceneList[i].name);
                }
            }
        }

        // 获取自定义文件夹数据
        if (configs.HotResPathList != null && configs.HotResPathList.Count > 0)
        {
            // 获取自定义文件夹中所有资源
            string[] guids = AssetDatabase.FindAssets("", configs.HotResPathList.ToArray());
            foreach (var item in guids)
            {
                // 通过GUID获取资源路径
                string path = AssetDatabase.GUIDToAssetPath(item);
                // 剔除文件夹
                if (AssetDatabase.IsValidFolder(path)) continue;
                if (!baseDefinitionData.Contains(path))
                {
                    // 添加自定义资源到缓存中
                    baseDefinitionData.Add(path);
                }
            }
        }

        #endregion

        // 验证是否需要进行资源标记
        if (baseDefinitionData.Count == 0)
        {
            Debug.Log("没有添加任何场景和资源文件路径，取消操作。");
            return;
        }

        // 获取所有依赖
        string[] dependencies = AssetDatabase.GetDependencies(baseDefinitionData.ToArray());

        #region 分析所有资源的资源类型

        // 分析所有的依赖
        allAssetItem.Clear();
        EditorUtility.DisplayProgressBar("查询依赖", "", 0);

        int length = dependencies.Length;
        for (int i = 0; i < length; i++)
        {
            string path = dependencies[i];

            EditorUtility.DisplayProgressBar("查询依赖", path, (float) i / length);
            // 获取资源类型
            AssetType assetType = GetAssetType(path);
            AssetItem assetItem = new AssetItem("", assetType);
            assetItem.AssetPath = path;
            // 判断是否为自定义资源
            assetItem.HotRes = IsCustomRes(path);
            allAssetItem.Add(path, assetItem);
        }

        EditorUtility.ClearProgressBar();

        #endregion

        assetItemDic.Clear();
        customAssetItemDic.Clear();
        List<AssetItem> assetItems = null;
        List<string> allPath = allAssetItem.Keys.ToList();

        EditorUtility.DisplayProgressBar("分析资源类型", "", 0);

        length = allPath.Count;
        for (int i = 0; i < length; i++)
        {
            EditorUtility.DisplayProgressBar("分析资源类型", allPath[i], (float) i / length);
            string groupName = "";
            string path = allPath[i];
            AssetItem assetItem = allAssetItem[path];
            switch (assetItem.AssetType)
            {
                case AssetType.None: // 未定义类型的资源
                    if (assetItem.HotRes)
                    {
                        assetItem.AssetType = AssetType.HotRes;
                        groupName = GetGroupName(path);
                        assetItem.GroupName = assetItem.AssetType.ToString() + "_" + groupName;
                        if (!customAssetItemDic.TryGetValue(assetItem.AssetType, out assetItems))
                        {
                            assetItems = new List<AssetItem>();
                            customAssetItemDic.Add(assetItem.AssetType, assetItems);
                        }

                        assetItems.Add(assetItem);

                        // 处理依赖关系
                        FixedDependencies(assetItem, path);
                    }

                    break;
                case AssetType.Packages:
                    // groupName = GetPackagesGroupName(path);
                    // Debug.LogWarning($"FromPackages:{path},GroupName:{groupName}");
                    // assetItem.GroupName = assetItem.AssetType.ToString() + "_" + groupName;
                    // Packages资源添加到Packages Group中
                    Debug.LogWarning($"FromPackages:{path},GroupName:Packages Group");
                    assetItem.GroupName = "Packages Group";
                    if (!assetItemDic.TryGetValue(assetItem.AssetType, out assetItems))
                    {
                        assetItems = new List<AssetItem>();
                        assetItemDic.Add(assetItem.AssetType, assetItems);
                    }

                    assetItems.Add(assetItem);
                    break;
                case AssetType.Config:
                case AssetType.Code:
                    groupName = GetGroupName(path);
                    assetItem.GroupName = assetItem.AssetType.ToString() + "_" + groupName;
                    if (!customAssetItemDic.TryGetValue(assetItem.AssetType, out assetItems))
                    {
                        assetItems = new List<AssetItem>();
                        customAssetItemDic.Add(assetItem.AssetType, assetItems);
                    }

                    assetItems.Add(assetItem);
                    break;
                case AssetType.Audio:
                case AssetType.Shader:
                case AssetType.Mat:
                case AssetType.Model:
                case AssetType.Prefab:
                case AssetType.Scene:
                    // case AssetType.SpriteAtlas:
                    groupName = GetGroupName(path);
                    assetItem.GroupName = assetItem.AssetType.ToString() + "_" + groupName;
                    if (IsCustomRes(path))
                    {
                        if (!customAssetItemDic.TryGetValue(assetItem.AssetType, out assetItems))
                        {
                            assetItems = new List<AssetItem>();
                            customAssetItemDic.Add(assetItem.AssetType, assetItems);
                        }
                    }
                    else
                    {
                        if (!assetItemDic.TryGetValue(assetItem.AssetType, out assetItems))
                        {
                            assetItems = new List<AssetItem>();
                            assetItemDic.Add(assetItem.AssetType, assetItems);
                        }
                    }

                    assetItems.Add(assetItem);

                    // 处理依赖关系
                    FixedDependencies(assetItem, path);
                    break;
                case AssetType.SpriteAtlas:
                    var sp = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
                    if (!spriteAtlas.Contains(sp)) spriteAtlas.Add(sp);

                    groupName = GetGroupName(path);
                    assetItem.GroupName = assetItem.AssetType.ToString() + "_" + groupName;
                    if (IsCustomRes(path))
                    {
                        if (!customAssetItemDic.TryGetValue(assetItem.AssetType, out assetItems))
                        {
                            assetItems = new List<AssetItem>();
                            customAssetItemDic.Add(assetItem.AssetType, assetItems);
                        }
                    }
                    else
                    {
                        if (!assetItemDic.TryGetValue(assetItem.AssetType, out assetItems))
                        {
                            assetItems = new List<AssetItem>();
                            assetItemDic.Add(assetItem.AssetType, assetItems);
                        }
                    }

                    assetItems.Add(assetItem);

                    // 处理依赖关系
                    SpriteAtlasFixedDependencies(assetItem, path);
                    break;
                case AssetType.SceneData:
                    groupName = GetGroupName(path);
                    string sceneName = assetItem.AssetPath;
                    int index = sceneName.LastIndexOfAny(new char[] {'\\', '/'});
                    sceneName = sceneName.Substring(0, index);
                    index = sceneName.LastIndexOfAny(new char[] {'\\', '/'});
                    sceneName = sceneName.Substring(index + 1, sceneName.Length - index - 1);
                    if (!sceneList.Contains(sceneName)) continue;
                    assetItem.GroupName = assetItem.AssetType.ToString() + "_" + sceneName + "_" + groupName;
                    if (!customAssetItemDic.TryGetValue(assetItem.AssetType, out assetItems))
                    {
                        assetItems = new List<AssetItem>();
                        customAssetItemDic.Add(assetItem.AssetType, assetItems);
                    }

                    assetItems.Add(assetItem);

                    // 处理依赖关系
                    FixedDependencies(assetItem, path);
                    break;
                case AssetType.Script:
                    break;
                default:
                    Debug.LogError($"未定义的资源类型:{path}");
                    break;
            }
        }

        EditorUtility.ClearProgressBar();

        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        var indexGroupTemplate = 0;
        if (isRemote) indexGroupTemplate = 1;
        AddressableAssetGroupTemplate groupTemplate =
            (AddressableAssetGroupTemplate) settings.GetGroupTemplateObject(indexGroupTemplate);

        // 验证label
        var labels = settings.GetLabels();
        if (!labels.Contains("Code")) settings.AddLabel("Code");
        if (!labels.Contains("Config")) settings.AddLabel("Config");

        // 这里的顺序是有讲究的...

        // 创建Packages类的Group
        CreateAssetGroup(assetItemDic, AssetType.Packages,
            "创建Asset的PackagesGroup", settings, groupTemplate);

        // 创建Config类的Group
        CreateAssetGroup(customAssetItemDic, AssetType.Config,
            "创建CustomAsset的ConfigGroup", settings, groupTemplate, true);

        // 创建Code类的Group
        CreateAssetGroup(customAssetItemDic, AssetType.Code,
            "创建CustomAsset的CodeGroup", settings, groupTemplate, true);

        // 创建Audio类的Group
        CreateAssetGroup(customAssetItemDic, AssetType.Audio,
            "创建CustomAsset的AudioGroup", settings, groupTemplate, true);
        CreateAssetGroup(assetItemDic, AssetType.Audio,
            "创建Asset的AudioGroup", settings, groupTemplate, true);

        // 创建Shader类的Group
        CreateAssetGroup(customAssetItemDic, AssetType.Shader,
            "创建CustomAsset的ShaderGroup", settings, groupTemplate, true);
        CreateAssetGroup(assetItemDic, AssetType.Shader,
            "创建Asset的ShaderGroup", settings, groupTemplate, true);

        // 创建Mat类的Group
        CreateAssetGroup(customAssetItemDic, AssetType.Mat,
            "创建CustomAsset的MatGroup", settings, groupTemplate, true);
        CreateAssetGroup(assetItemDic, AssetType.Mat,
            "创建Asset的MatGroup", settings, groupTemplate, true);

        // 创建Module类的Group
        CreateAssetGroup(customAssetItemDic, AssetType.Model,
            "创建CustomAsset的ModelGroup", settings, groupTemplate, true);
        CreateAssetGroup(assetItemDic, AssetType.Model,
            "创建Asset的ModelGroup", settings, groupTemplate, true);

        // 创建Prefab类的Group
        CreateAssetGroup(customAssetItemDic, AssetType.Prefab,
            "创建CustomAsset的PrefabGroup", settings, groupTemplate, true);
        CreateAssetGroup(assetItemDic, AssetType.Prefab,
            "创建Asset的PrefabGroup", settings, groupTemplate, true);

        // 创建SpriteAtlas类的Group
        CreateAssetGroup(customAssetItemDic, AssetType.SpriteAtlas,
            "创建CustomAsset的SpriteAtlasGroup", settings, groupTemplate);
        CreateAssetGroup(assetItemDic, AssetType.SpriteAtlas,
            "创建Asset的SpriteAtlasGroup", settings, groupTemplate);

        // 创建HotRsa None类的Group
        CreateAssetGroup(customAssetItemDic, AssetType.HotRes,
            "创建CustomAsset的HotResGroup", settings, groupTemplate, true);

        // 创建SceneData类的Group
        CreateAssetGroup(customAssetItemDic, AssetType.SceneData,
            "创建CustomAsset的SceneDataGroup", settings, groupTemplate);

        // 创建Scene类的Group
        CreateAssetGroup(customAssetItemDic, AssetType.Scene,
            "创建CustomAsset的SceneGroup", settings, groupTemplate, true);
        CreateAssetGroup(assetItemDic, AssetType.Scene,
            "创建asset的SceneGroup", settings, groupTemplate, true);
    }

    private void SignLocalAddressable()
    {
        isRemote = false;

        // 验证是否存在一个模板
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

        if (settings.GroupTemplateObjects.Count < 1)
        {
            Debug.LogWarning("当前不存在GroupTemplate，自动创建GroupTemplate");
            settings.CreateAndAddGroupTemplate("Packed Assets", "Pack remote assets into asset bundles.",
                new Type[] {typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema)});
        }

        var template = (AddressableAssetGroupTemplate) settings.GetGroupTemplateObject(0);
        var updateGroupSchema =
            template.GetSchemaByType(typeof(ContentUpdateGroupSchema)) as ContentUpdateGroupSchema;
        if (updateGroupSchema != null) updateGroupSchema.StaticContent = true;

        BundleNameUseHashOfFile(_localNamingStyle);
        
        configs.localNamingStyle = _localNamingStyle;

        SignAddressable();
    }

    private void SignRemoteAddressable()
    {
        isRemote = true;

        // 验证是否存在两个模板
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

        if (settings.GroupTemplateObjects.Count < 1)
        {
            Debug.LogWarning("当前不存在GroupTemplate，自动创建GroupTemplate");
            settings.CreateAndAddGroupTemplate("Packed Assets", "Pack remote assets into asset bundles.",
                new Type[] {typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema)});
            settings.CreateAndAddGroupTemplate("Packed Assets Remote", "Pack remote assets into asset bundles.",
                new Type[] {typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema)});
        }
        else if (settings.GroupTemplateObjects.Count < 2)
        {
            Debug.LogWarning("当前不存在GroupTemplate，自动创建GroupTemplate");
            settings.CreateAndAddGroupTemplate("Packed Assets Remote", "Pack remote assets into asset bundles.",
                new Type[] {typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema)});
        }

        var template = (AddressableAssetGroupTemplate) settings.GetGroupTemplateObject(1);
        var updateGroupSchema =
            template.GetSchemaByType(typeof(ContentUpdateGroupSchema)) as ContentUpdateGroupSchema;

        if (updateGroupSchema != null) updateGroupSchema.StaticContent = false;

        var assetGroupSchema = template.GetSchemaByType(typeof(BundledAssetGroupSchema)) as BundledAssetGroupSchema;
        if (assetGroupSchema != null)
        {
            assetGroupSchema.BuildPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteBuildPath);
            assetGroupSchema.LoadPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteLoadPath);
        }
        
        BundleNameUseHashOfFile(_remoteNamingStyle);
        configs.remoteNamingStyle = _remoteNamingStyle;

        SignAddressable();

        isRemote = false;
    }

    #endregion

    #region BundleName Use Hash Of File

    private void BundleNameUseHashOfFile(BundledAssetGroupSchema.BundleNamingStyle style)
    {
        Debug.Log("Bundle Name Use Hash Of File Start");
        string groupFolder = AddressableAssetSettingsDefaultObject.Settings.GroupFolder;
        string[] files = System.IO.Directory.GetFiles(groupFolder, "*.asset");
        List<string> deleteList = new List<string>();
        EditorUtility.DisplayProgressBar("Bundle Name Use Hash Of File", "", 0);
        int index = 0;
        foreach (var item in files)
        {
            index++;
            string groupName = System.IO.Path.GetFileNameWithoutExtension(item);

            if (IsIgnoreAssetGroup(groupName)) continue;

            AddressableAssetGroup group = AddressableAssetSettingsDefaultObject.Settings.FindGroup(groupName);
            // if (group == null || !group.ReadOnly) continue;
            if (group == null) continue;

            foreach (var schema in group.Schemas)
            {
                if (schema.GetType() != typeof(BundledAssetGroupSchema)) continue;

                var bundledAssetGroupSchema = schema as BundledAssetGroupSchema;
                bundledAssetGroupSchema.BundleNaming = style;
            }

            EditorUtility.DisplayProgressBar("Bundle Name Use Hash Of File", group.name, (float) index / files.Length);
        }

        EditorUtility.ClearProgressBar();
        Debug.Log("Bundle Name Use Hash Of File End");

        AssetDatabase.Refresh();
    }

    #endregion

    private void CreateAssetGroup(Dictionary<AssetType, List<AssetItem>> source, AssetType assetType, string title,
        AddressableAssetSettings settings, AddressableAssetGroupTemplate groupTemplate, bool simplify = false)
    {
        if (source.TryGetValue(assetType, out List<AssetItem> items))
        {
            EditorUtility.DisplayProgressBar(title, "", 0);
            int index = 0;
            int count = items.Count;
            foreach (var item in items)
            {
                string path = item.AssetPath;
                string guid = AssetDatabase.AssetPathToGUID(path);
                string name = System.IO.Path.GetFileNameWithoutExtension(item.AssetPath);

                // 更新进度
                EditorUtility.DisplayProgressBar(title, item.AssetPath, (float) index / count);
                if (simplify)
                {
                    if (assetType == AssetType.Config)
                        CreateOrUpdateConfigAssetGroup(settings, groupTemplate.SchemaObjects, item.GroupName, guid,
                            name);
                    else if (assetType == AssetType.Code)
                        CreateOrUpdateCodeAssetGroup(settings, groupTemplate.SchemaObjects, item.GroupName, guid,
                            name);
                    else CreateOrUpdateAssetGroup(settings, groupTemplate.SchemaObjects, item.GroupName, guid, name);
                }
                else
                {
                    if (assetType == AssetType.Config)
                        CreateOrUpdateConfigAssetGroup(settings, groupTemplate.SchemaObjects, item.GroupName, guid,
                            name);
                    else if (assetType == AssetType.Code)
                        CreateOrUpdateCodeAssetGroup(settings, groupTemplate.SchemaObjects, item.GroupName, guid,
                            name);
                    else CreateOrUpdateAssetGroup(settings, groupTemplate.SchemaObjects, item.GroupName, guid, name);
                }

                foreach (var i in item.Assets)
                {
                    if (i.Address) continue;
                    path = i.AssetPath;
                    guid = AssetDatabase.AssetPathToGUID(path);
                    name = System.IO.Path.GetFileNameWithoutExtension(i.AssetPath);

                    if (path.ToLower().EndsWith(".dll") || path.ToLower().EndsWith(".jslib"))
                        continue; // 忽略dll和jslib

                    if (assetType == AssetType.Config)
                        CreateOrUpdateConfigAssetGroup(settings, groupTemplate.SchemaObjects, item.GroupName, guid,
                            name);
                    else CreateOrUpdateAssetGroup(settings, groupTemplate.SchemaObjects, item.GroupName, guid, name);
                    i.Address = true;
                }

                index++;
            }

            EditorUtility.ClearProgressBar();
        }
    }

    private AddressableAssetEntry CreateOrUpdateAssetGroup(AddressableAssetSettings settings,
        List<AddressableAssetGroupSchema> schemas, string groupName, string guid, string name = "")
    {
        AddressableAssetGroup group = settings.FindGroup(g =>
            g != null && (g.Name == groupName || g.name.ToLower() == groupName.ToLower()));
        if (group == null)
        {
            group = settings.CreateGroup(groupName, false, true, true, schemas);
        }

        AddressableAssetEntry entry = group.GetAssetEntry(guid);
        if (entry == null)
        {
            entry = settings.CreateOrMoveEntry(guid, group, true);
            if (!string.IsNullOrEmpty(name)) entry.SetAddress(name);
            entry.SetLabel("default", true);
            return entry;
        }
        else
        {
            if (!entry.labels.Contains("default")) entry.SetLabel("default", true);
            return entry;
        }
    }

    private AddressableAssetEntry CreateOrUpdateConfigAssetGroup(AddressableAssetSettings settings,
        List<AddressableAssetGroupSchema> schemas, string groupName, string guid, string name = "")
    {
        AddressableAssetGroup group = settings.FindGroup(g =>
            g != null && (g.Name == groupName || g.name.ToLower() == groupName.ToLower()));
        if (group == null)
        {
            group = settings.CreateGroup(groupName, false, false, true, schemas);
        }

        AddressableAssetEntry entry = group.GetAssetEntry(guid);
        if (entry == null)
        {
            entry = settings.CreateOrMoveEntry(guid, group, false);
            if (!string.IsNullOrEmpty(name)) entry.SetAddress(name);
            entry.SetLabel("default", true);
            entry.SetLabel("Config", true);
            return entry;
        }
        else
        {
            if (!entry.labels.Contains("default")) entry.SetLabel("default", true);
            if (!entry.labels.Contains("Config")) entry.SetLabel("Config", true);
            return entry;
        }
    }

    private AddressableAssetEntry CreateOrUpdateCodeAssetGroup(AddressableAssetSettings settings,
        List<AddressableAssetGroupSchema> schemas, string groupName, string guid, string name = "")
    {
        AddressableAssetGroup group = settings.FindGroup(g =>
            g != null && (g.Name == groupName || g.name.ToLower() == groupName.ToLower()));
        if (group == null)
        {
            group = settings.CreateGroup(groupName, false, false, true, schemas);
        }

        AddressableAssetEntry entry = group.GetAssetEntry(guid);
        if (entry == null)
        {
            entry = settings.CreateOrMoveEntry(guid, group, false);
            if (!string.IsNullOrEmpty(name)) entry.SetAddress(name);
            entry.SetLabel("default", true);
            entry.SetLabel("Code", true);
            return entry;
        }
        else
        {
            if (!entry.labels.Contains("default")) entry.SetLabel("default", true);
            if (!entry.labels.Contains("Code")) entry.SetLabel("Code", true);
            return entry;
        }
    }

    /// <summary>
    /// 判断当前资源的类型
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private AssetType GetAssetType(string path)
    {
        // 判断Object的HideFlags，并设置忽略
        var o = AssetDatabase.LoadAssetAtPath<Object>(path);
        if (o == null) return AssetType.Script;

        if (o.hideFlags == HideFlags.DontSave || o.hideFlags == HideFlags.DontSaveInBuild ||
            o.hideFlags == HideFlags.HideAndDontSave) return AssetType.Script;

        if (path.StartsWith("Packages/"))
        {
            return AssetType.Packages;
        }

        // 需要过滤的资源类型可以放在这里
        if (path.EndsWith(".cs") || path.ToLower().EndsWith(".dll") || path.ToLower().EndsWith(".jslib"))
            return AssetType.Script;

        if (path.EndsWith(".unity")) return AssetType.Scene;

        if (path.EndsWith(".prefab")) return AssetType.Prefab;

        if (path.EndsWith(".mat")) return AssetType.Mat;

        if (path.EndsWith(".shader")) return AssetType.Shader;

        if (path.EndsWith(".spriteatlas")) return AssetType.SpriteAtlas;

        // 加载资源
        Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
        if (obj == null) return AssetType.None;

        if (obj is AudioClip) return AssetType.Audio;

        // 获取预制体类型
        PrefabAssetType prefabAssetType = PrefabUtility.GetPrefabAssetType(obj);
        if (prefabAssetType == PrefabAssetType.MissingAsset)
        {
            Debug.LogWarning($"Prefab MissingAsset:{path} ");
            return AssetType.None;
        }

        if (prefabAssetType == PrefabAssetType.Model) return AssetType.Model;
        if (prefabAssetType == PrefabAssetType.Regular || prefabAssetType == PrefabAssetType.Variant)
            return AssetType.Prefab;

        if (prefabAssetType == PrefabAssetType.NotAPrefab)
        {
            if (IsCustomRes(path))
            {
                // 添加限定字符'\''/'避免文件夹名称刚好包含Code、Config、Scene、Scenes等造成误解析
                if (path.Contains("\\Code\\") || path.Contains("/Code/")) return AssetType.Code;
                if (path.Contains("\\Config\\") || path.Contains("/Config/")) return AssetType.Config;
                if (path.Contains("\\Scene\\") || path.Contains("/Scene/") ||
                    path.Contains("\\Scenes\\") || path.Contains("/Scenes/"))
                    return AssetType.SceneData;
            }

            Debug.LogWarning($"未定义的资源：{path}");
            return AssetType.None;
        }

        Debug.LogError($"未定义的资源：{path}");
        return AssetType.None;
    }

    private void FixedDependencies(AssetItem assetItem, string path)
    {
        string[] dependencies = AssetDatabase.GetDependencies(path);
        foreach (var item in dependencies)
        {
            if (item == path) continue; // 依赖中包含自身，不再做处理
            AssetType assetType = GetAssetType(item);
            if (assetType == AssetType.None) // 处理那些未定义的类型
            {
                if (allAssetItem.TryGetValue(item, out AssetItem temp))
                {
                    if (temp.HotRes) return; // HotRes资源不需要被添加到其它AssetGroup中
                    if (string.IsNullOrEmpty(temp.GroupName))
                        temp.GroupName = assetItem.GroupName; // 未分配Group的加入到当前Group中
                    assetItem.Add(temp);
                }
                else
                {
                    Debug.LogError($"未知的资源:{item}");
                }
            }
        }
    }

    private void SpriteAtlasFixedDependencies(AssetItem assetItem, string path)
    {
        string[] dependencies = AssetDatabase.GetDependencies(path);
        foreach (var item in dependencies)
        {
            if (item == path) continue; // 依赖中包含自身，不再做处理
            AssetType assetType = GetAssetType(item);
            if (assetType == AssetType.None) // 处理那些未定义的类型
            {
                if (allAssetItem.TryGetValue(item, out AssetItem temp))
                {
                    // if (temp.HotRes) return; // HotRes资源不需要被添加到其它AssetGroup中
                    // if (string.IsNullOrEmpty(temp.GroupName))
                    //     temp.GroupName = assetItem.GroupName; // 未分配Group的加入到当前Group中

                    // 图集的所有引用放在图集中
                    temp.GroupName = assetItem.GroupName;
                    assetItem.Add(temp);
                }
                else
                {
                    Debug.LogError($"未知的资源:{item}");
                }
            }
        }
    }

    private bool IsIgnoreAssetGroup(string groupName)
    {
        return configs.IgnoreAssetGroupList.Contains(groupName);
    }

    private bool IsCustomRes(string path)
    {
        foreach (var item in configs.HotResPathList)
        {
            if (path.StartsWith(item)) return true;
        }

        return false;
    }

    private string GetPackagesGroupName(string path)
    {
        // TODO:这里目前用的文件夹名字，以后改为Package的名字
        string str = path.Substring(9, path.Length - 9);
        int end = str.IndexOf('/');
        if (end == -1) return "Packages";
        str = str.Substring(0, end);
        return str;
    }

    private string GetGroupName(string path)
    {
        string str = System.IO.Path.GetFileNameWithoutExtension(path);
        return str;
    }

    /// <summary>
    /// 清理Schemas
    /// </summary>
    /// <param name="group"></param>
    private void RemoveSchema(AddressableAssetGroup group)
    {
        var schemas = group.Schemas;
        if (schemas != null)
        {
            for (int i = 0; i < 3; i++)
            {
                switch (i)
                {
                    case 0:
                        if (group.HasSchema<PlayerDataGroupSchema>()) group.RemoveSchema<PlayerDataGroupSchema>();
                        break;
                    case 1:
                        if (group.HasSchema<BundledAssetGroupSchema>()) group.RemoveSchema<BundledAssetGroupSchema>();
                        break;
                    case 2:
                        if (group.HasSchema<ContentUpdateGroupSchema>()) group.RemoveSchema<ContentUpdateGroupSchema>();
                        break;
                }
            }
        }
    }
}