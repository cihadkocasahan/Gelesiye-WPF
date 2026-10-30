using FluentValidation;
using GelesiyeWPF.Common;
using GelesiyeWPF.Data;
using GelesiyeWPF.Domain;

namespace GelesiyeWPF.Services;

public class HareketService : IHareketService
{
    private readonly HareketRepository _repo;
    private readonly AyarlarRepository _ayarlarRepo;
    private readonly IValidator<Hareket> _validator;

    public HareketService(HareketRepository repo, AyarlarRepository ayarlarRepo, IValidator<Hareket> validator)
    {
        _repo = repo;
        _ayarlarRepo = ayarlarRepo;
        _validator = validator;
    }

    public ServiceResult<Hareket> Save(Hareket hareket, bool isNew)
    {
        var val = _validator.Validate(hareket);
        if (!val.IsValid)
            return ServiceResult<Hareket>.Fail(val.Errors.Select(e => e.ErrorMessage));

        try
        {
            if (isNew)
            {
                _repo.Insert(hareket);
                LogService.Info($"Fiş kaydedildi: ID={hareket.Id}, Tür={hareket.IslemTuru}, Borç={hareket.Borc:N2}, Alacak={hareket.Alacak:N2}, Cari={hareket.CariUnvan}");
            }
            else
            {
                _repo.Update(hareket);
                LogService.Info($"Fiş güncellendi: ID={hareket.Id}, Tür={hareket.IslemTuru}, Borç={hareket.Borc:N2}, Alacak={hareket.Alacak:N2}");
            }

            if (!string.IsNullOrWhiteSpace(hareket.Aciklama))
            {
                _ayarlarRepo.AddAciklama(hareket.IslemTuru, hareket.Aciklama);
            }

            return ServiceResult<Hareket>.Success(hareket);
        }
        catch (Exception ex)
        {
            LogService.Error($"Fiş kaydedilirken veritabanı hatası: {hareket.Aciklama}", ex);
            return ServiceResult<Hareket>.Fail(string.Format(LocalizationManager.T("Msg_FisKayitHata"), ex.Message));
        }
    }

    public ServiceResult Delete(int hareketId)
    {
        try
        {
            _repo.Delete(hareketId);
            LogService.Info($"Fiş silindi: ID={hareketId}");
            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            LogService.Error($"Fiş silinirken hata oluştu: ID={hareketId}", ex);
            return ServiceResult.Fail(string.Format(LocalizationManager.T("Msg_FisSilHata"), ex.Message));
        }
    }

    public List<Hareket> GetByCariId(int cariId) => _repo.GetByCariId(cariId);
    public List<Hareket> GetGenelHareketler(int firmaId, DateTime basTarih, DateTime bitTarih, int tur) =>
        _repo.GetGenelHareketler(firmaId, basTarih, bitTarih, tur);
}
