using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

class Program
{
    // Importowanie funkcji WinAPI do zarządzania dyskami sieciowymi
    [DllImport("mpr.dll")]
    private static extern int WNetCancelConnection2(string lpName, uint dwFlags, bool fForce);

    static void Main(string[] args)
    {
        Console.WriteLine("Czy chcesz odłączyć wszystkie mapowane dyski? (T/N)");
        string disconnectChoice = Console.ReadLine().ToUpper();

        if (disconnectChoice == "T")
        {
            // Rozłączanie wszystkich mapowanych dysków
            Console.WriteLine("Rozłączanie wszystkich mapowanych dysków...");

            DisconnectDrive("Y:");
            DisconnectDrive("Z:");
        }
        else
        {
            Console.WriteLine("Pominięto rozłączanie dysków.");
        }

        // Mapowanie pierwszego dysku z podanymi poświadczeniami
        string driveY = "Y:";
        string networkPathY = @"\\192.168.230.21\archive_ds001";
        string userY = "hsma";
        string passwordY = "123456";

        Console.WriteLine("Mapowanie dysku Y:");
        if (!MapDrive(driveY, networkPathY, userY, passwordY))
        {
            Console.WriteLine($"Nie udało się zmapować dysku {driveY}.");
            return;
        }

        // Prośba o poświadczenia dla drugiego dysku
        Console.WriteLine("Podaj login dla dysku Z:");
        string userZ = Console.ReadLine();

        Console.WriteLine("Podaj hasło dla dysku Z:");
        string passwordZ = Console.ReadLine();

        // Mapowanie drugiego dysku
        string driveZ = "Z:";
        string networkPathZ = @"\\192.168.230.30\Production_Oled";

        Console.WriteLine("Mapowanie dysku Z:");
        if (!MapDrive(driveZ, networkPathZ, userZ, passwordZ))
        {
            Console.WriteLine($"Nie udało się zmapować dysku {driveZ}.");
            return;
        }

        Console.WriteLine("Operacja zakończona.");
    }

    // Funkcja rozłączająca dysk
    private static void DisconnectDrive(string driveLetter)
    {
        int result = WNetCancelConnection2(driveLetter, 0, true);
        if (result == 0)
        {
            Console.WriteLine($"Dysk {driveLetter} rozłączony.");
        }
        else
        {
            Console.WriteLine($"Dysk {driveLetter} nie jest podłączony lub nie udało się go rozłączyć.");
        }
    }

    // Funkcja mapująca dysk
    private static bool MapDrive(string driveLetter, string networkPath, string username, string password)
    {
        ProcessStartInfo psi = new ProcessStartInfo("net", $"use {driveLetter} {networkPath} /user:{username} {password}")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using (Process process = Process.Start(psi))
            {
                process.WaitForExit();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                if (process.ExitCode == 0)
                {
                    Console.WriteLine($"Dysk {driveLetter} został pomyślnie zmapowany.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Błąd mapowania dysku {driveLetter}: {error}");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Wystąpił błąd podczas mapowania dysku {driveLetter}: {ex.Message}");
            return false;
        }
    }
}
