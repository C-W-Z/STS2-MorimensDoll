using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace Morimens.Anims;

public static class DollSpine
{
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

    public static float AttackAnimDelay { get; } = 0f;
    public static float CastAnimDelay { get; }  = 0f;
    public static float Skill1AnimDelay { get; }  = 0f;
    public static float Skill2AnimDelay { get; }  = 0f;
    public static float ExaltAnimDelay { get; }  = 0f;
    public static float ExSkillAnimDelay { get; }  = 0f;

    public static CreatureAnimator GetCreatureAnimator(MegaSprite controller)
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
        creatureAnimator.AddAnyState(State.Dead, hit);
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
                case State.Exalt:
                    track.SetTrackTime(0.5f);
                    track.SetTimeScale(1.5f);
                    break;
                case State.ExSkill:
                    track.SetTrackTime(0.5f);
                    track.SetTimeScale(2.0f);
                    break;
            }
        }));

        return creatureAnimator;
    }
}
