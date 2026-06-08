using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Morimens.Powers;

// 能力不需要放入角色池，RegisterPower 会直接注册模型。
[RegisterPower]
public sealed class HealToBlockPower : AbstractDollPower
{
    // Buff 是正面能力，Debuff 是负面能力。
    public override PowerType Type => PowerType.Buff;

    // Counter 表示可叠层，并用 Amount 表示层数。
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (creature == Owner && delta > 0)
            // Unpowered: 獲得的格擋值不受Buff(如敏捷)影響
            await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
}
