using System.Collections.Generic;
using System.Reflection;
using SereinFish.CatTools.Components;
using UnityEditor;
using UnityEngine;

namespace SereinFish.CatTools.Editor.Inspectors
{
    /// <summary>
    /// <see cref="CatRedirect"/> 的 Inspector。
    ///
    /// <para>在默认字段之外补充:</para>
    /// <list type="bullet">
    /// <item>带拖拽接收的路径框 —— 可以直接把层级窗口里的对象拖上去,也可以手敲路径;</item>
    /// <item>路径实时校验:找不到、含 <c>..</c>、指向自身子树都会即时提示;</item>
    /// <item>重定向链预览:目标(或它的祖先)上也有重定向时,显示对象最终会被带到哪里;</item>
    /// <item>「定位」按钮,在场景里选中并高亮目标对象。</item>
    /// </list>
    /// </summary>
    [CustomEditor(typeof(CatRedirect))]
    internal sealed class CatRedirectEditor : CatToolsComponentEditor
    {
        private const float PathFieldHeight = 18f;

        private static MethodInfo _sceneViewFrameMethod;
        private static bool _sceneViewFrameMethodSearched;

        private SerializedProperty _targetPath;
        private SerializedProperty _keepWorldTransform;

        private void OnEnable()
        {
            _targetPath = serializedObject.FindProperty(nameof(CatRedirect.TargetPath));
            _keepWorldTransform = serializedObject.FindProperty(nameof(CatRedirect.KeepWorldTransform));
        }

        /// <summary>
        /// 自定义整体布局:路径字段由本类绘制(带拖拽),其余字段走默认绘制。
        /// </summary>
        public override void OnInspectorGUI()
        {
            var component = (CatRedirect)target;

            EditorGUILayout.LabelField(component.DisplayName, EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            DrawRedirectInspector(component);

            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(_keepWorldTransform);
        }

        /// <summary>
        /// 基类的挂载点在默认字段之后;本类改写了 <see cref="OnInspectorGUI"/>,
        /// 这里保留空实现以免重复绘制。
        /// </summary>
        protected override void DrawComponentInspector()
        {
        }

        private void DrawRedirectInspector(CatRedirect component)
        {
            var root = FindAvatarRoot(component.transform);

            if (root == null)
            {
                EditorGUILayout.HelpBox(
                    "找不到 Avatar 根对象:目标路径需要相对于 Avatar 最上层对象来解析。",
                    MessageType.Error);
                return;
            }

            if (component.transform == root)
            {
                EditorGUILayout.HelpBox(
                    "本组件挂在 Avatar 根对象上。根对象没有父级可以承载它,构建时会被跳过。",
                    MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField("目标路径(相对于 Avatar 根对象)", EditorStyles.boldLabel);
            DrawPathField(root, component);

            EditorGUILayout.Space(2);
            EditorGUILayout.HelpBox(
                "把这个对象和它的全部子对象一起移动到目标路径下。\n" +
                "可以从层级窗口拖拽对象到上面的路径框,也可以手敲路径;留空表示 Avatar 根对象本身。",
                MessageType.Info);

            EditorGUILayout.Space(2);
            DrawResolvedTarget(root, component);
            DrawRedirectChain(root, component);
        }

        /// <summary>
        /// 绘制路径输入框:文本框 + 拖拽接收 + 定位按钮。
        /// </summary>
        private void DrawPathField(Transform root, CatRedirect component)
        {
            var row = EditorGUILayout.GetControlRect(false, PathFieldHeight);
            const float buttonWidth = 56f;

            var fieldRect = new Rect(row.x, row.y, row.width - buttonWidth - 4f, row.height);
            var buttonRect = new Rect(row.xMax - buttonWidth, row.y, buttonWidth, row.height);

            HandlePathDrop(fieldRect, root);

            EditorGUI.BeginChangeCheck();
            var text = EditorGUI.TextField(fieldRect, component.TargetPath);
            if (EditorGUI.EndChangeCheck())
            {
                _targetPath.stringValue = text;
                serializedObject.ApplyModifiedProperties();
            }

            using (new EditorGUI.DisabledScope(
                       CatToolsPathUtils.TryResolve(root, component.TargetPath, out _) != CatToolsPathStatus.Success))
            {
                if (!GUI.Button(buttonRect, "定位")) return;

                if (CatToolsPathUtils.TryResolve(root, component.TargetPath, out var found) ==
                    CatToolsPathStatus.Success)
                {
                    FocusInScene(found.gameObject);
                }
            }
        }

        /// <summary>
        /// 处理拖入路径框的对象。只接受位于当前 Avatar 之下的对象。
        /// </summary>
        private void HandlePathDrop(Rect rect, Transform root)
        {
            var current = Event.current;
            if (current == null || !rect.Contains(current.mousePosition)) return;

            if (current.type != EventType.DragUpdated && current.type != EventType.DragPerform) return;
            if (!TryGetDraggedTransform(out var dragged)) return;

            var path = CatToolsPathUtils.GetRelativePath(root, dragged);
            if (path == null)
            {
                // 拖进来的对象不在这个 Avatar 之下,不能作为目标。
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                return;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (current.type != EventType.DragPerform) return;

            DragAndDrop.AcceptDrag();

            _targetPath.stringValue = path;
            serializedObject.ApplyModifiedProperties();

            GUI.changed = true;
            current.Use();
        }

        private static bool TryGetDraggedTransform(out Transform dragged)
        {
            dragged = null;

            foreach (var reference in DragAndDrop.objectReferences)
            {
                if (reference is GameObject gameObject)
                {
                    dragged = gameObject.transform;
                    return true;
                }

                if (reference is Transform transform)
                {
                    dragged = transform;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 显示路径的解析结果。
        /// </summary>
        private void DrawResolvedTarget(Transform root, CatRedirect component)
        {
            if (string.IsNullOrWhiteSpace(component.TargetPath))
            {
                EditorGUILayout.HelpBox("目标路径为空:会把对象移到 Avatar 根对象下。", MessageType.Warning);
            }

            var status = CatToolsPathUtils.TryResolve(root, component.TargetPath, out var found);

            switch (status)
            {
                case CatToolsPathStatus.Success:
                    DrawObjectRow(root == found ? root.gameObject : found.gameObject);

                    if (found != root)
                    {
                        EditorGUILayout.LabelField("完整路径", CatToolsPathUtils.GetHierarchyPath(found),
                            EditorStyles.miniLabel);
                    }

                    break;

                case CatToolsPathStatus.InvalidSyntax:
                    EditorGUILayout.HelpBox(
                        "路径含有 '..' 段。路径必须位于 Avatar 之内,不允许向上回溯到 Avatar 之外,构建时会报错。",
                        MessageType.Error);
                    break;

                default:
                    EditorGUILayout.HelpBox(
                        "找不到这个路径对应的对象。可能是拼写错误,或者该对象要等其他插件构建时才生成;" +
                        "构建时仍找不到会报错并阻止上传。",
                        MessageType.Error);
                    break;
            }
        }

        /// <summary>
        /// 用只读的对象行展示解析结果,点击可以在 Project / 层级窗口里定位。
        /// </summary>
        private static void DrawObjectRow(GameObject gameObject)
        {
            var previous = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 0f;
            EditorGUILayout.ObjectField(" ", gameObject, typeof(GameObject), true);
            EditorGUIUtility.labelWidth = previous;
        }

        /// <summary>
        /// 预览「目标对象本身也挂着重定向」时的最终落点。
        /// </summary>
        private void DrawRedirectChain(Transform root, CatRedirect component)
        {
            if (CatToolsPathUtils.TryResolve(root, component.TargetPath, out var target) != CatToolsPathStatus.Success)
            {
                return;
            }

            var chain = BuildRedirectChain(root, component, target);
            if (chain.Count == 0) return;

            EditorGUILayout.HelpBox(
                "目标链:目标对象(或它的祖先)上也有重定向组件,本对象最终会被带到:\n" +
                string.Join(" → ", chain),
                MessageType.Warning);
        }

        /// <summary>
        /// 沿着目标上的重定向组件一路往下走,返回沿途的路径。检测到环时以提示结尾。
        /// </summary>
        private static List<string> BuildRedirectChain(Transform root, CatRedirect component, Transform target)
        {
            var chain = new List<string>();
            var visited = new HashSet<Transform> { component.transform };
            var current = target;

            while (current != null)
            {
                if (!visited.Add(current))
                {
                    chain.Add("…(检测到重定向循环,构建时会报错)");
                    break;
                }

                var redirect = FindRedirectInAncestors(current, component);
                if (redirect == null) break;

                if (CatToolsPathUtils.TryResolve(root, redirect.TargetPath, out var next) !=
                    CatToolsPathStatus.Success)
                {
                    chain.Add(redirect.TargetPathDisplay + "(找不到)");
                    break;
                }

                var path = CatToolsPathUtils.GetRelativePath(root, next);
                chain.Add(string.IsNullOrEmpty(path) ? CatToolsPathUtils.RootPlaceholder : path);

                current = next;
            }

            return chain;
        }

        /// <summary>
        /// 从对象自身向上找第一个重定向组件(对象自己或最近的祖先)。
        /// </summary>
        private static CatRedirect FindRedirectInAncestors(Transform transform, CatRedirect ignore)
        {
            var current = transform;
            while (current != null)
            {
                var redirect = current.GetComponent<CatRedirect>();
                if (redirect != null && redirect != ignore) return redirect;

                current = current.parent;
            }

            return null;
        }

        /// <summary>
        /// 查找路径的基准对象:优先用 VRChat Avatar 描述符所在的对象,否则取层级最上层对象。
        ///
        /// <para>这里用字符串查找 <c>VRCAvatarDescriptor</c>,避免 Editor 程序集硬引用 VRChat 程序集。</para>
        /// </summary>
        private static Transform FindAvatarRoot(Transform transform)
        {
            if (transform == null) return null;

            var current = transform;
            while (current != null)
            {
                if (current.GetComponent("VRCAvatarDescriptor") != null) return current;
                current = current.parent;
            }

            current = transform;
            while (current.parent != null) current = current.parent;

            return current;
        }

        /// <summary>
        /// 在场景窗口里选中并聚焦到某个对象。
        /// </summary>
        private static void FocusInScene(GameObject gameObject)
        {
            if (gameObject == null) return;

            Selection.activeGameObject = gameObject;
            EditorGUIUtility.PingObject(gameObject);

            var sceneViews = SceneView.sceneViews;
            if (sceneViews == null || sceneViews.Count == 0) return;

            if (!(sceneViews[0] is SceneView sceneView)) return;

            sceneView.Focus();

            // SceneView.Frame 在 2022.3 里是 internal 的,用反射调用;拿不到就只保留选中与 Ping。
            GetSceneViewFrameMethod()?.Invoke(sceneView,
                new object[] { new Bounds(gameObject.transform.position, Vector3.one) });
        }

        private static MethodInfo GetSceneViewFrameMethod()
        {
            if (_sceneViewFrameMethodSearched) return _sceneViewFrameMethod;

            _sceneViewFrameMethodSearched = true;

            foreach (var method in typeof(SceneView).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic |
                                                                BindingFlags.Public))
            {
                if (method.Name != "Frame") continue;

                var parameters = method.GetParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(Bounds))
                {
                    _sceneViewFrameMethod = method;
                    break;
                }
            }

            return _sceneViewFrameMethod;
        }
    }
}
