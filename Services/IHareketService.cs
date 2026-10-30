using GelesiyeWPF.Common;
using GelesiyeWPF.Domain;

namespace GelesiyeWPF.Services;

public interface IHareketService
{
    ServiceResult<Hareket> Save(Hareket hareket, bool isNew);
    ServiceResult Delete(int hareketId);
    List<Hareket> GetByCariId(int cariId);
    List<Hareket> GetGenelHareketler(int firmaId, DateTime basTarih, DateTime bitTarih, int tur);
}
