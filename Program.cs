using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

class Program
{
    [DllImport("user32.dll")]
    public static extern short GetAsyncKeyState(int vKey);

    static string device1 = "Наушники (EDIFIER W830BT Stereo)";
    static string device2 = "Динамики (4- LOXJIE AUDIO)";

    static void Main()
    {
        try
        {
            //Console.WriteLine("Ожидание нажатия клавиши Mute...");

            while (true)
            {
                Thread.Sleep(50); // Добавляем задержку, чтобы снизить нагрузку на CPU

                if ((GetAsyncKeyState(0xAD) & 0x8000) != 0)
                {
                    //Console.WriteLine("Клавиша Mute нажата, переключение...");
                    ToggleAudioDevice();

                    // Ждем отпускания клавиши, чтобы избежать повторных срабатываний
                    while ((GetAsyncKeyState(0xAD) & 0x8000) != 0)
                    {
                        Thread.Sleep(50);
                    }
                }
            }
        }
        catch (Exception ex)
        {
           // Console.WriteLine($"Ошибка: {ex.Message}\n{ex.StackTrace}");
           // Console.ReadLine();
        }
    }

    static void ToggleAudioDevice()
    {
        try
        {
            string currentDeviceId = GetCurrentAudioDeviceId();
            string targetDeviceId = currentDeviceId == GetDeviceId(device1) ? GetDeviceId(device2) : GetDeviceId(device1);

            //Console.WriteLine($"Переключение с {currentDeviceId} на {targetDeviceId}");

            SetAudioDevice(targetDeviceId);
           // Console.WriteLine("Аудиоустройство переключено.");

            UnmuteAndAdjustVolumeNirCmd();
        }
        catch (Exception ex)
        {
           // Console.WriteLine($"Ошибка при переключении устройства: {ex.Message}\n{ex.StackTrace}");
        }
    }

    static void UnmuteAndAdjustVolumeNirCmd()
    {
        //Console.WriteLine("Снимаем Mute и меняем громкость через NirCmd...");

        string nircmdPath = @"C:\nircmd\nircmd.exe";

        if (!System.IO.File.Exists(nircmdPath))
        {
            //Console.WriteLine("Ошибка: NirCmd не найден. Убедитесь, что путь указан правильно.");
            return;
        }

        Process.Start(nircmdPath, "mutesysvolume 0").WaitForExit();
        Process.Start(nircmdPath, "changesysvolume -655").WaitForExit();
        Thread.Sleep(100);
        Process.Start(nircmdPath, "changesysvolume 655").WaitForExit();

       // Console.WriteLine("Mute снят, громкость слегка изменена.");
    }

    static string GetCurrentAudioDeviceId()
    {
        using (Process process = Process.Start(new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = "-Command \"(Get-AudioDevice -Playback).ID\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        }))
        {
            process.WaitForExit();
            return process.StandardOutput.ReadLine()?.Trim() ?? "";
        }
    }

    static string GetDeviceId(string deviceName)
    {
        using (Process process = Process.Start(new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-Command \"(Get-AudioDevice -List | Where-Object {{$_.Name -eq '{deviceName}'}}).ID\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        }))
        {
            process.WaitForExit();
            return process.StandardOutput.ReadLine()?.Trim() ?? "";
        }
    }

    static void SetAudioDevice(string deviceId)
    {
        using (Process process = Process.Start(new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-Command \"Set-AudioDevice -ID '{deviceId}'; Start-Sleep -Milliseconds 500\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        }))
        {
            process.WaitForExit();
            //Console.WriteLine("Аудиоустройство переключено.");
        }
    }
}
