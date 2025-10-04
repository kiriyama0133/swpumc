using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace swpumc.Services;

/// <summary>
/// Java环境检测服务
/// </summary>
public class JavaEnvironmentService : IJavaEnvironmentService
{
    private readonly List<JavaInstallation> _javaInstallations = new();
    
    /// <summary>
    /// 获取所有检测到的Java安装
    /// </summary>
    public IReadOnlyList<JavaInstallation> JavaInstallations => _javaInstallations.AsReadOnly();

    /// <summary>
    /// 初始化Java环境并保存到全局配置
    /// </summary>
    public async Task InitializeJavaEnvironmentsAsync(IConfigService configService)
    {
        try
        {
            
            // 检测Java环境
            var javaInstallations = await DetectJavaEnvironmentsAsync();
            if (javaInstallations.Count > 0)
            {
                // 转换为JavaEnvironmentInfo格式
                var javaEnvironments = javaInstallations.Select(java => new Models.JavaEnvironmentInfo
                {
                    JavaPath = java.JavaHome,
                    Version = java.Version,
                    Vendor = java.Vendor,
                    Architecture = java.Architecture,
                    IsDefault = java.IsDefault,
                    LastDetected = DateTime.Now
                }).ToList();
                
                // 保存到全局配置
                await configService.UpdateJavaEnvironmentsAsync(javaEnvironments);
                
            }
            else
            {
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[JavaEnvironmentService] Java环境初始化失败: {ex.Message}");
            Console.WriteLine($"[JavaEnvironmentService] 错误详情: {ex}");
        }
    }

    /// <summary>
    /// 检测所有Java环境
    /// </summary>
    public async Task<List<JavaInstallation>> DetectJavaEnvironmentsAsync()
    {
        _javaInstallations.Clear();
        
        // 检测JAVA_HOME环境变量
        await DetectFromJavaHome();
        
        // 检测PATH中的java命令
        await DetectFromPath();
        
        // 检测常见安装路径
        await DetectFromCommonPaths();
        
        // 去重并排序
        var uniqueInstallations = _javaInstallations
            .GroupBy(j => j.JavaHome)
            .Select(g => g.First())
            .OrderByDescending(j => j.Version)
            .ToList();
            
        _javaInstallations.Clear();
        _javaInstallations.AddRange(uniqueInstallations);
        
        return uniqueInstallations;
    }

    /// <summary>
    /// 从JAVA_HOME环境变量检测
    /// </summary>
    private async Task DetectFromJavaHome()
    {
        var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
        if (!string.IsNullOrEmpty(javaHome) && Directory.Exists(javaHome))
        {
            var installation = await CreateJavaInstallation(javaHome);
            if (installation != null)
            {
                installation.DetectionMethod = "JAVA_HOME环境变量";
                _javaInstallations.Add(installation);
            }
        }
    }

    /// <summary>
    /// 从PATH环境变量检测
    /// </summary>
    private async Task DetectFromPath()
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(path)) return;

        var paths = path.Split(Path.PathSeparator);
        foreach (var pathItem in paths)
        {
            if (string.IsNullOrEmpty(pathItem)) continue;
            
            var javaExe = Path.Combine(pathItem, "java.exe");
            var javaExeUnix = Path.Combine(pathItem, "java");
            
            if (File.Exists(javaExe) || File.Exists(javaExeUnix))
            {
                var javaHome = GetJavaHomeFromJavaExe(pathItem);
                if (!string.IsNullOrEmpty(javaHome) && Directory.Exists(javaHome))
                {
                    var installation = await CreateJavaInstallation(javaHome);
                    if (installation != null)
                    {
                        installation.DetectionMethod = "PATH环境变量";
                        _javaInstallations.Add(installation);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 从常见安装路径检测
    /// </summary>
    private async Task DetectFromCommonPaths()
    {
        var commonPaths = GetCommonJavaPaths();
        
        foreach (var basePath in commonPaths)
        {
            if (!Directory.Exists(basePath)) continue;
            
            try
            {
                var javaDirs = Directory.GetDirectories(basePath)
                    .Where(dir => IsJavaInstallation(dir))
                    .ToList();
                    
                foreach (var javaDir in javaDirs)
                {
                    var installation = await CreateJavaInstallation(javaDir);
                    if (installation != null)
                    {
                        installation.DetectionMethod = "常见安装路径";
                        _javaInstallations.Add(installation);
                    }
                }
            }
            catch (Exception ex)
            {
                // 忽略访问权限错误
                Console.WriteLine($"无法访问路径 {basePath}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 创建Java安装信息
    /// </summary>
    private async Task<JavaInstallation> CreateJavaInstallation(string javaHome)
    {
        try
        {
            var javaExe = GetJavaExecutablePath(javaHome);
            if (string.IsNullOrEmpty(javaExe) || !File.Exists(javaExe))
                return null;

            var version = await GetJavaVersion(javaExe);
            if (string.IsNullOrEmpty(version))
                return null;

            var vendor = await GetJavaVendor(javaExe);
            var architecture = GetJavaArchitecture(javaHome);
            var isDefault = IsDefaultJavaInstallation(javaHome);

            return new JavaInstallation
            {
                JavaHome = javaHome,
                Version = version,
                Vendor = vendor,
                Architecture = architecture,
                IsDefault = isDefault,
                JavaExecutable = javaExe
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"创建Java安装信息失败 {javaHome}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 获取Java版本
    /// </summary>
    private async Task<string> GetJavaVersion(string javaExe)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = javaExe,
                Arguments = "-version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null) return null;

            var output = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            // 解析版本信息
            var versionMatch = Regex.Match(output, @"version\s+""([^""]+)""");
            if (versionMatch.Success)
            {
                return versionMatch.Groups[1].Value;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 获取Java供应商
    /// </summary>
    private async Task<string> GetJavaVendor(string javaExe)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = javaExe,
                Arguments = "-version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null) return "Unknown";

            var output = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            // 解析供应商信息
            if (output.Contains("Oracle"))
                return "Oracle";
            if (output.Contains("OpenJDK"))
                return "OpenJDK";
            if (output.Contains("Eclipse"))
                return "Eclipse Adoptium";
            if (output.Contains("Amazon"))
                return "Amazon Corretto";
            if (output.Contains("Microsoft"))
                return "Microsoft";
            if (output.Contains("Azul"))
                return "Azul Zulu";

            return "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// 获取Java架构
    /// </summary>
    private string GetJavaArchitecture(string javaHome)
    {
        try
        {
            var binPath = Path.Combine(javaHome, "bin");
            if (!Directory.Exists(binPath)) return "Unknown";

            // 检查是否存在64位JVM
            var jvmDll = Path.Combine(javaHome, "bin", "server", "jvm.dll");
            if (File.Exists(jvmDll))
            {
                return "64-bit";
            }

            // 检查是否存在32位JVM
            var jvmDll32 = Path.Combine(javaHome, "bin", "client", "jvm.dll");
            if (File.Exists(jvmDll32))
            {
                return "32-bit";
            }

            return "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// 获取Java可执行文件路径
    /// </summary>
    public string GetJavaExecutablePath(string javaHome)
    {
        if (string.IsNullOrEmpty(javaHome))
            return "";
            
        // 如果已经是可执行文件路径，直接返回
        if (javaHome.EndsWith("java.exe", StringComparison.OrdinalIgnoreCase) || 
            javaHome.EndsWith("java", StringComparison.OrdinalIgnoreCase))
            return javaHome;
            
        // 尝试在Java Home目录下找到java.exe
        var possiblePaths = new[]
        {
            Path.Combine(javaHome, "bin", "java.exe"),
            Path.Combine(javaHome, "bin", "java"),
            Path.Combine(javaHome, "java.exe"),
            Path.Combine(javaHome, "java")
        };
        
        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
                return path;
        }
        
        // 如果找不到，返回原始路径
        return javaHome;
    }

    /// <summary>
    /// 从java.exe路径获取JAVA_HOME
    /// </summary>
    private string GetJavaHomeFromJavaExe(string javaExePath)
    {
        try
        {
            var binDir = Path.GetDirectoryName(javaExePath);
            if (binDir == null) return null;

            var javaHome = Path.GetDirectoryName(binDir);
            return javaHome;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 检查是否为Java安装目录
    /// </summary>
    private bool IsJavaInstallation(string path)
    {
        try
        {
            var binPath = Path.Combine(path, "bin");
            if (!Directory.Exists(binPath)) return false;

            var javaExe = Path.Combine(binPath, "java.exe");
            var javaExeUnix = Path.Combine(binPath, "java");
            
            return File.Exists(javaExe) || File.Exists(javaExeUnix);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 检查是否为默认Java安装
    /// </summary>
    private bool IsDefaultJavaInstallation(string javaHome)
    {
        var javaHomeEnv = Environment.GetEnvironmentVariable("JAVA_HOME");
        return string.Equals(javaHome, javaHomeEnv, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 获取常见Java安装路径
    /// </summary>
    private List<string> GetCommonJavaPaths()
    {
        var paths = new List<string>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Windows常见路径
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            
            paths.Add(Path.Combine(programFiles, "Java"));
            paths.Add(Path.Combine(programFilesX86, "Java"));
            paths.Add(Path.Combine(programFiles, "Eclipse Adoptium"));
            paths.Add(Path.Combine(programFiles, "Eclipse Foundation"));
            paths.Add(Path.Combine(programFiles, "Amazon Corretto"));
            paths.Add(Path.Combine(programFiles, "Microsoft"));
            paths.Add(Path.Combine(programFiles, "Azul"));
            
            // 用户目录
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            paths.Add(Path.Combine(userProfile, ".jdks"));
            paths.Add(Path.Combine(userProfile, "AppData", "Local", "Programs", "Eclipse Adoptium"));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // macOS常见路径
            paths.Add("/Library/Java/JavaVirtualMachines");
            paths.Add("/System/Library/Java/JavaVirtualMachines");
            paths.Add("/usr/libexec/java_home");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Linux常见路径
            paths.Add("/usr/lib/jvm");
            paths.Add("/usr/local/lib/jvm");
            paths.Add("/opt/java");
            paths.Add("/opt/jdk");
        }

        return paths;
    }
}

/// <summary>
/// Java安装信息
/// </summary>
public class JavaInstallation
{
    /// <summary>
    /// Java安装路径
    /// </summary>
    public string JavaHome { get; set; } = string.Empty;

    /// <summary>
    /// Java版本
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Java供应商
    /// </summary>
    public string Vendor { get; set; } = string.Empty;

    /// <summary>
    /// 架构（32-bit/64-bit）
    /// </summary>
    public string Architecture { get; set; } = string.Empty;

    /// <summary>
    /// 是否为默认Java安装
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Java可执行文件路径
    /// </summary>
    public string JavaExecutable { get; set; } = string.Empty;

    /// <summary>
    /// 检测方法
    /// </summary>
    public string DetectionMethod { get; set; } = string.Empty;

    /// <summary>
    /// 显示名称
    /// </summary>
    public string DisplayName => $"{Vendor} {Version} ({Architecture})";

    /// <summary>
    /// 详细信息
    /// </summary>
    public string Details => $"路径: {JavaHome}\n检测方法: {DetectionMethod}";
}

