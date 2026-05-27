using LibUsbDotNet.Info;
using LibUsbDotNet.LibUsb;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;

namespace TestInfo.ViewModels;

public partial class MainWindowViewModel : ActivatableViewModelBase
{
    private readonly UsbContext _usbContext = new();
    private readonly bool _isHotPlugSupported;

    public MainWindowViewModel()
    {
        _isHotPlugSupported = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        this.WhenActivated((CompositeDisposable disposables) =>
        {
            RefreshDeviceListCommand.Execute()
                .Subscribe()
                .DisposeWith(disposables);
        });
    }

    public ObservableCollection<UsbDeviceInfo> DeviceInfos { get; } = new();



    [ReactiveCommand]
    private void RefreshDeviceList()
    {
        DeviceInfos.Clear();
        using var deviceList = _usbContext.List();

        foreach (var device in deviceList)
        {
            device.TryOpen();
            DeviceInfos.Add(device.Info);
        }
    }
}
