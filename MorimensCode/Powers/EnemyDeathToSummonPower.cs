using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Morimens.Minions;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Morimens.Powers;

[RegisterPower]
public sealed class EnemyDeathToSummonPower : AbstractDollPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (Owner.Player == null || wasRemovalPrevented || creature.Side != CombatSide.Enemy)
            return;

        for (int i = 0; i < Amount; i++)
        {
            await DollMinionCmd.Summon(choiceContext, Owner.Player, null);
        }
    }
}
