namespace SereinFish.CatTools
{
    /// <summary>
    /// CatTools 工具箱的全局常量。
    /// </summary>
    public static class CatToolsConstants
    {
        /// <summary>
        /// NDMF 插件的限定名。这是本插件在 NDMF 中的唯一身份标识,
        /// 一旦发布就必须保持稳定——其他插件会用它来声明前后顺序约束。
        /// </summary>
        public const string PluginQualifiedName = "sereinfish.cat.tools";

        /// <summary>
        /// 展示给用户的插件名。
        /// </summary>
        public const string PluginDisplayName = "CatTools 工具箱";

        /// <summary>
        /// 所有 CatTools 组件在 Add Component 菜单下的根路径。
        /// </summary>
        public const string ComponentMenuRoot = "CatTools/";
    }
}
