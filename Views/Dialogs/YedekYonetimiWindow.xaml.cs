using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using GelesiyeWPF.Data;
using GelesiyeWPF.Services;

namespace GelesiyeWPF.Views.Dialogs;

public class YedekDosyaItem
{
    public string Name { get; set; } = "";
    public DateTime CreationTime { get; set; }
    public long Length { get; set; }
    public string FullName { get; set; } = "";
    public string FormattedLength => FormatSize(Length);

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F2} MB";
    }
}

public partial class YedekYonetimiWindow : Window
{
    public bool VeriGeriYuklendi { get; private set; }

    public YedekYonetimiWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        edtKonum.Text = BackupService.GetBackupDir();
        ListeyiYenile();
    }

    private void ListeyiYenile()
    {
        var list = BackupService.GetBackupList();
        var items = list.Select(f => new YedekDosyaItem
        {
            Name = f.Name,
            CreationTime = f.CreationTime,
            Length = f.Length,
            FullName = f.FullName
        }).ToList();

        lvYedekler.ItemsSource = items;

        long toplamBoyut = items.Sum(i => i.Length);
        string sonYedek = DbFactory.GetAyarlarRepo().GetAyar("SonYedekTarihi", "Henüz alınmadı");
        lblIstatistik.Text = $"Toplam {items.Count} yedek dosyası | Kaplanan Alan: {FormatSize(toplamBoyut)} | Son Yedek: {sonYedek}";

        ButonDurumlari();
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F2} MB";
    }

    private YedekDosyaItem? SeciliYedek => lvYedekler.SelectedItem as YedekDosyaItem;

    private void ButonDurumlari()
    {
        if (btnGeriYukle == null || btnSil == null) return;
        bool secildi = SeciliYedek != null;
        btnGeriYukle.IsEnabled = secildi;
        btnSil.IsEnabled = secildi;
    }

    private void LvYedekler_SelectionChanged(object sender, SelectionChangedEventArgs e) => ButonDurumlari();

    private void LvYedekler_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (SeciliYedek != null) BtnGeriYukle_Click(sender, e);
    }

    private void BtnSimdiYedekle_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var f = BackupService.Backup();
            ListeyiYenile();
            DialogService.Bilgi(LocalizationManager.T("Msg_YeniYedekAlindi"), LocalizationManager.T("Title_YedekAlindi"), Path.GetFileName(f));
        }
        catch (Exception ex)
        {
            DialogService.Hata(LocalizationManager.T("Msg_YedeklemeBasarisiz"), LocalizationManager.T("Title_Hata"), ex.Message);
        }
    }

    private void BtnGeriYukle_Click(object sender, RoutedEventArgs e)
    {
        var item = SeciliYedek;
        if (item == null || !File.Exists(item.FullName)) return;

        if (!DialogService.Onay(
            string.Format(LocalizationManager.T("Msg_YedekGeriYuklemeOnay"), item.Name),
            LocalizationManager.T("Title_YedekGeriYuklemeOnayi")))
            return;

        try
        {
            BackupService.Restore(item.FullName);
            VeriGeriYuklendi = true;
            DialogService.Bilgi(LocalizationManager.T("Msg_VeritabaniGeriYuklendi"), LocalizationManager.T("Title_GeriYuklemeBasarili"));
            ListeyiYenile();
        }
        catch (Exception ex)
        {
            DialogService.Hata(LocalizationManager.T("Msg_GeriYuklemeBasarisiz"), LocalizationManager.T("Title_Hata"), ex.Message);
        }
    }

    private void BtnSil_Click(object sender, RoutedEventArgs e)
    {
        var item = SeciliYedek;
        if (item == null || !File.Exists(item.FullName)) return;

        if (!DialogService.Onay(string.Format(LocalizationManager.T("Msg_YedekSilmeOnay"), item.Name), LocalizationManager.T("Title_YedekSilmeOnayi")))
            return;

        try
        {
            File.Delete(item.FullName);
            ListeyiYenile();
        }
        catch (Exception ex)
        {
            DialogService.Hata(LocalizationManager.T("Msg_DosyaSilinemedi"), LocalizationManager.T("Title_Hata"), ex.Message);
        }
    }

    private void BtnKonumDegistir_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFolderDialog
        {
            Title = "Yedekleme Klasörünü Seçiniz",
            InitialDirectory = BackupService.GetBackupDir()
        };
        if (dlg.ShowDialog() == true)
        {
            DbFactory.GetAyarlarRepo().SetAyar("YedekKlasoru", dlg.FolderName);
            edtKonum.Text = dlg.FolderName;
            ListeyiYenile();
        }
    }

    private void BtnKlasoruAc_Click(object sender, RoutedEventArgs e)
    {
        var dir = BackupService.GetBackupDir();
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", dir) { UseShellExecute = true });
        }
        catch { }
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
