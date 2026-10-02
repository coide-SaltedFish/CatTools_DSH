using UnityEngine;

namespace SereinFish.CatTools.Components
{
    /// <summary>
    /// 重定向组件:在构建期把<b>组件所在对象及其全部子对象</b>移动到指定位置。
    ///
    /// <para>
    /// 「移动」在 Unity 里等价于更换父对象,因此子对象会作为一个整体被带走,
    /// 无需逐个处理。默认保持世界变换,也就是外观不变,只是层级归属发生变化。
    /// </para>
    ///
    /// <para>
    /// 目标路径支持两种填写方式(见 Inspector):从层级窗口把对象拖到路径框上,
    /// 或者直接手敲路径。路径相对于 <b>Avatar 最上层对象</b>,例如 <c>Armature/Hips/Spine</c>。
    /// </para>
    ///
    /// <para>
    /// 若子对象上也有重定向组件,构建期按「源对象深度从深到浅」处理(先子后父):
    /// 子对象先进入自己的目标,随后父对象再带着整棵子树搬到父对象的目标位置。
    /// 由于移动会改变其他组件的目标位置,构建期会重复若干轮直到层级稳定,
    /// 因此目标对象上的重定向链也会被一并跟随,对象最终落在链条的终点。
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu(CatToolsConstants.ComponentMenuRoot + "重定向/重定向对象")]
    public sealed class CatRedirect : CatToolsComponent
    {
        /// <summary>
        /// 目标对象路径,相对于 Avatar 最上层对象。空字符串表示 Avatar 根对象本身。
        /// </summary>
        [Tooltip("目标路径,相对于 Avatar 最上层对象,例如 Armature/Hips/Spine。\n" +
                 "可以把层级窗口里的对象直接拖到这个框上,也可以手敲路径。\n" +
                 "留空表示 Avatar 根对象本身。")]
        public string TargetPath = string.Empty;

        /// <summary>
        /// 是否保持对象当前的世界位置、旋转与缩放。
        /// </summary>
        [Tooltip("勾选:只改变层级归属,对象在世界空间中的位置 / 旋转 / 缩放保持不变(推荐)。\n" +
                 "取消勾选:保留对象的局部变换值,它会按新父级的坐标系重新定位。")]
        public bool KeepWorldTransform = true;

        /// <summary>
        /// 在 Inspector 与构建报告里显示的名字。
        /// </summary>
        public override string DisplayName => "重定向对象";

        /// <summary>
        /// 规范化之后的目标路径;指向根对象时返回 null。
        /// </summary>
        public string NormalizedTargetPath => CatToolsPathUtils.Normalize(TargetPath);

        /// <summary>
        /// 目标路径的界面显示文字。
        /// </summary>
        public string TargetPathDisplay =>
            NormalizedTargetPath ?? CatToolsPathUtils.RootPlaceholder;
    }
}
