using System.Reflection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MorimensDoll.Utils;

public static class PowerUtils
{
    // 效能優化：快取反射結果，避免每次召喚/合體都重新搜尋 MethodInfo
    private static MethodInfo? _cachedApplyMethod;

    /// <summary>
    /// 內部共用：精準獲取 PowerCmd.Apply 的單體泛型方法定義
    /// </summary>
    private static MethodInfo? GetApplyMethodDefinition()
    {
        if (_cachedApplyMethod == null)
        {
            _cachedApplyMethod = typeof(PowerCmd).GetMethods()
                .FirstOrDefault(m => m.Name == "Apply"
                                  && m.IsGenericMethod
                                  && m.GetParameters().Length == 6
                                  && m.GetParameters()[1].ParameterType == typeof(Creature));
        }
        return _cachedApplyMethod;
    }

    /// <summary>
    /// 共用功能：將指定的複數 Power 動態且依序掛載到目標生物身上
    /// </summary>
    public static async Task ApplyPowersDynamically(
        PlayerChoiceContext choiceContext,
        Creature target,
        IEnumerable<PowerModel> powers,
        Player player,
        CardModel? cardSource)
    {
        if (target == null || powers == null || !powers.Any()) return;

        MethodInfo? applyMethodDefinition = GetApplyMethodDefinition();

        // 安全防禦：萬一未來原版改版導致反射失敗，至少把最核心的力量（Strength）補上去
        if (applyMethodDefinition == null)
        {
            decimal totalStrength = powers.Where(p => p is StrengthPower).Sum(p => p.Amount);
            if (totalStrength > 0)
                await PowerCmd.Apply<StrengthPower>(choiceContext, target, totalStrength, player.Creature, cardSource);
            return;
        }

        // 依序動態呼叫泛型 Apply
        foreach (var p in powers)
        {
            MethodInfo genericMethod = applyMethodDefinition.MakeGenericMethod(p.GetType());

            object? resultTask = genericMethod.Invoke(null, [
                choiceContext,
                target,
                (decimal)p.Amount,
                player.Creature,
                cardSource,
                false // silent = false
            ]);

            if (resultTask is Task task)
                await task;
        }
    }
}
