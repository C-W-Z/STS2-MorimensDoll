using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MinionLib.Commands;
using Morimens.Anims;
using Morimens.Characters;
using Morimens.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Morimens.Cards;

[RegisterCard(typeof(DollCardPool))]
public sealed class MinionLimitUp() : AbstractDollCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override HashSet<CardTag> CanonicalTags => [];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<MinionLimitUpPower>(1m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, DollSpine.State.Exalt, DollSpine.ExaltAnimDelay);
        await PowerCmd.Apply<MinionLimitUpPower>(choiceContext, Owner.Creature, DynamicVars["MinionLimitUpPower"].BaseValue, Owner.Creature, this);
        await MinionAnimCmd.Rearrange();
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
        AddKeyword(CardKeyword.Retain);
    }
}
