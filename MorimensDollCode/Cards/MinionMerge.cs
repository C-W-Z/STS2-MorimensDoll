using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MinionLib.Targeting;
using MorimensDoll.Anims;
using MorimensDoll.Characters;
using MorimensDoll.Minion;
using STS2RitsuLib.Interop.AutoRegistration;

namespace MorimensDoll.Cards;

[RegisterCard(typeof(DollCardPool))] // TODO: MinionTargetTypes.AllMinions 要改成自訂的只有 DollMinion 的 TargetType
public sealed class MinionMerge() : AbstractMinionCard(2, CardType.Skill, CardRarity.Rare, MinionTargetTypes.AllMinions)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CheckMinionExistAndSummon(choiceContext);

        await CreatureCmd.TriggerAnim(Owner.Creature, DollSpine.State.Skill2, DollSpine.Skill2AnimDelay);
        await DollMinionCmd.MergeAllDollMinions(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.MaxHp.UpgradeValueBy(2m);
    }
}
