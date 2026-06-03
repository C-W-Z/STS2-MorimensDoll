using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MinionLib.Targeting;
using MorimensDoll.Anims;
using MorimensDoll.Characters;
using STS2RitsuLib.Interop.AutoRegistration;

namespace MorimensDoll.Cards;

[RegisterCard(typeof(DollCardPool))]
public sealed class MinionAttack() : AbstractMinionCard(1, CardType.Skill, CardRarity.Common, MinionTargetTypes.AllMinions)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);
        List<Creature> pets = await CheckMinionExistAndSummon(choiceContext);

        foreach (var minion in pets)
        {
            Creature? enemy = Owner.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
            if (enemy == null)
                break;
            // await MinionAnimCmd.PlayBumpAttackAsync(minion, enemy); // 播放撞击动画
            await CreatureCmd.TriggerAnim(minion, DollSpine.State.Attack, DollSpine.AttackAnimDelay);
            await CreatureCmd.Damage(choiceContext, enemy, 0m, ValueProp.Move, minion, this); // 造成伤害，方法和原版类似
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
