using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using GelesiyeWPF.Common;

namespace GelesiyeWPF.Views.Dialogs;

public partial class HakkindaWindow : Window
{
    public HakkindaWindow()
    {
        InitializeComponent();
        lblVersion.Text = string.Format(Services.LocalizationManager.T("Lbl_Versiyon"),
            $"{AppVersion.SemVer} (Win64) • b{AppVersion.Build} #{AppVersion.GitCommit}");
    }

    public static void ShowHakkinda(Window? owner = null)
    {
        var win = new HakkindaWindow();
        if (owner != null) win.Owner = owner;
        win.ShowDialog();
    }

    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    private void BtnKapat_Click(object sender, RoutedEventArgs e) => Close();

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) Close();
    }

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch { }
    }

    private void LnkWP_Click(object sender, RoutedEventArgs e) => OpenUrl("https://wa.me/905368888400");
    private void LnkLinkedIn_Click(object sender, RoutedEventArgs e) => OpenUrl("https://www.linkedin.com/in/cihadkocasahan/");
    private void LnkGitHub_Click(object sender, RoutedEventArgs e) => OpenUrl("https://github.com/cihadkocasahan");
    private void LnkEmail_Click(object sender, RoutedEventArgs e) => OpenUrl("mailto:cihadkocasahan@gmail.com");
}
