using System.Reflection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MinionLib.Commands;
using MinionLib.Minion;
using MorimensDoll.Anims;
using MorimensDoll.Minions;

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
        return await Summon(choiceContext, player, cardSource,
            origin.Creature.MaxHp,
            origin.Creature.GetPowerAmount<StrengthPower>(),
            origin.Creature.CurrentHp);
    }

    public static async Task<Creature> MergeAllDollMinions(PlayerChoiceContext choiceContext, Player player, CardModel? cardSource)
    {
        IEnumerable<DollMinion> minions = GetAllDollMinions(player);
        decimal maxHp = 0;
        decimal atk = 0;
        decimal hp = 0;
        List<PowerModel> powers = [];

        foreach (var minion in minions)
        {
            maxHp += minion.Creature.MaxHp;
            hp += minion.Creature.CurrentHp;
            atk += minion.Creature.GetPowerAmount<StrengthPower>();
            powers.AddRange([.. minion.Creature.Powers]);
            await CreatureCmd.Kill(minion.Creature);
        }

        // 召喚合體後的新隨從
        Creature newMinion = await Summon(choiceContext, player, cardSource, maxHp, 0, hp);

        // 不能寫 PowerCmd.Apply<power.GetType()>()，所以要透過反射

        // 透過反射，從 PowerCmd 類別中找出名為 "Apply" 且擁有 5 個參數，第二個參數為單體 Creature 的泛型方法定義
        MethodInfo? applyMethodDefinition = typeof(PowerCmd).GetMethods()
            .FirstOrDefault(m => m.Name == "Apply"
                              && m.IsGenericMethod
                              && m.GetParameters().Length == 6 // 包含 silent 共有 6 個參數
                              && m.GetParameters()[1].ParameterType == typeof(Creature));

        if (applyMethodDefinition == null)
        {
            // 找不到方法的安全防禦
            await PowerCmd.Apply<StrengthPower>(choiceContext, newMinion, atk, player.Creature, cardSource);
            return newMinion;
        }

        foreach (var p in powers)
        {
            // 將當前能力的執行期型別 (p.GetType()) 填入泛型定義中
            MethodInfo genericMethod = applyMethodDefinition.MakeGenericMethod(p.GetType());

            // 執行該方法。因為是靜態方法，第一個參數傳 null；第二個參數傳入對應的參數陣列
            object? resultTask = genericMethod.Invoke(null, [
                choiceContext,
                newMinion,
                p.Amount,
                player.Creature,
                cardSource
            ]);

            // 因為 Apply 是 async Task，將傳回值轉為 Task 並 await 它，確保能力依序掛載
            if (resultTask is Task task)
                await task;
        }

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

    public static IEnumerable<DollMinion> GetAllDollMinions(Player player)
    {
        IEnumerable<Creature>? pets = player.PlayerCombatState?.Pets;
        if (pets == null)
            return [];
        return pets.Select(p => p.Monster).OfType<DollMinion>();
    }
}
