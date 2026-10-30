using System.Windows.Data;
using System.Windows.Markup;
using GelesiyeWPF.Services;

namespace GelesiyeWPF.Converters;

/// <summary>
/// XAML içerisinde {conv:Loc Key} sözdizimiyle çalışan dinamik çoklu dil MarkupExtension'ı.
/// Dil değiştiğinde açık olan tüm pencerelerde metinleri restart gerektirmeden anında günceller.
/// </summary>
[MarkupExtensionReturnType(typeof(string))]
public class LocExtension : MarkupExtension
{
    public string Key { get; set; } = "";

    public LocExtension() { }
    public LocExtension(string key) => Key = key;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Key)) return "";

        var binding = new Binding($"[{Key}]")
        {
            Source = LocalizationManager.Instance,
            Mode = BindingMode.OneWay
        };

        return binding.ProvideValue(serviceProvider);
    }
}
