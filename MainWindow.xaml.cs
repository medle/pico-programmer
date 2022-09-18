
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

            // ordinary KeyDown doesn't give us SPACE down events
            TerminalRichBox.PreviewKeyDown += new KeyEventHandler(OnKeyDown);
        }

        private DeviceChangeMonitor deviceChangeMonitor;
        private ComPortMonitor comPortMonitor;

        protected override void OnSourceInitialized(EventArgs e)
        {
            deviceChangeMonitor = new DeviceChangeMonitor(this, OnDeviceChange);
            base.OnSourceInitialized(e);
            Log("Loaded");
        }

        protected override void OnClosed(EventArgs e)
        {
            SaveOptions();
            comPortMonitor.Close();
            base.OnClosed(e);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            char ch = KeyEventUtility.GetCharFromKey(e.Key);

            if (e.Key == Key.Return) ch = '\n';
            if (e.Key == Key.Space) ch = ' ';
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) ch = '\0';
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift) ch = '\0';
            if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt) ch = '\0';

            if (ch != 0) {
                if (comPortMonitor.TrySendChar(ch))
                    AddToTerminal(ch.ToString());
            }
        }

        private void OnDeviceChange(bool deviceArrived)
        {
            if (deviceArrived)
            {
                //Log($"Device arrived");
                Programmer.TrySendProgram(FirmwareFilePath.Text, TargetDevicePath.Text, Log);
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

        private void AddToTerminalByInvoke(string s)
        {
            Action a = () => AddToTerminal(s);
            Dispatcher.BeginInvoke(a);
        }

        private void AddToTerminal(string s)
        {
            if (s == null) return;
            var buf = new StringBuilder();
            foreach (char ch in s) {
                if (ch == '\r') continue;
                if (ch == '\n') {
                    if (buf.Length > 0) AddToTerminalText(buf.ToString());
                    buf.Clear();
                    AddToTerminalLineBreak();
                    continue;
                }
                buf.Append(ch);
            }

            if(buf.Length > 0) AddToTerminalText(buf.ToString());
        }

        private void AddToTerminalLineBreak()
        {
            var p = new Paragraph();
            p.Margin = new Thickness(0);
            TerminalRichBox.Document.Blocks.Add(p);
            TerminalRichBox.CaretPosition = p.ContentEnd;
            p.BringIntoView();
        }

        private void AddToTerminalText(string text)
        {
            if (TerminalRichBox.Document.Blocks.Count == 0) {
                AddToTerminalLineBreak();
            }

            var lastBlock = TerminalRichBox.Document.Blocks.Last() as Paragraph;
            if (lastBlock != null) {
                lastBlock.Margin = new Thickness(0);
                lastBlock.Inlines.Add(new Run(text));
                TerminalRichBox.CaretPosition = lastBlock.ContentEnd;
                lastBlock.BringIntoView();
            }

            // don't let the textbox grow indefinitely, it hangs when becomes too large
            if (TerminalRichBox.Document.Blocks.Count > 200) {
                var firstBlock = TerminalRichBox.Document.Blocks.First();
                TerminalRichBox.Document.Blocks.Remove(firstBlock);
            }
        }

        private void OnClearComLog_Click(object sender, RoutedEventArgs e)
        {
            TerminalRichBox.Document.Blocks.Clear();
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
