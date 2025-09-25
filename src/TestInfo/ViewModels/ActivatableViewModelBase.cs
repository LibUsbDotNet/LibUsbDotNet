using ReactiveUI;

namespace TestInfo.ViewModels;

public class ActivatableViewModelBase : ReactiveObject, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new();
}
