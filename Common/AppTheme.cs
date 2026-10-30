using System.Windows.Media;

namespace GelesiyeWPF.Common;

public static class AppTheme
{
    // ── Renkler (Delphi uGelesiyeTheme.pas'tan birebir çeviri) ──────────────

    // Header
    public static Color HeaderMain        => Color.FromRgb(0x10, 0x7F, 0xD2); // #107FD2 Kurumsal Mavi
    public static Color HeaderModal       => Color.FromRgb(0x10, 0x7F, 0xD2);
    public static Color HeaderModalText   => Colors.White;
    public static Color PanelBottom       => Colors.White;

    // Dekont başlık renkleri
    public static Color HeaderDekontBorc    => Color.FromRgb(0x1E, 0x3A, 0x5F); // Lacivert
    public static Color HeaderDekontTahsil  => Color.FromRgb(0x15, 0x80, 0x3D); // Zümrüt
    public static Color HeaderDekontOdeme   => Color.FromRgb(0xB9, 0x1C, 0x1C); // Kırmızı
    public static Color HeaderDekontAlacak  => Color.FromRgb(0x43, 0x38, 0xCA); // İndigo
    public static Color HeaderDekontDevir   => Color.FromRgb(0x33, 0x41, 0x55); // Slate

    // Form / Kart zeminleri
    public static Color FormBg          => Color.FromRgb(0xF1, 0xF5, 0xF9); // Slate-100
    public static Color CardBg          => Colors.White;
    public static Color BorderColor     => Color.FromRgb(0xE2, 0xE8, 0xF0); // Slate-200

    // Durum çubuğu
    public static Color StatusBg        => Color.FromRgb(0xF8, 0xFA, 0xFC);
    public static Color StatusBorder    => Color.FromRgb(0xE2, 0xE8, 0xF0);
    public static Color StatusTextMain  => Color.FromRgb(0x0F, 0x17, 0x2A);
    public static Color StatusTextMuted => Color.FromRgb(0x64, 0x74, 0x8B);
    public static Color StatusTextInfo  => Color.FromRgb(0x94, 0xA3, 0xB8);

    // Zebra Grid
    public static Color ZebraEven => Colors.White;
    public static Color ZebraOdd  => Color.FromRgb(0xF1, 0xF5, 0xF9);

    // İşlem tipi metin renkleri (grid)
    public static Color TextBorc    => Color.FromRgb(0xD4, 0x1A, 0x1A); // kırmızı (BGR dönüşümü)
    public static Color TextAlacak  => Color.FromRgb(0x20, 0x5E, 0x1B); // yeşil
    public static Color TextOdeme   => Color.FromRgb(0xB7, 0x53, 0x0D); // sıcak ton
    public static Color TextDevir   => Color.FromRgb(0xA2, 0x1F, 0x7B); // mor
    public static Color BorcluRed   => Color.FromRgb(0xD4, 0x1A, 0x1A);
    public static Color AlacakliGreen=> Color.FromRgb(0x20, 0x5E, 0x1B);
    public static Color SifirBlack  => Color.FromRgb(0x21, 0x21, 0x21);

    // İşlem tipi arka plan renkleri
    public static Color BgBorc   => Color.FromRgb(0xFF, 0xE8, 0xE8);
    public static Color BgAlacak => Color.FromRgb(0xEA, 0xFA, 0xEA);

    // Dialog renkleri
    public static Color DlgInfo    => Color.FromRgb(0x25, 0x63, 0xEB);
    public static Color DlgConfirm => Color.FromRgb(0x25, 0x63, 0xEB);
    public static Color DlgWarning => Color.FromRgb(0xF9, 0x73, 0x16);
    public static Color DlgError   => Color.FromRgb(0xEF, 0x44, 0x44);

    public static Color AccentPrimary => Color.FromRgb(0x10, 0x7F, 0xD2);

    // SolidColorBrush factory
    public static SolidColorBrush Brush(Color c) => new(c);

    // İşlem türüne göre başlık rengi
    public static Color GetDekontHeaderColor(IslemTuru tur) => tur switch
    {
        IslemTuru.Borc     => HeaderDekontBorc,
        IslemTuru.Tahsilat => HeaderDekontTahsil,
        IslemTuru.Odeme    => HeaderDekontOdeme,
        IslemTuru.Alacak   => HeaderDekontAlacak,
        IslemTuru.Devir    => HeaderDekontDevir,
        _                  => HeaderDekontBorc
    };

    public static Color GetIslemTextColor(IslemTuru tur) => tur switch
    {
        IslemTuru.Borc     => TextBorc,
        IslemTuru.Tahsilat => TextAlacak,
        IslemTuru.Alacak   => TextAlacak,
        IslemTuru.Odeme    => TextOdeme,
        IslemTuru.Devir    => TextDevir,
        _                  => SifirBlack
    };
}
