using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Morimens.Characters;
using STS2RitsuLib;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;
using STS2RitsuLib.Ui.Toast;

namespace Morimens.ExEnergy;

public static class ExEnergyManager
{
    private sealed class EnergySkillContext
    {
        public string ResourceId { get; init; } = "";
        public Func<IAwaker, int> GetBaseCost { get; init; } = null!;

        // 傳入(Awaker, 當前量, 上限值) 來決定實際數值與行為
        public Func<IAwaker, int, int, int> GetActualCost { get; init; } = null!;
        public Func<IAwaker, int, int, string> GetTitle { get; init; } = null!;
        public Func<IAwaker, int, int, string> GetDescription { get; init; } = null!;
        public Func<IAwaker, int, int, Func<IAwaker, Task>> GetExecuteCoreAction { get; init; } = null!;

        public LocString ToastTitle { get; init; } = null!;
        public LocString ToastBody { get; init; } = null!;
    }

    public static SecondaryResourceDefinition AliemusDefinition { get; private set; } = null!;
    public static SecondaryResourceDefinition KeyflareDefinition { get; private set; } = null!;
    public static string AliemusId { get; private set; } = string.Empty;
    public static string KeyflareId { get; private set; } = string.Empty;

    private const string ToastLocTable = "gameplay_ui";
    private const string ConfirmationUiLocalId = "confirmation_ui";
    // 這裡只宣告，不進行 Inline 初始化賦值
    private static readonly Dictionary<string, EnergySkillContext> EnergyContexts = new(StringComparer.OrdinalIgnoreCase);

    public static void Register()
    {
        var registry = RitsuLibFramework.GetSecondaryResourceRegistry(Entry.ModId);

        AliemusDefinition = registry.Register("aliemus", new SecondaryResourceDefinition(
            defaultAmount: 0,
            baseMaxAmount: 100,
            turnStartPolicy: SecondaryResourceTurnStartPolicy.None,
            persistencePolicy: SecondaryResourcePersistencePolicy.Run,
            smallIconPath: "res://Morimens/images/ui/AliemusText.png",
            largeIconPath: "res://Morimens/images/ui/Aliemus.png"
        ));
        AliemusId = AliemusDefinition.Id;

        KeyflareDefinition = registry.Register("keyflare", new SecondaryResourceDefinition(
            defaultAmount: 0,
            baseMaxAmount: 1000,
            turnStartPolicy: SecondaryResourceTurnStartPolicy.None,
            persistencePolicy: SecondaryResourcePersistencePolicy.Run,
            smallIconPath: "res://Morimens/images/ui/KeyflareText.png",
            largeIconPath: "res://Morimens/images/ui/Keyflare.png"
        ));
        KeyflareId = KeyflareDefinition.Id;

        // 確保 ID 都有值之後，再塞入字典中
        PopulateEnergyContexts();

        // 戰鬥計數器。使用的圖標就是你註冊時提供的圖標
        registry.RegisterCombatUi(
            "aliemus_combat_ui",
            parent =>
            {
                var row = NSecondaryResourceCounter.Create(AliemusDefinition, new SecondaryResourceCounterStyle
                {
                    FontSize = 32,
                    PositiveColor = Colors.Yellow,
                    AmountLabelOffset = new Vector2(100, 20),
                    IconStyle = SecondaryResourceIconStyle.Default with
                    {
                        Size = new Vector2(80, 80),
                        HoverTip = SecondaryResourceHoverTipStyle.Default with
                        {
                            ScreenOffset = new Vector2(150, -50),
                        }
                    },
                });
                // 自由指定位置。例如这里我们找到能量计数器的位置，放在它旁边
                var energyCounter = parent.GetNode<Control>("%EnergyCounterContainer");
                row.Position = energyCounter.Position + new Vector2(-80, -240);
                SetupExEnergyUi(row);
                return row;
            },
            ctx =>
            {
                // 只綁定在喚醒體身上
                // TODO: 會有問題是原版角色和其他模組角色如果拿到了喚醒體的牌，獲得狂氣後也無法顯示
                if (ctx.Player?.Character is IAwaker)
                    ctx.Node.Bind(ctx.Player);
            }
        );

        // TODO: NSecondaryResourceIcon._Ready()時將Icon改成各個鑰令的圖案
        registry.RegisterCombatUi(
            "keyflare_combat_ui",
            parent =>
            {
                var row = NSecondaryResourceCounter.Create(KeyflareDefinition, new SecondaryResourceCounterStyle
                {
                    FontSize = 32,
                    PositiveColor = Colors.Silver,
                    AmountLabelOffset = new Vector2(100, 20),
                    IconStyle = SecondaryResourceIconStyle.Default with
                    {
                        Size = new Vector2(80, 80),
                        HoverTip = SecondaryResourceHoverTipStyle.Default with
                        {
                            ScreenOffset = new Vector2(150, -50),
                        }
                    },
                });
                // 自由指定位置。例如这里我们找到能量计数器的位置，放在它旁边
                var energyCounter = parent.GetNode<Control>("%EnergyCounterContainer");
                row.Position = energyCounter.Position + new Vector2(-80, -120);
                SetupExEnergyUi(row);
                return row;
            },
            ctx =>
            {
                // 只綁定在喚醒體身上
                // TODO: 會有問題是原版角色和其他模組角色如果拿到了喚醒體的牌，獲得狂氣後也無法顯示
                if (ctx.Player?.Character is IAwaker)
                    ctx.Node.Bind(ctx.Player);
            }
        );

        RegisterSkillConfirmationUi();

        RegisterTurnEndAliemusGain();
    }

    // 抽取出來的字典配置方法
    private static void PopulateEnergyContexts()
    {
        EnergyContexts[AliemusId] = new EnergySkillContext
        {
            ResourceId = AliemusId,
            GetBaseCost = awaker => awaker.BaseAliemus,

            // 核心公式：達2倍上限則扣2倍(釋放超限)；否則消耗 = 上限 + 溢出部分的一半
            GetActualCost = (awaker, current, max) =>
                current >= max * 2
                    ? max * 2
                    : max + Math.Max(0, current - max) / 2,

            // 達2倍上限切換為超限爆發品類，否則為一般狂氣爆發
            GetTitle = (awaker, current, max) =>
                current >= max * 2 ? awaker.OverExaltTitle : awaker.ExaltTitle,

            GetDescription = (awaker, current, max) =>
                current >= max * 2 ? awaker.OverExaltDescription : awaker.ExaltDescription,

            GetExecuteCoreAction = (awaker, current, max) =>
                current >= max * 2
                    ? a => a.OverExalt()
                    : a => a.Exalt(),

            ToastTitle = new(ToastLocTable, "ALIEMUS_INSUFFICIENT.title"),
            ToastBody = new(ToastLocTable, "ALIEMUS_INSUFFICIENT.description")
        };

        EnergyContexts[KeyflareId] = new EnergySkillContext
        {
            ResourceId = KeyflareId,
            GetBaseCost = awaker => awaker.BaseKeyflare,

            // 永遠只消耗 1 倍上限
            GetActualCost = (awaker, current, max) => max,
            GetTitle = (awaker, current, max) => awaker.OverExaltTitle,
            GetDescription = (awaker, current, max) => awaker.OverExaltDescription,
            GetExecuteCoreAction = (awaker, current, max) => a => a.OverExalt(),

            ToastTitle = new(ToastLocTable, "KEYFLARE_INSUFFICIENT.title"),
            ToastBody = new(ToastLocTable, "KEYFLARE_INSUFFICIENT.description")
        };
    }

    private static void RegisterSkillConfirmationUi()
    {
        // 當 NCombatUi 生成時，自動把我們的 ConfirmationUi 掛進去
        ModNodeAttachmentRegistry.For(Entry.ModId)
            .RegisterReadyChild<NCombatUi, ConfirmationUi>(
                ConfirmationUiLocalId,
                static _ => new ConfirmationUi(),
                static (parent, node) =>
                {
                    // 讓彈窗鋪滿整個戰鬥 UI 或者是固定大小
                    node.Position = Vector2.Zero;
                    node.Size = parent.Size;
                },
                new NodeAttachmentOptions
                {
                    Name = "ConfirmationUi",
                    Order = 99, // 數字大一點，確保渲染在最上層
                    DuplicatePolicy = NodeAttachmentDuplicatePolicy.ReuseExistingByName
                });
    }

    private static void RegisterTurnEndAliemusGain()
    {
        RitsuLibFramework.SubscribeLifecycle<CardsFlushedEvent>(async evt =>
        {
            Entry.Logger.Debug($"回合結束：{evt.Player}");
            if (evt.Player.Character is not IAwaker)
                return;
            // TODO: 会经过 Gain Hook 修正，要改掉
            await SecondaryResourceCmd.Gain(evt.Player, AliemusId, 5, null);
        });
    }

    private static void SetupExEnergyUi(NSecondaryResourceCounter counter)
    {
        counter.Ready += () => OnCounterReady(counter);
    }

    private static void OnCounterReady(NSecondaryResourceCounter counter)
    {
        var energyId = GetResourceDefinitionId(counter);
        if (string.IsNullOrEmpty(energyId) || !EnergyContexts.ContainsKey(energyId))
            return;

        var realIcon = FindChildIcon(counter);
        if (realIcon == null)
            return;

        realIcon.GuiInput += (inputEvent) => OnIconGuiInput(inputEvent, counter, realIcon, energyId);
    }

    private static void OnIconGuiInput(InputEvent @event, NSecondaryResourceCounter counter, NSecondaryResourceIcon icon, string energyId)
    {
        if (@event is not InputEventMouseButton { Pressed: true })
            return;

        icon.AcceptEvent();

        if (!CombatManager.Instance.IsInProgress || CombatManager.Instance._state?.CurrentSide != CombatSide.Player)
            return;

        Player? player = LocalContext.GetMe(CombatManager.Instance._state);
        if (player == null || player.Character is not IAwaker awaker)
            return;

        EnergySkillContext context = EnergyContexts[energyId];

        // 1. 獲取當前基礎數值
        int currentAmount = SecondaryResourceCmd.Get(player, context.ResourceId);
        int baseMaxAmount = SecondaryResourceCmd.GetMax(player, context.ResourceId) ?? context.GetBaseCost(awaker);

        // 2. 透過 Context 策略動態計算實際所需的消耗量
        int requiredAmount = context.GetActualCost(awaker, currentAmount, baseMaxAmount);

        // 3. 檢查當前資源是否足夠 (若 Aliemus 不足基礎上限，requiredAmount會算成baseMaxAmount，完美擋下)
        if (currentAmount < requiredAmount)
        {
            ShowInsufficientToast(context.ToastTitle, context.ToastBody, requiredAmount);
            return;
        }

        // 4. 尋找 UI 樹中的 NCombatUi 與彈窗
        var combatUi = FindParentCombatUi(counter);
        if (combatUi == null)
            return;

        if (TryGetConfirmationDialog(combatUi, out var dialog))
        {
            // 5. 透過 Context 根據當前資源狀態動態取得對應的渲染文字與動作
            string title = context.GetTitle(awaker, currentAmount, baseMaxAmount);
            string description = context.GetDescription(awaker, currentAmount, baseMaxAmount);
            var executeAction = context.GetExecuteCoreAction(awaker, currentAmount, baseMaxAmount);

            dialog.Open(title, description, async () =>
            {
                await SecondaryResourceCmd.Lose(player, context.ResourceId, requiredAmount);
                await executeAction(awaker);
            });
        }
    }

    private static string? GetResourceDefinitionId(NSecondaryResourceCounter counter)
    {
        return counter.GetType()
            .GetField("_definition", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(counter) is SecondaryResourceDefinition def ? def.Id : null;
    }

    private static NSecondaryResourceIcon? FindChildIcon(Node parent)
    {
        foreach (var child in parent.GetChildren())
            if (child is NSecondaryResourceIcon icon)
                return icon;
        return null;
    }

    private static NCombatUi? FindParentCombatUi(Node? node)
    {
        while (node != null && node is not NCombatUi)
            node = node.GetParent();
        return node as NCombatUi;
    }

    private static bool TryGetConfirmationDialog(NCombatUi combatUi, out ConfirmationUi dialog)
    {
        return ModNodeAttachmentRegistry.For(Entry.ModId)
            .TryGetAttached(combatUi, ConfirmationUiLocalId, out dialog);
    }

    private static void ShowInsufficientToast(LocString titleLoc, LocString bodyLoc, int cost)
    {
        // 將動態數值注入本地化字串中，底層 SmartFormat 會自動替換樣版中的 {Cost}
        bodyLoc.AddObj("Cost", cost);

        RitsuToastService.Show(new RitsuToastRequest(
            body: bodyLoc.GetFormattedText(),       // 取得編譯格式化後的本地化文本
            title: titleLoc.GetFormattedText(),     // 取得本地化標題
            level: RitsuToastLevel.Warning,
            durationSeconds: 3.0,
            animationOverride: RitsuToastAnimationPreset.FadeSlide
        ));
    }
}
