using System.Windows;
using System.Windows.Input;

namespace GelesiyeWPF.Views.Dialogs;

public enum CloseActionResult
{
    Cancel,
    Minimize,
    Exit
}

public partial class CloseActionWindow : Window
{
    public CloseActionResult Result { get; private set; } = CloseActionResult.Cancel;
    public bool Remember => chkRemember.IsChecked == true;

    public CloseActionWindow()
    {
        InitializeComponent();
    }

    public static (CloseActionResult action, bool remember) Prompt(Window? owner = null)
    {
        var win = new CloseActionWindow();
        if (owner != null) win.Owner = owner;
        win.ShowDialog();
        return (win.Result, win.Remember);
    }

    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    private void BtnMinimize_Click(object sender, RoutedEventArgs e)
    {
        Result = CloseActionResult.Minimize;
        DialogResult = true;
    }

    private void BtnExit_Click(object sender, RoutedEventArgs e)
    {
        Result = CloseActionResult.Exit;
        DialogResult = true;
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        Result = CloseActionResult.Cancel;
        DialogResult = false;
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Result = CloseActionResult.Cancel;
            DialogResult = false;
        }
    }
}
