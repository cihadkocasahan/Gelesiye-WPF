using FluentValidation;
using GelesiyeWPF.Common;
using GelesiyeWPF.Data;
using GelesiyeWPF.Domain;

namespace GelesiyeWPF.Services;

public class CariService : ICariService
{
    private readonly CariRepository _repo;
    private readonly IValidator<Cari> _validator;

    public CariService(CariRepository repo, IValidator<Cari> validator)
    {
        _repo = repo;
        _validator = validator;
    }

    public ServiceResult<Cari> Save(Cari cari, bool isNew)
    {
        var val = _validator.Validate(cari);
        if (!val.IsValid)
            return ServiceResult<Cari>.Fail(val.Errors.Select(e => e.ErrorMessage));

        if (isNew && _repo.ExistsKodu(cari.CariKodu, cari.FirmaId))
            return ServiceResult<Cari>.Fail(string.Format(LocalizationManager.T("Msg_CariKoduZatenMevcut"), cari.CariKodu));

        if (!isNew && _repo.ExistsKodu(cari.CariKodu, cari.FirmaId, cari.Id))
            return ServiceResult<Cari>.Fail(string.Format(LocalizationManager.T("Msg_CariKoduBaskaMevcut"), cari.CariKodu));

        try
        {
            if (isNew)
            {
                _repo.Insert(cari);
                LogService.Info($"Yeni cari kartı açıldı: {cari.CariKodu} — {cari.Unvan}");
            }
            else
            {
                _repo.Update(cari);
                LogService.Info($"Cari kartı güncellendi: {cari.CariKodu} — {cari.Unvan}");
            }
            return ServiceResult<Cari>.Success(cari);
        }
        catch (Exception ex)
        {
            LogService.Error($"Cari kaydedilirken veritabanı hatası: {cari.Unvan}", ex);
            return ServiceResult<Cari>.Fail(string.Format(LocalizationManager.T("Msg_CariKayitHata"), ex.Message));
        }
    }

    public ServiceResult Delete(int cariId)
    {
        try
        {
            var cari = _repo.GetById(cariId);
            _repo.Delete(cariId);
            LogService.Info($"Cari kartı silindi: ID={cariId}, Unvan={cari?.Unvan ?? "Bilinmiyor"}");
            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            LogService.Error($"Cari silinirken hata oluştu: ID={cariId}", ex);
            return ServiceResult.Fail(string.Format(LocalizationManager.T("Msg_CariSilHata"), ex.Message));
        }
    }

    public string GetNextCariKodu(int firmaId) => _repo.GetNextCariKodu(firmaId);
    public Cari? GetById(int id) => _repo.GetById(id);
    public List<Cari> GetAll(CariFiltre filtre) => _repo.GetAll(filtre);
    public BakiyeOzet GetBakiyeOzet(int firmaId) => _repo.GetBakiyeOzet(firmaId);
}
