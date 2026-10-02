using nadena.dev.ndmf;

namespace SereinFish.CatTools.Editor.Passes
{
    /// <summary>
    /// 在 <see cref="BuildPhase.PlatformFinish"/> 阶段移除 Avatar 上所有 CatTools 组件。
    ///
    /// <para>
    /// CatTools 组件对构建器来说只是「说明书」,本身不该出现在上传的 Avatar 里
    /// —— 否则上传后会变成缺失脚本。这里统一清理,新增组件时就不必各自记得删掉自己。
    /// </para>
    /// </summary>
    internal sealed class CleanupComponentsPass : CatToolsPass<CleanupComponentsPass>
    {
        public override string QualifiedName => "sereinfish.cat.tools.cleanup-components";

        public override string DisplayName => "CatTools: 清理组件";

        protected override void Execute(BuildContext context)
        {
            // GetComponentsInChildren 返回的是快照数组,因此可以在遍历中安全地销毁。
            foreach (var component in FindAllComponents(context))
            {
                UnityEngine.Object.DestroyImmediate(component);
            }
        }
    }
}
