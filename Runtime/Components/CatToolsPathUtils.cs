using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SereinFish.CatTools.Components
{
    /// <summary>
    /// 路径解析的结果状态。
    /// </summary>
    public enum CatToolsPathStatus
    {
        /// <summary>路径解析成功。</summary>
        Success,

        /// <summary>路径字符串本身不合法(含有 <c>..</c> 这样的层级回溯段)。</summary>
        InvalidSyntax,

        /// <summary>路径合法,但层级里找不到对应的对象。</summary>
        NotFound,
    }

    /// <summary>
    /// CatTools 组件里「目标路径」的解析工具。
    ///
    /// <para>
    /// 路径的语义:<b>相对于 Avatar 最上层对象</b>(即 <c>Transform.root</c>),
    /// 用 <c>/</c> 分隔各级名称,例如 <c>Armature/Hips/Spine</c>。
    /// 空字符串、<c>.</c>、<c>/</c> 都表示「Avatar 根对象本身」。
    /// </para>
    ///
    /// <para>
    /// 之所以用字符串路径而不是直接引用 <c>GameObject</c>:构建发生在场景的克隆上,
    /// 直连引用在跨 prefab / 跨 Avatar 时容易失效,而路径可以在构建期的克隆里重新解析。
    /// </para>
    /// </summary>
    public static class CatToolsPathUtils
    {
        /// <summary>
        /// 路径为空(即指向根对象)时,在界面上显示的占位文字。
        /// </summary>
        public const string RootPlaceholder = "<Avatar 根对象>";

        /// <summary>
        /// 层级分隔符。
        /// </summary>
        public const char Separator = '/';

        /// <summary>
        /// 规范化路径:去掉首尾空白,反斜杠转正斜杠,折叠分隔符,去掉首尾的 <c>/</c>。
        /// 指向根对象的写法(空串 / <c>.</c> / <c>/</c>)统一返回 <c>null</c>。
        /// </summary>
        /// <param name="path">原始路径;可以为 null。</param>
        /// <returns>规范化后的路径;指向根对象时返回 null。</returns>
        public static string Normalize(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;

            var trimmed = path.Trim().Replace('\\', Separator);

            var segments = new List<string>();
            foreach (var raw in trimmed.Split(Separator))
            {
                var segment = raw.Trim();
                if (segment.Length == 0 || segment == ".") continue;

                segments.Add(segment);
            }

            if (segments.Count == 0) return null;

            return string.Join(Separator.ToString(), segments);
        }

        /// <summary>
        /// 判断路径是否含有 <c>..</c> 段。
        ///
        /// <para>
        /// <see cref="Transform.Find"/> 本身支持 <c>..</c> 向上回溯,那会让路径越过
        /// Avatar 根对象指向外部场景对象,进而把对象搬到 Avatar 之外 —— 这是必须挡住的。
        /// </para>
        /// </summary>
        public static bool HasParentTraversal(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            foreach (var raw in path.Replace('\\', Separator).Split(Separator))
            {
                if (raw.Trim() == "..") return true;
            }

            return false;
        }

        /// <summary>
        /// 按路径查找对象。
        /// </summary>
        /// <param name="root">路径的基准对象,通常是 Avatar 根对象的 Transform。</param>
        /// <param name="path">目标路径,语义见 <see cref="CatToolsPathUtils"/> 的说明。</param>
        /// <param name="result">解析结果;失败时为 null。</param>
        /// <returns>解析状态。</returns>
        public static CatToolsPathStatus TryResolve(Transform root, string path, out Transform result)
        {
            result = null;

            if (root == null) return CatToolsPathStatus.NotFound;

            // 根对象本身。
            var normalized = Normalize(path);
            if (normalized == null)
            {
                result = root;
                return CatToolsPathStatus.Success;
            }

            if (HasParentTraversal(path)) return CatToolsPathStatus.InvalidSyntax;

            return ResolveNormalized(root, normalized, out result);
        }

        private static CatToolsPathStatus ResolveNormalized(Transform root, string normalized, out Transform result)
        {
            result = null;

            if (TryFindExact(root, normalized, out result)) return CatToolsPathStatus.Success;

            // 容错:目标挂在 Avatar 之外(例如 Avatar 本身被嵌套在别的对象下),路径实际上
            // 是从场景根算起的。此时用「整条路径的唯一匹配」来兜底,并交由调用方提示用户。
            if (TryFindByName(root, normalized, out result)) return CatToolsPathStatus.Success;

            return CatToolsPathStatus.NotFound;
        }

        private static bool TryFindExact(Transform root, string normalized, out Transform result)
        {
            try
            {
                result = root.Find(normalized);
            }
            catch (Exception)
            {
                // Transform.Find 对含非法字符的名称会抛异常,这里一律当成「找不到」。
                result = null;
            }

            return result != null;
        }

        /// <summary>
        /// 兜底查找:把路径的每一级当作名称,在整个层级里找唯一一条匹配的链。
        /// 只有「恰好一个」匹配时才认,避免误伤。
        /// </summary>
        private static bool TryFindByName(Transform root, string normalized, out Transform result)
        {
            result = null;

            var segments = normalized.Split(Separator);
            var matches = new List<Transform>();

            CollectByName(root, segments, 0, matches, limit: 2);

            if (matches.Count == 1)
            {
                result = matches[0];
                return true;
            }

            return false;
        }

        private static void CollectByName(Transform current, string[] segments, int index,
            List<Transform> matches, int limit)
        {
            if (matches.Count >= limit) return;

            if (index == segments.Length)
            {
                matches.Add(current);
                return;
            }

            var wanted = segments[index];
            for (var i = 0; i < current.childCount; i++)
            {
                if (matches.Count >= limit) return;

                var child = current.GetChild(i);
                if (child.name != wanted) continue;

                CollectByName(child, segments, index + 1, matches, limit);
            }
        }

        /// <summary>
        /// 取得 <paramref name="target"/> 相对于 <paramref name="root"/> 的路径。
        /// </summary>
        /// <param name="root">路径基准对象。</param>
        /// <param name="target">目标对象;等于 <paramref name="root"/> 时返回空串。</param>
        /// <returns>相对路径;若 <paramref name="target"/> 不在 <paramref name="root"/> 之下则返回 null。</returns>
        public static string GetRelativePath(Transform root, Transform target)
        {
            if (root == null || target == null) return null;
            if (root == target) return string.Empty;

            var segments = new List<string>();
            var current = target;
            while (current != null && current != root)
            {
                segments.Add(current.name);
                current = current.parent;
            }

            if (current != root) return null;

            segments.Reverse();
            return string.Join(Separator.ToString(), segments);
        }

        /// <summary>
        /// 取得对象在层级面板里的完整路径,例如 <c>Avatar/Body/Head</c>。仅用于界面提示。
        /// </summary>
        public static string GetHierarchyPath(Transform transform)
        {
            if (transform == null) return string.Empty;

            var builder = new StringBuilder(transform.name);
            var current = transform.parent;
            while (current != null)
            {
                builder.Insert(0, current.name + Separator);
                current = current.parent;
            }

            return builder.ToString();
        }

        /// <summary>
        /// 判断 <paramref name="transform"/> 是否位于 <paramref name="ancestor"/> 的子树内
        /// (包含自身)。
        ///
        /// <para>
        /// 这里不用 <c>Transform.IsChildOf</c>:那是 <see cref="Component"/> 上的实例方法,
        /// 写成静态工具方法可以直接拿 <c>Transform</c> 调用,语义也更明确。
        /// </para>
        /// </summary>
        public static bool IsSelfOrDescendantOf(Transform transform, Transform ancestor)
        {
            if (transform == null || ancestor == null) return false;

            var current = transform;
            while (current != null)
            {
                if (current == ancestor) return true;
                current = current.parent;
            }

            return false;
        }
    }
}
