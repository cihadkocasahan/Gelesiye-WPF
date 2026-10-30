using System.Windows;
using System.Windows.Input;

namespace GelesiyeWPF.Views.Dialogs;

public enum RaporSecimResult
{
    Cancel,
    SeciliEkstre,
    CariOzet,
    GenelHareketler
}

public partial class RaporSecimWindow : Window
{
    private RaporSecimResult _seciliSecenek = RaporSecimResult.CariOzet;

    public RaporSecimResult Result { get; private set; } = RaporSecimResult.Cancel;

    public RaporSecimWindow(string? seciliCariUnvan, bool hasCari)
    {
        InitializeComponent();
        btnSeciliEkstre.IsEnabled = hasCari;
        _seciliSecenek = hasCari ? RaporSecimResult.SeciliEkstre : RaporSecimResult.CariOzet;

        if (hasCari && !string.IsNullOrWhiteSpace(seciliCariUnvan))
        {
            lblCariEkstreBaslik.Text = $"{seciliCariUnvan} — {Services.LocalizationManager.T("Rpr_SeciliEkstre")}";
        }
    }

    public static RaporSecimResult Execute(string? seciliCariUnvan, bool hasCari, Window? owner = null)
    {
        var win = new RaporSecimWindow(seciliCariUnvan, hasCari);
        if (owner != null) win.Owner = owner;
        win.ShowDialog();
        return win.Result;
    }

    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    private void BtnSeciliEkstre_Click(object sender, RoutedEventArgs e)
    {
        Result = RaporSecimResult.SeciliEkstre;
        DialogResult = true;
    }

    private void BtnCariOzet_Click(object sender, RoutedEventArgs e)
    {
        Result = RaporSecimResult.CariOzet;
        DialogResult = true;
    }

    private void BtnGenelHareketler_Click(object sender, RoutedEventArgs e)
    {
        Result = RaporSecimResult.GenelHareketler;
        DialogResult = true;
    }

    private void BtnGoruntule_Click(object sender, RoutedEventArgs e)
    {
        Result = _seciliSecenek;
        DialogResult = true;
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        Result = RaporSecimResult.Cancel;
        DialogResult = false;
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Result = RaporSecimResult.Cancel;
            DialogResult = false;
        }
        else if (e.Key == Key.Enter)
        {
            Result = _seciliSecenek;
            DialogResult = true;
        }
    }
}
