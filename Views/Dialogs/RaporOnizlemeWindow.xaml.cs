using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using GelesiyeWPF.Services;

namespace GelesiyeWPF.Views.Dialogs;

public partial class RaporOnizlemeWindow : Window
{
    private readonly FlowDocument _doc;
    private readonly string _baslik;

    public RaporOnizlemeWindow(FlowDocument doc, string baslik)
    {
        InitializeComponent();
        _doc = doc;
        _baslik = baslik;

        txtTitle.Text = baslik;
        Title = baslik;

        // Sayfa zeminini beyaz ve metinleri net yap
        _doc.Background = Brushes.White;
        if (double.IsNaN(_doc.PageWidth) || _doc.PageWidth <= 0)
        {
            _doc.PageWidth = 794;
        }
        if (double.IsNaN(_doc.PageHeight) || _doc.PageHeight <= 0)
        {
            _doc.PageHeight = 1123;
        }
        if (_doc.Foreground == null)
        {
            _doc.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(15, 23, 42));
        }

        docReader.Document = doc;
        docReader.ViewingMode = FlowDocumentReaderViewingMode.Page;
        docReader.Zoom = 100;
        UpdateZoomText();
    }

    private void UpdateZoomText()
    {
        if (txtZoom != null && docReader != null)
        {
            txtZoom.Text = $"%{(int)docReader.Zoom}";
        }
    }

    private void TitleBar_LeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            BtnMaximize_Click(sender, e);
            return;
        }
        if (e.ButtonState == MouseButtonState.Pressed && WindowState != WindowState.Maximized)
        {
            try { DragMove(); } catch (InvalidOperationException) { }
        }
    }

    private void BtnMaximize_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void Window_StateChanged(object? sender, EventArgs e) =>
        btnMaximize.Content = WindowState == WindowState.Maximized ? "❐" : "□";

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

    private void BtnYazdir_Click(object sender, RoutedEventArgs e)
    {
        if (_doc == null) return;
        try
        {
            var pd = new PrintDialog();
            if (pd.ShowDialog() != true) return;

            double origPageWidth = _doc.PageWidth;
            double origPageHeight = _doc.PageHeight;
            Thickness origPadding = _doc.PagePadding;
            double origColWidth = _doc.ColumnWidth;
            var origBg = _doc.Background;
            var origFg = _doc.Foreground;

            try
            {
                _doc.PageWidth = pd.PrintableAreaWidth;
                _doc.PageHeight = pd.PrintableAreaHeight;
                _doc.PagePadding = new Thickness(40);
                _doc.ColumnWidth = double.PositiveInfinity;
                _doc.Background = Brushes.White;
                _doc.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(15, 23, 42));

                var paginator = ((IDocumentPaginatorSource)_doc).DocumentPaginator;
                pd.PrintDocument(paginator, _baslik);
            }
            finally
            {
                _doc.PageWidth = origPageWidth;
                _doc.PageHeight = origPageHeight;
                _doc.PagePadding = origPadding;
                _doc.ColumnWidth = origColWidth;
                _doc.Background = origBg;
                _doc.Foreground = origFg;
            }
        }
        catch (Exception ex)
        {
            var fmt = LocalizationManager.T("Msg_YazdirmaHatasi");
            DialogService.Hata(string.Format(fmt, ex.Message));
        }
    }

    private void BtnTekSayfa_Click(object sender, RoutedEventArgs e) =>
        docReader.ViewingMode = FlowDocumentReaderViewingMode.Page;

    private void BtnCiftSayfa_Click(object sender, RoutedEventArgs e) =>
        docReader.ViewingMode = FlowDocumentReaderViewingMode.TwoPage;

    private void BtnKaydirma_Click(object sender, RoutedEventArgs e) =>
        docReader.ViewingMode = FlowDocumentReaderViewingMode.Scroll;

    private void BtnYakinlas_Click(object sender, RoutedEventArgs e)
    {
        docReader.Zoom = Math.Min(300, docReader.Zoom + 15);
        UpdateZoomText();
    }

    private void BtnUzaklas_Click(object sender, RoutedEventArgs e)
    {
        docReader.Zoom = Math.Max(50, docReader.Zoom - 15);
        UpdateZoomText();
    }

    private void BtnResetZoom_Click(object sender, RoutedEventArgs e)
    {
        docReader.Zoom = 100;
        UpdateZoomText();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
        else if (e.Key == Key.P && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            e.Handled = true;
            BtnYazdir_Click(sender, e);
        }
        else if ((e.Key == Key.OemPlus || e.Key == Key.Add) && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            e.Handled = true;
            BtnYakinlas_Click(sender, e);
        }
        else if ((e.Key == Key.OemMinus || e.Key == Key.Subtract) && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            e.Handled = true;
            BtnUzaklas_Click(sender, e);
        }
        else if ((e.Key == Key.D0 || e.Key == Key.NumPad0) && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            e.Handled = true;
            BtnResetZoom_Click(sender, e);
        }
    }
}
