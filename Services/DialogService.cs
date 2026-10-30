using System.Windows;
using GelesiyeWPF.Common;
using GelesiyeWPF.Views.Dialogs;

namespace GelesiyeWPF.Services;

/// <summary>Uygulama genelinde standart modal iletişim kutularını yöneten servis.</summary>
public static class DialogService
{
    private static Window? ActiveOwner() =>
        Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
        ?? Application.Current?.MainWindow;

    private static bool Show(DialogTur tur, string mesaj, string baslik, string? detay, bool varsayilanEvet = true)
    {
        var dlg = new AppDialog(tur, mesaj, baslik, detay, varsayilanEvet);
        var owner = ActiveOwner();
        if (owner != null && owner.IsVisible && !ReferenceEquals(owner, dlg)) dlg.Owner = owner;
        return dlg.ShowDialog() == true;
    }

    public static void Bilgi(string mesaj, string baslik = "Bilgi", string? detay = null)  => Show(DialogTur.Bilgi,  mesaj, baslik, detay);
    public static void Uyari(string mesaj, string baslik = "Uyarı", string? detay = null)  => Show(DialogTur.Uyari,  mesaj, baslik, detay);
    public static void Hata(string mesaj, string baslik = "Hata", string? detay = null)     => Show(DialogTur.Hata,   mesaj, baslik, detay);
    public static bool Onay(string mesaj, string baslik = "Onay", string? detay = null, bool varsayilanEvet = true) => Show(DialogTur.Onay, mesaj, baslik, detay, varsayilanEvet);
}
