using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MinionLib.Commands;
using MinionLib.Minion;
using MorimensDoll.Anims;
using MorimensDoll.Minions;

namespace MorimensDoll.Minion;

public static class DollMinionCmd
{
    public static async Task Summon(PlayerChoiceContext choiceContext, Player player, CardModel? cardSource, decimal hp = DollMinion.MAX_HP, decimal atk = 1m)
    {
        await MinionCmd.AddMinion<DollMinion>(choiceContext, player, new MinionSummonOptions(
            MaxHp: hp,                  // 血量
            PrimaryStatAmount: atk,     // 力量
            Source: cardSource,             // 召喚來源（牌）
            Position: MinionPosition.Front)); // 站位但不重要因為 DollMinionLayout 會自動調整
    }

    public static async Task SummonHalfHP(PlayerChoiceContext choiceContext, Player player, CardModel? cardSource)
    {
        await Summon(choiceContext, player, cardSource, DollMinion.MAX_HP / 2, 1m);
    }

    public static async Task AttackRandomEnemy(PlayerChoiceContext choiceContext, DollMinion minion, CardModel? cardSource)
    {
        Creature? enemy = minion.Creature.PetOwner?.RunState.Rng.CombatTargets.NextItem(minion.CombatState.HittableEnemies);
        if (enemy == null)
            return;

        // await MinionAnimCmd.PlayBumpAttackAsync(minion, enemy); // 播放撞击动画
        await CreatureCmd.TriggerAnim(minion.Creature, DollSpine.State.Attack, DollSpine.AttackAnimDelay);
        await CreatureCmd.Damage(choiceContext, enemy, 0m, ValueProp.Move, minion.Creature, cardSource);
    }
}
