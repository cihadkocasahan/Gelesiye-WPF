using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using GelesiyeWPF.Common;
using GelesiyeWPF.Data;
using GelesiyeWPF.Domain;
using GelesiyeWPF.Services;

namespace GelesiyeWPF.Views.Dialogs;

public partial class AyarlarWindow : Window
{
    private readonly AyarlarRepository _repo = DbFactory.GetAyarlarRepo();
    private readonly FirmaRepository   _firmaRepo = DbFactory.GetFirmaRepo();
    private readonly IlIlceRepository  _ilRepo = DbFactory.GetIlIlceRepo();
    private Firma? _firma;
    private bool _loading = true;
    private string _baslangicDili = "tr-TR";

    public bool VeriDegisti { get; private set; }

    public AyarlarWindow(int baslangicSekmesi = 0)
    {
        _loading = true;
        InitializeComponent();
        if (baslangicSekmesi >= 0 && baslangicSekmesi < tcAyarlar.Items.Count)
            tcAyarlar.SelectedIndex = baslangicSekmesi;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _loading = true;
        try
        {
            _firma = _firmaRepo.GetDefault();

            // 1. Firma Bilgileri
            if (_firma != null)
            {
                edtFirmaUnvan.Text   = _firma.Unvan ?? "";
                edtFirmaTelefon.Text = _firma.Telefon ?? "";
                edtFirmaFaks.Text    = _firma.Faks ?? "";
                edtFirmaVergiDairesi.Text = _firma.VergiDairesi ?? "";
                edtFirmaVergiNo.Text = _firma.VergiNo ?? "";
                edtFirmaAdres.Text   = _firma.Adres ?? "";

                cmbFirmaIl.ItemsSource = _ilRepo.GetIller();
                cmbFirmaIl.Text = _firma.Il ?? "";
                cmbFirmaIlce.ItemsSource = string.IsNullOrEmpty(_firma.Il) ? null : _ilRepo.GetIlceler(_firma.Il);
                cmbFirmaIlce.Text = _firma.Ilce ?? "";
            }
            else
            {
                cmbFirmaIl.ItemsSource = _ilRepo.GetIller();
            }

            // 2. Genel Ayarlar
            cmbParaBirimi.Text = _repo.GetAyar("ParaBirimi", _firma?.ParaBirimi ?? "TL");
            _baslangicDili = _repo.GetAyar("UygulamaDili", "tr-TR");
            cmbDil.SelectedIndex = _baslangicDili.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            cmbKapatmaEylemi.SelectedIndex = Math.Clamp(_repo.GetInt("KapatmaEylemi", 0), 0, 2);
            chkSifre.IsChecked = _repo.GetBool("SifreKorumasi");
            chkZebraGrid.IsChecked = _repo.GetAyar("ZebraGrid", "1") == "1";

            // Filtre Varsayılanları & Hatırlama
            cmbDefAramaModu.SelectedIndex = Math.Clamp(_repo.GetInt("Varsayilan_AramaModu", 0), 0, 1);
            cmbDefHesapDurumu.SelectedIndex = Math.Clamp(_repo.GetInt("Varsayilan_HesapDurumu", 0), 0, 2);
            cmbDefBakiyeFiltresi.SelectedIndex = Math.Clamp(_repo.GetInt("Varsayilan_BakiyeFiltresi", 0), 0, 5);
            chkFiltreHatirla.IsChecked = _repo.GetBool("Filtre_SonSecimiHatirla", false);

            // 3. Yedekleme
            cmbOtoYedek.SelectedIndex = Math.Clamp(_repo.GetInt("OtoYedekleme", 1), 0, 4);
            int maks = _repo.GetInt("MaksYedekSayisi", 10);
            cmbMaksYedek.SelectedIndex = maks switch
            {
                5 => 0,
                10 => 1,
                20 => 2,
                50 => 3,
                0 => 4,
                _ => 1
            };
            edtYedekKlasoru.Text = BackupService.GetBackupDir();
            SonYedekGuncelle();

            // 5. Açıklamalar
            if (cmbAciklamaTuru != null && cmbAciklamaTuru.SelectedIndex < 0)
                cmbAciklamaTuru.SelectedIndex = 0;
        }
        finally
        {
            _loading = false;
        }

        AciklamalariYukle();
    }

    private void CmbDil_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || cmbDil == null) return;
        string seciliDil = cmbDil.SelectedIndex == 1 ? "en-US" : "tr-TR";
        LocalizationManager.Instance.SetLanguage(seciliDil, kaydet: false);
    }

    private void CmbFirmaIl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || cmbFirmaIlce == null || cmbFirmaIl?.SelectedItem is not string il) return;
        cmbFirmaIlce.ItemsSource = _ilRepo.GetIlceler(il);
        cmbFirmaIlce.Text = "";
    }

    private void SonYedekGuncelle()
    {
        var son = _repo.GetAyar("SonYedekTarihi", "");
        lblSonYedek.Text = string.IsNullOrEmpty(son) ? "Son Yedekleme: Henüz alınmadı." : $"Son Yedekleme: {son}";
    }

    // ── Şifre Değiştir ──
    private void BtnSifreGuncelle_Click(object sender, RoutedEventArgs e)
    {
        string eski = edtEskiSifre.Password;
        string yeni = edtYeniSifre.Password;
        string tekrar = edtYeniSifreTekrar.Password;

        string kayitli = _repo.GetAyar("GirisSifresi", "");
        if (string.IsNullOrEmpty(kayitli)) kayitli = "gel";

        if (eski != kayitli)
        {
            DialogService.Uyari(LocalizationManager.T("Msg_MevcutSifreHatali"), LocalizationManager.T("Title_SifreGuncelleme"));
            edtEskiSifre.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(yeni))
        {
            DialogService.Uyari(LocalizationManager.T("Msg_YeniSifreBosOlamaz"), LocalizationManager.T("Title_SifreGuncelleme"));
            edtYeniSifre.Focus();
            return;
        }

        if (yeni != tekrar)
        {
            DialogService.Uyari(LocalizationManager.T("Msg_YeniSifreUyusmuyor"), LocalizationManager.T("Title_SifreGuncelleme"));
            edtYeniSifreTekrar.Focus();
            return;
        }

        _repo.SetAyar("GirisSifresi", yeni);
        _repo.SetAyar("SifreKorumasi", "1");
        chkSifre.IsChecked = true;

        edtEskiSifre.Clear();
        edtYeniSifre.Clear();
        edtYeniSifreTekrar.Clear();
        DialogService.Bilgi(LocalizationManager.T("Msg_SifreBasariylaGuncellendi"), LocalizationManager.T("Title_SifreGuncellendi"));
    }

    // ── Yedek Klasörü ──
    private void BtnYedekKlasoruSec_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFolderDialog
        {
            Title = "Yedekleme Klasörünü Seçiniz",
            InitialDirectory = BackupService.GetBackupDir()
        };
        if (dlg.ShowDialog() == true)
        {
            edtYedekKlasoru.Text = dlg.FolderName;
        }
    }

    private void BtnYedekKlasoruVarsayilan_Click(object sender, RoutedEventArgs e)
    {
        edtYedekKlasoru.Text = BackupService.GetDefaultBackupDir();
    }

    private void BtnYedekYonetimiAc_Click(object sender, RoutedEventArgs e)
    {
        var win = new YedekYonetimiWindow { Owner = this };
        win.ShowDialog();
        if (win.VeriGeriYuklendi)
        {
            VeriDegisti = true;
            DialogResult = true;
        }
        else
        {
            edtYedekKlasoru.Text = BackupService.GetBackupDir();
            SonYedekGuncelle();
        }
    }

    // ── Açıklama Şablonları ──
    private IslemTuru SeciliIslemTuru => (IslemTuru)Math.Clamp(cmbAciklamaTuru?.SelectedIndex ?? 0, 0, 4);

    private void CmbAciklamaTuru_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_loading && lbAciklamalar != null) AciklamalariYukle();
    }

    private void AciklamalariYukle()
    {
        if (lbAciklamalar == null || cmbAciklamaTuru == null) return;
        lbAciklamalar.ItemsSource = _repo.GetAciklamalar(SeciliIslemTuru);
    }

    private void BtnAciklamaEkle_Click(object sender, RoutedEventArgs e)
    {
        var metin = edtYeniAciklama.Text.Trim();
        if (string.IsNullOrEmpty(metin)) return;
        _repo.AddAciklama(SeciliIslemTuru, metin);
        edtYeniAciklama.Clear();
        AciklamalariYukle();
    }

    private void BtnAciklamaSil_Click(object sender, RoutedEventArgs e)
    {
        if (lbAciklamalar.SelectedItem is not string acik) return;
        _repo.DeleteAciklama(SeciliIslemTuru, acik);
        AciklamalariYukle();
    }

    private void BtnAciklamaTemizle_Click(object sender, RoutedEventArgs e)
    {
        if (!DialogService.Onay(LocalizationManager.T("Msg_AciklamalariSilOnay"), LocalizationManager.T("Title_AciklamalariSil")))
            return;
        _repo.ClearAciklamalar(SeciliIslemTuru);
        AciklamalariYukle();
    }

    // ── Kaydet ──
    private void BtnKaydet_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Firma Bilgileri
            if (_firma != null)
            {
                _firma.Unvan        = edtFirmaUnvan.Text.Trim();
                _firma.Telefon      = edtFirmaTelefon.Text.Trim();
                _firma.Faks         = edtFirmaFaks.Text.Trim();
                _firma.Il           = cmbFirmaIl.Text.Trim();
                _firma.Ilce         = cmbFirmaIlce.Text.Trim();
                _firma.VergiDairesi = edtFirmaVergiDairesi.Text.Trim();
                _firma.VergiNo      = edtFirmaVergiNo.Text.Trim();
                _firma.Adres        = edtFirmaAdres.Text.Trim();
                _firmaRepo.Update(_firma);
            }

            // Genel Ayarlar
            string pb = string.IsNullOrWhiteSpace(cmbParaBirimi.Text) ? "TL" : cmbParaBirimi.Text.Trim();
            _repo.SetAyar("ParaBirimi", pb);

            string seciliDil = cmbDil.SelectedIndex == 1 ? "en-US" : "tr-TR";
            _repo.SetAyar("UygulamaDili", seciliDil);
            LocalizationManager.Instance.SetLanguage(seciliDil);

            _repo.SetAyar("KapatmaEylemi", cmbKapatmaEylemi.SelectedIndex.ToString());
            _repo.SetAyar("SifreKorumasi", chkSifre.IsChecked == true ? "1" : "0");
            _repo.SetAyar("ZebraGrid", chkZebraGrid.IsChecked == true ? "1" : "0");

            // Filtre Varsayılanları & Hatırlama
            _repo.SetAyar("Varsayilan_AramaModu", cmbDefAramaModu.SelectedIndex.ToString());
            _repo.SetAyar("Varsayilan_HesapDurumu", cmbDefHesapDurumu.SelectedIndex.ToString());
            _repo.SetAyar("Varsayilan_BakiyeFiltresi", cmbDefBakiyeFiltresi.SelectedIndex.ToString());
            _repo.SetAyar("Filtre_SonSecimiHatirla", chkFiltreHatirla.IsChecked == true ? "1" : "0");

            // Yedekleme Ayarları
            _repo.SetAyar("OtoYedekleme", cmbOtoYedek.SelectedIndex.ToString());
            int maks = cmbMaksYedek.SelectedIndex switch
            {
                0 => 5,
                1 => 10,
                2 => 20,
                3 => 50,
                4 => 0,
                _ => 10
            };
            _repo.SetAyar("MaksYedekSayisi", maks.ToString());
            _repo.SetAyar("YedekKlasoru", edtYedekKlasoru.Text.Trim());

            DialogResult = true;
        }
        catch (Exception ex)
        {
            DialogService.Hata(LocalizationManager.T("Msg_AyarlarKaydedilemedi"), LocalizationManager.T("Title_Hata"), ex.Message);
        }
    }

    private void BtnIptal_Click(object sender, RoutedEventArgs e)
    {
        if (LocalizationManager.Instance.CurrentLanguage != _baslangicDili)
            LocalizationManager.Instance.SetLanguage(_baslangicDili, kaydet: false);
        DialogResult = false;
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            if (LocalizationManager.Instance.CurrentLanguage != _baslangicDili)
                LocalizationManager.Instance.SetLanguage(_baslangicDili, kaydet: false);
            DialogResult = false;
        }
    }

    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }
}
