using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using GelesiyeWPF.Common;
using GelesiyeWPF.Domain;

namespace GelesiyeWPF.Services;

/// <summary>
/// Delphi uPrintService (FastReport) karşılığı. FlowDocument tabanlı önizleme + yazdırma:
/// cari listesi, cari hesap ekstresi, tahsilat/ödeme/borç/alacak makbuzu.
/// </summary>
public static class PrintService
{
    private static readonly CultureInfo Tr = new("tr-TR");
    private static readonly Brush Header = new SolidColorBrush(Color.FromRgb(16, 127, 210)); // #107FD2
    private static readonly Brush TextDark = new SolidColorBrush(Color.FromRgb(15, 23, 42)); // #0F172A
    private static readonly Brush Muted  = new SolidColorBrush(Color.FromRgb(100, 116, 139)); // #64748B
    private static readonly Brush Red    = new SolidColorBrush(Color.FromRgb(212, 26, 26)); // #D41A1A Borçlu
    private static readonly Brush Green  = new SolidColorBrush(Color.FromRgb(32, 94, 27)); // #205E1B Alacaklı
    private static readonly Brush CellBorder = new SolidColorBrush(Color.FromRgb(226, 232, 240)); // #E2E8F0

    // ───────────────────────── Önizleme penceresi ─────────────────────────
    public static void Onizle(FlowDocument doc, string baslik)
    {
        var win = new Views.Dialogs.RaporOnizlemeWindow(doc, baslik)
        {
            Owner = Application.Current.MainWindow
        };
        win.ShowDialog();
    }

    // ───────────────────────── Yardımcılar ─────────────────────────
    private static FlowDocument YeniDoc() => new()
    {
        FontFamily = new FontFamily("Segoe UI"),
        FontSize = 12,
        PagePadding = new Thickness(48),
        ColumnWidth = double.PositiveInfinity,
        PageWidth = 794,
        PageHeight = 1123,
        Background = Brushes.White,
        Foreground = TextDark
    };

    private static Paragraph Baslik(string metin, double size = 20) => new(new Run(metin))
    {
        FontSize = size, FontWeight = FontWeights.Bold, Foreground = Header, Margin = new Thickness(0, 0, 0, 2)
    };

    private static Paragraph Alt(string metin) => new(new Run(metin))
    {
        Foreground = Muted, FontSize = 11, Margin = new Thickness(0, 0, 0, 10)
    };

    private static TableCell Hucre(string metin, bool sag = false, bool kalin = false, Brush? renk = null, bool baslik = false)
    {
        var p = new Paragraph(new Run(metin))
        {
            Margin = new Thickness(6, 4, 6, 4),
            TextAlignment = sag ? TextAlignment.Right : TextAlignment.Left,
            FontWeight = (kalin || baslik) ? FontWeights.SemiBold : FontWeights.Normal,
            Foreground = renk ?? (baslik ? Brushes.White : TextDark)
        };

        return new TableCell(p)
        {
            BorderBrush = CellBorder,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Background = baslik ? Header : Brushes.White
        };
    }

    private static Table TabloOlustur(params (string baslik, double genislik, bool sag)[] kolonlar)
    {
        var t = new Table { CellSpacing = 0 };
        foreach (var k in kolonlar)
            t.Columns.Add(new TableColumn { Width = new GridLength(k.genislik, GridUnitType.Star) });
        var rg = new TableRowGroup();
        var hr = new TableRow();
        foreach (var k in kolonlar) hr.Cells.Add(Hucre(k.baslik, k.sag, baslik: true));
        rg.Rows.Add(hr);
        t.RowGroups.Add(rg);
        return t;
    }

    private static string Para(decimal d, string pb) => $"{d.ToString("N2", Tr)} {pb}";

    private static string BakiyeMetni(decimal b, string pb) =>
        b > 0.001m ? $"{Para(b, pb)} (Borçlu)" :
        b < -0.001m ? $"{Para(-b, pb)} (Alacaklı)" : $"{Para(0, pb)}";

    private static Brush BakiyeRenk(decimal b) =>
        b > 0.001m ? Red : b < -0.001m ? Green : TextDark;

    // ───────────────────────── Cari Listesi ─────────────────────────
    public static void CariListesi(IEnumerable<Cari> cariler, string firmaUnvan, string pb)
    {
        var liste = cariler.ToList();
        var doc = YeniDoc();
        doc.Blocks.Add(Baslik("CARİ LİSTESİ"));
        doc.Blocks.Add(Alt($"{firmaUnvan}  •  {DateTime.Now:dd.MM.yyyy HH:mm}  •  {liste.Count} kayıt"));

        var t = TabloOlustur(("Kod", 1, false), ("Cari Unvan", 4, false), ("Şehir", 1.4, false),
                             ("Telefon", 1.6, false), ("Bakiye", 2, true));
        var rg = t.RowGroups[0];
        foreach (var c in liste)
        {
            var r = new TableRow();
            r.Cells.Add(Hucre(c.CariKodu));
            r.Cells.Add(Hucre(c.Unvan));
            r.Cells.Add(Hucre(c.Il));
            r.Cells.Add(Hucre(string.IsNullOrEmpty(c.Gsm) ? c.Telefon : c.Gsm));
            r.Cells.Add(Hucre(BakiyeMetni(c.Bakiye, pb), true, true, BakiyeRenk(c.Bakiye)));
            rg.Rows.Add(r);
        }
        doc.Blocks.Add(t);

        decimal borc = liste.Where(c => c.Bakiye > 0.001m).Sum(c => c.Bakiye);
        decimal alacak = liste.Where(c => c.Bakiye < -0.001m).Sum(c => -c.Bakiye);
        doc.Blocks.Add(new Paragraph(new Run(
            $"Toplam Borç: {Para(borc, pb)}     Toplam Alacak: {Para(alacak, pb)}     Net: {Para(borc - alacak, pb)}"))
        { FontWeight = FontWeights.Bold, Foreground = TextDark, Margin = new Thickness(0, 14, 0, 0) });

        Onizle(doc, "Cari Listesi");
    }

    // ───────────────────────── Genel Hareketler Raporu ─────────────────────────
    public static void GenelHareketler(IEnumerable<Hareket> hareketler, string firmaUnvan, string pb)
    {
        var liste = hareketler.OrderBy(h => h.Tarih).ThenBy(h => h.Id).ToList();
        var doc = YeniDoc();
        doc.Blocks.Add(Baslik("GENEL HAREKETLER RAPORU"));
        doc.Blocks.Add(Alt($"{firmaUnvan}  •  {DateTime.Now:dd.MM.yyyy HH:mm}  •  {liste.Count} hareket kaydı"));

        var t = TabloOlustur(("Tarih", 1.4, false), ("Cari Unvan", 3.2, false), ("İşlem Türü", 1.5, false),
                             ("Açıklama", 2.6, false), ("Borç", 1.5, true), ("Alacak", 1.5, true));
        var rg = t.RowGroups[0];
        foreach (var h in liste)
        {
            var r = new TableRow();
            r.Cells.Add(Hucre(h.Tarih.ToString("dd.MM.yyyy HH:mm")));
            r.Cells.Add(Hucre(h.CariUnvan, kalin: true));
            r.Cells.Add(Hucre(h.IslemTuruText));
            r.Cells.Add(Hucre(h.Aciklama));
            r.Cells.Add(Hucre(h.Borc > 0 ? h.Borc.ToString("N2", Tr) : "", true, true, Red));
            r.Cells.Add(Hucre(h.Alacak > 0 ? h.Alacak.ToString("N2", Tr) : "", true, true, Green));
            rg.Rows.Add(r);
        }
        doc.Blocks.Add(t);

        decimal tb = liste.Sum(h => h.Borc);
        decimal ta = liste.Sum(h => h.Alacak);
        doc.Blocks.Add(new Paragraph(new Run(
            $"Toplam Borç: {Para(tb, pb)}     Toplam Alacak: {Para(ta, pb)}     Fark: {Para(tb - ta, pb)}"))
        { FontWeight = FontWeights.Bold, Foreground = TextDark, Margin = new Thickness(0, 14, 0, 0) });

        Onizle(doc, "Genel Hareketler Raporu");
    }

    // ───────────────────────── Cari Hesap Ekstresi ─────────────────────────
    public static FlowDocument BuildEkstreDoc(Cari cari, IEnumerable<Hareket> hareketler, string firmaUnvan, string pb,
                                              DateTime? bas = null, DateTime? bit = null)
    {
        var tum = hareketler.OrderBy(h => h.Tarih).ThenBy(h => h.Id).ToList();
        var secili = tum.Where(h => (bas == null || h.Tarih.Date >= bas.Value.Date) &&
                                    (bit == null || h.Tarih.Date <= bit.Value.Date)).ToList();

        decimal devir = bas == null ? 0 : tum.Where(h => h.Tarih.Date < bas.Value.Date).Sum(h => h.Borc - h.Alacak);

        var doc = YeniDoc();
        doc.Blocks.Add(Baslik("CARİ HESAP EKSTRESİ"));
        doc.Blocks.Add(Alt($"{firmaUnvan}  •  {DateTime.Now:dd.MM.yyyy HH:mm}"));

        doc.Blocks.Add(new Paragraph(new Run($"{cari.Unvan}  [{cari.CariKodu}]"))
        { FontSize = 15, FontWeight = FontWeights.Bold, Foreground = TextDark, Margin = new Thickness(0) });
        var detay = string.Join("  •  ", new[] { cari.Yetkili, cari.Gsm, cari.Telefon, $"{cari.Ilce} {cari.Il}".Trim() }
            .Where(s => !string.IsNullOrWhiteSpace(s)));
        if (detay.Length > 0) doc.Blocks.Add(Alt(detay));
        if (bas != null || bit != null)
            doc.Blocks.Add(Alt($"Dönem: {bas:dd.MM.yyyy} – {bit:dd.MM.yyyy}"));

        var t = TabloOlustur(("Tarih", 1.6, false), ("İşlem", 1.6, false), ("Açıklama", 3, false),
                             ("Borç", 1.5, true), ("Alacak", 1.5, true), ("Bakiye", 1.8, true));
        var rg = t.RowGroups[0];

        decimal yuruyen = devir;
        if (bas != null)
        {
            var r0 = new TableRow();
            r0.Cells.Add(Hucre("")); r0.Cells.Add(Hucre("Devir")); r0.Cells.Add(Hucre("Dönem başı devir bakiyesi"));
            r0.Cells.Add(Hucre("", true)); r0.Cells.Add(Hucre("", true));
            r0.Cells.Add(Hucre(yuruyen.ToString("N2", Tr), true, true, BakiyeRenk(yuruyen)));
            rg.Rows.Add(r0);
        }

        foreach (var h in secili)
        {
            yuruyen += h.Borc - h.Alacak;
            var r = new TableRow();
            r.Cells.Add(Hucre(h.Tarih.ToString("dd.MM.yyyy HH:mm")));
            r.Cells.Add(Hucre(h.IslemTuruText));
            r.Cells.Add(Hucre(h.Aciklama));
            r.Cells.Add(Hucre(h.Borc > 0 ? h.Borc.ToString("N2", Tr) : "", true, true, Red));
            r.Cells.Add(Hucre(h.Alacak > 0 ? h.Alacak.ToString("N2", Tr) : "", true, true, Green));
            r.Cells.Add(Hucre(yuruyen.ToString("N2", Tr), true, true, BakiyeRenk(yuruyen)));
            rg.Rows.Add(r);
        }
        doc.Blocks.Add(t);

        decimal tb = secili.Sum(h => h.Borc), ta = secili.Sum(h => h.Alacak);
        doc.Blocks.Add(new Paragraph(new Run(
            $"Toplam Borç: {Para(tb, pb)}     Toplam Alacak: {Para(ta, pb)}"))
        { FontWeight = FontWeights.SemiBold, Foreground = TextDark, Margin = new Thickness(0, 12, 0, 0) });
        doc.Blocks.Add(new Paragraph(new Run($"Son Bakiye: {BakiyeMetni(yuruyen, pb)}"))
        { FontSize = 15, FontWeight = FontWeights.Bold, Foreground = BakiyeRenk(yuruyen) });

        return doc;
    }

    public static void Ekstre(Cari cari, IEnumerable<Hareket> hareketler, string firmaUnvan, string pb,
                              DateTime? bas = null, DateTime? bit = null)
    {
        var doc = BuildEkstreDoc(cari, hareketler, firmaUnvan, pb, bas, bit);
        Onizle(doc, $"Ekstre — {cari.Unvan}");
    }

    // ───────────────────────── Makbuz / Dekont ─────────────────────────
    public static void Makbuz(Cari cari, Hareket h, decimal oncekiBakiye, decimal sonBakiye,
                              string firmaUnvan, string pb)
    {
        decimal tutar = h.Borc > 0 ? h.Borc : h.Alacak;
        var doc = YeniDoc();
        doc.PageWidth = 560;

        doc.Blocks.Add(Baslik(h.IslemTuruText.ToUpper(Tr) + " MAKBUZU", 18));
        doc.Blocks.Add(Alt($"{firmaUnvan}  •  {h.Tarih:dd.MM.yyyy HH:mm}  •  Fiş No: {h.Id}"));

        var t = new Table { CellSpacing = 0 };
        t.Columns.Add(new TableColumn { Width = new GridLength(1.2, GridUnitType.Star) });
        t.Columns.Add(new TableColumn { Width = new GridLength(2, GridUnitType.Star) });
        var rg = new TableRowGroup(); t.RowGroups.Add(rg);

        void Satir(string k, string v, bool vurgu = false, Brush? renk = null)
        {
            var r = new TableRow();
            r.Cells.Add(Hucre(k, kalin: true));
            r.Cells.Add(Hucre(v, true, vurgu, renk));
            rg.Rows.Add(r);
        }

        Satir("Cari Hesap", $"{cari.Unvan} [{cari.CariKodu}]");
        Satir("Açıklama", h.Aciklama);
        Satir("İşlem Öncesi Bakiye", BakiyeMetni(oncekiBakiye, pb), false, BakiyeRenk(oncekiBakiye));
        Satir("İşlem Tutarı", Para(tutar, pb), true, h.Borc > 0 ? Red : Green);
        Satir("İşlem Sonrası Bakiye", BakiyeMetni(sonBakiye, pb), true, BakiyeRenk(sonBakiye));
        doc.Blocks.Add(t);

        doc.Blocks.Add(new Paragraph(new Run("Yalnız: " + TurkceYazi.Cevir(tutar, pb)))
        { FontStyle = FontStyles.Italic, Foreground = Muted, Margin = new Thickness(0, 14, 0, 30) });

        var imza = new Table();
        imza.Columns.Add(new TableColumn()); imza.Columns.Add(new TableColumn());
        var irg = new TableRowGroup(); imza.RowGroups.Add(irg);
        var ir = new TableRow();
        ir.Cells.Add(new TableCell(new Paragraph(new Run("Teslim Eden")) { TextAlignment = TextAlignment.Center, Foreground = Muted }));
        ir.Cells.Add(new TableCell(new Paragraph(new Run("Teslim Alan")) { TextAlignment = TextAlignment.Center, Foreground = Muted }));
        irg.Rows.Add(ir);
        doc.Blocks.Add(imza);

        Onizle(doc, "Makbuz");
    }
}

/// <summary>Tutarı Türkçe yazıya çevirir (örn. 1.250,50 → "BinİkiyüzelliTL Elli Kuruş").</summary>
public static class TurkceYazi
{
    private static readonly string[] Birler = { "", "Bir", "İki", "Üç", "Dört", "Beş", "Altı", "Yedi", "Sekiz", "Dokuz" };
    private static readonly string[] Onlar  = { "", "On", "Yirmi", "Otuz", "Kırk", "Elli", "Altmış", "Yetmiş", "Seksen", "Doksan" };
    private static readonly string[] Binler = { "", "Bin", "Milyon", "Milyar", "Trilyon" };

    private static string UcBasamak(int n)
    {
        var s = "";
        int y = n / 100, o = n / 10 % 10, b = n % 10;
        if (y > 0) s += (y == 1 ? "" : Birler[y]) + "Yüz";
        s += Onlar[o] + Birler[b];
        return s;
    }

    public static string Sayi(long n)
    {
        if (n == 0) return "Sıfır";
        var parcalar = new List<string>();
        int grup = 0;
        while (n > 0)
        {
            int p = (int)(n % 1000);
            if (p > 0)
            {
                var yazi = UcBasamak(p);
                if (grup == 1 && p == 1) yazi = "";   // "Bir Bin" yerine "Bin"
                parcalar.Insert(0, yazi + Binler[grup]);
            }
            n /= 1000; grup++;
        }
        return string.Concat(parcalar);
    }

    public static string Cevir(decimal tutar, string paraBirimi = "TL")
    {
        tutar = Math.Round(Math.Abs(tutar), 2);
        long tam = (long)tutar;
        int kurus = (int)Math.Round((tutar - tam) * 100);
        var s = $"{Sayi(tam)} {paraBirimi}";
        if (kurus > 0) s += $" {Sayi(kurus)} Kuruş";
        return s;
    }
}
