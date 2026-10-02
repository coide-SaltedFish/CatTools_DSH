using nadena.dev.ndmf;

namespace SereinFish.CatTools.Editor.Passes
{
    /// <summary>
    /// 在 <see cref="BuildPhase.Resolving"/> 阶段建立本次构建的 CatTools 组件清单。
    ///
    /// <para>
    /// 放在最早的可用阶段,是为了让后续所有功能 Pass(也包括其他插件的 Pass)
    /// 都能直接通过 <see cref="CatToolsComponentRegistry.Get"/> 拿到清单,
    /// 而不必各自重新遍历一次 Avatar 层级。
    /// </para>
    /// </summary>
    internal sealed class CollectComponentsPass : CatToolsPass<CollectComponentsPass>
    {
        public override string QualifiedName => "sereinfish.cat.tools.collect-components";

        public override string DisplayName => "CatTools: 收集组件";

        protected override void Execute(BuildContext context)
        {
            // 只建立清单,不做任何修改。后续功能 Pass 会消费它。
            CatToolsComponentRegistry.Get(context);
        }
    }
}
