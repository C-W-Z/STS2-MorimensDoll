using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MinionLib.Targeting;
using MorimensDoll.Anims;
using MorimensDoll.Characters;
using STS2RitsuLib.Interop.AutoRegistration;

namespace MorimensDoll.Cards;

[RegisterCard(typeof(DollCardPool))]
public sealed class DoomAllCreatures() : AbstractDollCard(0, CardType.Attack, CardRarity.Uncommon, MinionTargetTypes.AllCreatures)
{
    protected override HashSet<CardTag> CanonicalTags => [];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DoomPower>(5m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<DoomPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);
        await CreatureCmd.TriggerAnim(Owner.Creature, DollSpine.State.Skill2, DollSpine.Skill2AnimDelay);
        // all creatures
        await PowerCmd.Apply<DoomPower>(choiceContext, CombatState.Creatures, DynamicVars.Doom.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Doom.UpgradeValueBy(5m);
    }
}
