using nadena.dev.ndmf;
using SereinFish.CatTools.Editor.Passes;
using UnityEngine;

// 把本插件注册到 NDMF。NDMF 启动时会扫描所有程序集上的这个特性。
[assembly: ExportsPlugin(typeof(SereinFish.CatTools.Editor.CatToolsPlugin))]

namespace SereinFish.CatTools.Editor
{
    /// <summary>
    /// CatTools 工具箱的 NDMF 插件入口。
    ///
    /// <para>
    /// 这里是整个工具箱的「接线板」:每个功能都实现为一个独立的 <see cref="Pass{T}"/>,
    /// 然后在这里挂到合适的构建阶段上。构建阶段按以下顺序执行:
    /// </para>
    /// <list type="bullet">
    /// <item><description><c>FirstChance</c> — 平台初始化之前,留给「替换整个 Avatar」这类极端操作</description></item>
    /// <item><description><c>PlatformInit</c> — 平台后端初始化</description></item>
    /// <item><description><c>Resolving</c> — 早期解析,此时 Avatar 结构尚未被大幅修改</description></item>
    /// <item><description><c>Generating</c> — 生成供后续插件(例如 Modular Avatar)使用的组件</description></item>
    /// <item><description><c>Transforming</c> — 通用的 Avatar 变换,绝大多数功能放这里</description></item>
    /// <item><description><c>Optimizing</c> — 纯粹的性能优化,需要跑得很晚</description></item>
    /// <item><description><c>PlatformFinish</c> — 平台相关的收尾与校验</description></item>
    /// </list>
    /// </summary>
    internal sealed class CatToolsPlugin : Plugin<CatToolsPlugin>
    {
        public override string QualifiedName => CatToolsConstants.PluginQualifiedName;

        public override string DisplayName => CatToolsConstants.PluginDisplayName;

        /// <summary>
        /// NDMF 在构建报告、参数占用显示等 UI 中使用的主题色。
        /// </summary>
        public override Color? ThemeColor => new Color(0.96f, 0.72f, 0.35f);

        protected override void Configure()
        {
            // Resolving:尽早收集所有 CatTools 组件,供后续各功能的 Pass 复用。
            InPhase(BuildPhase.Resolving)
                .Run(CollectComponentsPass.Instance);

            // ↓↓↓ 以后每个新功能在这里挂一个 Pass ↓↓↓
            //
            // InPhase(BuildPhase.Transforming)
            //     .Run(MyFeaturePass.Instance);
            //
            // 同一个阶段内多次调用 InPhase 会按声明顺序串行执行;
            // 需要跨插件排序时,用 .Run(...).BeforePlugin("别的插件限定名")。

            // PlatformFinish:收尾之前清掉所有 CatTools 组件,
            // 保证它们不会进入最终上传的 Avatar。
            InPhase(BuildPhase.PlatformFinish)
                .Run(CleanupComponentsPass.Instance);
        }
    }
}
