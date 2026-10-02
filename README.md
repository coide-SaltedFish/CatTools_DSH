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

## 目录结构

```
CatTools/
├── package.json                          VPM/UPM 包清单(仓库根目录即包根目录)
├── README.md
├── CHANGELOG.md
├── LICENSE.md
├── docs~/                                开发参考文档(以 ~ 结尾 → Unity 不会打进包)
├── Runtime/                              ── 随 Avatar 一起存在的那一半
│   ├── SereinFish.CatTools.Runtime.asmdef
│   ├── AssemblyInfo.cs
│   ├── CatToolsConstants.cs              插件限定名、显示名、菜单根路径
│   └── Components/
│       ├── CatToolsComponent.cs          所有 CatTools 组件的抽象基类
│       ├── CatToolsPathUtils.cs          目标路径的规范化与解析
│       └── CatRedirect.cs                重定向对象组件
└── Editor/                               ── 只在编辑器里存在的那一半
    ├── SereinFish.CatTools.Editor.asmdef
    ├── CatToolsPlugin.cs                 NDMF 插件入口,在这里接管线
    ├── Passes/
    │   ├── CatToolsPass.cs               功能 Pass 的基类
    │   ├── CatToolsComponentRegistry.cs  一次构建的组件清单
    │   ├── CollectComponentsPass.cs      Resolving 阶段:建立清单
    │   ├── CatRedirectPass.cs            Transforming 阶段:重定向对象
    │   └── CleanupComponentsPass.cs      PlatformFinish 阶段:清除组件
    └── Inspectors/
        ├── CatToolsComponentEditor.cs    组件 Inspector 的基类
        └── CatRedirectEditor.cs          重定向组件的 Inspector
```

**为什么分成两个程序集:** `Runtime` 里的组件是纯数据,只引用 NDMF 的 **runtime** 程序集
(为了拿到 `INDMFEditorOnly`,它在没有 VRChat SDK 的工程里会退化成一个空接口),
绝不引用 `nadena.dev.ndmf` 主程序集或 `UnityEditor` —— 后者是 Editor 平台限定的,
runtime 程序集引用它会直接编译失败。
所有构建逻辑集中在 `Editor`,并 `includePlatforms: ["Editor"]`,不会被打进最终产物。

`CatToolsComponent` 实现了 `INDMFEditorOnly`,这是向 NDMF 与 VRChat SDK 声明
「本组件只在编辑器里有意义」的标准做法(Modular Avatar 的 `AvatarTagComponent` 同理),
保证它不会被上传到 VRChat。此外 `CleanupComponentsPass` 还会在收尾阶段主动删掉它们。

## 安装

### 方式一:作为本地包引入(开发时推荐)

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

### 方式二:直接放进工程的 Packages 目录

把整个仓库复制成 `<你的工程>/Packages/sereinfish.cat.tools/`。

### 方式三:通过 VPM 安装

等本包发布到 VPM 仓库后,在 VRChat Creator Companion 里添加该仓库即可。
`package.json` 中的 `url` / `repo` 字段目前留空,发布前需要补上。

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

同一阶段内多次调用 `InPhase` 会按声明顺序串行执行。需要和其他插件排序时用
`.Run(...).BeforePlugin("对方的插件限定名")`。

目前 CatTools 在这些阶段挂了 Pass:

- `Resolving` → `CollectComponentsPass`:遍历一次层级,把结果存进
  `CatToolsComponentRegistry`,后续 Pass 通过 `CatToolsComponentRegistry.Get(context)` 取用。
- `Transforming` → `CatRedirectPass`:应用「重定向对象」组件。
- `PlatformFinish` → `CleanupComponentsPass`:销毁所有 `CatToolsComponent`,
  避免它们出现在上传的 Avatar 上变成「缺失脚本」。

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

## 如何新增一个功能组件

### 1. 写组件(声明)

`Runtime/Components/MyComponent.cs`:

```csharp
using SereinFish.CatTools;
using SereinFish.CatTools.Components;
using UnityEngine;

namespace SereinFish.CatTools.Components
{
    [AddComponentMenu(CatToolsConstants.ComponentMenuRoot + "示例/我的组件")]
    public sealed class MyComponent : CatToolsComponent
    {
        public override string DisplayName => "我的组件";

        [Tooltip("这个字段会出现在 Inspector 里")]
        public float SomeValue = 1.0f;
    }
}
```

### 2. 写 Inspector(可选)

`Editor/Inspectors/MyComponentEditor.cs`:

```csharp
using SereinFish.CatTools.Components;
using SereinFish.CatTools.Editor.Inspectors;
using UnityEditor;
using UnityEngine;

namespace SereinFish.CatTools.Editor.Inspectors
{
    [CustomEditor(typeof(MyComponent))]
    internal sealed class MyComponentEditor : CatToolsComponentEditor
    {
        protected override void DrawComponentInspector()
        {
            EditorGUILayout.HelpBox("在这里解释这个组件的作用。", MessageType.Info);
        }
    }
}
```

不写这个文件的话,Unity 会用默认 Inspector;框架的统一标题头也就没有了。

### 3. 写处理它的 Pass(应用)

`Editor/Passes/MyComponentPass.cs`:

```csharp
using nadena.dev.ndmf;
using SereinFish.CatTools.Components;

namespace SereinFish.CatTools.Editor.Passes
{
    internal sealed class MyComponentPass : CatToolsPass<MyComponentPass>
    {
        public override string QualifiedName => "sereinfish.cat.tools.my-component";
        public override string DisplayName => "CatTools: 我的组件";

        protected override void Execute(BuildContext context)
        {
            foreach (var component in FindComponents<MyComponent>(context))
            {
                // 在这里把 component 的声明应用到 Avatar 上。
                // context.AvatarRootObject / AvatarRootTransform 是 Avatar 的根。
            }
        }
    }
}
```

### 4. 注册到管线

在 `Editor/CatToolsPlugin.cs` 的 `Configure()` 里加一行:

```csharp
InPhase(BuildPhase.Transforming)
    .Run(MyComponentPass.Instance);
```

组件不需要自己删除自己 —— `CleanupComponentsPass` 会在收尾时统一处理。

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
若你只想授权第 3 版本身、不含「或更新版本」,把它改成 `GPL-3.0-only` 并相应调整上面的声明。

> 采用 GPL 后请注意:**本插件的使用者若再分发,也必须以 GPL 兼容条款开源**。
> 如果你想允许别人把 CatTools 用在闭源作品里,应该改用 MIT 或 Apache-2.0。
