using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GelesiyeWPF.Common;

namespace GelesiyeWPF.Views.Dialogs;

public partial class AppDialog : Window
{
    public AppDialog(DialogTur tur, string mesaj, string baslik, string? detay, bool varsayilanEvet = true)
    {
        InitializeComponent();

        lblBaslik.Text = baslik;
        lblMesaj.Text  = mesaj;
        if (!string.IsNullOrWhiteSpace(detay))
        {
            lblDetay.Text = detay;
            lblDetay.Visibility = Visibility.Visible;
        }

        var (renk, ikon) = tur switch
        {
            DialogTur.Onay  => (AppTheme.DlgConfirm, "❓"),
            DialogTur.Uyari => (AppTheme.DlgWarning, "⚠"),
            DialogTur.Hata  => (AppTheme.DlgError,   "⛔"),
            _               => (AppTheme.DlgInfo,    "ℹ"),
        };
        bdHeader.Background = new SolidColorBrush(renk);
        lblIkon.Text = ikon;
        lblIkon.Foreground = new SolidColorBrush(renk);

        if (tur == DialogTur.Onay)
        {
            btnTamam.Content = Services.LocalizationManager.T("Btn_Evet");
            btnIptal.Content = Services.LocalizationManager.T("Btn_Hayir");
            btnTamam.IsDefault = varsayilanEvet;
            btnIptal.IsDefault = !varsayilanEvet;
            Loaded += (_, _) =>
            {
                if (varsayilanEvet) btnTamam.Focus();
                else btnIptal.Focus();
            };
        }
        else
        {
            btnTamam.Content = Services.LocalizationManager.T("Btn_Tamam");
            btnTamam.IsDefault = true;
            btnIptal.Visibility = Visibility.Collapsed;
            Loaded += (_, _) => btnTamam.Focus();
        }
    }

    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    private void BtnTamam_Click(object sender, RoutedEventArgs e) => DialogResult = true;
    private void BtnIptal_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
            e.Handled = true;
        }
    }
}
