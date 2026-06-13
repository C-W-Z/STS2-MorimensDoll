using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Morimens.Characters;
using STS2RitsuLib;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;
using STS2RitsuLib.Ui.Toast;

namespace Morimens.ExEnergy;

public static class ExEnergyManager
{
    public static SecondaryResourceDefinition AliemusDefinition { get; private set; } = null!;
    public static SecondaryResourceDefinition KeyflareDefinition { get; private set; } = null!;
    public static string AliemusId { get; private set; } = string.Empty;
    public static string KeyflareId { get; private set; } = string.Empty;

    // 🔴 修正 1：這裡只宣告，不進行 Inline 初始化賦值
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

        // 🔴 修正 2：確保 ID 都有值之後，再塞入字典中
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

        RitsuLibFramework.SubscribeLifecycle<CardsFlushedEvent>(async evt =>
        {
            Entry.Logger.Debug($"回合結束：{evt.Player}");
            if (evt.Player.Character is not IAwaker)
                return;
            // TODO: 会经过 Gain Hook 修正，要改掉
            await SecondaryResourceCmd.Gain(evt.Player, AliemusId, 5, null);
        });

        RegisterSkillConfirmationUi();
    }

    // 🔴 修正 3：抽取出來的字典配置方法
    private static void PopulateEnergyContexts()
    {
        EnergyContexts[AliemusId] = new EnergySkillContext
        {
            ResourceId = AliemusId,
            GetBaseCost = awaker => awaker.BaseAliemus,
            GetTitle = awaker => awaker.ExaltTitle,
            GetDescription = awaker => awaker.ExaltDescription,
            ExecuteCoreAction = (awaker, player) => awaker.Exalt(player),
            ToastTitle = "狂氣不足",
            ToastMessageSuffix = "點狂氣才能釋放狂氣爆發。"
        };

        EnergyContexts[KeyflareId] = new EnergySkillContext
        {
            ResourceId = KeyflareId,
            GetBaseCost = awaker => awaker.BaseKeyflare,
            GetTitle = awaker => awaker.SuperExaltTitle,
            GetDescription = awaker => awaker.SuperExaltDescription,
            ExecuteCoreAction = (awaker, player) => awaker.SuperExalt(player),
            ToastTitle = "能量不足",
            ToastMessageSuffix = "點能量才能釋放超凡爆發。"
        };
    }

    private static void RegisterSkillConfirmationUi()
    {
        // 當 NCombatUi 生成時，自動把我們的 SkillConfirmationDialog 掛進去
        ModNodeAttachmentRegistry.For(Entry.ModId)
            .RegisterReadyChild<NCombatUi, ConfirmationUi>(
                "skill_confirm_dialog",
                static _ => new ConfirmationUi(),
                static (parent, node) =>
                {
                    // 讓彈窗鋪滿整個戰鬥 UI 或者是固定大小
                    node.Position = Vector2.Zero;
                    node.Size = parent.Size;
                },
                new NodeAttachmentOptions
                {
                    Name = "SkillConfirmationDialog",
                    Order = 99, // 數字大一點，確保渲染在最上層
                    DuplicatePolicy = NodeAttachmentDuplicatePolicy.ReuseExistingByName
                });
    }

    private sealed class EnergySkillContext
    {
        public string ResourceId { get; init; } = "";
        public Func<IAwaker, int> GetBaseCost { get; init; } = null!;
        public Func<IAwaker, string> GetTitle { get; init; } = null!;
        public Func<IAwaker, string> GetDescription { get; init; } = null!;
        public Func<IAwaker, Player, Task> ExecuteCoreAction { get; init; } = null!;
        public string ToastTitle { get; init; } = "";
        public string ToastMessageSuffix { get; init; } = "";
    }

    private static void SetupExEnergyUi(NSecondaryResourceCounter counter)
    {
        counter.Ready += () => OnCounterReady(counter);
    }

    private static void OnCounterReady(NSecondaryResourceCounter counter)
    {
        var energyId = GetResourceDefinitionId(counter);
        if (string.IsNullOrEmpty(energyId) || !EnergyContexts.ContainsKey(energyId)) return;

        var realIcon = FindChildIcon(counter);
        if (realIcon == null) return;

        realIcon.GuiInput += (inputEvent) => OnIconGuiInput(inputEvent, counter, realIcon, energyId);
    }

    private static void OnIconGuiInput(InputEvent @event, NSecondaryResourceCounter counter, NSecondaryResourceIcon icon, string energyId)
    {
        if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Right }) return;
        icon.AcceptEvent();

        if (!CombatManager.Instance.IsInProgress || CombatManager.Instance._state?.CurrentSide != CombatSide.Player)
            return;

        Player? player = LocalContext.GetMe(CombatManager.Instance._state);
        if (player == null || player.Character is not IAwaker awaker)
            return;

        var context = EnergyContexts[energyId];

        int currentAmount = SecondaryResourceCmd.Get(player, context.ResourceId);
        int requiredAmount = SecondaryResourceCmd.GetMax(player, context.ResourceId) ?? context.GetBaseCost(awaker);

        if (currentAmount < requiredAmount)
        {
            ShowInsufficientToast(context.ToastTitle, requiredAmount, context.ToastMessageSuffix);
            return;
        }

        // 4. 尋找 UI 樹中的 NCombatUi 與彈窗
        var combatUi = FindParentCombatUi(counter);
        if (combatUi == null) return;

        if (TryGetConfirmationDialog(combatUi, out var dialog))
        {
            dialog.Open(context.GetTitle(awaker), context.GetDescription(awaker), async () =>
            {
                await SecondaryResourceCmd.Lose(player, context.ResourceId, requiredAmount);
                await context.ExecuteCoreAction(awaker, player);
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
            .TryGetAttached(combatUi, "skill_confirm_dialog", out dialog);
    }

    private static void ShowInsufficientToast(string title, int cost, string messageSuffix)
    {
        RitsuToastService.Show(new RitsuToastRequest(
            body: $"需要{cost}{messageSuffix}",
            title: title,
            level: RitsuToastLevel.Warning,
            durationSeconds: 3.0,
            animationOverride: RitsuToastAnimationPreset.FadeSlide
        ));
    }
}
