using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MorimensDoll.Characters;
using MorimensDoll.Minion;
using STS2RitsuLib.Interop.AutoRegistration;

namespace MorimensDoll.Cards;

[RegisterCard(typeof(DollCardPool))]
[RegisterCharacterStarterCard(typeof(Doll), 1)]
public sealed class SummonFew() : AbstractDollCard(2, CardType.Skill, CardRarity.Common, TargetType.None)
{
    protected override HashSet<CardTag> CanonicalTags => [];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new HpLossVar(4m), new RepeatVar(2)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable, Owner.Creature, this);
        for (int i = 0; i < DynamicVars.Repeat.BaseValue; i++)
            await DollMinionCmd.Summon(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.HpLoss.UpgradeValueBy(-2);
    }
}
