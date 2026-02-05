using Avalonia.Controls;
using Avalonia.Input;
using System.Diagnostics;

namespace pacstallion.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Homepage_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not TextBlock tb)
            return;

        var url = tb.Text;

        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
}
