namespace GelesiyeWPF.Services;

public static class DovizKuruService
{
    private static decimal _usd;
    private static decimal _eur;
    private static DateTime _lastFetch = DateTime.MinValue;

    public static event Action? KurGuncellendi;

    public static decimal Usd => _usd;
    public static decimal Eur => _eur;

    public static async Task GuncelleAsync()
    {
        try
        {
            // TCMB XML endpoint
            using var http   = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var xml          = await http.GetStringAsync("https://www.tcmb.gov.tr/kurlar/today.xml");
            _usd = ParseKur(xml, "USD");
            _eur = ParseKur(xml, "EUR");
            _lastFetch = DateTime.Now;
            KurGuncellendi?.Invoke();
        }
        catch { /* ağ yoksa eski değerler kalır */ }
    }

    private static decimal ParseKur(string xml, string kod)
    {
        var tag   = $"CurrencyCode=\"{kod}\"";
        var idx   = xml.IndexOf(tag, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return 0;

        var forexBuying = Extract(xml, idx, "<ForexBuying>");
        return forexBuying;
    }

    private static decimal Extract(string xml, int start, string tag)
    {
        var idx = xml.IndexOf(tag, start, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return 0;
        var vs  = idx + tag.Length;
        var ve  = xml.IndexOf('<', vs);
        if (ve < 0) return 0;
        var val = xml[vs..ve].Trim().Replace(',', '.');
        return decimal.TryParse(val, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 0;
    }

    public static string GetKurMetni()
    {
        if (_usd == 0 && _eur == 0) return "";
        return $"💱  USD {_usd:N2}  |  EUR {_eur:N2}  TL";
    }
}
