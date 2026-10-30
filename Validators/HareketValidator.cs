using FluentValidation;
using GelesiyeWPF.Domain;
using GelesiyeWPF.Services;

namespace GelesiyeWPF.Validators;

public class HareketValidator : AbstractValidator<Hareket>
{
    public HareketValidator()
    {
        RuleFor(x => x.CariId)
            .GreaterThan(0).WithMessage(_ => LocalizationManager.T("Val_GecerliCariSecilmeli"));

        RuleFor(x => x.FirmaId)
            .GreaterThan(0).WithMessage(_ => LocalizationManager.T("Val_GecerliFirmaSecilmeli"));

        RuleFor(x => x.Borc)
            .GreaterThanOrEqualTo(0m).WithMessage(_ => LocalizationManager.T("Val_BorcNegatifOlamaz"));

        RuleFor(x => x.Alacak)
            .GreaterThanOrEqualTo(0m).WithMessage(_ => LocalizationManager.T("Val_AlacakNegatifOlamaz"));

        RuleFor(x => x)
            .Must(x => x.Borc > 0 || x.Alacak > 0)
            .WithMessage(_ => LocalizationManager.T("Val_TutarSifirdanBuyukOlmali"));

        RuleFor(x => x.Aciklama)
            .MaximumLength(250).WithMessage(_ => LocalizationManager.T("Val_AciklamaMaks250"));
    }
}
