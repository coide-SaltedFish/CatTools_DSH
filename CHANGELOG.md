# 更新日志

本文件记录 CatTools 工具箱所有值得注意的改动。

格式参考 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/),
版本号遵循 [语义化版本](https://semver.org/lang/zh-CN/)。

## [0.1.0] - 未发布

首个公开版本:框架 + 功能组件「重定向对象」。

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
- **VPM 仓库列表**:`https://coide-SaltedFish.github.io/CatTools_DSH/index.json`。
  在 VCC 里 Add Repo 填入该地址即可安装 / 更新本包。
- 源码仓库:`https://github.com/coide-SaltedFish/CatTools_DSH`。

### 修复

- 目标路径留空(表示「移动到 Avatar 根对象」)此前会静默失败,现在可以正常生效。

### 变更

- 许可改为 **GNU GPL v3**(`GPL-3.0-or-later`),替换最初的 MIT。
- 作者署名统一为「一只大猫条」。
- NDMF 依赖下限由 `1.14.8` 放宽到 `1.13.1`:已在 Unity 2022.3.22f1 +
  NDMF 1.13.1 + VRChat SDK 3.10.4 环境下实际编译通过,确认所用 API 在该版本已存在。
