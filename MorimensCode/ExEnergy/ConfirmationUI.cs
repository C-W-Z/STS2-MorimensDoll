using Godot;

namespace Morimens.ExEnergy;

public sealed partial class SkillConfirmationDialog : Control
{
    private Panel _backgroundPanel = null!;
    private Button _confirmButton = null!;
    private Button _cancelButton = null!;
    private Action? _onConfirmAction;

    public override void _Ready()
    {
        // 1. 建立一個置中的對話框面板
        _backgroundPanel = new Panel
        {
            Size = new Vector2(400f, 200f),
            // 這裡可以透過程式碼或樣式讓它居中，暫時給個固定坐標
            Position = new Vector2(760f, 440f)
        };
        AddChild(_backgroundPanel);

        // 2. 加上提示文字
        var label = new Label
        {
            Text = "確定要釋放技能嗎？",
            Position = new Vector2(50f, 40f),
            Size = new Vector2(300f, 40f)
        };
        _backgroundPanel.AddChild(label);

        // 3. 建立確認按鈕
        _confirmButton = new Button
        {
            Text = "確認釋放",
            Position = new Vector2(50f, 120f),
            Size = new Vector2(120f, 40f)
        };
        _confirmButton.Pressed += OnConfirmPressed;
        _backgroundPanel.AddChild(_confirmButton);

        // 4. 建立取消按鈕
        _cancelButton = new Button
        {
            Text = "取消",
            Position = new Vector2(230f, 120f),
            Size = new Vector2(120f, 40f)
        };
        _cancelButton.Pressed += OnCancelPressed;
        _backgroundPanel.AddChild(_cancelButton);

        // 預設隱藏，只有被呼叫時才顯示
        Visible = false;
    }

    // 開啟彈窗的方法，允許傳入「按了確認後要執行的代碼 (Callback)」
    public void Open(Action onConfirm)
    {
        _onConfirmAction = onConfirm;
        Visible = true;
    }

    private void OnConfirmPressed()
    {
        Visible = false;
        _onConfirmAction?.Invoke(); // 執行傳進來的技能釋放邏輯
    }

    private void OnCancelPressed()
    {
        Visible = false; // 單純關閉，什麼都不做
    }
}
