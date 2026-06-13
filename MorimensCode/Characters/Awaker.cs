using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MinionLib.Powers.Patches;
using Morimens.Cards;
using Morimens.ExEnergy;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Scaffolding.Characters;

namespace Morimens.Characters;

public abstract class Awaker<TCardPool, TRelicPool, TPotionPool> : ModCharacterTemplate<TCardPool, TRelicPool, TPotionPool>, ISecondaryResourceHookListener, IAwaker
    where TCardPool : CardPoolModel
    where TRelicPool : RelicPoolModel
    where TPotionPool : PotionPoolModel
{
    public virtual int BaseAliemus => 100;
    public virtual int BaseKeyflare => 1000;

    // ─── 快取欄位 ───
    private CombatState? _cachedCombatState;
    private AbstractExaltCard? _cachedExaltCard;
    private AbstractExaltCard? _cachedOverExaltCard;

    // ─── 工廠方法 (Factory Methods) ───
    // 子類別只需要實作這兩個方法，回傳對應的卡牌範本即可
    protected abstract AbstractExaltCard CreateExaltCardInstance();
    protected abstract AbstractExaltCard CreateOverExaltCardInstance();

    // ─── 核心輔助方法：獲取並動態更新快取的卡牌 ───
    protected AbstractExaltCard GetExaltCard()
    {
        // 只有第一次會執行 ToMutable() 分配記憶體，後續皆重複使用
        var combatState = CombatManager.Instance._state;

        // 當戰鬥對局改變（例如重開局），或是快取尚未建立時，重新調用工廠獲取綁定新環境的卡牌
        if (_cachedExaltCard == null || _cachedCombatState != combatState)
        {
            _cachedCombatState = combatState;
            _cachedExaltCard = CreateExaltCardInstance(); // 重新 ToMutable() 完美向新對局注入 CombatState
        }

        _cachedExaltCard.UpgradePreviewType = CardUpgradePreviewType.Combat;

        if (combatState != null)
        {
            _cachedExaltCard.Owner ??= LocalContext.GetMe(combatState)!;
            _cachedExaltCard.DynamicVars.ClearPreview();
            _cachedExaltCard.UpdateDynamicVarPreview(CardPreviewMode.Normal, null, _cachedExaltCard.DynamicVars);
        }

        return _cachedExaltCard;
    }

    protected AbstractExaltCard GetOverExaltCard()
    {
        var combatState = CombatManager.Instance._state;

        if (_cachedOverExaltCard == null || _cachedCombatState != combatState)
        {
            _cachedCombatState = combatState;
            _cachedOverExaltCard = CreateOverExaltCardInstance();
        }

        _cachedOverExaltCard.UpgradePreviewType = CardUpgradePreviewType.Combat;

        if (combatState != null)
        {
            _cachedOverExaltCard.Owner ??= LocalContext.GetMe(combatState)!;
            _cachedOverExaltCard.DynamicVars.ClearPreview();
            _cachedOverExaltCard.UpdateDynamicVarPreview(CardPreviewMode.Normal, null, _cachedOverExaltCard.DynamicVars);
        }

        return _cachedOverExaltCard;
    }

    public virtual string ExaltTitle => GetExaltCard().Title;
    public virtual string ExaltDescription => GetExaltCard().GetDescriptionForPile(PileType.Hand);
    public virtual async Task Exalt() => await GetExaltCard().Execute();
    public virtual string OverExaltTitle => GetOverExaltCard().Title;
    public virtual string OverExaltDescription => GetOverExaltCard().GetDescriptionForPile(PileType.Hand);
    public virtual async Task OverExalt() => await GetOverExaltCard().Execute();

    public decimal ModifyMaxSecondaryResource(SecondaryResourceMaxContext context, decimal amount)
    {
        if (context.Definition.Id == ExEnergyManager.AliemusId)
            return amount + BaseAliemus - (ExEnergyManager.AliemusDefinition.BaseMaxAmount ?? 0);
        return amount;
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        Entry.Logger.Debug($"AfterDamageReceived: {dealer?.Name} deals to {target.Name}");
        if (dealer?.Side != CombatSide.Enemy)
            return;

        // 1. MinionLib 正在處理溢傷流程中 (IsHandling.Value == true)
        // 2. 當前觸發 Hook 的目標，正好是第一階段被壓制傷害的玩家 (SuppressedOwner.Value == originalTarget)
        // 3. 這次傷害結算是原版塞進去的 0 傷幽靈結果 (UnblockedDamage <= 0)
        if (MinionGuardianOverkillPatch.IsHandling.Value
            && MinionGuardianOverkillPatch.SuppressedOwner.Value == target
            && result.UnblockedDamage <= 0
            && !result.WasFullyBlocked)
        {
            Entry.Logger.Debug($"阻止 MinionGuardianOverkillPatch 的幽靈傷害觸發狂氣+1");
            return;
        }

        // 获得 1 点狂氣
        // TODO: 会经过 Gain Hook 修正，要改掉
        if (target.Player != null && LocalContext.IsMe(target.Player))
            await SecondaryResourceCmd.Gain(target.Player, ExEnergyManager.AliemusId, 1, this);
    }
}
