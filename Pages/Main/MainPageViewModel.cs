using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Material.Icons;

namespace swpumc.Pages.Main;

internal class MainPageViewModel : BaseViewModel
{
    public static string Title { get; set; } = "游戏启动";
    public static string Description { get; set; } = "游戏版本选择和启动";
    public static MaterialIconKind Icon { get; set; } = MaterialIconKind.Login;
    public static int Index { get; set; } = 1;
}
