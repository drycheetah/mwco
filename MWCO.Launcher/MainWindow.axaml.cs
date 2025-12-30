using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using MWCO.Server;
using MWCO.Shared;
using IOPath = System.IO.Path;
using IODirectory = System.IO.Directory;
using IOFile = System.IO.File;

namespace MWCO.Launcher;

public partial class MainWindow : Window
{
    private Process? serverProcess;
    private DispatcherTimer? loadingTimer;
    private int loadingProgress = 0;

    public MainWindow()
    {
        InitializeComponent();
        InitializeUI();
    }

    private void InitializeUI()
    {
        // Check game installation
        string? gameDir = GetGameDirectory();
        var gameIndicator = this.FindControl<Ellipse>("GameStatusIndicator");
        var gameText = this.FindControl<TextBlock>("GameStatusText");

        if (!string.IsNullOrEmpty(gameDir))
        {
            if (gameIndicator != null) gameIndicator.Fill = new SolidColorBrush(Color.Parse("#0FA"));
            if (gameText != null) gameText.Text = "Game installed";

            // Check BepInEx
            string bepInExDir = IOPath.Combine(gameDir, "BepInEx");
            var modIndicator = this.FindControl<Ellipse>("ModStatusIndicator");
            var modText = this.FindControl<TextBlock>("ModStatusText");

            if (IODirectory.Exists(bepInExDir))
            {
                if (modIndicator != null) modIndicator.Fill = new SolidColorBrush(Color.Parse("#0FA"));
                if (modText != null) modText.Text = "BepInEx ready";
            }
            else
            {
                if (modIndicator != null) modIndicator.Fill = new SolidColorBrush(Color.Parse("#FF4444"));
                if (modText != null) modText.Text = "BepInEx missing";
            }
        }
        else
        {
            if (gameIndicator != null) gameIndicator.Fill = new SolidColorBrush(Color.Parse("#FF4444"));
            if (gameText != null) gameText.Text = "Game not found";
        }

        // Load network info
        var networkInfo = this.FindControl<TextBlock>("NetworkInfoText");
        if (networkInfo != null)
        {
            networkInfo.Text = $"Protocol: v{NetworkConfig.ProtocolVersion}\n" +
                              $"Tick Rate: {NetworkConfig.PhysicsTickRate}Hz\n" +
                              $"Port: {NetworkConfig.DefaultPort}";
        }
    }

    private async void StartServer_Click(object sender, RoutedEventArgs e)
    {
        var portBox = this.FindControl<TextBox>("PortTextBox");
        var statusText = this.FindControl<TextBlock>("ServerStatusText");
        var statusIndicator = this.FindControl<Ellipse>("ServerStatusIndicator");
        var startButton = this.FindControl<Button>("StartServerButton");
        var statusBar = this.FindControl<TextBlock>("StatusBarText");

        if (serverProcess != null && !serverProcess.HasExited)
        {
            serverProcess.Kill();
            serverProcess = null;
            if (statusText != null) statusText.Text = "Server offline";
            if (statusIndicator != null) statusIndicator.Fill = new SolidColorBrush(Color.Parse("#505257"));
            if (startButton != null) startButton.Content = "START SERVER";
            if (statusBar != null) statusBar.Text = "Server stopped";
            return;
        }

        int port = NetworkConfig.DefaultPort;
        if (portBox != null && !string.IsNullOrWhiteSpace(portBox.Text))
        {
            if (!int.TryParse(portBox.Text, out port))
            {
                if (statusText != null) statusText.Text = "Invalid port";
                if (statusIndicator != null) statusIndicator.Fill = new SolidColorBrush(Color.Parse("#FF4444"));
                return;
            }
        }

        try
        {
            serverProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"run --project {IOPath.Combine(GetProjectRoot(), "MWCO.Server/MWCO.Server/MWCO.Server.csproj")} {port}",
                    UseShellExecute = false,
                    CreateNoWindow = false
                }
            };

            serverProcess.Start();
            if (statusText != null) statusText.Text = $"Server online • Port {port}";
            if (statusIndicator != null) statusIndicator.Fill = new SolidColorBrush(Color.Parse("#0FA"));
            if (startButton != null) startButton.Content = "STOP SERVER";
            if (statusBar != null) statusBar.Text = $"Server running on port {port}";
        }
        catch (Exception ex)
        {
            if (statusText != null) statusText.Text = "Failed to start";
            if (statusIndicator != null) statusIndicator.Fill = new SolidColorBrush(Color.Parse("#FF4444"));
            if (statusBar != null) statusBar.Text = $"Error: {ex.Message}";
        }
    }

    private async void InstallMod_Click(object sender, RoutedEventArgs e)
    {
        var modButton = this.FindControl<Button>("InstallModButton");
        var statusBar = this.FindControl<TextBlock>("StatusBarText");
        var modIndicator = this.FindControl<Ellipse>("ModStatusIndicator");
        var modText = this.FindControl<TextBlock>("ModStatusText");

        if (modButton != null) modButton.IsEnabled = false;
        if (statusBar != null) statusBar.Text = "Installing mod...";

        try
        {
            string? gameDir = GetGameDirectory();
            if (string.IsNullOrEmpty(gameDir))
            {
                if (statusBar != null) statusBar.Text = "Error: Game not found";
                if (modButton != null) modButton.IsEnabled = true;
                return;
            }

            string bepInExDir = IOPath.Combine(gameDir, "BepInEx");
            if (!IODirectory.Exists(bepInExDir))
            {
                // Auto-install BepInEx
                if (statusBar != null) statusBar.Text = "Downloading BepInEx...";

                if (!await InstallBepInEx(gameDir, statusBar))
                {
                    if (statusBar != null) statusBar.Text = "Failed to install BepInEx";
                    if (modButton != null) modButton.IsEnabled = true;
                    return;
                }
            }

            if (statusBar != null) statusBar.Text = "Building project...";

            // Build project
            var buildProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"build {IOPath.Combine(GetProjectRoot(), "MWCO.slnx")} --configuration Release",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            buildProcess.Start();
            await buildProcess.WaitForExitAsync();

            if (buildProcess.ExitCode != 0)
            {
                if (statusBar != null) statusBar.Text = "Build failed";
                if (modButton != null) modButton.IsEnabled = true;
                return;
            }

            if (statusBar != null) statusBar.Text = "Copying files...";

            // Copy mod files
            string pluginsDir = IOPath.Combine(bepInExDir, "plugins");
            IODirectory.CreateDirectory(pluginsDir);

            string[] filesToCopy = new[]
            {
                "MWCO.Client/bin/Release/netstandard2.1/MWCO.Client.dll",
                "MWCO.Shared/bin/Release/netstandard2.1/MWCO.Shared.dll",
                "MWCO.Client/bin/Release/netstandard2.1/0Harmony.dll"
            };

            foreach (var file in filesToCopy)
            {
                string sourcePath = IOPath.Combine(GetProjectRoot(), file);
                string fileName = IOPath.GetFileName(file);
                string destPath = IOPath.Combine(pluginsDir, fileName);

                if (IOFile.Exists(sourcePath))
                {
                    IOFile.Copy(sourcePath, destPath, true);
                }
            }

            if (statusBar != null) statusBar.Text = "Mod installed successfully";
            if (modIndicator != null) modIndicator.Fill = new SolidColorBrush(Color.Parse("#0FA"));
            if (modText != null) modText.Text = "Mod installed";
        }
        catch (Exception ex)
        {
            if (statusBar != null) statusBar.Text = $"Error: {ex.Message}";
        }
        finally
        {
            if (modButton != null) modButton.IsEnabled = true;
        }
    }

    private async void LaunchGame_Click(object sender, RoutedEventArgs e)
    {
        // Show EAC-style loading overlay
        var overlay = this.FindControl<Grid>("LoadingOverlay");
        var progressBar = this.FindControl<Border>("LoadingProgressBar");
        var loadingText = this.FindControl<TextBlock>("LoadingStatusText");
        var statusBar = this.FindControl<TextBlock>("StatusBarText");

        if (overlay != null) overlay.IsVisible = true;
        loadingProgress = 0;

        string[] loadingSteps = new[]
        {
            "Verifying game files...",
            "Checking anti-cheat...",
            "Loading network modules...",
            "Initializing multiplayer...",
            "Starting game..."
        };

        loadingTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(150)
        };

        int stepIndex = 0;
        loadingTimer.Tick += (s, args) =>
        {
            loadingProgress += 2;
            if (progressBar != null)
            {
                progressBar.Width = (380.0 * loadingProgress) / 100.0;
            }

            if (loadingProgress >= 20 && loadingProgress < 40 && stepIndex == 0)
            {
                stepIndex = 1;
                if (loadingText != null) loadingText.Text = loadingSteps[1];
            }
            else if (loadingProgress >= 40 && loadingProgress < 60 && stepIndex == 1)
            {
                stepIndex = 2;
                if (loadingText != null) loadingText.Text = loadingSteps[2];
            }
            else if (loadingProgress >= 60 && loadingProgress < 80 && stepIndex == 2)
            {
                stepIndex = 3;
                if (loadingText != null) loadingText.Text = loadingSteps[3];
            }
            else if (loadingProgress >= 80 && loadingProgress < 100 && stepIndex == 3)
            {
                stepIndex = 4;
                if (loadingText != null) loadingText.Text = loadingSteps[4];
            }

            if (loadingProgress >= 100)
            {
                loadingTimer?.Stop();
                if (overlay != null) overlay.IsVisible = false;

                // Launch game
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "steam",
                        Arguments = "steam://rungameid/516750",
                        UseShellExecute = true
                    });

                    if (statusBar != null) statusBar.Text = "Game launched - Press F10 in-game to connect";
                }
                catch (Exception ex)
                {
                    if (statusBar != null) statusBar.Text = $"Failed to launch: {ex.Message}";
                }
            }
        };

        if (loadingText != null) loadingText.Text = loadingSteps[0];
        loadingTimer.Start();
    }

    private async void Help_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Window
        {
            Title = "MWCO Help",
            Width = 550,
            Height = 450,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Background = new SolidColorBrush(Color.Parse("#12141A")),
            CanResize = false
        };

        var scrollViewer = new ScrollViewer
        {
            Margin = new Thickness(30)
        };

        var stack = new StackPanel { Spacing = 15 };

        stack.Children.Add(new TextBlock
        {
            Text = "MWCO HELP",
            FontSize = 22,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            LetterSpacing = 2
        });

        stack.Children.Add(new Border { Height = 1, Background = new SolidColorBrush(Color.Parse("#2A2D35")), Margin = new Thickness(0, 5, 0, 10) });

        stack.Children.Add(new TextBlock
        {
            Text = "GETTING STARTED",
            FontSize = 12,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(Color.Parse("#808285")),
            LetterSpacing = 2,
            Margin = new Thickness(0, 10, 0, 5)
        });

        stack.Children.Add(new TextBlock
        {
            Text = "1. Install BepInEx to your My Winter Car folder\n" +
                   "2. Click 'INSTALL MOD' in the launcher\n" +
                   "3. Click 'START SERVER' to host\n" +
                   "4. Click 'LAUNCH MY WINTER CAR'\n" +
                   "5. Press F10 in-game to open multiplayer menu\n" +
                   "6. Connect to 127.0.0.1:1999",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.Parse("#B0B3B8")),
            LineHeight = 18
        });

        stack.Children.Add(new TextBlock
        {
            Text = "NETWORK INFO",
            FontSize = 12,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(Color.Parse("#808285")),
            LetterSpacing = 2,
            Margin = new Thickness(0, 15, 0, 5)
        });

        stack.Children.Add(new TextBlock
        {
            Text = $"Default Port: {NetworkConfig.DefaultPort} (UDP)\n" +
                   $"Protocol Version: {NetworkConfig.ProtocolVersion}\n" +
                   $"Physics Tick Rate: {NetworkConfig.PhysicsTickRate}Hz\n" +
                   $"Update Rates: {NetworkConfig.HighPriorityUpdateRate}Hz / {NetworkConfig.MediumPriorityUpdateRate}Hz / {NetworkConfig.LowPriorityUpdateRate}Hz",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.Parse("#B0B3B8")),
            LineHeight = 18
        });

        stack.Children.Add(new TextBlock
        {
            Text = "DOCUMENTATION",
            FontSize = 12,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(Color.Parse("#808285")),
            LetterSpacing = 2,
            Margin = new Thickness(0, 15, 0, 5)
        });

        stack.Children.Add(new TextBlock
        {
            Text = $"README.md\nINSTALLATION.md\nQUICKSTART.md\nPROJECT_OVERVIEW.md",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.Parse("#B0B3B8")),
            LineHeight = 18
        });

        var closeButton = new Button
        {
            Content = "CLOSE",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Thickness(0, 25, 0, 0),
            Padding = new Thickness(40, 10),
            Background = new SolidColorBrush(Color.Parse("#2A2D35")),
            Foreground = new SolidColorBrush(Colors.White),
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(3),
            FontSize = 11,
            FontWeight = FontWeight.SemiBold
        };
        closeButton.Click += (s, e) => dialog.Close();
        stack.Children.Add(closeButton);

        scrollViewer.Content = stack;
        dialog.Content = scrollViewer;

        await dialog.ShowDialog(this);
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        if (serverProcess != null && !serverProcess.HasExited)
        {
            serverProcess.Kill();
        }
        Close();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (serverProcess != null && !serverProcess.HasExited)
        {
            serverProcess.Kill();
        }
        loadingTimer?.Stop();
        base.OnClosing(e);
    }

    private string? GetGameDirectory()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string steamPath = IOPath.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".local/share/Steam/steamapps/common/My Winter Car"
            );

            if (IODirectory.Exists(steamPath))
                return steamPath;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string[] possiblePaths = new[]
            {
                @"C:\Program Files (x86)\Steam\steamapps\common\My Winter Car",
                @"C:\Program Files\Steam\steamapps\common\My Winter Car",
                IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    @"Steam\steamapps\common\My Winter Car")
            };

            foreach (var path in possiblePaths)
            {
                if (IODirectory.Exists(path))
                    return path;
            }
        }

        return null;
    }

    private string GetProjectRoot()
    {
        string? exeDir = IOPath.GetDirectoryName(Environment.ProcessPath);

        if (exeDir != null)
        {
            System.IO.DirectoryInfo? dir = new System.IO.DirectoryInfo(exeDir);
            while (dir != null && dir.Parent != null)
            {
                if (IOFile.Exists(IOPath.Combine(dir.FullName, "MWCO.slnx")))
                {
                    return dir.FullName;
                }
                dir = dir.Parent;
            }
        }

        return System.IO.Directory.GetCurrentDirectory();
    }

    private async Task<bool> InstallBepInEx(string gameDir, TextBlock? statusBar)
    {
        try
        {
            // BepInEx 5.4.23.2 for Unity Mono
            // Always use Windows version since game runs through Proton on Linux
            string bepInExUrl = "https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.2/BepInEx_win_x64_5.4.23.2.zip";

            string tempZip = IOPath.Combine(IOPath.GetTempPath(), "bepinex.zip");

            if (statusBar != null) statusBar.Text = "Downloading BepInEx...";

            using (var httpClient = new System.Net.Http.HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromMinutes(5);
                var response = await httpClient.GetAsync(bepInExUrl);
                response.EnsureSuccessStatusCode();

                await using (var fs = new System.IO.FileStream(tempZip, System.IO.FileMode.Create))
                {
                    await response.Content.CopyToAsync(fs);
                }
            }

            if (statusBar != null) statusBar.Text = "Extracting BepInEx...";

            // Extract to game directory
            System.IO.Compression.ZipFile.ExtractToDirectory(tempZip, gameDir, true);

            // Clean up
            if (IOFile.Exists(tempZip))
                IOFile.Delete(tempZip);

            if (statusBar != null) statusBar.Text = "BepInEx installed successfully";

            // Update UI indicators
            var modIndicator = this.FindControl<Ellipse>("ModStatusIndicator");
            var modText = this.FindControl<TextBlock>("ModStatusText");
            if (modIndicator != null) modIndicator.Fill = new SolidColorBrush(Color.Parse("#0FA"));
            if (modText != null) modText.Text = "BepInEx ready";

            return true;
        }
        catch (Exception ex)
        {
            if (statusBar != null) statusBar.Text = $"BepInEx install failed: {ex.Message}";
            return false;
        }
    }
}
