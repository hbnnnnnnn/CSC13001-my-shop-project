namespace CSC13001_my_shop_project.Presentation;

public sealed partial class Shell : UserControl, IContentControlProvider
{
    public Shell()
    {
        this.InitializeComponent();
        DataContext = new ShellViewModel();
    }

    public ContentControl ContentControl => MainContent;
}
