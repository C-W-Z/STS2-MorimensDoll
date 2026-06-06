using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Godot;

namespace MorimensDoll.Characters;

[RegisterCharacter]
public sealed class Doll : ModCharacterTemplate<DollCardPool, DollRelicPool, DollPotionPool>
{
    public static readonly Color ThemeColor = new(0.42f, 0.65f, 0.72f);

    private const string SceneRoot = $"{Entry.ResPath}/scenes/characters";
    private const string ImageRoot = $"{Entry.ResPath}/images/characters";
    private const string CharacterScenePath = $"{SceneRoot}/Doll_character.tscn";
    private const string EnergyCounterScenePath = $"{SceneRoot}/Doll_energy_counter.tscn";
    private const string MerchantScenePath = $"{SceneRoot}/Doll_merchant.tscn";
    private const string RestSiteScenePath = $"{SceneRoot}/Doll_rest_site.tscn";
    private const string CharacterSelectBgScenePath = $"{SceneRoot}/Doll_character_select_bg.tscn";

    // 角色名称颜色。
    public override Color NameColor => ThemeColor;
    // 能量图标轮廓颜色。
    public override Color EnergyLabelOutlineColor => new(0.08f, 0.18f, 0.24f);
    // 地图绘制颜色。
    public override Color MapDrawingColor => ThemeColor;

    // 人物性别（男女中立）。
    public override CharacterGender Gender => CharacterGender.Neutral;

    // 初始血量和金币。
    public override int StartingHp => 75;
    public override int StartingGold => 99;

    // CharacterAssetProfile 按类别拆分。你只写需要替换的部分，其他字段会保留回退。
    // AssetProfile 只指定模板自带的静态占位资源；没有复制的音频、拖尾、转场等资源继续从占位角色回退。
    public override CharacterAssetProfile AssetProfile => new(
        Scenes: new CharacterSceneAssetSet(
            // 人物模型 tscn 路径。
            VisualsPath: CharacterScenePath,
            // 能量表盘 tscn 路径。
            EnergyCounterPath: EnergyCounterScenePath,
            // 商店人物场景。
            MerchantAnimPath: MerchantScenePath,
            // 篝火休息场景。
            RestSiteAnimPath: RestSiteScenePath),
        Ui: new CharacterUiAssetSet(
            // 人物头像路径。
            IconTexturePath: $"{ImageRoot}/Doll_character_icon.png",
            // 人物头像轮廓。
            IconOutlineTexturePath: $"{ImageRoot}/Doll_character_icon_outline.png",
            // 人物选择背景。
            CharacterSelectBgPath: CharacterSelectBgScenePath,
            // 人物选择图标。
            CharacterSelectIconPath: $"{ImageRoot}/Doll_character_select.png",
            // 人物选择图标-锁定状态。
            CharacterSelectLockedIconPath: $"{ImageRoot}/Doll_character_select_locked.png",
            // 地图上的角色标记图标、表情轮盘上的角色头像。
            MapMarkerPath: $"{ImageRoot}/Doll_map_marker.png"),
        Audio: new CharacterAudioAssetSet(
            AttackSfx: "event:/MorimensDoll/sfx/Doll/Attack",
            CastSfx: "event:/MorimensDoll/sfx/Doll/Cast",
            DeathSfx: "event:/MorimensDoll/sfx/Doll/Death"
        ));

    // 某个字段没写时，RitsuLib 会从占位角色配置里补齐。
    public override string? PlaceholderCharacterId => "necrobinder";
    // 如果你的人物不需要时间线小故事，加上这句。
    public override bool RequiresEpochAndTimeline => false;
    // 攻击和施法动画延迟，以对齐动画。静态占位资源不需要延迟。
    public override float AttackAnimDelay => 0f;
    public override float CastAnimDelay => 0f;

    // 让 RitsuLib 把普通 Godot 场景转换成游戏需要的 NCreatureVisuals。
    // 自动转换人物场景，让你不需要手动挂脚本。复制即可。
    protected override NCreatureVisuals? TryCreateCreatureVisuals()
    {
        return RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(
            CharacterScenePath);
    }

    // 攻击建筑师的攻击特效列表。
    public override List<string> GetArchitectAttackVfx()
    {
        return
        [
            "vfx/vfx_attack_blunt",
            "vfx/vfx_heavy_blunt",
            "vfx/vfx_attack_slash",
            "vfx/vfx_bloody_impact",
            "vfx/vfx_rock_shatter"
        ];
    }

    public static class State
    {
        public const string Idle = "Idle";
        public const string Relaxed = "Relaxed";
        public const string Dead = "Dead";
        public const string Attack = "Attack";
        public const string Cast = "Cast";
        public const string Skill1 = "Skill1";
        public const string Skill2 = "Skill2";
        public const string Exalt = "Exalt";
        public const string ExSkill = "ExSkill";
        public const string Hit = "Hit";
        public const string Move = "Move";
        public const string Back = "Back";
    }

    protected override CreatureAnimator? SetupCustomCreatureAnimator(MegaSprite controller)
    {
        const string STATE_IDLE = "Idle_1";
        const string STATE_ATTACK = "Attack";
        const string STATE_DEFENCE = "Defence";
        const string STATE_SKILL1 = "Skill1";
        const string STATE_SKILL2 = "Skill2";
        const string STATE_EXALT = "Exalt";
        const string STATE_EXSKILL = "ExSkill";
        const string STATE_HIT = "Hit";
        const string STATE_MOVE = "Move";
        const string STATE_BACK = "Back";

        // 设定动画名和是否循环播放
        AnimState idle = new(STATE_IDLE, isLooping: true);
        AnimState attack = new(STATE_ATTACK);
        AnimState defence = new(STATE_DEFENCE);
        AnimState skill1 = new(STATE_SKILL1);
        AnimState skill2 = new(STATE_SKILL2);
        AnimState exalt = new(STATE_EXALT);
        AnimState exSkill = new(STATE_EXSKILL);
        AnimState hit = new(STATE_HIT);
        AnimState move = new(STATE_MOVE);
        AnimState back = new(STATE_BACK);

        // 设定播放后自动跳转，例如这里都是返回idle
        attack.NextState = idle;
        defence.NextState = idle;
        skill1.NextState = idle;
        skill2.NextState = idle;
        exalt.NextState = idle;
        exSkill.NextState = idle;
        hit.NextState = idle;
        move.NextState = idle;
        back.NextState = idle;

        // 绑定播放动画名
        CreatureAnimator creatureAnimator = new(idle, controller);
        creatureAnimator.AddAnyState(State.Idle, idle);
        creatureAnimator.AddAnyState(State.Relaxed, idle);
        creatureAnimator.AddAnyState(State.Dead, idle);
        creatureAnimator.AddAnyState(State.Attack, attack);
        creatureAnimator.AddAnyState(State.Cast, defence);
        creatureAnimator.AddAnyState(State.Skill1, skill1);
        creatureAnimator.AddAnyState(State.Skill2, skill2);
        creatureAnimator.AddAnyState(State.Exalt, exalt);
        creatureAnimator.AddAnyState(State.ExSkill, exSkill);
        creatureAnimator.AddAnyState(State.Hit, hit);
        creatureAnimator.AddAnyState(State.Move, move);
        creatureAnimator.AddAnyState(State.Back, back);

        // 訂閱動畫啟動訊號，做全局自動設定
        controller.ConnectAnimationStarted(Callable.From<GodotObject, GodotObject, GodotObject>((sprite, animState, trackEntryObj) =>
        {
            // 將 Godot 傳進來的原生指標轉化為 MegaCrit 的 MegaTrackEntry 物件
            MegaTrackEntry track = new(Variant.From(trackEntryObj));

            // 安全地透過底層 Spine 物件取得該動畫的字串名稱（例如 "Attack"）
            string currentAnimName = track.GetAnimation().BoundObject.Call("get_name").AsString();

            switch (currentAnimName)
            {
                case State.Attack:
                    track.SetTrackTime(0.45f); // 永遠從 0.45 秒的位置開始
                    break;
                case State.Cast:
                    track.SetTrackTime(0.1f); // 永遠從 0.1 秒的位置開始
                    track.SetTimeScale(1.5f); // 永遠 1.5 倍速播放
                    break;
            }
        }));

        return creatureAnimator;
    }
}
