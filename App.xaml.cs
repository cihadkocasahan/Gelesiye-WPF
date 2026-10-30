using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using GelesiyeWPF.Data;
using GelesiyeWPF.Services;
using GelesiyeWPF.Views;
using GelesiyeWPF.Views.Dialogs;

namespace GelesiyeWPF;

/// <summary>Uygulama girişi: SQLite başlat → şifre kontrolü → ana pencere.</summary>
public partial class App : Application
{
    public App()
    {
        // 1. UI Thread unhandled exceptions
        DispatcherUnhandledException += (sender, args) =>
        {
            LogService.Error("UI Thread üzerinde işlenmemiş kritik hata oluştu.", args.Exception);
            MessageBox.Show(
                $"Beklenmeyen bir hata oluştu:\n\n{args.Exception.Message}\n\nDetaylar log dosyasına kaydedildi:\n{LogService.LogDirectory}\\gelesiye_error.log",
                "Gelesiye — Beklenmeyen Hata",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            args.Handled = true;
        };

        // 2. Non-UI / Background Thread unhandled exceptions
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                LogService.Error("AppDomain üzerinde yakalanamayan kritik hata oluştu.", ex);
            }
        };

        // 3. Unobserved Task exceptions
        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            LogService.Error("Arka plan görevinde (Task) yakalanmamış hata oluştu.", args.Exception);
            args.SetObserved();
        };
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        LogService.Info("=================== Gelesiye Başlatılıyor ===================");
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var tr = new CultureInfo("tr-TR");
        CultureInfo.DefaultThreadCurrentCulture = tr;
        CultureInfo.DefaultThreadCurrentUICulture = tr;
        Thread.CurrentThread.CurrentCulture = tr;
        FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage("tr-TR")));

        try
        {
            SQLitePCL.Batteries_V2.Init();
            DbFactory.Initialize();
            Services = ConfigureServices();
            string dil = DbFactory.GetAyarlarRepo().GetAyar("UygulamaDili", "tr-TR");
            LocalizationManager.Instance.SetLanguage(dil, kaydet: false);
            LogService.Info($"SQLite veritabanı, servisler (DI) ve uygulama dili ({dil}) başarıyla hazırlandı.");
        }
        catch (Exception ex)
        {
            LogService.Error("Veritabanı veya servisler başlatılamadı.", ex);
            MessageBox.Show("Veritabanı başlatılamadı:\n" + ex.Message + $"\n\nLog: {LogService.LogDirectory}", "Gelesiye",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
            return;
        }

        if (e.Args.Contains("--capture-screenshots"))
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var docDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "docs", "screenshots"));
            Directory.CreateDirectory(docDir);

            // Verileri hazırla
            DemoSeed.EnsureRealisticMix(DbFactory.GetDb(), 1);
            var repo = DbFactory.GetCariRepo();
            var cariler = repo.GetAll(new Common.CariFiltre { FirmaId = 1 });
            var cari1 = cariler.FirstOrDefault() ?? new Domain.Cari { Unvan = "Örnek Ticaret Ltd. Şti." };

            // 1. Giriş Ekranı
            var login = new LoginWindow();
            SaveWindowScreenshot(login, Path.Combine(docDir, "00_giris_ekrani.png"), 420, 240);

            // 2. Ana Ekran (Cari Listesi, Kartlar, Filtreler)
            var main = Services.GetRequiredService<Views.MainWindow>();
            main.Width = 1040;
            main.Height = 680;
            SaveWindowScreenshot(main, Path.Combine(docDir, "01_ana_ekran.png"), 1040, 680);

            // 3. Hareketler Ekranı
            var main2 = Services.GetRequiredService<Views.MainWindow>();
            main2.HareketleriAc();
            SaveWindowScreenshot(main2, Path.Combine(docDir, "02_hareket_ekstresi.png"), 1040, 680);

            // 4. Cari Kartı (Düzenleme/Yeni)
            var cariEdit = new CariEditWindow(cari1, isNew: false);
            SaveWindowScreenshot(cariEdit, Path.Combine(docDir, "03_cari_karti.png"), 620, 520);

            // 5. Dekont / Fiş Girişi (Tahsilat Fişi)
            var dekont = new DekontEditWindow(Common.IslemTuru.Tahsilat, cari1.Unvan);
            SaveWindowScreenshot(dekont, Path.Combine(docDir, "04_dekont_girisi.png"), 520, 420);

            // 6. FlowDocument Baskı Önizleme Penceresi
            var hrkList = DbFactory.GetHareketRepo().GetByCariId(cari1.Id);
            var doc = PrintService.BuildEkstreDoc(cari1, hrkList, "Gelesiye Ticaret", "TL");
            var onizle = new RaporOnizlemeWindow(doc, "Cari Hesap Ekstresi — " + cari1.Unvan);
            onizle.Width = 980;
            onizle.Height = 720;
            SaveWindowScreenshot(onizle, Path.Combine(docDir, "05_rapor_onizleme.png"), 980, 720);

            // 7. Yedek Yönetimi
            var ydk = new YedekYonetimiWindow();
            SaveWindowScreenshot(ydk, Path.Combine(docDir, "06_yedek_yonetimi.png"), 640, 480);

            // 8. Ayarlar
            var ayr = new AyarlarWindow(0);
            SaveWindowScreenshot(ayr, Path.Combine(docDir, "07_ayarlar.png"), 600, 460);

            Console.WriteLine("ALL_SCREENSHOTS_CAPTURED_SUCCESSFULLY");
            Shutdown(0);
            return;
        }

        if (e.Args.Contains("--test-dialogs"))
        {
            // Test each dialog instantiation and tab switching
            for (int i = 0; i < 5; i++)
            {
                var ayar = new Views.Dialogs.AyarlarWindow(i);
            }
            var ydk = new Views.Dialogs.YedekYonetimiWindow();
            var cari = new Views.Dialogs.CariEditWindow(new Domain.Cari(), true);
            var dkt = new Views.Dialogs.DekontEditWindow(Common.IslemTuru.Borc, "Test");
            var hak = new Views.Dialogs.HakkindaWindow();
            var ozet = new Views.Dialogs.ListeToplamlariWindow(new Common.BakiyeOzet(), "TL");
            var rap = new Views.Dialogs.RaporSecimWindow("Test", true);
            var onizle = new Views.Dialogs.RaporOnizlemeWindow(new System.Windows.Documents.FlowDocument(), "Test Önizleme");
            var cls = new Views.Dialogs.CloseActionWindow();

            // Verify dynamic localization switching
            LocalizationManager.Instance.SetLanguage("en-US", kaydet: false);
            string enTitle = LocalizationManager.T("App_Title");
            string enClose = LocalizationManager.T("Btn_Kapat");
            string enBackup = LocalizationManager.T("Title_YedekYonetimi");
            var valEn = new Validators.CariValidator().Validate(new Domain.Cari());
            string enValMsg = valEn.Errors.FirstOrDefault()?.ErrorMessage ?? "";

            LocalizationManager.Instance.SetLanguage("tr-TR", kaydet: false);
            string trTitle = LocalizationManager.T("App_Title");
            string trClose = LocalizationManager.T("Btn_Kapat");
            string trBackup = LocalizationManager.T("Title_YedekYonetimi");
            var valTr = new Validators.CariValidator().Validate(new Domain.Cari());
            string trValMsg = valTr.Errors.FirstOrDefault()?.ErrorMessage ?? "";

            // Verify realistic balance mix
            DemoSeed.EnsureRealisticMix(DbFactory.GetDb(), 1);
            var repo = DbFactory.GetCariRepo();
            var cariler = repo.GetAll(new Common.CariFiltre { FirmaId = 1 });
            int borclu = cariler.Count(c => c.Bakiye > 0.001m);
            int alacakli = cariler.Count(c => c.Bakiye < -0.001m);
            int sifir = cariler.Count(c => Math.Abs(c.Bakiye) <= 0.001m);

            Console.WriteLine($"ALL_DIALOGS_LOADED_SUCCESSFULLY: EN='{enTitle}', '{enClose}', '{enBackup}', VAL='{enValMsg}' | TR='{trTitle}', '{trClose}', '{trBackup}', VAL='{trValMsg}' | CARILER: Toplam={cariler.Count}, Borclu={borclu}, Alacakli={alacakli}, Sifir={sifir}");
            Shutdown(0);
            return;
        }

        if (!LoginWindow.Execute())
        {
            LogService.Info("Kullanıcı giriş yapmadan çıkışı seçti veya haklar tükendi.");
            Shutdown(0);
            return;
        }

        LogService.Info("Kullanıcı girişi başarılı.");
        BackupService.CheckAutoBackup(onOpen: true);

        try
        {
            var main = Services.GetRequiredService<Views.MainWindow>();
            MainWindow = main;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            main.Show();
            LogService.Info("Ana pencere başarıyla yüklendi ve gösterildi.");
        }
        catch (Exception ex)
        {
            LogService.Error("Ana pencere açılırken kritik hata oluştu.", ex);
            MessageBox.Show(
                $"Ana pencere açılırken hata oluştu:\n\n{ex.Message}\n\nLog dosyası:\n{LogService.LogDirectory}\\gelesiye_error.log",
                "Gelesiye — Başlatma Hatası",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    public static IServiceProvider Services { get; private set; } = null!;

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Database & Repositories
        services.AddSingleton(_ => DbFactory.GetDb());
        services.AddTransient(_ => DbFactory.GetCariRepo());
        services.AddTransient(_ => DbFactory.GetHareketRepo());
        services.AddTransient(_ => DbFactory.GetFirmaRepo());
        services.AddTransient(_ => DbFactory.GetAyarlarRepo());
        services.AddTransient(_ => DbFactory.GetIlIlceRepo());

        // Validators
        services.AddTransient<FluentValidation.IValidator<Domain.Cari>, Validators.CariValidator>();
        services.AddTransient<FluentValidation.IValidator<Domain.Hareket>, Validators.HareketValidator>();
        services.AddTransient<FluentValidation.IValidator<Domain.Firma>, Validators.FirmaValidator>();

        // Services
        services.AddTransient<ICariService, CariService>();
        services.AddTransient<IHareketService, HareketService>();

        // ViewModels
        services.AddTransient<ViewModels.CariListViewModel>();
        services.AddTransient<ViewModels.HareketViewModel>();

        // Views
        services.AddTransient<Views.MainWindow>();

        return services.BuildServiceProvider();
    }

    private static void SaveWindowScreenshot(Window win, string outputPath, int width = 0, int height = 0)
    {
        int w = width > 0 ? width : (int)(double.IsNaN(win.Width) || win.Width <= 0 ? 980 : win.Width);
        int h = height > 0 ? height : (int)(double.IsNaN(win.Height) || win.Height <= 0 ? 650 : win.Height);

        win.Width = w;
        win.Height = h;
        win.Measure(new Size(w, h));
        win.Arrange(new Rect(0, 0, w, h));
        win.UpdateLayout();

        var rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(win);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(rtb));

        var dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        using var fs = File.Create(outputPath);
        encoder.Save(fs);
    }
}
