using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.Threading;
using NAudio.CoreAudioApi;

class Program
{
    // Импортируем функцию для перехвата клавиш
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
                Console.WriteLine("Ожидание клавиши..."); // Проверка работы цикла
                if ((GetAsyncKeyState(0xAD) & 0x8000) != 0) // 0xAD - клавиша Mute
                {
                    Console.WriteLine("Клавиша Mute нажата, переключение...");
                    ToggleAudioDevice();
                    Thread.Sleep(500); // Задержка, чтобы избежать повторного срабатывания
                }
                Thread.Sleep(50);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}\n{ex.StackTrace}");
            Console.ReadLine(); // Оставить консоль открытой для просмотра ошибки
        }
    }

    static void ToggleAudioDevice()
    {
        try
        {
            var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToList();
            var currentDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            Console.WriteLine("Текущий аудиовыход: " + currentDevice.FriendlyName);
            Console.WriteLine("Доступные аудиоустройства:");
            foreach (var device in devices)
            {
                Console.WriteLine(" - " + device.FriendlyName);
            }

            var targetDevice = devices.FirstOrDefault(d => d.FriendlyName.Contains(currentDevice.FriendlyName == device1 ? device2 : device1));

            if (targetDevice != null)
            {
                var policyConfig = new PolicyConfigClient();
                policyConfig.SetDefaultEndpoint(targetDevice.ID);
                Console.WriteLine($"Переключено на: {targetDevice.FriendlyName}");
            }
            else
            {
                Console.WriteLine("Устройство не найдено! Убедитесь, что название указано верно.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при переключении устройства: {ex.Message}\n{ex.StackTrace}");
        }
    }
}

// Класс для смены устройства по умолчанию
[ComImport]
[Guid("568b9108-44bf-40b4-9006-86afe5b5a620")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface IPolicyConfig
{
    [PreserveSig]
    int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string wszDeviceId, int eRole);
}

class PolicyConfigClient
{
    public void SetDefaultEndpoint(string deviceId)
    {
        try
        {
            Type policyConfigType = Type.GetTypeFromCLSID(new Guid("568b9108-44bf-40b4-9006-86afe5b5a620"));
            IPolicyConfig policyConfig = (IPolicyConfig)Activator.CreateInstance(policyConfigType);
            policyConfig.SetDefaultEndpoint(deviceId, 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при установке устройства по умолчанию: {ex.Message}");
        }
    }
}
