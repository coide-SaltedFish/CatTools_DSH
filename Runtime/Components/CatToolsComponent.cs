using nadena.dev.ndmf;
using UnityEngine;

namespace SereinFish.CatTools.Components
{
    /// <summary>
    /// 所有 CatTools 组件的抽象基类。
    ///
    /// <para>
    /// CatTools 组件是「声明式」的:它只描述用户想要的结果,不直接修改模型。
    /// 真正的改动由 Editor 程序集中的 NDMF 构建 Pass 在构建期应用,
    /// 应用完毕后组件本身会被移除,因此不会出现在上传的 Avatar 上,也不会污染原始 prefab。
    /// </para>
    ///
    /// <para>新增一个组件的步骤:</para>
    /// <list type="number">
    /// <item>在 <c>Runtime/Components/</c> 下继承本类;</item>
    /// <item>加上 <c>[AddComponentMenu(CatToolsConstants.ComponentMenuRoot + "分类/组件名")]</c>,
    ///       以及按需的 <c>[DisallowMultipleComponent]</c> / <c>[HelpURL]</c>;</item>
    /// <item>在 <c>Editor/Inspectors/</c> 下写对应的 Editor(可继承 <c>CatToolsComponentEditor</c>);</item>
    /// <item>在 <c>Editor/Passes/</c> 下实现处理它的 Pass,并在 <c>CatToolsPlugin.Configure()</c> 中注册。</item>
    /// </list>
    /// <remarks>
    /// 这里实现 <see cref="INDMFEditorOnly"/>,是向 NDMF 与 VRChat SDK 声明
    /// 「这个组件只在编辑器里有意义」的标准做法。
    /// 该接口位于 NDMF 的 <b>runtime</b> 程序集,所以在没安装 VRChat SDK 的工程里同样能编译通过
    /// —— 它在缺少 VRC.SDKBase 时会退化成一个空接口。
    /// </remarks>
    public abstract class CatToolsComponent : MonoBehaviour, INDMFEditorOnly
    {
        /// <summary>
        /// 在 Inspector 标题中显示的名称,默认使用类型名。
        /// 子类可以重写为更友好的中文名。
        /// </summary>
        public virtual string DisplayName => GetType().Name;
    }
}
