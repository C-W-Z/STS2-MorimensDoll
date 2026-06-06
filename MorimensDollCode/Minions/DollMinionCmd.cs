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
using MorimensDoll.Utils;

namespace MorimensDoll.Minion;

public static class DollMinionCmd
{
    public static async Task<Creature> Summon(PlayerChoiceContext choiceContext, Player player, CardModel? cardSource, decimal? maxHp = null, decimal atk = 1m, decimal? hp = null)
    {
        return await MinionCmd.AddMinion<DollMinion>(choiceContext, player, new MinionSummonOptions(
            MaxHp: maxHp,               // 血量
            PrimaryStatAmount: atk,     // 力量
            SecondaryStatAmount: hp,    // 目前血量
            Source: cardSource,         // 召喚來源（牌）
            Position: MinionPosition.Front)); // 站位但不重要因為 DollMinionLayout 會自動調整
    }

    public static async Task<Creature> SummonCopy(PlayerChoiceContext choiceContext, Player player, DollMinion origin, CardModel? cardSource)
    {
        var powers = origin.Creature.Powers.ToList();
        Creature newMinion = await Summon(choiceContext, player, cardSource, origin.Creature.MaxHp, 0, origin.Creature.CurrentHp);
        // TODO: 用PowerCmd.Apply會受到能力加成影響（如力量加成、災厄加成等），要改成更底層的賦予powers
        await PowerUtils.ApplyPowersDynamically(choiceContext, newMinion, powers, player, cardSource);
        return newMinion;
    }

    public static async Task<Creature> MergeAllDollMinions(PlayerChoiceContext choiceContext, Player player, CardModel? cardSource)
    {
        IEnumerable<DollMinion> minions = GetAllDollMinions(player);
        decimal maxHp = 0;
        decimal hp = 0;
        List<PowerModel> powers = [];

        foreach (var minion in minions)
        {
            maxHp += minion.Creature.MaxHp;
            hp += minion.Creature.CurrentHp;
            powers.AddRange([.. minion.Creature.Powers]); // 收集所有能力
            // TODO: 是否要用殺死這件事情有待商榷，可能會有意外情況，或者導致太強？
            await CreatureCmd.Kill(minion.Creature);
        }

        Creature newMinion = await Summon(choiceContext, player, cardSource, maxHp, 0, hp);
        // TODO: 用PowerCmd.Apply會受到能力加成影響（如力量加成、災厄加成等），要改成更底層的賦予powers
        await PowerUtils.ApplyPowersDynamically(choiceContext, newMinion, powers, player, cardSource);
        return newMinion;
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

    public static List<DollMinion> GetAllDollMinions(Player player)
    {
        IEnumerable<Creature>? pets = player.PlayerCombatState?.Pets;
        if (pets == null)
            return [];
        return [.. pets.Select(p => p.Monster).OfType<DollMinion>()];
    }
}
