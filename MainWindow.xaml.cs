
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PicoProgrammer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            RecallOptions();
            comPortMonitor = new ComPortMonitor(AddToTerminalByInvoke, Log);
            RefreshComPort();
        }

        private DeviceChangeDetector detector;
        private ComPortMonitor comPortMonitor;

        protected override void OnSourceInitialized(EventArgs e)
        {
            detector = new DeviceChangeDetector(this, OnDeviceChange);
            base.OnSourceInitialized(e);
            Log("Loaded");
        }

        protected override void OnClosed(EventArgs e)
        {
            SaveOptions();
            comPortMonitor.Close();
            base.OnClosed(e);
        }

        private void OnDeviceChange(bool deviceArrived)
        {
            if (deviceArrived)
            {
                Log($"Device arrived");
                Programmer.SendProgram(FirmwareFilePath.Text, TargetDevicePath.Text, Log);
                RefreshComPort();
            }
        }

        private void RefreshComPort()
        {
            if (string.IsNullOrEmpty(ComPort.Text)) comPortMonitor.Close();
            else comPortMonitor.Refresh(ComPort.Text);
        }

        private void Log(string s)
        {
            string stamp = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");
            string message = $"{stamp}: {s}";
            int index = LogListBox.Items.Add(message);
            LogListBox.SelectedIndex = index;
            LogListBox.ScrollIntoView(LogListBox.SelectedItem);
        }

        private void AddToTerminal(string s)
        {
            foreach (char ch in s) AddToTerminal(ch);
        }

        private void AddToTerminalByInvoke(char ch)
        {
            Action a = () => AddToTerminal(ch);
            Dispatcher.BeginInvoke(a);
        }

        private void AddToTerminal(char ch)
        {
            if (ch == '\r') return;

            if (TerminalRichBox.Document.Blocks.Count == 0 || ch == '\n') {
                var p = new Paragraph();
                TerminalRichBox.Document.Blocks.Add(p);
                if (ch == '\n') return;
            }

            var last = TerminalRichBox.Document.Blocks.Last() as Paragraph;
            if (last != null) {
                last.Margin = new Thickness(0);
                last.Inlines.Add(new Run(ch.ToString()));
                TerminalRichBox.ScrollToVerticalOffset(double.MaxValue);
            }
        }

        private void RecallOptions()
        {
            try
            {
                var options = Options.Recall();
                FirmwareFilePath.Text = options.FirmwareFilePath;
                TargetDevicePath.Text = options.TargetDevicePath;
                ComPort.Text = options.ComPort;
            }
            catch (Exception e)
            {
                Log("Failed to restore options: " + e.Message);
            }
        }

        private void SaveOptions()
        {
            try
            {
                var options = new Options();
                options.FirmwareFilePath = FirmwareFilePath.Text;
                options.TargetDevicePath = TargetDevicePath.Text;
                options.ComPort = ComPort.Text;
                options.Save();
            }
            catch (Exception e) {
                Log("Failed to save options: " + e.Message);
            }
        }
    }
}
