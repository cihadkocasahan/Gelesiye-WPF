using System.Globalization;
using System.Windows;
using System.Windows.Input;
using GelesiyeWPF.Common;

namespace GelesiyeWPF.Views.Dialogs;

public partial class ListeToplamlariWindow : Window
{
    private static readonly CultureInfo Tr = new("tr-TR");

    public ListeToplamlariWindow(BakiyeOzet ozet, string paraBirimi = "TL")
    {
        InitializeComponent();

        string Fmt(decimal val) => $"{val.ToString("N2", Services.LocalizationManager.Instance.CurrentCulture)} {paraBirimi}";

        edtAktifBorclu.Text   = Fmt(ozet.AktifBorcluToplam);
        edtAktifAlacakli.Text = Fmt(ozet.AktifAlacakliToplam);
        edtPasifBorclu.Text   = Fmt(ozet.PasifBorcluToplam);
        edtPasifAlacakli.Text = Fmt(ozet.PasifAlacakliToplam);
        edtGenelBorclu.Text   = Fmt(ozet.GenelBorcluToplam);
        edtGenelAlacakli.Text = Fmt(ozet.GenelAlacakliToplam);

        lblNetBakiye.Text = Fmt(ozet.NetBakiye);
        lblCariSayilari.Text = $"{Services.LocalizationManager.T("Lbl_ToplamCari")}: {ozet.ToplamCariSayisi}\n{Services.LocalizationManager.T("Stat_Aktif")}: {ozet.AktifCariSayisi}  |  {Services.LocalizationManager.T("Stat_Pasif")}: {ozet.PasifCariSayisi}";
    }

    public static void ShowOzet(BakiyeOzet ozet, string paraBirimi, Window? owner = null)
    {
        var win = new ListeToplamlariWindow(ozet, paraBirimi);
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
}
