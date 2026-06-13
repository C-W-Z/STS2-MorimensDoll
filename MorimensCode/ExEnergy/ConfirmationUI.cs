using Godot;
using MegaCrit.Sts2.addons.mega_text;

namespace Morimens.ExEnergy;

public sealed partial class ConfirmationUi : Control
{
    private Panel _backgroundPanel = null!;
    private MegaRichTextLabel _titleLabel = null!;
    private MegaRichTextLabel _descriptionLabel = null!;
    private Button _confirmButton = null!;
    private Button _cancelButton = null!;
    private Action? _onConfirmAction;

    // TODO: 目前位置是用 1920x1080 來計算的，要測試在不同螢幕下的情況
    public override void _Ready()
    {
        _backgroundPanel = new Panel
        {
            Size = new Vector2(600f, 300f),
            Position = new Vector2(660f, 390f)
        };
        AddChild(_backgroundPanel);

        var panelStyle = new StyleBoxFlat
        {
            BgColor = new Color(0.12f, 0.1f, 0.08f, 0.98f),
            BorderWidthTop = 3,
            BorderWidthBottom = 3,
            BorderWidthLeft = 3,
            BorderWidthRight = 3,
            BorderColor = new Color(0.75f, 0.6f, 0.4f),
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6,
            CornerRadiusBottomRight = 6
        };
        _backgroundPanel.AddThemeStyleboxOverride("panel", panelStyle);

        // 按鈕的一般樣式（深色）
        var btnNormalStyle = new StyleBoxFlat
        {
            BgColor = new Color(0.2f, 0.16f, 0.13f),
            BorderWidthTop = 2,
            BorderWidthBottom = 2,
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            BorderColor = new Color(0.5f, 0.4f, 0.3f),
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomLeft = 4,
            CornerRadiusBottomRight = 4
        };

        // 按鈕滑鼠懸停樣式（變亮）
        var btnHoverStyle = new StyleBoxFlat
        {
            BgColor = new Color(0.3f, 0.24f, 0.2f),
            BorderWidthTop = 2,
            BorderWidthBottom = 2,
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            BorderColor = new Color(0.9f, 0.75f, 0.5f),
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomLeft = 4,
            CornerRadiusBottomRight = 4
        };

        _titleLabel = new MegaRichTextLabel
        {
            Position = new Vector2(40f, 20f),
            Size = new Vector2(520f, 45f),
            HorizontalAlignment = HorizontalAlignment.Center,
            MinFontSize = 20,
            MaxFontSize = 28
        };
        _backgroundPanel.AddChild(_titleLabel);

        _descriptionLabel = new MegaRichTextLabel
        {
            Position = new Vector2(50f, 80f),
            Size = new Vector2(500f, 115f),
            BbcodeEnabled = true,
            ScrollActive = false,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            // MinFontSize = 14,
            MaxFontSize = 24
        };
        _backgroundPanel.AddChild(_descriptionLabel);

        _confirmButton = new Button
        {
            Text = "確認釋放",
            Position = new Vector2(70f, 225f),
            Size = new Vector2(200f, 50f)
        };
        _confirmButton.AddThemeFontSizeOverride("font_size", 20);
        _confirmButton.AddThemeStyleboxOverride("normal", btnNormalStyle);
        _confirmButton.AddThemeStyleboxOverride("hover", btnHoverStyle);
        _confirmButton.Pressed += OnConfirmPressed;
        _backgroundPanel.AddChild(_confirmButton);

        _cancelButton = new Button
        {
            Text = "取消",
            Position = new Vector2(330f, 225f),
            Size = new Vector2(200f, 50f)
        };
        _cancelButton.AddThemeFontSizeOverride("font_size", 20);
        _cancelButton.AddThemeStyleboxOverride("normal", btnNormalStyle);
        _cancelButton.AddThemeStyleboxOverride("hover", btnHoverStyle);
        _cancelButton.Pressed += OnCancelPressed;
        _backgroundPanel.AddChild(_cancelButton);

        Visible = false;
    }

    public void Open(string title, string description, Action onConfirm)
    {
        _titleLabel.Text = title;
        _descriptionLabel.Text = description;
        _onConfirmAction = onConfirm;
        Visible = true;
    }

    private void OnConfirmPressed()
    {
        Visible = false;
        _onConfirmAction?.Invoke();
    }

    private void OnCancelPressed()
    {
        Visible = false;
    }
}
