using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Morimens.Anims;
using Morimens.CardTags;
using Morimens.Characters;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Morimens.Cards;

[RegisterCard(typeof(DollCardPool))]
[RegisterCharacterStarterCard(typeof(Doll), 1)]
public sealed class HealBlockSelfDiscardAll() : AbstractDollCard(3, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    protected override HashSet<CardTag> CanonicalTags => [DollCardTag.Heal];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HealVar(5m),
        // CalculatedBlock = 0 + 2 * 手牌數
        new CalculationBaseVar(0m),
        new CalculationExtraVar(2m),
        new CalculatedBlockVar(ValueProp.Move).WithMultiplier((card, _) => PileType.Hand.GetPile(card.Owner).Cards.Count)
    ];

    public override bool GainsBlock => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, DollSpine.State.Skill2, DollSpine.Skill2AnimDelay);
        var blockAmount = DynamicVars.CalculatedBlock.Calculate(cardPlay.Target);
        IEnumerable<CardModel> cards = PileType.Hand.GetPile(Owner).Cards;
        await CardCmd.Discard(choiceContext, cards);
        await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue + blockAmount, true);
        await CreatureCmd.GainBlock(Owner.Creature, blockAmount, DynamicVars.CalculatedBlock.Props, cardPlay);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
