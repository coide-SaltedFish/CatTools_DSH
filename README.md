# CatTools 工具箱

基于 [NDMF](https://ndmf.nadena.dev/)(Non-Destructive Modular Framework)的 VRChat Avatar 非破坏性工具箱。

CatTools 以「组件」为工作单位:你在 Avatar 上挂一个 CatTools 组件,只**描述想要的结果**;
真正的改动由 NDMF 在构建期(上传 Avatar、或进入 Play 模式预览时)统一应用。
原始模型、prefab、材质不会被修改,组件的效果可以随时增删、叠加与回退。

> **当前状态:v0.1.0 开发中。** 插件管线与程序集骨架已就绪,第一个功能组件「重定向对象」已可用。

## 环境要求

| 项 | 要求 |
| --- | --- |
| Unity | 2022.3.22f1(VRChat 官方推荐的 Avatar 开发版本) |
| VRChat SDK | SDK 3.0 Avatars |
| NDMF | `nadena.dev.ndmf` ≥ 1.13.1,< 2.0.0 |

对 NDMF 的依赖已写在 `package.json` 的 `vpmDependencies` 中,通过 VPM 安装时会自动一并装上。

## 安装

### 方式一:通过 VPM 安装(普通用户推荐)

在 VRChat Creator Companion(VCC)里打开 **Settings → Packages → Add Repo**,
填入本包的 VPM 仓库地址:

```
https://coide-SaltedFish.github.io/CatTools_DSH/index.json
```

添加后 CatTools 工具箱会出现在包列表里,直接 Install 即可;
`package.json` 里声明的 NDMF 依赖(VPM 的 `vpmDependencies`)会一并装上。

> 也可以把上面这个地址粘进 VCC 的 **Add Repo** 弹窗的输入框,效果相同。

### 方式二:作为本地包引入(开发时推荐)

在 Unity 工程的 `Packages/manifest.json` 里加一行,路径按实际情况调整:

```json
{
  "dependencies": {
    "sereinfish.cat.tools": "file:../../CatTools"
  }
}
```

也可以走菜单 **Window → Package Manager → + → Add package from disk…**,选择本仓库的 `package.json`。
之后改动本仓库的代码会立刻反映到工程里。

### 方式三:直接放进工程的 Packages 目录

把整个仓库复制成 `<你的工程>/Packages/sereinfish.cat.tools/`。

> ⚠️ **本包与上一代 CatTools 是两套并行的包。**
> 上一代的包 id 是 `io.github.sereinfish.cat.tools`,仓库地址是
> `https://coide-SaltedFish.github.io/CatTools/index.json`;
> 本包的包 id 是 `sereinfish.cat.tools`。两者 id 不同,VCC 会视为两个独立包。
> 请确认你要装的是哪一个,不要把两个仓库地址搞混。

## 核心概念

一次 NDMF 构建会依次经过这些阶段(取自 NDMF 的 `BuildPhase`):

| 阶段 | 用途 |
| --- | --- |
| `FirstChance` | 平台初始化之前,留给「替换整个 Avatar」这类极端操作 |
| `PlatformInit` | 平台后端初始化 |
| `Resolving` | 早期解析,此时 Avatar 结构尚未被大幅修改 |
| `Generating` | 生成供后续插件(例如 Modular Avatar)使用的组件 |
| `Transforming` | 通用的 Avatar 变换,**绝大多数功能放这里** |
| `Optimizing` | 纯粹的性能优化,需要跑得很晚 |
| `PlatformFinish` | 平台相关的收尾与校验 |

重定向对象在 `Transforming` 阶段生效,因此下游插件(例如 Modular Avatar)看到的是移动之后的结构。

## 已有组件

### 重定向对象(CatRedirect)

菜单:`Add Component → CatTools → 重定向 → 重定向对象`。

在构建期把**组件所在对象及其全部子对象**移动到指定位置。移动等价于更换父对象,
所以子对象会整体被带走;默认保持世界位置、旋转、缩放,外观不变,只是层级归属变了。

| 字段 | 说明 |
| --- | --- |
| 目标路径 | 相对 **Avatar 最上层对象**的路径,如 `Armature/Hips/Spine`;留空表示 Avatar 根对象 |
| 保持世界变换 | 勾选(默认):只改层级归属;取消:保留局部变换值,按新父级坐标系重新定位 |

路径可以从层级窗口**直接拖拽**到路径框上,也可以手敲。Inspector 会实时校验路径,
并在目标(或它的祖先)上也有重定向组件时,显示对象最终会被带到的位置。

行为要点:

- **子先父后,并重复多轮直到稳定。** 构建期每一轮按「源对象深度从深到浅」处理:
  先把该搬出去的内容搬出去,再让父对象带着整棵子树移动。因为一次移动会改变其他组件的
  目标位置,所以会重复若干轮直到层级不再变化(轮数上限为「组件数 + 2」)。
- **目标链会一并处理。** A 指向 B、B 又指向 C 时,把组件挂在 A 上,最终会落到 C 下。
- **错误会阻止上传。** 路径找不到、路径含 `..`、目标落在自身子树内,都会在构建报告里报错。

## 许可

Copyright (C) 2025 SereinFish

本项目以 **GNU 通用公共许可证第 3 版(或更新版本)** 发布,完整条款见 [LICENSE.md](LICENSE.md)。

```
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
```

`package.json` 的 `license` 字段用的是 SPDX 标识 `GPL-3.0-or-later`。

> 采用 GPL 后请注意:**本插件的使用者若再分发,也必须以 GPL 兼容条款开源**。
