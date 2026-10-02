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
            DrawCatToolsHeader();

            EditorGUILayout.Space(4);

            DrawDefaultInspector();

            DrawComponentInspector();
        }

        /// <summary>
        /// 绘制统一的标题头。
        /// 注意:基类 <see cref="UnityEditor.Editor"/> 自己已经有一个 <c>DrawHeader()</c>,
        /// 所以这里必须换个名字,否则会触发 CS0108「隐藏继承成员」警告。
        /// </summary>
        private void DrawCatToolsHeader()
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
