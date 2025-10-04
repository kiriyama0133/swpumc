using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using Avalonia.Controls.Notifications;
using swpumc.Services;
using swpumc.Services.API;
using System.Threading;

namespace swpumc.Controls.Loginform;

public partial class SkinLoginFormViewModel : ObservableObject
{
    // 验证方式选择
    [ObservableProperty]
    private bool _isOfflineSelected = true;

    [ObservableProperty]
    private bool _isMicrosoftSelected = false;

    [ObservableProperty]
    private bool _isThirdPartySelected = false;

    // 离线游玩
    [ObservableProperty]
    private string _offlineName = string.Empty;

    // 微软验证
    [ObservableProperty]
    private string _microsoftEmail = string.Empty;
    
    [ObservableProperty]
    private string _deviceCode = string.Empty;
    
    [ObservableProperty]
    private string _verificationUrl = string.Empty;
    
    [ObservableProperty]
    private bool _isMicrosoftAuthInProgress = false;

    // 第三方验证
    [ObservableProperty]
    private string _thirdPartyEmail = string.Empty;

    [ObservableProperty]
    private string _thirdPartyPassword = string.Empty;

    // 通用属性

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError = false;

    // 登录成功事件
    public event Action? OnLoginSuccess;

        // Dialog管理器
        private readonly ISukiDialogManager _dialogManager;
        
        // Toast管理器
        private readonly ISukiToastManager _toastManager;
        
        // 服务依赖
        private readonly IYggdrasilService _yggdrasilService;
        private readonly IPlayerManagementService _playerManagementService;
        private readonly IMicrosoftAuthService _microsoftAuthService;

    public SkinLoginFormViewModel(ISukiDialogManager dialogManager, ISukiToastManager toastManager, IYggdrasilService yggdrasilService, IPlayerManagementService playerManagementService, IMicrosoftAuthService microsoftAuthService)
    {
        _dialogManager = dialogManager;
        _toastManager = toastManager;
        _yggdrasilService = yggdrasilService;
        _playerManagementService = playerManagementService;
        _microsoftAuthService = microsoftAuthService;
    }

    // 验证方式选择命令
    [RelayCommand]
    private void SelectOffline()
    {
        IsOfflineSelected = true;
        IsMicrosoftSelected = false;
        IsThirdPartySelected = false;
        Console.WriteLine("[SkinLoginForm] 选择离线游玩");
    }

    [RelayCommand]
    private void SelectMicrosoft()
    {
        IsOfflineSelected = false;
        IsMicrosoftSelected = true;
        IsThirdPartySelected = false;
        Console.WriteLine("[SkinLoginForm] 选择微软验证");
    }

    [RelayCommand]
    private void SelectThirdParty()
    {
        IsOfflineSelected = false;
        IsMicrosoftSelected = false;
        IsThirdPartySelected = true;
        Console.WriteLine("[SkinLoginForm] 选择第三方验证");
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsLoading)
            return;

        // 根据选择的验证方式执行不同的登录逻辑
        if (IsOfflineSelected)
        {
            await HandleOfflineLoginAsync();
        }
        else if (IsMicrosoftSelected)
        {
            await HandleMicrosoftLoginAsync();
        }
        else if (IsThirdPartySelected)
        {
            await HandleThirdPartyLoginAsync();
        }
    }

    private async Task HandleOfflineLoginAsync()
    {
        if (string.IsNullOrWhiteSpace(OfflineName))
        {
            ShowErrorToast("输入错误", "请输入玩家名称");
            return;
        }

        IsLoading = true;
        HasError = false;
        StatusMessage = "正在创建离线账户...";
        Console.WriteLine($"[SkinLoginForm] 开始离线登录: {OfflineName}");

        try
        {
            // 创建离线用户账户
            var saveSuccess = await _playerManagementService.SaveOfflineUserAsync(OfflineName);
            
            if (saveSuccess)
            {
                Console.WriteLine("[SkinLoginForm] 离线账户创建成功，刷新玩家数据...");
                await _playerManagementService.LoadPlayersAsync();
                Console.WriteLine($"[SkinLoginForm] 玩家数据刷新完成，当前有 {_playerManagementService.Players.Count} 个玩家");
                
                ShowSuccessToast("登录成功", $"欢迎，{OfflineName}！");
                OnLoginSuccess?.Invoke();
            }
            else
            {
                ShowErrorToast("登录失败", "创建离线账户失败，请重试");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SkinLoginForm] 离线登录异常: {ex.Message}");
            ShowErrorToast("登录异常", $"登录失败: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            Console.WriteLine($"[SkinLoginForm] 离线登录流程结束");
        }
    }

    private async Task HandleMicrosoftLoginAsync()
    {
        if (string.IsNullOrWhiteSpace(MicrosoftEmail))
        {
            ShowErrorToast("输入错误", "请输入微软账户邮箱");
            return;
        }

        if (!IsValidEmail(MicrosoftEmail))
        {
            ShowErrorToast("输入错误", "请输入有效的邮箱地址");
            return;
        }

        IsLoading = true;
        HasError = false;
        StatusMessage = "正在连接微软账户...";
        Console.WriteLine($"[SkinLoginForm] 开始微软验证: {MicrosoftEmail}");

        try
        {
            // 启动设备流认证
            var result = await _microsoftAuthService.StartDeviceFlowAsync(OnDeviceCodeReceived, CancellationToken.None);
            
            if (!result.Success)
            {
                ShowErrorToast("认证失败", result.ErrorMessage ?? "未知错误");
                return;
            }

            // 保存微软用户信息
            var saveSuccess = await _playerManagementService.SaveMicrosoftUserAsync(
                result.PlayerName ?? "Microsoft User",
                result.PlayerUuid ?? Guid.NewGuid().ToString(),
                result.AccessToken ?? "",
                result.RefreshToken ?? "",
                result.ExpiresAt != default ? result.ExpiresAt : DateTime.Now.AddHours(1)
            );

            if (saveSuccess)
            {
                Console.WriteLine("[SkinLoginForm] 微软认证成功，保存用户信息");
                ShowSuccessToast("登录成功", $"欢迎回来，{result.PlayerName}！");
                
                // 刷新玩家数据
                await _playerManagementService.LoadPlayersAsync();
                Console.WriteLine($"[SkinLoginForm] 玩家数据刷新完成，当前有 {_playerManagementService.Players.Count} 个玩家");
                
                // 触发登录成功事件
                OnLoginSuccess?.Invoke();
            }
            else
            {
                ShowErrorToast("保存失败", "无法保存用户信息");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SkinLoginForm] 微软验证异常: {ex.Message}");
            ShowErrorToast("登录异常", $"登录失败: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            IsMicrosoftAuthInProgress = false;
            Console.WriteLine($"[SkinLoginForm] 微软验证流程结束");
        }
    }

    private void OnDeviceCodeReceived(Models.DeviceCodeResponse deviceCode)
    {
        DeviceCode = deviceCode.UserCode ?? "";
        VerificationUrl = deviceCode.VerificationUrl ?? "";
        
        Console.WriteLine($"[SkinLoginForm] 设备代码: {DeviceCode}");
        Console.WriteLine($"[SkinLoginForm] 验证URL: {VerificationUrl}");
        
        // 显示设备代码信息
        ShowInformationToast("设备认证", $"请在浏览器中访问 {VerificationUrl} 并输入代码: {DeviceCode}");
    }

    private async Task HandleThirdPartyLoginAsync()
    {
        if (string.IsNullOrWhiteSpace(ThirdPartyEmail) || string.IsNullOrWhiteSpace(ThirdPartyPassword))
        {
            ShowErrorToast("输入错误", "请输入邮箱和密码");
            return;
        }

        if (!IsValidEmail(ThirdPartyEmail))
        {
            ShowErrorToast("输入错误", "请输入有效的邮箱地址");
            return;
        }

        IsLoading = true;
        HasError = false;
        StatusMessage = "正在连接第三方服务器...";
        Console.WriteLine($"[SkinLoginForm] 开始第三方验证: {ThirdPartyEmail}");

        try
        {
            // 执行第三方 Yggdrasil认证
            var authResult = await _yggdrasilService.AuthenticateAsync(ThirdPartyEmail, ThirdPartyPassword);
            
            if (authResult != null)
            {
                Console.WriteLine($"[SkinLoginForm] 第三方认证成功，保存用户信息");
                
                // 保存用户信息到配置
                var saveSuccess = await _playerManagementService.SaveUserInfoAsync(authResult, ThirdPartyEmail);
                
                if (saveSuccess)
                {
                    // 刷新玩家数据
                    Console.WriteLine("[SkinLoginForm] 登录成功，刷新玩家数据...");
                    await _playerManagementService.LoadPlayersAsync();
                    Console.WriteLine($"[SkinLoginForm] 玩家数据刷新完成，当前有 {_playerManagementService.Players.Count} 个玩家");
                    
                    ShowSuccessToast("登录成功", $"欢迎回来，{authResult.SelectedProfile?.Name ?? "用户"}！");
                    
                    // 触发登录成功事件，让父组件销毁登录表单
                    Console.WriteLine("[SkinLoginFormViewModel] 准备触发OnLoginSuccess事件");
                    OnLoginSuccess?.Invoke();
                    Console.WriteLine("[SkinLoginFormViewModel] OnLoginSuccess事件已触发");
                }
                else
                {
                    ShowErrorToast("登录失败", "保存用户信息失败，请重试");
                }
            }
            else
            {
                ShowErrorToast("登录失败", "邮箱或密码错误，请重试");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SkinLoginForm] 第三方认证异常: {ex.Message}");
            ShowErrorToast("登录异常", $"登录失败: {ex.Message}");
        }
        finally
        {
            // 在UI线程上结束loading状态
            IsLoading = false;
            Console.WriteLine($"[SkinLoginForm] 第三方登录流程结束");
        }
    }

        [RelayCommand]
        private void ForgotPassword()
        {
            ShowInformationToast("功能提示", "忘记密码功能暂未实现");
        }

        [RelayCommand]
        private void Register()
        {
            ShowInformationToast("功能提示", "注册功能暂未实现");
        }

    [RelayCommand]
    private void ClearForm()
    {
        // 重置验证方式选择
        IsOfflineSelected = true;
        IsMicrosoftSelected = false;
        IsThirdPartySelected = false;
        
        // 清空所有输入
        OfflineName = string.Empty;
        MicrosoftEmail = string.Empty;
        ThirdPartyEmail = string.Empty;
        ThirdPartyPassword = string.Empty;
        
        // 清空微软认证相关字段
        DeviceCode = string.Empty;
        VerificationUrl = string.Empty;
        IsMicrosoftAuthInProgress = false;
        
        // 重置状态
        StatusMessage = string.Empty;
        HasError = false;
    }


    // Toast显示方法
    private void ShowErrorToast(string title, string message)
    {
        _toastManager.CreateToast()
            .WithTitle(title)
            .WithContent(message)
            .OfType(NotificationType.Error)
            .Dismiss().ByClicking()
            .Dismiss().After(TimeSpan.FromSeconds(4))
            .Queue();
    }

    private void ShowSuccessToast(string title, string message)
    {
        Console.WriteLine($"[SkinLoginFormViewModel] 准备显示成功Toast: {title} - {message}");
        Console.WriteLine($"[SkinLoginFormViewModel] 使用的ToastManager: {_toastManager?.GetType().Name} (HashCode: {_toastManager?.GetHashCode()})");
        try
        {
            _toastManager.CreateToast()
                .WithTitle(title)
                .WithContent(message)
                .OfType(NotificationType.Success)
                .Dismiss().ByClicking()
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Queue();
            Console.WriteLine("[SkinLoginFormViewModel] Toast已添加到队列");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SkinLoginFormViewModel] Toast显示失败: {ex.Message}");
        }
    }

    private void ShowInformationToast(string title, string message)
    {
        _toastManager.CreateToast()
            .WithTitle(title)
            .WithContent(message)
            .OfType(NotificationType.Information)
            .Dismiss().ByClicking()
            .Dismiss().After(TimeSpan.FromSeconds(3))
            .Queue();
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

}
