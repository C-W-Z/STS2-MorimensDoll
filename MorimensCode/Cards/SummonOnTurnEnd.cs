using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Morimens.Anims;
using Morimens.Characters;
using Morimens.Minions;
using Morimens.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Morimens.Cards;

[RegisterCard(typeof(DollCardPool))]
public sealed class SummonOnTurnEnd() : AbstractDollCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override HashSet<CardTag> CanonicalTags => [];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new RepeatVar(0), new PowerVar<SummonOnTurnEndPower>(3m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, DollSpine.State.Skill2, DollSpine.Skill2AnimDelay);

        for (int i = 0; i < DynamicVars.Repeat.BaseValue; i++)
        {
            await DollMinionCmd.Summon(choiceContext, Owner, this);
        }

        await PowerCmd.Apply<SummonOnTurnEndPower>(choiceContext, Owner.Creature, DynamicVars["SummonOnTurnEndPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1);
    }
}
