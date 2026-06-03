using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using MinionLib.RitsuAdapters;
using MorimensDoll.Anims;
using STS2RitsuLib.Interop.AutoRegistration;

namespace MorimensDoll.Minions.Actions;

[RegisterPower]
public class MinionAttackAction : ModActionTemplate
{
    public override TargetType TargetType => TargetType.AnyEnemy;           // 目标类型
    public override bool AutoRemoveAtTurnEnd => true;                       // 是否在回合结束自动移除
    public override PowerType Type => PowerType.Buff;                       // Power 的类型
    public override PowerStackType StackType => PowerStackType.Counter;     // Power 的堆叠属性

    // 核心重载，定义 Action 被触发时的行为，类似于卡牌的 OnPlay
    // 和卡牌一样，如果目标无需选定（如所有敌人），target 将会是 null
    protected override async Task OnAct(PlayerChoiceContext choiceContext, Creature? target)
    {
        if (target == null) return;

        // await MinionAnimCmd.PlayBumpAttackAsync(Owner, target);                             // 播放撞击动画（在 MinionAnimCmd 中定义）
        await CreatureCmd.TriggerAnim(Owner, DollSpine.State.Attack, DollSpine.AttackAnimDelay);
        await CreatureCmd.Damage(choiceContext, target, 0m, ValueProp.Move, Owner, null);   // 造成伤害
        await PowerCmd.Remove<MinionAttackAction>(Owner);
    }
}
