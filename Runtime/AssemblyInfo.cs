using System.Runtime.CompilerServices;

// CatTools 的 Runtime 程序集只放「声明式组件」,所有构建期逻辑都在 Editor 程序集里。
// 允许 Editor 访问 Runtime 的 internal 成员,方便两边共享内部工具而不必公开 API。
[assembly: InternalsVisibleTo("SereinFish.CatTools.Editor")]
