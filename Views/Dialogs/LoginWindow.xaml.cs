using System.Windows;
using System.Windows.Input;
using GelesiyeWPF.Data;
using GelesiyeWPF.Services;

namespace GelesiyeWPF.Views.Dialogs;

public partial class LoginWindow : Window
{
    private int _kalan = 3;

    public LoginWindow() => InitializeComponent();

    /// <summary>Şifre koruması kapalıysa doğrudan true döner (Delphi TfrmLogin.Execute).</summary>
    public static bool Execute()
    {
        var repo = DbFactory.GetAyarlarRepo();
        if (repo.GetAyar("SifreKorumasi", "0") != "1") return true;
        return new LoginWindow().ShowDialog() == true;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e) => edtSifre.Focus();

    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    private void EdtSifre_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) BtnGiris_Click(sender, e);
    }

    private void BtnGiris_Click(object sender, RoutedEventArgs e)
    {
        var kayitli = DbFactory.GetAyarlarRepo().GetAyar("GirisSifresi", "");
        if (kayitli == "") kayitli = "gel";   // Delphi varsayılanı

        if (edtSifre.Password == kayitli)
        {
            DialogResult = true;
            return;
        }

        _kalan--;
        if (_kalan <= 0)
        {
            DialogService.Hata(LocalizationManager.T("Msg_GirisEngellendi"), LocalizationManager.T("Title_GirisEngellendi"));
            DialogResult = false;
            return;
        }

        DialogService.Uyari(LocalizationManager.T("Msg_HataliSifre"), LocalizationManager.T("Title_HataliSifre"),
            string.Format(LocalizationManager.T("Lbl_KalanHak"), _kalan));
        lblKalanHak.Text = string.Format(LocalizationManager.T("Lbl_KalanHak"), _kalan);
        edtSifre.SelectAll();
        edtSifre.Focus();
    }

    private void BtnCikis_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
