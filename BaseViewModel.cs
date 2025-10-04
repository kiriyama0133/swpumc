using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace swpumc;

internal interface  BaseViewModel
{
    // 注意：导航属性现在是静态的，在具体的 ViewModel 类中定义
    // 这些属性用于 NavigationService 自动发现页面信息
}
