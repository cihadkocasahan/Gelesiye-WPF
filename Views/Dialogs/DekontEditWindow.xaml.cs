using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GelesiyeWPF.Common;
using GelesiyeWPF.Data;
using GelesiyeWPF.Domain;
using GelesiyeWPF.Services;

namespace GelesiyeWPF.Views.Dialogs;

public partial class DekontEditWindow : Window
{
    private static readonly CultureInfo Tr = new("tr-TR");

    public IslemTuru IslemTuru { get; }
    public decimal   Tutar { get; private set; }
    public string    Aciklama { get; private set; } = "";
    public DateTime  IslemTarihi { get; private set; }

    /// <param name="hareket">Düzenleme modunda mevcut fiş, yeni fişte null.</param>
    public DekontEditWindow(IslemTuru tur, string cariUnvan, Hareket? hareket = null)
    {
        InitializeComponent();
        IslemTuru = tur;

        string prefix = tur.ToDisplayString();
        bdHeader.Background = new SolidColorBrush(AppTheme.GetDekontHeaderColor(tur));
        lblBaslik.Text = hareket != null
            ? $"{prefix} ({LocalizationManager.T("Btn_Duzenle")}) — {cariUnvan}"
            : $"{prefix} — {cariUnvan}";

        cmbAciklama.ItemsSource = DbFactory.GetAyarlarRepo().GetAciklamalar(tur);
        edtCariHesap.Text = cariUnvan;

        if (hareket != null)
        {
            dtpTarih.SelectedDate = hareket.Tarih.Date;
            edtSaat.Text   = hareket.Tarih.ToString("HH:mm");
            edtTutar.Text  = (hareket.Borc > 0 ? hareket.Borc : hareket.Alacak).ToString("N2", LocalizationManager.Instance.CurrentCulture);
            cmbAciklama.Text = hareket.Aciklama;
        }
        else
        {
            dtpTarih.SelectedDate = DateTime.Today;
            edtSaat.Text  = DateTime.Now.ToString("HH:mm");
            edtTutar.Text = "0,00";
            if (cmbAciklama.Items.Count > 0) cmbAciklama.SelectedIndex = 0;
        }

        Loaded += (_, _) => { edtTutar.Focus(); edtTutar.SelectAll(); };
    }

    private void EdtTutar_GotFocus(object sender, KeyboardFocusChangedEventArgs e) => edtTutar.SelectAll();

    private bool Dogrula()
    {
        var s = edtTutar.Text.Replace("TL", "", StringComparison.OrdinalIgnoreCase).Trim();
        if (!decimal.TryParse(s, NumberStyles.Number, Tr, out var val) || val <= 0)
        {
            DialogService.Uyari(AppConsts.MSG_TUTAR_SIFIR_OLAMAZ, LocalizationManager.T("Title_GecersizTutar"));
            edtTutar.Focus();
            return false;
        }

        if (!TimeSpan.TryParseExact(edtSaat.Text.Trim(), @"h\:mm", CultureInfo.InvariantCulture, out var saat) &&
            !TimeSpan.TryParse(edtSaat.Text.Trim(), CultureInfo.InvariantCulture, out saat))
            saat = DateTime.Now.TimeOfDay;

        Tutar = val;
        Aciklama = cmbAciklama.Text.Trim();
        IslemTarihi = (dtpTarih.SelectedDate ?? DateTime.Today).Date + new TimeSpan(saat.Hours, saat.Minutes, 0);
        return true;
    }

    private void BtnKaydet_Click(object sender, RoutedEventArgs e)
    {
        if (Dogrula()) DialogResult = true;
    }

    private void BtnIptal_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F2 || (e.Key == Key.Enter && Keyboard.FocusedElement == edtTutar))
        { BtnKaydet_Click(sender, e); e.Handled = true; }
        else if (e.Key == Key.Escape) { DialogResult = false; e.Handled = true; }
    }

    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }
}
