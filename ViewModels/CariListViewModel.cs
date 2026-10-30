using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using GelesiyeWPF.Common;
using GelesiyeWPF.Data;
using GelesiyeWPF.Domain;

namespace GelesiyeWPF.ViewModels;

public partial class CariListViewModel : ObservableObject
{
    private readonly CariRepository _repo;

    [ObservableProperty] private ObservableCollection<Cari> cariler = [];
    [ObservableProperty] private Cari? seciliCari;
    [ObservableProperty] private string aramaMetni = "";
    [ObservableProperty] private AramaModu aramaModu = AramaModu.YazildigiGibi;
    [ObservableProperty] private HesapDurumu hesapDurumuFiltre = HesapDurumu.Tumu;
    [ObservableProperty] private BakiyeFiltresi bakiyeFiltresi = BakiyeFiltresi.Tumu;
    [ObservableProperty] private SorguTuru sorguTuru = SorguTuru.Unvan;
    [ObservableProperty] private BakiyeOzet ozet = new();
    [ObservableProperty] private int firmaId;

    // Sıralama durumu
    public int  SortColumn { get; set; } = -1;
    public bool SortAsc    { get; set; } = true;

    public CariListViewModel(CariRepository repo) => _repo = repo;

    public void YukleCariler()
    {
        if (FirmaId == 0) return;

        var filtre = new CariFiltre
        {
            FirmaId       = FirmaId,
            AramaMetni    = AramaMetni,
            AramaModu     = AramaModu,
            SorguTuru     = SorguTuru,
            HesapDurumu   = HesapDurumuFiltre,
            BakiyeFiltresi= BakiyeFiltresi
        };

        var liste = _repo.GetAll(filtre);

        // Kolon sıralaması (DB'den gelen doğal sıranın üstüne)
        if (SortColumn >= 0)
        {
            liste = (SortColumn switch
            {
                0 => SortAsc ? liste.OrderBy(c => c.CariKodu)       : liste.OrderByDescending(c => c.CariKodu),
                1 => SortAsc ? liste.OrderBy(c => c.Unvan)          : liste.OrderByDescending(c => c.Unvan),
                2 => SortAsc ? liste.OrderBy(c => c.Notlar)         : liste.OrderByDescending(c => c.Notlar),
                3 => SortAsc ? liste.OrderBy(c => c.Il)             : liste.OrderByDescending(c => c.Il),
                4 => SortAsc ? liste.OrderBy(c => c.SonIslemTarihi) : liste.OrderByDescending(c => c.SonIslemTarihi),
                5 => SortAsc ? liste.OrderBy(c => c.Bakiye)         : liste.OrderByDescending(c => c.Bakiye),
                _ => liste.AsEnumerable()
            }).ToList();
        }

        Cariler = new ObservableCollection<Cari>(liste);
        OzetGuncelle();
    }

    private void OzetGuncelle()
    {
        if (FirmaId == 0) return;
        Ozet = _repo.GetBakiyeOzet(FirmaId);
    }

    public string GetNextCariKodu() => _repo.GetNextCariKodu(FirmaId);

    public int IndexOfCari(int cariId)
    {
        for (int i = 0; i < Cariler.Count; i++)
            if (Cariler[i].Id == cariId) return i;
        return -1;
    }
}
