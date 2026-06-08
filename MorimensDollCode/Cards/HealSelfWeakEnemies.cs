using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MorimensDoll.Anims;
using MorimensDoll.CardTags;
using MorimensDoll.Characters;
using STS2RitsuLib.Interop.AutoRegistration;

namespace MorimensDoll.Cards;

[RegisterCard(typeof(DollCardPool))]
[RegisterCharacterStarterCard(typeof(Doll), 1)]
public sealed class HealSelfWeakEnemies() : AbstractDollCard(2, CardType.Skill, CardRarity.Basic, TargetType.AllEnemies)
{
    protected override HashSet<CardTag> CanonicalTags => [DollCardTag.Heal];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(5m), new PowerVar<WeakPower>(1m)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<WeakPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);
        await CreatureCmd.TriggerAnim(Owner.Creature, DollSpine.State.Skill1, DollSpine.Skill1AnimDelay);
        await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue, true);
        // all enemies
        await PowerCmd.Apply<WeakPower>(choiceContext, CombatState.HittableEnemies, DynamicVars.Weak.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Weak.UpgradeValueBy(1m);
    }
}
