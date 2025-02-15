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
            Console.WriteLine("Ожидание нажатия клавиши Mute...");

            while (true)
            {
                Console.WriteLine("Ожидание клавиши...");
                if ((GetAsyncKeyState(0xAD) & 0x8000) != 0)
                {
                    Console.WriteLine("Клавиша Mute нажата, переключение...");
                    ToggleAudioDevice();
                    Thread.Sleep(500);
                }
                Thread.Sleep(50);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}\n{ex.StackTrace}");
            Console.ReadLine();
        }
    }

    static void ToggleAudioDevice()
    {
        try
        {
            string currentDeviceId = GetCurrentAudioDeviceId();
            string targetDeviceId = currentDeviceId == GetDeviceId(device1) ? GetDeviceId(device2) : GetDeviceId(device1);

            Console.WriteLine($"Переключение на устройство с ID: {targetDeviceId}");

            SetAudioDevice(targetDeviceId);
            UnmuteAudio(); // Снимаем Mute после переключения
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при переключении устройства: {ex.Message}\n{ex.StackTrace}");
        }
    }

    static string GetCurrentAudioDeviceId()
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = "-Command \"(Get-AudioDevice -Playback).ID\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(psi))
        {
            process.WaitForExit();
            return process.StandardOutput.ReadLine().Trim();
        }
    }

    static string GetDeviceId(string deviceName)
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-Command \"(Get-AudioDevice -List | Where-Object {{$_.Name -eq '{deviceName}'}}).ID\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(psi))
        {
            process.WaitForExit();
            return process.StandardOutput.ReadLine()?.Trim() ?? "";
        }
    }

    static void SetAudioDevice(string deviceId)
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-Command \"Set-AudioDevice -ID '{deviceId}'\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(psi))
        {
            process.WaitForExit();
            Console.WriteLine(process.StandardOutput.ReadToEnd());
        }
    }

    static void UnmuteAudio()
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = "-Command \"(Get-AudioDevice -Playback) | Set-Volume -Volume 50; (Get-AudioDevice -Playback) | Set-Volume -Mute $false\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(psi))
        {
            process.WaitForExit();
            Console.WriteLine("Громкость установлена на 50%, звук включен после переключения.");
        }
    }

}
