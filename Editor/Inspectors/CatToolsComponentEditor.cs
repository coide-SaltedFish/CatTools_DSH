using SereinFish.CatTools.Components;
using UnityEditor;
using UnityEngine;

namespace SereinFish.CatTools.Editor.Inspectors
{
    /// <summary>
    /// 所有 CatTools 组件 Inspector 的基类。
    ///
    /// <para>
    /// 它负责绘制统一的标题头,子类只需重写 <see cref="DrawComponentInspector"/>
    /// 来补充自己的 UI,就能获得一致的观感。
    /// </para>
    ///
    /// <para>新增一个 Inspector 的写法:</para>
    /// <code>
    /// [CustomEditor(typeof(MyComponent))]
    /// internal sealed class MyComponentEditor : CatToolsComponentEditor
    /// {
    ///     protected override void DrawComponentInspector()
    ///     {
    ///         EditorGUILayout.HelpBox("说明文字", MessageType.Info);
    ///     }
    /// }
    /// </code>
    /// </summary>
    public abstract class CatToolsComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawHeader();

            EditorGUILayout.Space(4);

            DrawDefaultInspector();

            DrawComponentInspector();
        }

        private void DrawHeader()
        {
            var component = target as CatToolsComponent;
            var title = component != null ? component.DisplayName : target.GetType().Name;

            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        /// <summary>
        /// 子类在这里绘制组件自己的额外 UI。默认什么都不画。
        /// </summary>
        protected virtual void DrawComponentInspector()
        {
        }
    }
}
