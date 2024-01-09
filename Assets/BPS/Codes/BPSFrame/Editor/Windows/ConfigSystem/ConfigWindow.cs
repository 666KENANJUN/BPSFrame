using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BPSFrame.Editor
{
    public class ConfigWindow : OdinEditorWindow
    {
        public int num;
        public ConfigWindowSetting setting;     // 通过面板拖拽赋值
        public VisualTreeAsset editorUIAsset;   // 通过面板拖拽赋值
        [MenuItem("BPSFrame/配置窗口")]
        public static void ShowExample()
        {
            ConfigWindow wnd = GetWindow<ConfigWindow>();
            wnd.titleContent = new GUIContent("ConfigWindow");
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            root.Add(new IMGUIContainer(base.OnImGUI));
            root.Add(editorUIAsset.Instantiate());
        }

        protected override object GetTarget()
        {
            return setting;
        }
    }
}
