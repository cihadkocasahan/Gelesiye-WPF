using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using GelesiyeWPF.Common;
using GelesiyeWPF.Data;
using GelesiyeWPF.Domain;

namespace GelesiyeWPF.ViewModels;

public partial class HareketViewModel : ObservableObject
{
    private readonly HareketRepository _repo;
    private readonly CariRepository    _cariRepo;

    [ObservableProperty] private ObservableCollection<Hareket> hareketler = [];
    [ObservableProperty] private Hareket? seciliHareket;
    [ObservableProperty] private Cari? cari;

    public int  SortColumn { get; set; } = -1;
    public bool SortAsc    { get; set; } = true;

    // Toplamlar
    public decimal ToplamBorc   => Hareketler.Sum(h => h.Borc);
    public decimal ToplamAlacak => Hareketler.Sum(h => h.Alacak);
    public decimal Bakiye       => ToplamBorc - ToplamAlacak;
    public int     IslemSayisi  => Hareketler.Count;

    public HareketViewModel(HareketRepository repo, CariRepository cariRepo)
    {
        _repo     = repo;
        _cariRepo = cariRepo;
    }

    public void CariYukle(int cariId)
    {
        Cari = _cariRepo.GetById(cariId);
        YukleHareketler();
    }

    public void YukleHareketler()
    {
        if (Cari == null) return;
        var liste = _repo.GetByCariId(Cari.Id);

        if (SortColumn >= 0)
        {
            liste = (SortColumn switch
            {
                0 => SortAsc ? liste.OrderBy(h => h.Tarih)      : liste.OrderByDescending(h => h.Tarih),
                1 => SortAsc ? liste.OrderBy(h => h.IslemTuruText): liste.OrderByDescending(h => h.IslemTuruText),
                2 => SortAsc ? liste.OrderBy(h => h.Aciklama)   : liste.OrderByDescending(h => h.Aciklama),
                3 => SortAsc ? liste.OrderBy(h => h.Borc)       : liste.OrderByDescending(h => h.Borc),
                4 => SortAsc ? liste.OrderBy(h => h.Alacak)     : liste.OrderByDescending(h => h.Alacak),
                5 => SortAsc ? liste.OrderBy(h => h.YuruyenBakiye): liste.OrderByDescending(h => h.YuruyenBakiye),
                _ => liste.AsEnumerable()
            }).ToList();
        }

        Hareketler = new ObservableCollection<Hareket>(liste);
        OnPropertyChanged(nameof(ToplamBorc));
        OnPropertyChanged(nameof(ToplamAlacak));
        OnPropertyChanged(nameof(Bakiye));
        OnPropertyChanged(nameof(IslemSayisi));
    }

    public void RefreshCari()
    {
        if (Cari != null)
            Cari = _cariRepo.GetById(Cari.Id);
    }
}
