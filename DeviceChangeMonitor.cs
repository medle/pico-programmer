using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoProgrammer
{
    /// <summary>
    /// Method to get notifications when a system device change occurs,
    /// such as USB drive connecting or disconnecting.
    /// https://qa.sqlite.in/qa/?qa=946706/
    /// </summary>
    public class DeviceChangeMonitor : System.Windows.Forms.NativeWindow
    {
        // https://docs.microsoft.com/en-us/windows/win32/devio/wm-devicechange
        private const int WM_DEVICECHANGE = 0x0219;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;

        private Action<bool> onDeviceChange;

        public DeviceChangeMonitor(Window window, Action<bool> onDeviceChange)
        {
            var helper = new System.Windows.Interop.WindowInteropHelper(window);
            this.AssignHandle(helper.Handle);
            this.onDeviceChange = onDeviceChange;
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == WM_DEVICECHANGE)
            {
                if (m.WParam == (IntPtr)DBT_DEVICEARRIVAL)
                {
                    if (onDeviceChange != null) onDeviceChange(true);
                } 
                
                if(m.WParam == (IntPtr)DBT_DEVICEREMOVECOMPLETE) 
                {
                    if (onDeviceChange != null) onDeviceChange(false);
                }
            }

            base.WndProc(ref m);
        }
    }
}
