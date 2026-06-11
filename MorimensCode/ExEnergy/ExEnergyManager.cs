using Godot;
using Morimens.Characters;
using STS2RitsuLib;
using STS2RitsuLib.Combat.SecondaryResources;

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
            smallIconPath: "res://Morimens/images/ui/Aliemus.png",
            largeIconPath: "res://Morimens/images/ui/Aliemus.png"
        ));
        AliemusId = AliemusDefinition.Id;

        KeyflareDefinition = registry.Register("keyflare", new SecondaryResourceDefinition(
            defaultAmount: 0,
            baseMaxAmount: 1000,
            turnStartPolicy: SecondaryResourceTurnStartPolicy.None,
            persistencePolicy: SecondaryResourcePersistencePolicy.Run,
            smallIconPath: "res://Morimens/images/ui/Keyflare.png",
            largeIconPath: "res://Morimens/images/ui/Keyflare.png"
        ));
        KeyflareId = KeyflareDefinition.Id;

        // 在 ModResources.Register() 中追加：

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
                    AmountLabelOffset = new Vector2(80, 0),
                    IconStyle = SecondaryResourceIconStyle.Default with
                    {
                        Size = new Vector2(80, 80),
                        HoverTip = SecondaryResourceHoverTipStyle.Default,
                    },
                });
                // 自由指定位置。例如这里我们找到能量计数器的位置，放在它旁边
                var energyCounter = parent.GetNode<Control>("%EnergyCounterContainer");
                row.Position = energyCounter.Position + new Vector2(0, -240);
                return row;
            },
            ctx => ctx.Node.Bind(ctx.Player)
        );

        registry.RegisterCombatUi(
            "keyflare_combat_ui",
            parent =>
            {
                var row = NSecondaryResourceCounter.Create(KeyflareDefinition, new SecondaryResourceCounterStyle
                {
                    FontSize = 32,
                    PositiveColor = Colors.Silver,
                    // FormatAmount = (amount, max) => amount.ToString(),
                    AmountLabelOffset = new Vector2(20, 20),
                    IconStyle = SecondaryResourceIconStyle.Default with
                    {
                        Size = new Vector2(80, 80),
                        HoverTip = SecondaryResourceHoverTipStyle.Default,
                    },
                });
                // 自由指定位置。例如这里我们找到能量计数器的位置，放在它旁边
                var energyCounter = parent.GetNode<Control>("%EnergyCounterContainer");
                row.Position = energyCounter.Position + new Vector2(0, -120);
                return row;
            },
            ctx => ctx.Node.Bind(ctx.Player)
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

        // 限定仅对特定角色始终显示
        // registry.AlwaysShowInCombatUiForCharacter<Doll>(AliemusDefinition.LocalId);
        // registry.AlwaysShowInCombatUiForCharacter<Doll>(KeyflareDefinition.LocalId);

        registry.RegisterCombatUiAlwaysVisibleWhen(AliemusDefinition.LocalId, IsMorimensCharacter);
        registry.RegisterCombatUiAlwaysVisibleWhen(KeyflareDefinition.LocalId, IsMorimensCharacter);

        // 永远显示（不受角色限制）
        // registry.AlwaysShowInCombatUi(AliemusDefinition.LocalId);

        RitsuLibFramework.SubscribeLifecycle<CardsFlushedEvent>(async evt =>
        {
            Entry.Logger.Debug($"回合結束：{evt.Player}");
            // TODO: 会经过 Gain Hook 修正，要改掉
            await SecondaryResourceCmd.Gain(evt.Player, AliemusId, 5, null);
        });
    }

    private static bool IsMorimensCharacter(SecondaryResourceCombatVisibilityContext context)
    {
        if (context.Player?.Character == null) return false;

        var type = context.Player.Character.GetType();
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(MorimensCharacter<,,>))
                return true;

            type = type.BaseType;
        }
        return false;
    }
}
