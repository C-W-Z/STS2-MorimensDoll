using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Morimens.Anims;
using Morimens.Characters;
using Morimens.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Morimens.Cards;

[RegisterCard(typeof(DollCardPool))]
public sealed class MinionDeathToDraw() : AbstractDollCard(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override HashSet<CardTag> CanonicalTags => [];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<MinionDeathToDrawPower>(1m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, DollSpine.State.Skill2, DollSpine.Skill2AnimDelay);
        await PowerCmd.Apply<MinionDeathToDrawPower>(choiceContext, Owner.Creature, DynamicVars["MinionDeathToDrawPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
