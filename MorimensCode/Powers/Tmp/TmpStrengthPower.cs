using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Morimens.Powers.Tmp;

[RegisterPower(Inherit = true)]
public abstract class TmpStrengthPower<T> : ModTemporaryAppliedPowerTemplate<T, StrengthPower> where T : AbstractModel
{
    // 自定义图标路径
#pragma warning disable RITSU013
    public override PowerAssetProfile AssetProfile => new(
        IconPath: ImageHelper.GetImagePath("atlases/power_atlas.sprites/strength_power.tres"),
        BigIconPath: ImageHelper.GetImagePath("powers/strength_power.png")
    );
#pragma warning restore RITSU013

    protected override bool IsPositive => true; // 正面效果还是负面

    public override bool AllowNegative => false; // 是否可以是負的

    protected override bool UntilEndOfOtherSideTurn => false; // 为 true 时，在另一方回合结束时过期；否则在拥有者一方回合结束时过期。

    protected override int LastForXExtraTurns => 0; // 额外持续回合数

    public override LocString Title => new("powers", "MORIMENS_POWER_TMP_STRENGTH_POWER.title");

    public override LocString Description => new("powers", "MORIMENS_POWER_TMP_STRENGTH_POWER.description");
}
