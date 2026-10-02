using System;
using System.Collections.Generic;
using nadena.dev.ndmf;
using SereinFish.CatTools.Components;
using UnityEngine;

namespace SereinFish.CatTools.Editor.Passes
{
    /// <summary>
    /// 处理 <see cref="CatRedirect"/>:在构建期把对象连同其子对象移动到指定位置。
    ///
    /// <para><b>执行模型:多轮「深 → 浅」尝试,直到层级不再变化。</b></para>
    ///
    /// <para>每一轮都按源对象深度从深到浅(先子后父)遍历所有组件,并<b>即时解析</b>目标路径:</para>
    /// <list type="bullet">
    /// <item><b>先子后父</b>是为了让内容先就位 —— 父对象移动会带走整棵子树,
    ///       所以该搬出去的东西必须先搬出去;</item>
    /// <item><b>重复多轮</b>是因为一次移动会改变其他组件的目标路径。例如
    ///       <c>Item</c> → <c>Mid</c>、<c>Mid</c> → <c>Final</c>、
    ///       <c>Item</c> 的子对象 <c>Nested</c> 也 → <c>Mid</c>:
    ///       第一轮 <c>Item</c> 先进入 <c>Mid</c>,随后 <c>Mid</c> 带着它搬到 <c>Final</c> 下,
    ///       此时根下已经没有 <c>Mid</c>,<c>Nested</c> 的目标要靠第二轮重新解析
    ///       才能落到 <c>Final/Mid</c> 下。</item>
    /// </list>
    ///
    /// <para>
    /// 终止条件:某一轮既没有发生移动、也没有产生新的报错,或者达到轮数上限
    /// (组件数 + 2,足够让链条收敛)。已经站在目标位置的对象会被直接跳过,所以不会空转,
    /// 也不存在循环依赖导致的死循环 —— 循环依赖在校验阶段就会被判定为错误。
    /// </para>
    /// </summary>
    internal sealed class CatRedirectPass : CatToolsPass<CatRedirectPass>
    {
        public override string QualifiedName => "sereinfish.cat.tools.redirect";

        public override string DisplayName => "CatTools: 重定向对象";

        protected override void Execute(BuildContext context)
        {
            var components = FindComponents<CatRedirect>(context);
            if (components.Length == 0) return;

            var root = context.AvatarRootTransform;
            if (root == null)
            {
                ErrorReport.ReportException(new InvalidOperationException("找不到 Avatar 根对象,无法解析重定向路径。"));
                return;
            }

            // 源对象深度从深到浅 → 每轮都先子后父。
            // 深度只在这里算一次,处理顺序因此与后续的层级变化无关,稳定可预期。
            Array.Sort(components, (a, b) => GetDepth(b.transform).CompareTo(GetDepth(a.transform)));

            var reported = new HashSet<CatRedirect>();
            var maxRounds = components.Length + 2;

            for (var round = 0; round < maxRounds; round++)
            {
                var changed = false;

                foreach (var component in components)
                {
                    using (ErrorReport.WithContextObject(component))
                    {
                        if (TryMove(component, root, reported)) changed = true;
                    }
                }

                if (!changed) break;
            }
        }

        /// <summary>
        /// 尝试移动一个组件所在的对象。
        /// </summary>
        /// <returns>本次调用是否改变了层级。</returns>
        private static bool TryMove(CatRedirect component, Transform root, HashSet<CatRedirect> reported)
        {
            // 组件可能已经被前面的操作连带改写(理论上不会),保险起见判空。
            if (component == null) return false;

            var source = component.transform;
            var status = CatToolsPathUtils.TryResolve(root, component.TargetPath, out var destination);

            switch (status)
            {
                case CatToolsPathStatus.InvalidSyntax:
                    ReportOnce(component, reported,
                        $"目标路径 '{component.TargetPath}' 含有 '..' 段。路径必须位于 Avatar 之内," +
                        "不允许向上回溯到 Avatar 之外。");
                    return false;

                case CatToolsPathStatus.NotFound:
                    ReportOnce(component, reported,
                        $"找不到目标对象 '{component.TargetPathDisplay}'。" +
                        "路径相对于 Avatar 最上层对象,请检查拼写,或直接从层级窗口拖拽对象到路径框上。");
                    return false;
            }

            if (destination == null)
            {
                ReportOnce(component, reported, "目标路径解析结果为空,无法确定移动目标。");
                return false;
            }

            if (source == root)
            {
                ReportOnce(component, reported, "重定向组件挂在 Avatar 根对象上:根对象没有父级可以承载它,无法移动。");
                return false;
            }

            // 目标就是自己(或自己的子孙):这会把对象塞进自己的子树里,形成不可能的层级。
            if (CatToolsPathUtils.IsSelfOrDescendantOf(destination, source))
            {
                ReportOnce(component, reported,
                    $"目标 '{component.TargetPathDisplay}' 位于对象自身或它的子对象之内,无法移动。");
                return false;
            }

            if (source.parent == destination) return false; // 已经在目标下,无需移动。

            source.SetParent(destination, component.KeepWorldTransform);
            return true;
        }

        /// <summary>
        /// 同一个组件只上报一次错误。
        /// </summary>
        private static void ReportOnce(CatRedirect component, HashSet<CatRedirect> reported, string message)
        {
            if (!reported.Add(component)) return;

            // 本地化报错设施尚未接入,当前用 ReportException 这一临时手段
            // (严重级别 InternalError,会阻止上传;文案里会带堆栈)。
            ErrorReport.ReportException(new InvalidOperationException(message));
        }

        /// <summary>
        /// 取得对象在层级中的深度,Avatar 根对象为 0。仅用于排序。
        /// </summary>
        private static int GetDepth(Transform transform)
        {
            var depth = 0;
            var current = transform;
            while (current.parent != null)
            {
                depth++;
                current = current.parent;
            }

            return depth;
        }
    }
}
