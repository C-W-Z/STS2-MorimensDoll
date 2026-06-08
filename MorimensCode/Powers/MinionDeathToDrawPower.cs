using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Morimens.Minions;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Morimens.Powers;

[RegisterPower]
public sealed class MinionDeathToDrawPower : AbstractDollPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (Owner.Player == null || wasRemovalPrevented || creature.Monster is not DollMinion)
            return;

        await CardPileCmd.Draw(choiceContext, Amount, Owner.Player);
    }
}
