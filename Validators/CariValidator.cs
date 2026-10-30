using FluentValidation;
using GelesiyeWPF.Domain;
using GelesiyeWPF.Services;

namespace GelesiyeWPF.Validators;

public class CariValidator : AbstractValidator<Cari>
{
    public CariValidator()
    {
        RuleFor(x => x.CariKodu)
            .NotEmpty().WithMessage(_ => LocalizationManager.T("Val_CariKoduBosOlamaz"))
            .MaximumLength(50).WithMessage(_ => LocalizationManager.T("Val_CariKoduMaks50"));

        RuleFor(x => x.Unvan)
            .NotEmpty().WithMessage(_ => LocalizationManager.T("Val_UnvanBosOlamaz"))
            .MaximumLength(150).WithMessage(_ => LocalizationManager.T("Val_UnvanMaks150"));

        RuleFor(x => x.CariLimit)
            .GreaterThanOrEqualTo(0m).WithMessage(_ => LocalizationManager.T("Val_LimitNegatifOlamaz"));

        RuleFor(x => x.Eposta)
            .EmailAddress().WithMessage(_ => LocalizationManager.T("Val_GecersizEposta"))
            .When(x => !string.IsNullOrWhiteSpace(x.Eposta));

        RuleFor(x => x.Telefon)
            .MaximumLength(30).WithMessage(_ => LocalizationManager.T("Val_TelefonMaks30"));

        RuleFor(x => x.Gsm)
            .MaximumLength(30).WithMessage(_ => LocalizationManager.T("Val_GsmMaks30"));

        RuleFor(x => x.VergiNo)
            .MaximumLength(20).WithMessage(_ => LocalizationManager.T("Val_VergiNoMaks20"));
    }
}
