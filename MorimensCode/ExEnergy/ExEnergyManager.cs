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

        // 限定仅对特定角色始终显示
        // registry.AlwaysShowInCombatUiForCharacter<Doll>(AliemusDefinition.LocalId);
        // registry.AlwaysShowInCombatUiForCharacter<Doll>(KeyflareDefinition.LocalId);

        // registry.RegisterCombatUiAlwaysVisibleWhen(AliemusDefinition.LocalId, IsMorimensCharacter);
        // registry.RegisterCombatUiAlwaysVisibleWhen(KeyflareDefinition.LocalId, _ => false);

        // 永远显示（不受角色限制）
        // registry.AlwaysShowInCombatUi(AliemusDefinition.LocalId);

        // 战斗计数器。使用的图标就是你注册时提供的图标
        registry.RegisterCombatUi(
            "aliemus_combat_ui",
            parent =>
            {
                var row = NSecondaryResourceCounter.Create(AliemusDefinition, new SecondaryResourceCounterStyle
                {
                    FontSize = 32,
                    PositiveColor = Colors.Yellow,
                    // FormatAmount = (amount, max) => amount.ToString(),
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
                    // FormatAmount = (amount, max) => amount.ToString(),
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

        // 卡牌面上的次级资源费用显示。使用的图标就是你注册时提供的图标
        // registry.RegisterCardUi(
        //     "mana_card_ui",
        //     parent =>
        //     {
        //         var ui = NSecondaryResourceCardCostUi.Create(ManaId, new SecondaryResourceCardCostUiStyle
        //         {
        //             IconSize = new Vector2(48, 48),
        //             FontSize = 24,
        //         });
        //         // 自由指定位置。例如这里我们找到能量图标的位置，放在它旁边
        //         var energyIcon = parent.GetNode<TextureRect>("%EnergyIcon");
        //         ui.Position = energyIcon.Position + new Vector2(0, 80);
        //         return ui;
        //     },
        //     ctx => ctx.Node.Refresh(ctx)
        // );

        RitsuLibFramework.SubscribeLifecycle<CardsFlushedEvent>(async evt =>
        {
            Entry.Logger.Debug($"回合結束：{evt.Player}");
            if (evt.Player.Character is not IAwaker)
                return;
            // TODO: 会经过 Gain Hook 修正，要改掉
            await SecondaryResourceCmd.Gain(evt.Player, AliemusId, 5, null);
        });

        // 當 NCombatUi 生成時，自動把我們的 SkillConfirmationDialog 掛進去
        ModNodeAttachmentRegistry.For(Entry.ModId)
            .RegisterReadyChild<NCombatUi, SkillConfirmationDialog>(
                "skill_confirm_dialog",
                static _ => new SkillConfirmationDialog(),
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

    private static bool IsMorimensCharacter(SecondaryResourceCombatVisibilityContext context)
    {
        Entry.Logger.Debug($"IsMorimensCharacter: {context.Player?.Character}, {context.Player?.Character is IAwaker}");
        return context.Player?.Character is IAwaker;
    }

    private static void SetupExEnergyUi(NSecondaryResourceCounter counter)
    {
        // 異步等待 counter 真正進入場景樹並執行完 _Ready()
        counter.Ready += () =>
        {
            // 進到肚子裡尋找真正吃掉滑鼠事件的 NSecondaryResourceIcon
            NSecondaryResourceIcon? realIcon = null;
            foreach (var child in counter.GetChildren())
            {
                if (child is NSecondaryResourceIcon icon)
                {
                    realIcon = icon;
                    break;
                }
            }

            if (realIcon != null)
            {
                Entry.Logger.Debug("[ExEnergy] 成功找到內建 Icon 節點，開始綁定右鍵事件！");

                // 直接把右鍵監聽掛在 Icon 上
                realIcon.GuiInput += (inputEvent) => // 🔴 這裡拿掉 async，因為改在內部 Callback 處理
                {
                    if (inputEvent is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Right })
                    {
                        Entry.Logger.Debug("[ExEnergy] 檢測到右鍵點擊！準備彈出確認 UI！");

                        // 告訴 Godot 這個事件我們處理了，不要再往後傳
                        realIcon.AcceptEvent();

                        // ======= 🛠️ 這裡加入「尋找彈窗並打開」的邏輯 =======

                        // 1. 沿著 UI 樹往上爬，找出頂層的 NCombatUi
                        Node? current = counter;
                        while (current != null && current is not NCombatUi)
                        {
                            current = current.GetParent();
                        }

                        if (current is NCombatUi combatUi)
                        {
                            // 2. 透過 RitsuLib 的附加節點系統，抓到我們掛在戰鬥主畫面上的彈窗
                            if (ModNodeAttachmentRegistry.For(Entry.ModId)
                                .TryGetAttached<NCombatUi, SkillConfirmationDialog>(combatUi, "skill_confirm_dialog", out var dialog))
                            {
                                // 3. 亮出彈窗！把「點下確認後才要執行的非同步技能」包成 async lambda 丟進去
                                dialog.Open(async () =>
                                {
                                    Entry.Logger.Debug("[ExEnergy] 玩家點擊了確認！開始釋放技能！");
                                    await TryTriggerExalt();
                                });
                            }
                            else
                            {
                                Entry.Logger.Error("[ExEnergy] 錯誤：在 NCombatUi 上找不到註冊的 skill_confirm_dialog 附加節點！");
                            }
                        }
                        else
                        {
                            Entry.Logger.Error("[ExEnergy] 錯誤：找不到 NCombatUi 父節點，無法掛載彈窗！");
                        }
                        // =================================================
                    }
                };
            }
            else
            {
                Entry.Logger.Error("[ExEnergy] 錯誤：在 Counter 內找不到 NSecondaryResourceIcon 子節點！");
            }
        };
    }

    private static async Task TryTriggerExalt()
    {
        // 確保目前在戰鬥中，且輪到玩家的回合
        if (!CombatManager.Instance.IsInProgress || CombatManager.Instance._state?.CurrentSide != CombatSide.Player)
            return;

        Player? player = LocalContext.GetMe(CombatManager.Instance._state); // 獲取本地玩家
        if (player == null || player.Character is not IAwaker awaker)
            return;

        int aliemus = SecondaryResourceCmd.Get(player, AliemusId);
        int maxAliemus = SecondaryResourceCmd.GetMax(player, AliemusId) ?? awaker.BaseAliemus;

        if (aliemus < maxAliemus)
        {
            RitsuToastService.Show(new RitsuToastRequest(
                body: $"需要{maxAliemus}點狂氣才能釋放狂氣爆發。", // 正文，必填
                title: "狂氣不足", // 标题，可空
                               // image: null, // 左侧图片，可空
                level: RitsuToastLevel.Warning,
                durationSeconds: 3.0, // 显示秒数，null 用默认 3.5 秒
                                      // onClick: () => Entry.Logger.Info(""), // 点击正文时触发，可空
                                      // Fade：仅淡入淡出
                                      // FadeSlide：淡入淡出并滑动，全局默认
                                      // FadeScale：淡入淡出并缩放
                animationOverride: RitsuToastAnimationPreset.FadeSlide
            ));
            return;
        }

        await SecondaryResourceCmd.Lose(player, AliemusId, maxAliemus);
        await awaker.Exalt(player);
    }
}
