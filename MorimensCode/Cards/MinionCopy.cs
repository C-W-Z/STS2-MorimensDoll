using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Morimens.Anims;
using Morimens.Characters;
using Morimens.Minions;
using Morimens.Targeting;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Morimens.Cards;

[RegisterCard(typeof(DollCardPool))]
public sealed class MinionCopy() : AbstractMinionCard(2, CardType.Skill, CardRarity.Rare, DollTargetTypes.AnyDollMinionOrNone)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<DollMinion> minions = await CheckMinionExistAndSummon(choiceContext);
        if (minions.Count == 0)
            return;

        ArgumentNullException.ThrowIfNull(cardPlay.Target?.Monster);
        await CreatureCmd.TriggerAnim(Owner.Creature, DollSpine.State.Skill2, DollSpine.Skill2AnimDelay);
        await DollMinionCmd.SummonCopy(choiceContext, Owner, (DollMinion)cardPlay.Target.Monster, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
