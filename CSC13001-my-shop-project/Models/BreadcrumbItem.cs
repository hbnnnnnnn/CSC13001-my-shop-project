using System.Windows.Input;

namespace CSC13001_my_shop_project.Models;

public sealed class BreadcrumbItem
{
    public string Label { get; set; } = "";
    public bool IsClickable { get; set; }
    public ICommand? NavigateCommand { get; set; }
    public object? CommandParameter { get; set; }
}
