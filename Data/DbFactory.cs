using GelesiyeWPF.Services;
using GelesiyeWPF.Validators;

namespace GelesiyeWPF.Data;

/// <summary>
/// Merkezi repository ve servis fabrikası.
/// </summary>
public static class DbFactory
{
    private static Database? _db;

    public static void Initialize(string? dbPath = null)
    {
        Database.Initialize(dbPath);
        _db = Database.Instance;
    }

    public static FirmaRepository   GetFirmaRepo()   => new(_db!);
    public static CariRepository    GetCariRepo()    => new(_db!);
    public static HareketRepository GetHareketRepo() => new(_db!);
    public static AyarlarRepository GetAyarlarRepo() => new(_db!);
    public static IlIlceRepository  GetIlIlceRepo()  => new(_db!);
    public static Database          GetDb()          => _db!;

    public static ICariService GetCariService() =>
        new CariService(GetCariRepo(), new CariValidator());

    public static IHareketService GetHareketService() =>
        new HareketService(GetHareketRepo(), GetAyarlarRepo(), new HareketValidator());
}
