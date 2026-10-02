# 更新日志

本文件记录 CatTools 工具箱所有值得注意的改动。

格式参考 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/),
版本号遵循 [语义化版本](https://semver.org/lang/zh-CN/)。

## [0.1.0] - 未发布

初始化框架骨架,尚无具体功能组件。

### 变更

- 许可改为 **GNU GPL v3**(`GPL-3.0-or-later`),替换最初的 MIT。
- 作者署名统一为「一只大猫条」。
- NDMF 依赖下限由 `1.14.8` 放宽到 `1.13.1`:已在 Unity 2022.3.22f1 +
  NDMF 1.13.1 + VRChat SDK 3.10.4 环境下**实际编译通过**,确认所用 API 在该版本已存在。

### 新增

- VPM/UPM 包结构:仓库根目录即包根目录,含 `package.json`(包名 `sereinfish.cat.tools`)。
- `SereinFish.CatTools.Runtime` 程序集:存放给构建器读取的声明式组件。
- `SereinFish.CatTools.Editor` 程序集:存放全部构建期逻辑,限定 `Editor` 平台。
- `CatToolsPlugin`:通过 `[assembly: ExportsPlugin]` 注册到 NDMF,
  限定名 `sereinfish.cat.tools`,并声明构建阶段管线。
- `CatToolsPass<T>`:所有功能 Pass 的基类,内置 `FindComponents<T>` / `FindAllComponents` 工具。
- `CatToolsComponentRegistry` 与 `CollectComponentsPass`:
  在 `Resolving` 阶段建立本次构建的组件清单并缓存,供后续 Pass 复用。
- `CleanupComponentsPass`:在 `PlatformFinish` 阶段清除所有 CatTools 组件,
  避免它们出现在上传的 Avatar 上。
- `CatToolsComponent`:所有 CatTools 组件的抽象基类。
- `CatToolsComponentEditor`:所有组件 Inspector 的基类,提供统一标题头。
- `CatToolsConstants`:插件限定名、显示名、菜单根路径等全局常量。
- `AssemblyInfo.cs`:`InternalsVisibleTo` 让 Editor 程序集可访问 Runtime 的 internal 成员。
- Editor 程序集对 `com.vrchat.avatars` 与 `nadena.dev.modular-avatar` 声明了
  `versionDefines`(`CATTOLS_VRCSDK3_AVATARS` / `CATTOLS_MODULAR_AVATAR`),
  便于后续写条件编译。
