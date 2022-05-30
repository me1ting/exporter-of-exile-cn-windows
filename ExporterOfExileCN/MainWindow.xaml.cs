using ExporterOfExileCN.Core;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ExporterOfExileCN
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Config config;
        private BackendRunner backendRunner;
        private BitmapImage successIcon;
        private BitmapImage errorIcon;

        public MainWindow()
        {
            InitializeComponent();
            config = Config.Load();
            backendRunner = new BackendRunner();

            successIcon = new BitmapImage(new Uri("pack://application:,,,/Resources/success.png"));
            errorIcon = new BitmapImage(new Uri("pack://application:,,,/Resources/error.png"));

            portInput.Text = config.ListenPort.ToString();
            StartBackend();
        }

        private void LogView(object sender, RoutedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = "explorer.exe";
            string execPath = AppDomain.CurrentDomain.BaseDirectory;
            p.StartInfo.Arguments = @" /select, " + execPath + "log.txt";
            p.StartInfo.Arguments = @" /select, " + execPath + "log.txt";
            p.Start();
        }

        private void UpdatePortButtonClick(object sender, RoutedEventArgs e)
        {
            var newPort = int.Parse(portInput.Text);
            config.ListenPort = newPort;
            Config.Save(config);
            StartBackend();

            MessageBox.Show("更新成功");
            portUpdateButton.IsEnabled = false;
        }

        private void PatchSelectorClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (!string.IsNullOrEmpty(config.LastPatchedPath) && Directory.Exists(config.LastPatchedPath))
            {
                openFileDialog.InitialDirectory = config.LastPatchedPath;
            }
            openFileDialog.Filter = "ImportTab.lua (ImportTab.lua)|ImportTab.lua";
            if (openFileDialog.ShowDialog() == true)
            {
                var fileName = openFileDialog.FileName;
                patchFileName.Text = fileName;
                patchFileName.ToolTip = fileName;

                var port = portInput.Text;
                var hostname = $"http://localhost:{port}";
                if (Patcher.IsNeededPatch(fileName, hostname))
                {
                    patchButton.IsEnabled = true;
                }
                else
                {
                    patchButton.IsEnabled = false;
                    ShowHelpMessage("已打补丁，不需要重复操作");
                }
            }
        }

        private void PatchButtonClick(object sender, RoutedEventArgs e)
        {
            var filename = patchFileName.Text;
            var port = config.ListenPort;
            var hostname = $"http://localhost:{port}/";
            Patcher.Patch(filename, hostname);

            var directory = Path.GetDirectoryName(filename);
            config.LastPatchedPath = directory;
            Config.Save(config);

            patchButton.IsEnabled = false;
            MessageBox.Show("打补丁成功");
        }

        private void RestartClick(object sender, RoutedEventArgs e)
        {
            StopBackend();
            StartBackend();
            MessageBox.Show("重启中");
        }

        private void NumberValidationOnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void NumberValidationOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void NumberValidationOnPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsNumberic(text))
                { e.CancelCommand(); }
            }
            else { e.CancelCommand(); }
        }

        private bool IsNumberic(string _string)
        {
            if (string.IsNullOrEmpty(_string))
                return false;
            foreach (char c in _string)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }

        private void PortInputTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(portInput.Text))
            {
                portUpdateButton.IsEnabled = false;
            }
            else
            {
                var newPort = int.Parse(portInput.Text);
                if (newPort != config.ListenPort)
                {
                    portUpdateButton.IsEnabled = true;
                }
            }
        }

        private void ShowHelpMessage(string message)
        {
            helpMessageShow.Text = message;
        }

        private void StartBackend()
        {
            backendRunner.Start(config);
            ShowStatus(true);
        }

        private void StopBackend()
        {
            backendRunner.Stop();
            ShowStatus(false);
        }

        private void ShowStatus(bool isRunning)
        {
            if (isRunning)
            {

                statusIcon.Source = successIcon;
                statusText.Text = "运行中";
            }
            else
            {
                statusIcon.Source = errorIcon;
                statusText.Text = "已停止";
            }
        }
    }
}
