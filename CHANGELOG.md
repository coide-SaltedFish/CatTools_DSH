# 更新日志

本文件记录 CatTools 工具箱所有值得注意的改动。

格式参考 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/),
版本号遵循 [语义化版本](https://semver.org/lang/zh-CN/)。

## [0.1.0] - 未发布

框架骨架 + 第一个功能组件「重定向对象」。

### 新增

- **重定向对象组件 `CatRedirect`**(菜单 `CatTools/重定向/重定向对象`):
  构建期把组件所在对象<b>及其全部子对象</b>移动到指定路径下。
  - 默认保持世界位置 / 旋转 / 缩放,可用 `KeepWorldTransform` 切回「保留局部变换」。
  - 目标路径相对 **Avatar 最上层对象**;Inspector 支持从层级窗口**拖拽对象**填入,
    也支持直接手敲,并实时校验路径是否存在、是否含 `..`、是否落在自身子树内。
  - 目标(或它的祖先)上也有重定向组件时,Inspector 会预览对象最终会被带到的位置。
  - 构建期按「源对象深度从深到浅」多轮处理直到层级稳定:
    子对象先就位,父对象再带着整棵子树搬运,因此重定向链会被一并跟随。
  - 路径找不到、含 `..`、目标在自身子树内等情况会上报错误并阻止上传。
- `CatToolsPathUtils`(Runtime):目标路径的规范化、解析、相对路径计算与 `..` 检测。
- `CatRedirectPass`(Editor,`Transforming` 阶段):应用重定向。
- `CatRedirectEditor`(Editor):重定向组件的 Inspector,含拖拽路径框与重定向链预览。

### 修复

- `CatToolsPathUtils.TryResolve` 在路径为空(表示 Avatar 根对象)时会返回成功但结果为 null,
  导致「移动到根对象」静默失败。

### 变更

- 许可改为 **GNU GPL v3**(`GPL-3.0-or-later`),替换最初的 MIT。
- 作者署名统一为「一只大猫条」。
- NDMF 依赖下限由 `1.14.8` 放宽到 `1.13.1`:已在 Unity 2022.3.22f1 +
  NDMF 1.13.1 + VRChat SDK 3.10.4 环境下**实际编译通过**,确认所用 API 在该版本已存在。
- README 里 NDMF 的版本要求同步改为 ≥ 1.13.1。

### 新增(框架)

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
