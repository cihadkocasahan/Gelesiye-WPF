using GelesiyeWPF.Common;
using GelesiyeWPF.Domain;

namespace GelesiyeWPF.Services;

public interface ICariService
{
    ServiceResult<Cari> Save(Cari cari, bool isNew);
    ServiceResult Delete(int cariId);
    string GetNextCariKodu(int firmaId);
    Cari? GetById(int id);
    List<Cari> GetAll(CariFiltre filtre);
    BakiyeOzet GetBakiyeOzet(int firmaId);
}
