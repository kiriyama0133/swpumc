using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using swpumc.Services;
using swpumc.ViewModels;

namespace swpumc.Pages.ModpackDownload;

public class ModpackDownloadPageViewModel : BaseViewModel, INotifyPropertyChanged
{
    public static string Title { get; set; } = "整合包下载";
    public static string Description { get; set; } = "Modpack Download";
    public static MaterialIconKind Icon { get; set; } = MaterialIconKind.Archive;
    public static int Index { get; set; } = 3;

    private readonly IModpackDownloadService _modpackService;
    private readonly IConfigService _configService;
    private readonly VersionServiceFactory _versionServiceFactory;
    
    private string _selectedFilePath = string.Empty;
    private bool _hasSelectedFile = false;
    private bool _isValidating = false;
    private bool _isInstalling = false;
    private bool _canInstall = false;
    private double _installProgress = 0;
    private string _installProgressText = string.Empty;
    private string _modpackTypeName = string.Empty;
    private string _modpackValidationMessage = string.Empty;
    private MaterialIconKind _modpackTypeIcon = MaterialIconKind.QuestionMark;
    private string _modpackTypeColor = "#FF6B6B";

    public string SelectedFilePath
    {
        get => _selectedFilePath;
        set => SetProperty(ref _selectedFilePath, value);
    }

    public bool HasSelectedFile
    {
        get => _hasSelectedFile;
        set => SetProperty(ref _hasSelectedFile, value);
    }

    public bool IsValidating
    {
        get => _isValidating;
        set => SetProperty(ref _isValidating, value);
    }

    public bool IsInstalling
    {
        get => _isInstalling;
        set 
        {
            if (SetProperty(ref _isInstalling, value))
            {
                OnPropertyChanged(nameof(CanInstall));
            }
        }
    }

    public bool CanInstall
    {
        get => _canInstall && !_isInstalling;
        set => SetProperty(ref _canInstall, value);
    }

    public double InstallProgress
    {
        get => _installProgress;
        set => SetProperty(ref _installProgress, value);
    }

    public string InstallProgressText
    {
        get => _installProgressText;
        set => SetProperty(ref _installProgressText, value);
    }

    public string ModpackTypeName
    {
        get => _modpackTypeName;
        set => SetProperty(ref _modpackTypeName, value);
    }

    public string ModpackValidationMessage
    {
        get => _modpackValidationMessage;
        set => SetProperty(ref _modpackValidationMessage, value);
    }

    public MaterialIconKind ModpackTypeIcon
    {
        get => _modpackTypeIcon;
        set => SetProperty(ref _modpackTypeIcon, value);
    }

    public string ModpackTypeColor
    {
        get => _modpackTypeColor;
        set => SetProperty(ref _modpackTypeColor, value);
    }

    public ICommand SelectFileCommand { get; }
    public ICommand ValidateModpackCommand { get; }
    public ICommand InstallModpackCommand { get; }

    public ModpackDownloadPageViewModel(IModpackDownloadService modpackService, IConfigService configService, VersionServiceFactory versionServiceFactory)
    {
        Console.WriteLine($"[ModpackDownloadPageViewModel] 构造函数开始 - 时间: {DateTime.Now:HH:mm:ss.fff}");
        
        _modpackService = modpackService;
        _configService = configService;
        _versionServiceFactory = versionServiceFactory;
        
        SelectFileCommand = new AsyncRelayCommand(SelectFileAsync);
        ValidateModpackCommand = new AsyncRelayCommand(ValidateModpackAsync);
        InstallModpackCommand = new AsyncRelayCommand(InstallModpackAsync);
        
        Console.WriteLine($"[ModpackDownloadPageViewModel] 构造函数结束 - 时间: {DateTime.Now:HH:mm:ss.fff}");
    }

    private async Task SelectFileAsync()
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel((App.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow);
            if (topLevel?.StorageProvider == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "选择整合包文件",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("整合包文件")
                    {
                        Patterns = new[] { "*.zip", "*.mrpack" }
                    },
                    new FilePickerFileType("所有文件")
                    {
                        Patterns = new[] { "*.*" }
                    }
                }
            });

            if (files.Count > 0)
            {
                var file = files[0];
                var path = file.Path.LocalPath;
                
                SelectedFilePath = path;
                HasSelectedFile = true;
                CanInstall = false;
                
                // 重置整合包信息
                ModpackTypeName = "未知类型";
                ModpackValidationMessage = "请点击验证按钮验证整合包";
                ModpackTypeIcon = MaterialIconKind.QuestionMark;
                ModpackTypeColor = "#FF6B6B";
                
                Console.WriteLine($"[ModpackDownloadPageViewModel] 文件选择完成: {path}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ModpackDownloadPageViewModel] 文件选择失败: {ex.Message}");
        }
    }

    private async Task ValidateModpackAsync()
    {
        if (string.IsNullOrEmpty(SelectedFilePath))
        {
            return;
        }

        try
        {
            IsValidating = true;
            ModpackValidationMessage = "正在验证整合包...";
            
            var result = await _modpackService.ValidateModpackAsync(SelectedFilePath);
            
            if (result.IsValid)
            {
                ModpackTypeName = GetModpackTypeName(result.Type);
                ModpackValidationMessage = "整合包验证成功，可以安装";
                ModpackTypeIcon = GetModpackTypeIcon(result.Type);
                ModpackTypeColor = "#4CAF50";
                CanInstall = true;
                
                Console.WriteLine($"[ModpackDownloadPageViewModel] 整合包验证成功: {result.Type}");
            }
            else
            {
                ModpackTypeName = "无效的整合包";
                ModpackValidationMessage = result.ErrorMessage ?? "整合包格式不正确";
                ModpackTypeIcon = MaterialIconKind.Error;
                ModpackTypeColor = "#F44336";
                CanInstall = false;
                
                Console.WriteLine($"[ModpackDownloadPageViewModel] 整合包验证失败: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            ModpackTypeName = "验证失败";
            ModpackValidationMessage = $"验证过程中发生错误: {ex.Message}";
            ModpackTypeIcon = MaterialIconKind.Error;
            ModpackTypeColor = "#F44336";
            CanInstall = false;
            
            Console.WriteLine($"[ModpackDownloadPageViewModel] 验证异常: {ex.Message}");
        }
        finally
        {
            IsValidating = false;
        }
    }

    private async Task InstallModpackAsync()
    {
        if (string.IsNullOrEmpty(SelectedFilePath) || !CanInstall)
        {
            return;
        }

        try
        {
            IsInstalling = true;
            InstallProgress = 0;
            InstallProgressText = "开始安装整合包...";
            
            // 获取Minecraft文件夹和Java路径
            var minecraftFolder = _versionServiceFactory.GetMinecraftFolderPath();
            var javaPath = _configService.GetDefaultJavaPath();
            
            // 创建整合包下载服务实例
            var modpackService = ModpackDownloadServiceFactory.Create(minecraftFolder, javaPath);
            
            // 验证整合包类型并执行相应的安装
            var validationResult = await _modpackService.ValidateModpackAsync(SelectedFilePath);
            
            var installResult = validationResult.Type switch
            {
                ModpackType.CurseForge => await modpackService.InstallCurseforgeModpackAsync(SelectedFilePath, 
                    (progress, status) => UpdateInstallProgress(progress, status)),
                ModpackType.Modrinth => await modpackService.InstallModrinthModpackAsync(SelectedFilePath, 
                    (progress, status) => UpdateInstallProgress(progress, status)),
                ModpackType.MCBBS => await modpackService.InstallMcbbsModpackAsync(SelectedFilePath, 
                    (progress, status) => UpdateInstallProgress(progress, status)),
                _ => new ModpackInstallResult { IsSuccess = false, ErrorMessage = "不支持的整合包类型" }
            };
            
            if (installResult.IsSuccess)
            {
                InstallProgressText = "整合包安装完成！";
                Console.WriteLine($"[ModpackDownloadPageViewModel] 整合包安装成功: {installResult.GameCoreId}");
            }
            else
            {
                InstallProgressText = $"安装失败: {installResult.ErrorMessage}";
                Console.WriteLine($"[ModpackDownloadPageViewModel] 整合包安装失败: {installResult.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            InstallProgressText = $"安装异常: {ex.Message}";
            Console.WriteLine($"[ModpackDownloadPageViewModel] 安装异常: {ex.Message}");
        }
        finally
        {
            IsInstalling = false;
        }
    }

    private void UpdateInstallProgress(double progress, string status)
    {
        Dispatcher.UIThread.Post(() =>
        {
            InstallProgress = progress;
            InstallProgressText = $"{status} ({progress:F1}%)";
        });
    }

    private static string GetModpackTypeName(ModpackType type)
    {
        return type switch
        {
            ModpackType.CurseForge => "CurseForge 整合包",
            ModpackType.Modrinth => "Modrinth 整合包",
            ModpackType.MCBBS => "MCBBS 整合包",
            _ => "未知类型"
        };
    }

    private static MaterialIconKind GetModpackTypeIcon(ModpackType type)
    {
        return type switch
        {
            ModpackType.CurseForge => MaterialIconKind.Package,
            ModpackType.Modrinth => MaterialIconKind.PackageVariant,
            ModpackType.MCBBS => MaterialIconKind.PackageVariantClosed,
            _ => MaterialIconKind.QuestionMark
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
