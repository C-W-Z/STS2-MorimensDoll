using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Morimens.Powers;

// see DollMinionLayout and MinionGuardianPatch
[RegisterPower]
public sealed class MinionGuardianPower : AbstractDollPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

#pragma warning disable RITSU013
    public override PowerAssetProfile AssetProfile => new(
        // 使用原版遊戲的DieForYouPower的Icon
        IconPath: ImageHelper.GetImagePath("atlases/power_atlas.sprites/die_for_you_power.tres"),
        BigIconPath: ImageHelper.GetImagePath("powers/die_for_you_power.png")
    );
#pragma warning restore RITSU013

    public override Creature ModifyUnblockedDamageTarget(Creature target, decimal amount, ValueProp props, Creature? dealer)
    {
        // 如果受擊目標不是主人（代表傷害已經被其他隨從攔截過去了）
        if (target != Owner.PetOwner?.Creature)
        {
            var flag = true;

            // 檢查目標身上是否有守衛 Power
            if (target.PetOwner == Owner.PetOwner && Owner.PetOwner != null && target.HasPower<MinionGuardianPower>())
            {
                var pets = target.PetOwner!.PlayerCombatState!.Pets;
                // 如果自己在寵物隊伍中的順位比對方更靠前，就把傷害從對方手中「搶過來」
                if (pets.IndexOf(Owner) < pets.IndexOf(target))
                    flag = false;
            }

            if (flag) return target;
        }

        // 基本死亡與傷害類型檢查
        if (Owner.IsDead) return target;
        if (!props.HasFlag(ValueProp.Move) || props.HasFlag(ValueProp.Unpowered)) return target;

        // 完美攔截，傷害轉移到自己身上
        return Owner;
    }
}
