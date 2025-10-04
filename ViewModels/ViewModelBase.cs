using CommunityToolkit.Mvvm.ComponentModel;

namespace swpumc.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private int _index = 0;
}
