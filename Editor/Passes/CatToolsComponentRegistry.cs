using System;
using System.Collections.Generic;
using nadena.dev.ndmf;
using SereinFish.CatTools.Components;

namespace SereinFish.CatTools.Editor.Passes
{
    /// <summary>
    /// 一次构建中收集到的、全部 CatTools 组件的清单。
    ///
    /// <para>
    /// 清单在 <see cref="BuildPhase.Resolving"/> 阶段由 <see cref="CollectComponentsPass"/> 建立,
    /// 之后任何 Pass 都可以通过 <see cref="Get"/> 拿到同一份实例
    /// —— NDMF 的 <c>BuildContext.GetState</c> 会按类型缓存,所以不会重复遍历层级。
    /// </para>
    /// </summary>
    public sealed class CatToolsComponentRegistry
    {
        private readonly List<CatToolsComponent> _components = new List<CatToolsComponent>();

        internal CatToolsComponentRegistry(BuildContext context)
        {
            foreach (var component in context.AvatarRootObject.GetComponentsInChildren<CatToolsComponent>(true))
            {
                // 缺失脚本的占位对象会是 null,直接跳过。
                if (component == null) continue;

                _components.Add(component);
            }
        }

        /// <summary>
        /// 本次构建中收集到的全部 CatTools 组件。
        /// </summary>
        public IReadOnlyList<CatToolsComponent> Components => _components;

        /// <summary>
        /// 本次构建中收集到的组件总数。
        /// </summary>
        public int Count => _components.Count;

        /// <summary>
        /// 取出指定类型的所有组件(包含其子类)。
        /// </summary>
        public IEnumerable<T> OfType<T>() where T : CatToolsComponent
        {
            foreach (var component in _components)
            {
                // 组件可能在收集之后被销毁(例如它挂在 tag 为 EditorOnly 的对象上,
                // 而 NDMF 的 RemoveEditorOnlyPass 会删掉整个对象),这里跳过 Unity 的「假 null」。
                if (component == null) continue;

                if (component is T typed)
                {
                    yield return typed;
                }
            }
        }

        /// <summary>
        /// 取得本次构建的组件清单;若尚未建立则先建立。
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="context"/> 为 null。</exception>
        public static CatToolsComponentRegistry Get(BuildContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return context.GetState(ctx => new CatToolsComponentRegistry(ctx));
        }
    }
}
