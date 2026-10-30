using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using GelesiyeWPF.Common;
using GelesiyeWPF.Domain;

namespace GelesiyeWPF.Converters;

[ValueConversion(typeof(decimal), typeof(string))]
public class ParaFormatConverter : IValueConverter
{
    public string? ParaBirimi { get; set; } = "TL";

    public object Convert(object value, Type _, object __, CultureInfo ___) =>
        value is decimal d ? $"{d:N2} {ParaBirimi}" : "0,00 TL";

    public object ConvertBack(object value, Type _, object __, CultureInfo ___) =>
        decimal.TryParse(value?.ToString()?.Replace(" TL", "").Replace(" ", ""),
            NumberStyles.Any, new CultureInfo("tr-TR"), out var d) ? d : 0m;
}

[ValueConversion(typeof(decimal), typeof(Brush))]
public class BakiyeBrushConverter : IValueConverter
{
    public object Convert(object value, Type _, object __, CultureInfo ___)
    {
        if (value is decimal d)
        {
            if (d > 0.001m) return new SolidColorBrush(AppTheme.BorcluRed);
            if (d < -0.001m) return new SolidColorBrush(AppTheme.AlacakliGreen);
        }
        return new SolidColorBrush(AppTheme.SifirBlack);
    }
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => DependencyProperty.UnsetValue;
}

[ValueConversion(typeof(IslemTuru), typeof(Brush))]
public class IslemTurBrushConverter : IValueConverter
{
    public object Convert(object value, Type _, object __, CultureInfo ___)
    {
        if (value is IslemTuru t)
            return new SolidColorBrush(AppTheme.GetIslemTextColor(t));
        return new SolidColorBrush(AppTheme.SifirBlack);
    }
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => DependencyProperty.UnsetValue;
}

[ValueConversion(typeof(bool), typeof(Visibility))]
public class BoolToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }
    public object Convert(object value, Type _, object __, CultureInfo ___)
    {
        bool b = value is bool bv && bv;
        if (Invert) b = !b;
        return b ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => DependencyProperty.UnsetValue;
}

[ValueConversion(typeof(decimal), typeof(string))]
public class BakiyeTextConverter : IValueConverter
{
    public object Convert(object value, Type _, object __, CultureInfo ___)
    {
        if (value is decimal d)
        {
            if (d > 0.001m)  return $"{d:N2} TL  ▲";
            if (d < -0.001m) return $"{d:N2} TL  ▼";
            return "0,00 TL";
        }
        return "0,00 TL";
    }
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => DependencyProperty.UnsetValue;
}
