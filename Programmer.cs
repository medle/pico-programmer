
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PicoProgrammer
{
    public class Programmer
    {
        public static void TrySendProgram(
            string firmwareFilePath, string targetDevicePath, Action<string> logger)
        {
            string extension = ".uf2";
            if (!firmwareFilePath.ToLower().EndsWith(extension)) {
                logger($"File {firmwareFilePath} does not have extension {extension}.");
                return;
            }

            if (!File.Exists(firmwareFilePath)) {
                logger($"File {firmwareFilePath} does not exists.");
                return;
            }

            if (!Directory.Exists(targetDevicePath)) {
                logger($"Directory {targetDevicePath} does not exists.");
                return;
            }

            var sourceTime = File.GetLastWriteTime(firmwareFilePath);

            var targetFilePath = Path.Combine(targetDevicePath, Path.GetFileName(firmwareFilePath));
            if (File.Exists(targetFilePath)) {
                var targetTime = File.GetLastWriteTime(targetFilePath);
                if (sourceTime >= targetTime) {
                    logger("File on the target device is newer, cancelling copying.");
                    return;
                }
            }

            try
            {
                File.Copy(firmwareFilePath, targetFilePath, true);
                string stamp = sourceTime.ToString("yyyy.MM.dd HH:mm:ss");
                logger($"Copied to [{targetFilePath}] file time [{stamp}].");
            }
            catch (Exception e) 
            {
                logger($"Error copying file {firmwareFilePath} to {targetDevicePath}: {e.Message}");
            }
        }
    }
}
