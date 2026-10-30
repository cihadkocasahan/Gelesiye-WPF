using FluentValidation;
using GelesiyeWPF.Domain;
using GelesiyeWPF.Services;

namespace GelesiyeWPF.Validators;

public class FirmaValidator : AbstractValidator<Firma>
{
    public FirmaValidator()
    {
        RuleFor(x => x.Kod)
            .NotEmpty().WithMessage(_ => LocalizationManager.T("Val_FirmaKoduBosOlamaz"))
            .MaximumLength(30).WithMessage(_ => LocalizationManager.T("Val_FirmaKoduMaks30"));

        RuleFor(x => x.Unvan)
            .NotEmpty().WithMessage(_ => LocalizationManager.T("Val_FirmaUnvanBosOlamaz"))
            .MaximumLength(150).WithMessage(_ => LocalizationManager.T("Val_FirmaUnvanMaks150"));

        RuleFor(x => x.BorcLimiti)
            .GreaterThanOrEqualTo(0m).WithMessage(_ => LocalizationManager.T("Val_FirmaBorcLimitiNegatifOlamaz"));
    }
}
