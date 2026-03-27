using Windows.UI;

namespace CSC13001_my_shop_project.Presentation.Controls;

public class CustomCheckBox : CheckBox
{
    private Border? _border;
    private TextBlock? _glyph;

    private static readonly SolidColorBrush CheckedBrush = 
        new SolidColorBrush(Color.FromArgb(0xFF, 0xF3, 0xB5, 0x5C));
    private static readonly SolidColorBrush UncheckedBackground = 
        new SolidColorBrush(Color.FromArgb(0x80, 0xF5, 0xED, 0xE4));
    private static readonly SolidColorBrush UncheckedBorder = 
        new SolidColorBrush(Color.FromArgb(0xFF, 0xE8, 0xDD, 0xD4));
    private static readonly SolidColorBrush PointerOverBorder = 
        new SolidColorBrush(Color.FromArgb(0xFF, 0x63, 0x66, 0xF1));

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _border = GetTemplateChild("CheckBoxBorder") as Border;
        _glyph = GetTemplateChild("CheckGlyph") as TextBlock;

        Checked += (s, e) => ApplyCheckedState();
        Unchecked += (s, e) => ApplyUncheckedState();
        PointerEntered += (s, e) => ApplyPointerOverState();
        PointerExited += (s, e) => ApplyPointerExitState();

        if (IsChecked == true) ApplyCheckedState();
        else ApplyUncheckedState();
    }

    private void ApplyCheckedState()
    {
        if (_border == null) return;
        _border.Background = CheckedBrush;
        _border.BorderBrush = CheckedBrush;
        if (_glyph != null) _glyph.Visibility = Visibility.Visible;
    }

    private void ApplyUncheckedState()
    {
        if (_border == null) return;
        _border.Background = UncheckedBackground;
        _border.BorderBrush = UncheckedBorder;
        if (_glyph != null) _glyph.Visibility = Visibility.Collapsed;
    }

    private void ApplyPointerOverState()
    {
        if (_border == null) return;
        _border.BorderBrush = PointerOverBorder;
    }

    private void ApplyPointerExitState()
    {
        if (_border == null) return;
        _border.BorderBrush = IsChecked == true ? CheckedBrush : UncheckedBorder;
    }
}