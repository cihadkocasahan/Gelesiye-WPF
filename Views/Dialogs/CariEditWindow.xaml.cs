using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GelesiyeWPF.Common;
using GelesiyeWPF.Data;
using GelesiyeWPF.Domain;
using GelesiyeWPF.Services;

namespace GelesiyeWPF.Views.Dialogs;

public partial class CariEditWindow : Window
{
    private static readonly CultureInfo Tr = new("tr-TR");
    private readonly Cari _cari;
    private readonly bool _isNew;
    private readonly ICariService _cariService = DbFactory.GetCariService();
    private readonly IlIlceRepository _ilRepo = DbFactory.GetIlIlceRepo();
    private bool _loading = true;

    public Cari Sonuc => _cari;

    public CariEditWindow(Cari cari, bool isNew)
    {
        _loading = true;
        InitializeComponent();
        _cari  = cari;
        _isNew = isNew;
        LoadData();
    }

    private void LoadData()
    {
        _loading = true;
        lblBaslik.Text = _isNew
            ? LocalizationManager.T("Title_YeniCari")
            : $"{LocalizationManager.T("Title_CariDuzenle")} — {_cari.Unvan}";

        edtCariKodu.Text  = _cari.CariKodu;
        edtUnvan.Text     = _cari.Unvan;
        edtYetkili.Text   = _cari.Yetkili;
        edtGsm.Text       = _cari.Gsm;
        edtTelefon.Text   = _cari.Telefon;
        edtFaks.Text      = _cari.Faks;
        edtAdres.Text     = _cari.Adres;
        edtEposta.Text    = _cari.Eposta;
        edtWeb.Text       = _cari.Web;
        edtVergiDairesi.Text = _cari.VergiDairesi;
        edtVergiNo.Text   = _cari.VergiNo;
        edtCariLimit.Text = _cari.CariLimit.ToString("N2", LocalizationManager.Instance.CurrentCulture);
        dtpKayitTarihi.SelectedDate = _cari.KayitTarihi;
        cmbHesapDurumu.SelectedIndex = _cari.HesapDurumu == HesapDurumu.Pasif ? 1 : 0;
        mmoNotlar.Text    = _cari.Notlar;

        cmbIl.ItemsSource = _ilRepo.GetIller();

        // Yeni cari: varsayılan firmanın il/ilçesi
        if (_isNew)
        {
            var frm = DbFactory.GetFirmaRepo().GetDefault();
            if (frm != null) { _cari.Il = frm.Il; _cari.Ilce = frm.Ilce; }
        }

        cmbIl.Text = _cari.Il;
        cmbIlce.ItemsSource = string.IsNullOrEmpty(_cari.Il) ? null : _ilRepo.GetIlceler(_cari.Il);
        cmbIlce.Text = _cari.Ilce;
        _loading = false;

        Loaded += (_, _) => edtUnvan.Focus();
    }

    private void CmbIl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || cmbIlce == null || cmbIl?.SelectedItem is not string il) return;
        cmbIlce.ItemsSource = _ilRepo.GetIlceler(il);
        cmbIlce.Text = "";
    }

    private static decimal ParseTutar(string s, decimal varsayilan)
    {
        s = s.Replace("TL", "", StringComparison.OrdinalIgnoreCase).Trim();
        return decimal.TryParse(s, NumberStyles.Number, Tr, out var d) ? d : varsayilan;
    }

    private void ReadFormData()
    {
        _cari.CariKodu     = edtCariKodu.Text.Trim();
        _cari.Unvan        = edtUnvan.Text.Trim();
        _cari.Yetkili      = edtYetkili.Text.Trim();
        _cari.Gsm          = edtGsm.Text.Trim();
        _cari.Telefon      = edtTelefon.Text.Trim();
        _cari.Faks         = edtFaks.Text.Trim();
        _cari.Adres        = edtAdres.Text.Trim();
        _cari.Il           = cmbIl.Text.Trim();
        _cari.Ilce         = cmbIlce.Text.Trim();
        _cari.VergiDairesi = edtVergiDairesi.Text.Trim();
        _cari.VergiNo      = edtVergiNo.Text.Trim();
        _cari.Eposta       = edtEposta.Text.Trim();
        _cari.Web          = edtWeb.Text.Trim();
        _cari.Notlar       = mmoNotlar.Text;
        _cari.KayitTarihi  = dtpKayitTarihi.SelectedDate ?? DateTime.Today;
        _cari.CariLimit    = ParseTutar(edtCariLimit.Text, (decimal)AppConsts.DEF_BORC_LIMITI);
        _cari.HesapDurumu  = cmbHesapDurumu.SelectedIndex == 1 ? HesapDurumu.Pasif : HesapDurumu.Aktif;
    }

    private void BtnKaydet_Click(object sender, RoutedEventArgs e)
    {
        ReadFormData();
        var res = _cariService.Save(_cari, _isNew);
        if (!res.IsSuccess)
        {
            DialogService.Uyari(res.ErrorMessage ?? LocalizationManager.T("Msg_GecersizBilgi"), LocalizationManager.T("Title_DogrulamaUyarisi"));
            if (string.IsNullOrWhiteSpace(_cari.CariKodu)) edtCariKodu.Focus();
            else if (string.IsNullOrWhiteSpace(_cari.Unvan)) edtUnvan.Focus();
            return;
        }

        DialogResult = true;
    }

    private void BtnIptal_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F2) { BtnKaydet_Click(sender, e); e.Handled = true; }
        else if (e.Key == Key.Escape) { DialogResult = false; e.Handled = true; }
    }

    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }
}
