using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using GelesiyeWPF.Common;
using GelesiyeWPF.Data;
using GelesiyeWPF.Domain;
using GelesiyeWPF.Services;
using GelesiyeWPF.ViewModels;
using GelesiyeWPF.Views.Dialogs;

namespace GelesiyeWPF.Views;

/// <summary>
/// Ana pencere — Delphi fMain.pas karşılığı. İki sayfa: Cari Listesi ve Hareketler.
/// </summary>
public partial class MainWindow : Window
{
    private static readonly CultureInfo Tr = new("tr-TR");

    private CariListViewModel _cariVm = null!;
    private HareketViewModel  _hrkVm  = null!;
    private readonly ICariService _cariService = DbFactory.GetCariService();
    private readonly IHareketService _hareketService = DbFactory.GetHareketService();
    private Firma _firma = null!;
    private string _pb = "TL";
    private bool _ready;
    private TrayService? _trayService;
    private bool _balloonShown;
    private bool _forceClose;

    public MainWindow()
    {
        InitializeComponent();

        // Window.ContextMenu sadece "Fiş ekle" menüsü için kaynak; pencere genelinde sağ tıkta açılmasın.
        ContextMenu = null;

        LocalizationManager.Instance.PropertyChanged += (_, _) =>
        {
            Dispatcher.Invoke(() =>
            {
                Title = AppConsts.GetTitleWithVersion(_firma?.Unvan ?? "");
                DurumYaz(HareketSayfasinda
                    ? $"📒 {LocalizationManager.T("Btn_Hareketler")} — {_hrkVm?.Cari?.Unvan}"
                    : $"📋 {LocalizationManager.T("Status_CariListesi")}");
                if (_ready)
                {
                    OzetKartlariGuncelle();
                    if (HareketSayfasinda) HareketEkraniGuncelle();
                }
            });
        };
    }

    // ═════════════════════════ Başlatma / kapanış ═════════════════════════

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Title = AppConsts.GetTitleWithVersion();
        lblVersion.Text = AppConsts.GetStatusBarVersion().Trim();

        KurVeVm();
        KurSagTikMenuleri();

        lvCariler.AddHandler(System.Windows.Controls.Primitives.ButtonBase.ClickEvent,
            new RoutedEventHandler(LvCariler_HeaderClick));
        lvHareketler.AddHandler(System.Windows.Controls.Primitives.ButtonBase.ClickEvent,
            new RoutedEventHandler(LvHareketler_HeaderClick));

        _trayService = new TrayService(this);
        _trayService.Initialize();
        _trayService.DoubleClick += ShowFromTray;
        _trayService.RightClick += ShowTrayMenu;

        FiltreleriYukle();
        _ready = true;
        CariYenile();
        SayfaGoster(false);

        DovizKuruService.KurGuncellendi += () => Dispatcher.Invoke(() => lblDovizKur.Text = DovizKuruService.GetKurMetni());
        lblDovizKur.Text = LocalizationManager.T("Status_KurGuncelleniyor");
        _ = DovizKuruService.GuncelleAsync().ContinueWith(_ =>
            Dispatcher.Invoke(() => lblDovizKur.Text = DovizKuruService.GetKurMetni()));

        if (_cariVm.Cariler.Count == 0)
            DurumYaz(LocalizationManager.T("Msg_CariKartiYokUyarisi"));
        edtAramaMetni.Focus();
    }

    /// <summary>Firma + repository + view model'leri (yeniden) kurar. Geri yükleme sonrası da çağrılır.</summary>
    private void KurVeVm()
    {
        var firmaRepo = DbFactory.GetFirmaRepo();
        _firma = firmaRepo.GetDefault() ?? new Firma { Id = 1, Unvan = "TEST" };
        _pb = DbFactory.GetAyarlarRepo().GetAyar("ParaBirimi", _firma.ParaBirimi);

        DemoSeed.EnsureRealisticMix(DbFactory.GetDb(), _firma.Id);

        _cariVm = new CariListViewModel(DbFactory.GetCariRepo()) { FirmaId = _firma.Id };
        _hrkVm  = new HareketViewModel(DbFactory.GetHareketRepo(), DbFactory.GetCariRepo());

        lblFirmaBaslik.Text = $"{_firma.Unvan}  •  {_firma.Donem}";
        Title = AppConsts.GetTitleWithVersion(_firma.Unvan);
        ZebraAyariniUygula();
    }

    private void ZebraAyariniUygula()
    {
        bool zebra = DbFactory.GetAyarlarRepo().GetAyar("ZebraGrid", "1") == "1";
        lvCariler.AlternationCount = zebra ? 2 : 0;
        lvHareketler.AlternationCount = zebra ? 2 : 0;
    }

    private void FiltreleriYukle()
    {
        var repo = DbFactory.GetAyarlarRepo();
        bool hatirla = repo.GetBool("Filtre_SonSecimiHatirla", false);

        int aModu  = hatirla ? repo.GetInt("SonFiltre_AramaModu", repo.GetInt("Varsayilan_AramaModu", 0))
                             : repo.GetInt("Varsayilan_AramaModu", 0);
        int hDurum = hatirla ? repo.GetInt("SonFiltre_HesapDurumu", repo.GetInt("Varsayilan_HesapDurumu", 0))
                             : repo.GetInt("Varsayilan_HesapDurumu", 0);
        int bFilt  = hatirla ? repo.GetInt("SonFiltre_BakiyeFiltresi", repo.GetInt("Varsayilan_BakiyeFiltresi", 0))
                             : repo.GetInt("Varsayilan_BakiyeFiltresi", 0);

        cmbAra.SelectedIndex   = Math.Clamp(aModu, 0, 1);
        cmbHesap.SelectedIndex = Math.Clamp(hDurum, 0, 2);
        cmbFiltre.SelectedIndex = Math.Clamp(bFilt, 0, 5);

        _cariVm.AramaModu = cmbAra.SelectedIndex == 1 ? AramaModu.Iceren : AramaModu.YazildigiGibi;
        _cariVm.HesapDurumuFiltre = (HesapDurumu)cmbHesap.SelectedIndex;
        _cariVm.BakiyeFiltresi = (BakiyeFiltresi)cmbFiltre.SelectedIndex;
    }

    private void SonFiltreleriKaydet()
    {
        if (!_ready) return;
        var repo = DbFactory.GetAyarlarRepo();
        if (repo.GetBool("Filtre_SonSecimiHatirla", false))
        {
            repo.SetAyar("SonFiltre_AramaModu", cmbAra.SelectedIndex.ToString());
            repo.SetAyar("SonFiltre_HesapDurumu", cmbHesap.SelectedIndex.ToString());
            repo.SetAyar("SonFiltre_BakiyeFiltresi", cmbFiltre.SelectedIndex.ToString());
        }
    }

    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_forceClose)
        {
            _trayService?.Dispose();
            BackupService.CheckAutoBackup(onOpen: false);
            return;
        }

        var repo = DbFactory.GetAyarlarRepo();
        int kapatmaEylemi = repo.GetInt("KapatmaEylemi", 0);

        if (kapatmaEylemi == 0) // Her zaman sor
        {
            var (action, remember) = CloseActionWindow.Prompt(this);
            if (action == CloseActionResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            if (action == CloseActionResult.Minimize)
            {
                e.Cancel = true;
                HideToTray();
                if (remember) repo.SetAyar("KapatmaEylemi", "2");
                return;
            }
            if (remember) repo.SetAyar("KapatmaEylemi", "1");
        }
        else if (kapatmaEylemi == 2) // Simge durumuna küçült (Tray)
        {
            e.Cancel = true;
            HideToTray();
            return;
        }

        _trayService?.Dispose();
        BackupService.CheckAutoBackup(onOpen: false);
    }

    private void HideToTray()
    {
        Hide();
        if (!_balloonShown)
        {
            _balloonShown = true;
            _trayService?.ShowBalloon(
                "Gelesiye — Basit Veresiye Takip",
                "Program sistem tepsisinde (saatin yanında) çalışmaya devam ediyor. Tekrar açmak için simgeye çift tıklayabilirsiniz.");
        }
    }

    public void ShowFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
        Focus();
    }

    private void ShowTrayMenu()
    {
        var m = new ContextMenu();
        MenuItem Ekle(string baslik, RoutedEventHandler h)
        {
            var mi = new MenuItem { Header = baslik };
            mi.Click += h;
            m.Items.Add(mi);
            return mi;
        }

        Ekle(LocalizationManager.T("Tray_Goster"), (_, _) => ShowFromTray());
        m.Items.Add(new Separator());
        Ekle(LocalizationManager.T("Tray_YedekAl"), (_, _) => YedekAl());
        Ekle(LocalizationManager.T("Tray_YedekYonetimi"), (_, _) => YedekYonetimiAc());
        m.Items.Add(new Separator());
        Ekle(LocalizationManager.T("Tray_Hakkinda"), (_, _) => Hakkinda());
        Ekle(LocalizationManager.T("Tray_Cikis"), (_, _) => { _forceClose = true; Close(); });

        m.IsOpen = true;
    }

    // ═════════════════════════ Başlık çubuğu ═════════════════════════

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
        {
            BtnMaximize_Click(sender, e);
            e.Handled = true;
        }
    }

    private void TitleBar_LeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount != 1 || e.ButtonState != MouseButtonState.Pressed) return;
        if (WindowState == WindowState.Maximized) return;
        try { DragMove(); } catch (InvalidOperationException) { }
    }

    private void Window_StateChanged(object? sender, EventArgs e) =>
        btnMaximize.Content = WindowState == WindowState.Maximized ? "❐" : "□";

    private void BtnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void BtnMaximize_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

    // ═════════════════════════ Ayarlar menüsü (⚙) ═════════════════════════

    private void BtnMenu_Click(object sender, RoutedEventArgs e)
    {
        var m = new ContextMenu();
        MenuItem Ekle(string baslik, RoutedEventHandler h)
        {
            var mi = new MenuItem { Header = baslik };
            mi.Click += h;
            m.Items.Add(mi);
            return mi;
        }

        Ekle(LocalizationManager.T("Menu_Ayarlar"), (_, _) => AyarlarAc());
        Ekle(LocalizationManager.T("Menu_YedekYonetimi"), (_, _) => YedekYonetimiAc());
        Ekle(LocalizationManager.T("Menu_ListeToplamlari"), (_, _) => BtnOzet_Click(this, new RoutedEventArgs()));
        Ekle(LocalizationManager.T("Menu_RaporSecimi"), (_, _) => BtnYazdir_Click(this, new RoutedEventArgs()));
        m.Items.Add(new Separator());
        Ekle(LocalizationManager.T("Menu_HizliYedek"), (_, _) => YedekAl());
        m.Items.Add(new Separator());
        Ekle(LocalizationManager.T("Menu_DemoYukle"), (_, _) => DemoYukle());
        Ekle(LocalizationManager.T("Menu_YilSonuDevir"), (_, _) => YilSonuDevir());
        m.Items.Add(new Separator());
        Ekle(LocalizationManager.T("Menu_Hakkinda"), (_, _) => Hakkinda());
        Ekle(LocalizationManager.T("Menu_Cikis"), (_, _) => Close());

        m.PlacementTarget = btnMenu;
        m.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
        m.IsOpen = true;
    }

    private void YedekYonetimiAc()
    {
        var win = new YedekYonetimiWindow { Owner = this };
        win.ShowDialog();
        if (win.VeriGeriYuklendi)
        {
            KurVeVm();
            SayfaGoster(false);
            CariYenile();
        }
    }

    private void AyarlarAc()
    {
        var w = new AyarlarWindow { Owner = this };
        if (w.ShowDialog() == true)
        {
            if (w.VeriDegisti) { KurVeVm(); _hrkVm = new HareketViewModel(DbFactory.GetHareketRepo(), DbFactory.GetCariRepo()); }
            _pb = DbFactory.GetAyarlarRepo().GetAyar("ParaBirimi", _firma.ParaBirimi);
            ZebraAyariniUygula();
            FiltreleriYukle();
            SayfaGoster(false);
            CariYenile();
        }
    }

    private void YedekAl()
    {
        try
        {
            var f = BackupService.Backup();
            DialogService.Bilgi(LocalizationManager.T("Msg_YedeklemeBasarili"), LocalizationManager.T("Title_YedekAlindi"), f);
            DurumYaz(string.Format(LocalizationManager.T("Status_YedekAlindi"), System.IO.Path.GetFileName(f)));
        }
        catch (Exception ex) { DialogService.Hata(LocalizationManager.T("Msg_YedeklemeBasarisiz"), LocalizationManager.T("Title_Hata"), ex.Message); }
    }

    private void DemoYukle()
    {
        if (!DialogService.Onay(LocalizationManager.T("Msg_DemoOnay"), LocalizationManager.T("Title_DemoVeri"))) return;
        try
        {
            int n = DemoSeed.Seed(DbFactory.GetDb(), _firma.Id, force: true);
            CariYenile();
            DialogService.Bilgi(string.Format(LocalizationManager.T("Msg_DemoEklendi"), n), LocalizationManager.T("Title_DemoVeri"));
        }
        catch (Exception ex) { DialogService.Hata(LocalizationManager.T("Msg_DemoEklenemedi"), LocalizationManager.T("Title_Hata"), ex.Message); }
    }

    private void YilSonuDevir()
    {
        int yeniDonem = _firma.Donem + 1;
        if (!DialogService.Onay(
                string.Format(LocalizationManager.T("Msg_DevirOnay"), _firma.Donem, yeniDonem),
                LocalizationManager.T("Title_YilSonuDevir"), LocalizationManager.T("Msg_DevirDevamMi"))) return;
        try
        {
            var repo = DbFactory.GetFirmaRepo();
            var yeni = new Firma
            {
                Kod = _firma.Kod + "_" + yeniDonem, Unvan = _firma.Unvan, Donem = yeniDonem,
                ParaBirimi = _firma.ParaBirimi, BorcLimiti = _firma.BorcLimiti, Telefon = _firma.Telefon,
                Faks = _firma.Faks, Adres = _firma.Adres, Il = _firma.Il, Ilce = _firma.Ilce,
                VergiDairesi = _firma.VergiDairesi, VergiNo = _firma.VergiNo
            };
            repo.Insert(yeni);
            repo.DevirYap(_firma.Id, yeni.Id, true);
            repo.SetDefault(yeni.Id);

            KurVeVm();
            SayfaGoster(false);
            CariYenile();
            DialogService.Bilgi(AppConsts.MSG_DEVIR_BASARILI, LocalizationManager.T("Title_YilSonuDevir"));
        }
        catch (Exception ex) { DialogService.Hata(LocalizationManager.T("Msg_DevirBasarisiz"), LocalizationManager.T("Title_Hata"), ex.Message); }
    }

    private void Hakkinda() => HakkindaWindow.ShowHakkinda(this);

    // ═════════════════════════ Sayfa geçişleri ═════════════════════════

    private bool HareketSayfasinda => pgHareketler.Visibility == Visibility.Visible;

    internal void SayfaGoster(bool hareketler)
    {
        pgCariList.Visibility   = hareketler ? Visibility.Collapsed : Visibility.Visible;
        pgHareketler.Visibility = hareketler ? Visibility.Visible   : Visibility.Collapsed;
        DurumYaz(hareketler ? $"📒 {LocalizationManager.T("Status_HareketDetayi")} — {_hrkVm.Cari?.Unvan}" : LocalizationManager.T("Status_CariListesi"));
        (hareketler ? (Control)lvHareketler : edtAramaMetni).Focus();
    }

    private void DurumYaz(string s) => lblStatus.Text = s;

    // ═════════════════════════ Cari listesi ═════════════════════════

    internal Cari? SeciliCari => lvCariler.SelectedItem as Cari;
    internal Hareket? SeciliHareket => lvHareketler.SelectedItem as Hareket;

    private void CariYenile(int? seciliId = null)
    {
        if (!_ready) return;
        seciliId ??= SeciliCari?.Id;

        _cariVm.YukleCariler();
        lvCariler.ItemsSource = _cariVm.Cariler;

        if (seciliId != null)
        {
            int idx = _cariVm.IndexOfCari(seciliId.Value);
            if (idx >= 0) lvCariler.SelectedIndex = idx;
        }
        if (lvCariler.SelectedIndex < 0 && _cariVm.Cariler.Count > 0) lvCariler.SelectedIndex = 0;

        OzetKartlariGuncelle();
        ButonDurumlari();
    }

    private void OzetKartlariGuncelle()
    {
        var o = _cariVm.Ozet;
        valCardBorc.Text   = Para(o.AktifBorcluToplam);
        valCardAlacak.Text = Para(o.AktifAlacakliToplam);
        valCardNet.Text    = Para(o.NetBakiye);
        valCardCari.Text   = $"{LocalizationManager.T("Stat_Aktif")}: {o.AktifCariSayisi}  |  {LocalizationManager.T("Stat_Pasif")}: {o.PasifCariSayisi}";
        lblCardCariSub.Text = $"{LocalizationManager.T("Stat_Borclu")}: {o.BorcluCariSayisi}  |  {LocalizationManager.T("Stat_Alacakli")}: {o.AlacakliCariSayisi}  |  {LocalizationManager.T("Stat_Sifir")}: {o.SifirBakiyeCariSayisi}";
    }

    private string Para(decimal d) => $"{d.ToString("N2", LocalizationManager.Instance.CurrentCulture)} {_pb}";

    private void ButonDurumlari()
    {
        bool var = SeciliCari != null;
        btnDegistir.IsEnabled = btnSil.IsEnabled = btnHareketler.IsEnabled = btnYazdir.IsEnabled = var;
    }

    // Filtreler
    private void EdtAramaMetni_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!_ready) return;
        _cariVm.AramaMetni = edtAramaMetni.Text;
        CariYenile();
    }

    private void EdtAramaMetni_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down && lvCariler.Items.Count > 0)
        {
            lvCariler.Focus();
            if (lvCariler.SelectedIndex < 0) lvCariler.SelectedIndex = 0;
            (lvCariler.ItemContainerGenerator.ContainerFromIndex(lvCariler.SelectedIndex) as ListViewItem)?.Focus();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && SeciliCari != null)
        {
            HareketleriAc();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && edtAramaMetni.Text.Length > 0)
        {
            edtAramaMetni.Clear();
            e.Handled = true;
        }
    }

    private void CmbAra_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_ready) return;
        _cariVm.AramaModu = cmbAra.SelectedIndex == 1 ? AramaModu.Iceren : AramaModu.YazildigiGibi;
        SonFiltreleriKaydet();
        CariYenile();
    }

    private void CmbHesap_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_ready) return;
        _cariVm.HesapDurumuFiltre = (HesapDurumu)Math.Clamp(cmbHesap.SelectedIndex, 0, 2);
        SonFiltreleriKaydet();
        CariYenile();
    }

    private void CmbFiltre_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_ready) return;
        _cariVm.BakiyeFiltresi = (BakiyeFiltresi)Math.Clamp(cmbFiltre.SelectedIndex, 0, 5);
        SonFiltreleriKaydet();
        CariYenile();
    }

    // Liste olayları
    private void LvCariler_SelectionChanged(object sender, SelectionChangedEventArgs e) => ButonDurumlari();

    private void LvCariler_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is DependencyObject d && ItemOf(d) != null && SeciliCari != null)
            HareketleriAc();
    }

    private static ListViewItem? ItemOf(DependencyObject d)
    {
        while (d != null && d is not ListViewItem) d = System.Windows.Media.VisualTreeHelper.GetParent(d);
        return d as ListViewItem;
    }

    private void LvCariler_HeaderClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not GridViewColumnHeader h || h.Column == null) return;
        int idx = ((GridView)lvCariler.View).Columns.IndexOf(h.Column);
        if (idx < 0) return;
        _cariVm.SortAsc = _cariVm.SortColumn == idx ? !_cariVm.SortAsc : true;
        _cariVm.SortColumn = idx;
        CariYenile();
    }

    // Toolbar
    private void BtnEkle_Click(object sender, RoutedEventArgs e)
    {
        var c = new Cari { FirmaId = _firma.Id, CariKodu = _cariVm.GetNextCariKodu() };
        var w = new CariEditWindow(c, true) { Owner = this };
        if (w.ShowDialog() == true)
        {
            CariYenile(c.Id);
            DurumYaz(string.Format(LocalizationManager.T("Status_CariEklendi"), c.Unvan));
        }
    }

    private void BtnDegistir_Click(object sender, RoutedEventArgs e)
    {
        var c = SeciliCari;
        if (c == null) return;

        var tam = DbFactory.GetCariRepo().GetById(c.Id) ?? c;
        var w = new CariEditWindow(tam, false) { Owner = this };
        if (w.ShowDialog() == true)
        {
            CariYenile(tam.Id);
            DurumYaz(string.Format(LocalizationManager.T("Status_CariGuncellendi"), tam.Unvan));
        }
    }

    private void BtnSil_Click(object sender, RoutedEventArgs e)
    {
        var c = SeciliCari;
        if (c == null) return;
        if (!DialogService.Onay(AppConsts.MSG_SILME_ONAY, LocalizationManager.T("Title_CariSil"),
                string.Format(LocalizationManager.T("Msg_CariSilmeDetay"), c.Unvan))) return;
        var res = _cariService.Delete(c.Id);
        if (res.IsSuccess)
        {
            CariYenile(0);
            DurumYaz(string.Format(LocalizationManager.T("Status_CariSilindi"), c.Unvan));
        }
        else
        {
            DialogService.Hata(LocalizationManager.T("Msg_SilmeBasarisiz"), LocalizationManager.T("Title_Hata"), res.ErrorMessage ?? "");
        }
    }

    private void BtnOzet_Click(object sender, RoutedEventArgs e)
    {
        CariYenile();
        ListeToplamlariWindow.ShowOzet(_cariVm.Ozet, _pb, this);
    }

    private void BtnYazdir_Click(object sender, RoutedEventArgs e)
    {
        var res = RaporSecimWindow.Execute(SeciliCari?.Unvan, SeciliCari != null, this);
        switch (res)
        {
            case RaporSecimResult.SeciliEkstre when SeciliCari != null:
                EkstreYazdir(SeciliCari);
                break;
            case RaporSecimResult.CariOzet:
                PrintService.CariListesi(_cariVm.Cariler, _firma.Unvan, _pb);
                break;
            case RaporSecimResult.GenelHareketler:
                var hrkList = DbFactory.GetHareketRepo().GetGenelHareketler(_firma.Id,
                    new DateTime(_firma.Donem, 1, 1), new DateTime(_firma.Donem, 12, 31), 0);
                PrintService.GenelHareketler(hrkList, _firma.Unvan, _pb);
                break;
        }
    }

    private void EkstreYazdir(Cari c)
    {
        var hareketler = DbFactory.GetHareketRepo().GetByCariId(c.Id);
        var tam = DbFactory.GetCariRepo().GetById(c.Id) ?? c;
        PrintService.Ekstre(tam, hareketler, _firma.Unvan, _pb);
    }

    private void BtnHareketler_Click(object sender, RoutedEventArgs e) => HareketleriAc();

    internal void HareketleriAc()
    {
        var c = SeciliCari;
        if (c == null) return;
        _hrkVm.SortColumn = -1;
        _hrkVm.CariYukle(c.Id);
        HareketEkraniGuncelle();
        SayfaGoster(true);
    }

    // ═════════════════════════ Hareketler sayfası ═════════════════════════

    private void HareketEkraniGuncelle()
    {
        var c = _hrkVm.Cari;
        lblHrkCariUnvan.Text = c?.Unvan ?? "";
        lblHrkCariIl.Text    = string.Join(" / ", new[] { c?.Il, c?.Ilce }.Where(s => !string.IsNullOrWhiteSpace(s)));
        lblHrkCariTel.Text   = string.IsNullOrWhiteSpace(c?.Gsm) ? c?.Telefon ?? "" : c!.Gsm;

        lvHareketler.ItemsSource = _hrkVm.Hareketler;
        valHrkBorc.Text    = Para(_hrkVm.ToplamBorc);
        valHrkAlacak.Text  = Para(_hrkVm.ToplamAlacak);
        valHrkBakiye.Text  = Para(_hrkVm.Bakiye);
        valHrkBakiye.Foreground = new SolidColorBrush(_hrkVm.Bakiye > 0.001m 
            ? AppTheme.BorcluRed 
            : (_hrkVm.Bakiye < -0.001m ? AppTheme.AlacakliGreen : AppTheme.SifirBlack));
        valHrkIslem.Text   = _hrkVm.IslemSayisi.ToString();
        if (lvHareketler.Items.Count > 0)
        {
            lvHareketler.SelectedIndex = lvHareketler.Items.Count - 1;
            lvHareketler.ScrollIntoView(lvHareketler.SelectedItem);
        }
        HareketButonDurumlari();
    }

    private void HareketButonDurumlari()
    {
        bool var = SeciliHareket != null;
        btnHrkDuzenle.IsEnabled = btnHrkSil.IsEnabled = var;
    }

    private void LvHareketler_SelectionChanged(object sender, SelectionChangedEventArgs e) => HareketButonDurumlari();

    private void LvHareketler_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is DependencyObject d && ItemOf(d) != null) BtnHrkDuzenle_Click(sender, e);
    }

    private void LvHareketler_HeaderClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not GridViewColumnHeader h || h.Column == null) return;
        int idx = ((GridView)lvHareketler.View).Columns.IndexOf(h.Column);
        if (idx < 0) return;
        _hrkVm.SortAsc = _hrkVm.SortColumn == idx ? !_hrkVm.SortAsc : true;
        _hrkVm.SortColumn = idx;
        _hrkVm.YukleHareketler();
        HareketEkraniGuncelle();
    }

    private void BtnCariListesi_Click(object sender, RoutedEventArgs e)
    {
        int? id = _hrkVm.Cari?.Id;
        SayfaGoster(false);
        CariYenile(id);
    }

    private void BtnHrkFisler_Click(object sender, RoutedEventArgs e)
    {
        ctxFisler.PlacementTarget = btnHrkFisler;
        ctxFisler.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
        ctxFisler.IsOpen = true;
    }

    private void MiHesabaBorcEkle_Click(object sender, RoutedEventArgs e) => FisEkle(IslemTuru.Borc);
    private void MiTahsilat_Click(object sender, RoutedEventArgs e)       => FisEkle(IslemTuru.Tahsilat);
    private void MiHesabaAlacak_Click(object sender, RoutedEventArgs e)   => FisEkle(IslemTuru.Alacak);
    private void MiOdeme_Click(object sender, RoutedEventArgs e)          => FisEkle(IslemTuru.Odeme);

    /// <summary>Tahsilat/Alacak → ALACAK sütunu; Borç/Ödeme/Devir → BORÇ sütunu (Delphi ile aynı).</summary>
    private static bool AlacakSutunu(IslemTuru t) => t is IslemTuru.Tahsilat or IslemTuru.Alacak;

    private void FisEkle(IslemTuru tur)
    {
        var cari = _hrkVm.Cari;
        if (cari == null) return;

        var w = new DekontEditWindow(tur, cari.Unvan) { Owner = this };
        if (w.ShowDialog() != true) return;

        // Risk limiti kontrolü (yalnız borç yazan işlemlerde)
        if (!AlacakSutunu(tur))
        {
            decimal yeni = _hrkVm.Bakiye + w.Tutar;
            if (cari.CariLimit > 0 && yeni > cari.CariLimit &&
                !DialogService.Onay(string.Format(LocalizationManager.T("Msg_RiskLimitiAsimi"), Para(yeni), Para(cari.CariLimit)),
                    LocalizationManager.T("Title_RiskLimiti"), LocalizationManager.T("Msg_YinedeKaydedilsinMi")))
                return;
        }

        decimal onceki = _hrkVm.Bakiye;
        var h = new Hareket
        {
            FirmaId = _firma.Id, CariId = cari.Id, CariUnvan = cari.Unvan,
            Tarih = w.IslemTarihi, IslemTuru = tur, Aciklama = w.Aciklama,
            Borc   = AlacakSutunu(tur) ? 0 : w.Tutar,
            Alacak = AlacakSutunu(tur) ? w.Tutar : 0
        };

        var res = _hareketService.Save(h, isNew: true);
        if (res.IsSuccess)
        {
            _hrkVm.YukleHareketler();
            _hrkVm.RefreshCari();
            HareketEkraniGuncelle();
            DurumYaz(string.Format(LocalizationManager.T("Status_FisKaydedildi"), tur.ToDisplayString(), Para(w.Tutar)));

            if (DialogService.Onay(string.Format(LocalizationManager.T("Msg_FisKaydedildi"), tur.ToDisplayString()), 
                                   LocalizationManager.T("Title_Makbuz"), 
                                   LocalizationManager.T("Msg_MakbuzYazdirilsinMi"), 
                                   varsayilanEvet: false))
                PrintService.Makbuz(cari, h, onceki, _hrkVm.Bakiye, _firma.Unvan, _pb);
        }
        else
        {
            DialogService.Hata(LocalizationManager.T("Msg_FisKaydedilemedi"), LocalizationManager.T("Title_Hata"), res.ErrorMessage ?? "");
        }
    }

    private void BtnHrkDuzenle_Click(object sender, RoutedEventArgs e)
    {
        var h = SeciliHareket;
        var cari = _hrkVm.Cari;
        if (h == null || cari == null) return;

        var w = new DekontEditWindow(h.IslemTuru, cari.Unvan, h) { Owner = this };
        if (w.ShowDialog() != true) return;

        h.Tarih = w.IslemTarihi;
        h.Aciklama = w.Aciklama;
        h.Borc   = AlacakSutunu(h.IslemTuru) ? 0 : w.Tutar;
        h.Alacak = AlacakSutunu(h.IslemTuru) ? w.Tutar : 0;
        var res = _hareketService.Save(h, isNew: false);
        if (res.IsSuccess)
        {
            _hrkVm.YukleHareketler();
            HareketEkraniGuncelle();
            DurumYaz(LocalizationManager.T("Status_FisGuncellendi"));
        }
        else
        {
            DialogService.Hata(LocalizationManager.T("Msg_FisGuncellenemedi"), LocalizationManager.T("Title_Hata"), res.ErrorMessage ?? "");
        }
    }

    private void BtnHrkSil_Click(object sender, RoutedEventArgs e)
    {
        var h = SeciliHareket;
        if (h == null) return;
        if (!DialogService.Onay(AppConsts.MSG_SILME_ONAY, LocalizationManager.T("Title_FisSil"),
                $"{h.Tarih:dd.MM.yyyy} — {h.IslemTuruText} — {Para(h.Borc > 0 ? h.Borc : h.Alacak)}")) return;
        var res = _hareketService.Delete(h.Id);
        if (res.IsSuccess)
        {
            _hrkVm.YukleHareketler();
            HareketEkraniGuncelle();
            DurumYaz(LocalizationManager.T("Status_FisSilindi"));
        }
        else
        {
            DialogService.Hata(LocalizationManager.T("Msg_SilmeBasarisiz"), LocalizationManager.T("Title_Hata"), res.ErrorMessage ?? "");
        }
    }

    private void BtnHrkYazdir_Click(object sender, RoutedEventArgs e)
    {
        var cari = _hrkVm.Cari;
        if (cari == null) return;

        var m = new ContextMenu();
        var a = new MenuItem { Header = LocalizationManager.T("Menu_CariHesapEkstresi") };
        a.Click += (_, _) => PrintService.Ekstre(cari, _hrkVm.Hareketler, _firma.Unvan, _pb);
        var b = new MenuItem { Header = LocalizationManager.T("Menu_SeciliFisMakbuzu"), IsEnabled = SeciliHareket != null };
        b.Click += (_, _) =>
        {
            var h = SeciliHareket;
            if (h == null) return;
            decimal sonra = h.YuruyenBakiye;
            decimal once  = sonra - (h.Borc - h.Alacak);
            PrintService.Makbuz(cari, h, once, sonra, _firma.Unvan, _pb);
        };
        m.Items.Add(a); m.Items.Add(b);
        m.PlacementTarget = btnHrkYazdir;
        m.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
        m.IsOpen = true;
    }

    private void BtnOncekiCari_Click(object sender, RoutedEventArgs e) => CariGec(-1);
    private void BtnSonrakiCari_Click(object sender, RoutedEventArgs e) => CariGec(+1);

    private void CariGec(int yon)
    {
        if (_hrkVm.Cari == null || _cariVm.Cariler.Count == 0) return;
        int idx = _cariVm.IndexOfCari(_hrkVm.Cari.Id);
        int yeni = idx + yon;
        if (yeni < 0 || yeni >= _cariVm.Cariler.Count) return;

        lvCariler.SelectedIndex = yeni;
        _hrkVm.CariYukle(_cariVm.Cariler[yeni].Id);
        HareketEkraniGuncelle();
        DurumYaz(string.Format(LocalizationManager.T("Status_HareketlerCari"), _hrkVm.Cari?.Unvan));
    }

    // ═════════════════════════ Durum çubuğu ═════════════════════════

    private async void LblDovizKur_Click(object sender, MouseButtonEventArgs e)
    {
        lblDovizKur.Text = LocalizationManager.T("Status_KurGuncelleniyor");
        await DovizKuruService.GuncelleAsync();
        var t = DovizKuruService.GetKurMetni();
        lblDovizKur.Text = string.IsNullOrEmpty(t) ? LocalizationManager.T("Status_KurAlinamadi") : t;
    }

    private void BtnHesapMakinesi_Click(object sender, RoutedEventArgs e)
    {
        try { Process.Start(new ProcessStartInfo("calc.exe") { UseShellExecute = true }); }
        catch (Exception ex) { DialogService.Hata(LocalizationManager.T("Msg_HesapMakinesiAcilamadi"), LocalizationManager.T("Title_Hata"), ex.Message); }
    }

    // ═════════════════════════ Klavye kısayolları ═════════════════════════

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        bool yazmaAlani = Keyboard.FocusedElement is TextBox or ComboBox;
        bool alt = (Keyboard.Modifiers & ModifierKeys.Alt) != 0;
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (HareketSayfasinda)
        {
            switch (key)
            {
                case Key.F5: FisEkle(IslemTuru.Borc);     e.Handled = true; return;
                case Key.F6: FisEkle(IslemTuru.Tahsilat); e.Handled = true; return;
                case Key.F7: FisEkle(IslemTuru.Alacak);   e.Handled = true; return;
                case Key.F8: FisEkle(IslemTuru.Odeme);    e.Handled = true; return;
                case Key.F4: BtnHrkDuzenle_Click(this, e); e.Handled = true; return;
                case Key.Delete when !yazmaAlani: BtnHrkSil_Click(this, e); e.Handled = true; return;
                case Key.Escape: BtnCariListesi_Click(this, e); e.Handled = true; return;
                case Key.Left  when alt || !yazmaAlani: CariGec(-1); e.Handled = true; return;
                case Key.Right when alt || !yazmaAlani: CariGec(+1); e.Handled = true; return;
            }
            return;
        }

        switch (key)
        {
            case Key.F2: BtnOzet_Click(this, e);       e.Handled = true; break;
            case Key.F3: BtnEkle_Click(this, e);       e.Handled = true; break;
            case Key.F4: BtnDegistir_Click(this, e);   e.Handled = true; break;
            case Key.Delete when !yazmaAlani: BtnSil_Click(this, e); e.Handled = true; break;
            case Key.Enter when !yazmaAlani && SeciliCari != null:
                HareketleriAc(); e.Handled = true; break;
            case Key.F5 or Key.F6 or Key.F7 or Key.F8 when SeciliCari != null:
                // Cari listesinden doğrudan fiş: önce hareket ekranına geç
                HareketleriAc();
                FisEkle(key switch { Key.F5 => IslemTuru.Borc, Key.F6 => IslemTuru.Tahsilat,
                                     Key.F7 => IslemTuru.Alacak, _ => IslemTuru.Odeme });
                e.Handled = true; break;
        }
    }

    // ═════════════════════════ Sağ tık menüleri ═════════════════════════

    private void KurSagTikMenuleri()
    {
        MenuItem Mi(string h, RoutedEventHandler c)
        {
            var mi = new MenuItem { Header = h };
            mi.Click += c;
            return mi;
        }

        var cm = new ContextMenu();
        cm.Items.Add(Mi(LocalizationManager.T("Ctx_YeniCari"),       BtnEkle_Click));
        cm.Items.Add(Mi(LocalizationManager.T("Ctx_Duzenle"),        BtnDegistir_Click));
        cm.Items.Add(Mi(LocalizationManager.T("Ctx_Sil"),            BtnSil_Click));
        cm.Items.Add(new Separator());
        cm.Items.Add(Mi(LocalizationManager.T("Ctx_Hareketler"),     BtnHareketler_Click));
        cm.Items.Add(Mi(LocalizationManager.T("Ctx_Yazdir"),         BtnYazdir_Click));
        cm.Items.Add(Mi(LocalizationManager.T("Ctx_Yenile"),         (_, _) => CariYenile()));
        lvCariler.ContextMenu = cm;

        var hm = new ContextMenu();
        hm.Items.Add(Mi(LocalizationManager.T("Ctx_HesabaBorcEkle"),   MiHesabaBorcEkle_Click));
        hm.Items.Add(Mi(LocalizationManager.T("Ctx_TahsilatYap"),       MiTahsilat_Click));
        hm.Items.Add(Mi(LocalizationManager.T("Ctx_HesabaAlacakEkle"), MiHesabaAlacak_Click));
        hm.Items.Add(Mi(LocalizationManager.T("Ctx_OdemeYap"),          MiOdeme_Click));
        hm.Items.Add(new Separator());
        hm.Items.Add(Mi(LocalizationManager.T("Ctx_Duzenle"),          BtnHrkDuzenle_Click));
        hm.Items.Add(Mi(LocalizationManager.T("Ctx_Sil"),              BtnHrkSil_Click));
        hm.Items.Add(Mi(LocalizationManager.T("Ctx_Yazdir"),           BtnHrkYazdir_Click));
        hm.Items.Add(Mi(LocalizationManager.T("Ctx_Yenile"),           (_, _) => { _hrkVm.YukleHareketler(); HareketEkraniGuncelle(); }));
        lvHareketler.ContextMenu = hm;
    }
}
