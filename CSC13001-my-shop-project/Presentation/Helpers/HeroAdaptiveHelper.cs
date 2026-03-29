namespace CSC13001_my_shop_project.Presentation.Helpers;

public static class HeroAdaptiveHelper
{
    public static void Apply(double width, double minWidth, UIElement heroPanel, ColumnDefinition? heroColumn, GridLength? visibleWidth)
    {
        if (width >= minWidth)
        {
            heroPanel.Visibility = Visibility.Visible;
            if (heroColumn != null && visibleWidth != null)
                heroColumn.Width = visibleWidth.Value;
        }
        else
        {
            // heroPanel.Visibility = Visibility.Collapsed;
            if (heroColumn != null)
                heroColumn.Width = new GridLength(0);
        }
    }
}