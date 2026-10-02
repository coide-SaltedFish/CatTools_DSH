using nadena.dev.ndmf;
using SereinFish.CatTools.Components;

namespace SereinFish.CatTools.Editor.Passes
{
    /// <summary>
    /// CatTools 所有构建 Pass 的基类,提供查找 CatTools 组件的公共工具。
    ///
    /// <para>新增一个功能 Pass 的写法:</para>
    /// <code>
    /// internal sealed class MyFeaturePass : CatToolsPass&lt;MyFeaturePass&gt;
    /// {
    ///     public override string QualifiedName =&gt; "sereinfish.cat.tools.my-feature";
    ///     public override string DisplayName =&gt; "CatTools: 我的功能";
    ///
    ///     protected override void Execute(BuildContext context)
    ///     {
    ///         foreach (var c in FindComponents&lt;MyComponent&gt;(context))
    ///         {
    ///             // 在这里把 c 的声明应用到 Avatar 上
    ///         }
    ///     }
    /// }
    /// </code>
    /// <para>写完后在 <see cref="CatToolsPlugin.Configure"/> 里注册即可。</para>
    /// </summary>
    /// <typeparam name="T">派生类自身(CRTP 写法,NDMF 要求 Pass 是单例)。</typeparam>
    public abstract class CatToolsPass<T> : Pass<T> where T : CatToolsPass<T>, new()
    {
        /// <summary>
        /// 查找 Avatar 下所有指定类型的 CatTools 组件,包含挂在未激活对象上的。
        /// </summary>
        protected static TComponent[] FindComponents<TComponent>(BuildContext context)
            where TComponent : CatToolsComponent
        {
            return context.AvatarRootObject.GetComponentsInChildren<TComponent>(true);
        }

        /// <summary>
        /// 查找 Avatar 下所有 CatTools 组件,包含挂在未激活对象上的。
        /// </summary>
        protected static CatToolsComponent[] FindAllComponents(BuildContext context)
        {
            return context.AvatarRootObject.GetComponentsInChildren<CatToolsComponent>(true);
        }
    }
}
