using Avalonia.Controls;
using Avalonia.ReactiveUI;
using TestInfo.ViewModels;

namespace TestInfo.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
    }
}