using MegaCrit.Sts2.Core.Models;
using MinionLib.Layout;
using MinionLib.Minion;
using Morimens.Minions;
using Morimens.Powers;

namespace Morimens.Minions;

public class DollMinionLayout : IMinionLayout
{
    // 讓這個佈局器一直保持啟用
    public bool IsActive => true;

    public void ApplyLayout(MinionLayoutContext context)
    {
        // 1. 找出當前戰鬥房間中所有未處理隨從的主人
        var owners = context.UnhandledMinions
            .Select(c => c.Entity.PetOwner)
            .Where(o => o != null)
            .Distinct();

        foreach (var owner in owners)
        {
            if (owner?.PlayerCombatState?.Pets == null) continue;

            // 2. 動態計算當前的前排最大上限（基礎值 + Power 帶來的附加值）
            int currentFrontLimit = DollMinion.BASE_FRONT_LIMIT + owner.Creature.GetPowerAmount<MinionLimitUpPower>();

            int frontCount = 0;

            // 3. 依照 Pets 隊列的自然順序（召喚順序）重新分組
            foreach (var petEntity in owner.PlayerCombatState.Pets)
            {
                if (petEntity.Monster is DollMinion minion)
                {
                    if (frontCount < currentFrontLimit)
                    {
                        minion.Position = MinionPosition.Front;
                        frontCount++;
                        if (!minion.Creature.HasPower<MinionGuardianPower>())
                            ModelDb.Power<MinionGuardianPower>().ToMutable().ApplyInternal(minion.Creature, 1);
                    }
                    else
                    {
                        // 超出總上限時的防呆，擠在後排
                        minion.Position = MinionPosition.Back;
                        minion.Creature.GetPower<MinionGuardianPower>()?.RemoveInternal();
                    }
                }
            }
        }

        // 關鍵點：我們在這個方法裡「不」向 context.Positions 寫入任何資料。
        // 這會讓所有隨從維持在 "Unhandled" 狀態，留給下一個優先級低的 DefaultMinionLayout 繪製坐標。
    }
}
