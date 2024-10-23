using System;
using System.Globalization;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Enter the path to the journal log:");
        string _journalLog = Console.ReadLine();

        Console.WriteLine("Enter the path to the delivery log:");
        string _deliveryLog = Console.ReadLine();

        Console.WriteLine("Enter the path to save delivery order results:");
        string deliveryOrderPath = Console.ReadLine();

        Console.WriteLine("Enter the city district to filter by:");
        string cityDistrict = Console.ReadLine();

        Console.WriteLine("Enter the first delivery datetime (format: yyyy-MM-dd HH:mm:ss):");
        string datefirst = Console.ReadLine();
        DateTime firstDeliveryDateTime;
        DateTime.TryParseExact(datefirst, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out firstDeliveryDateTime);

        var ipAddresses = ReadLogFile(_journalLog);
        var filteredIPs = FilterIPAddresses(ipAddresses, firstDeliveryDateTime.AddMinutes(30), cityDistrict);

        WriteResults(deliveryOrderPath, filteredIPs);
        Log(_deliveryLog, "Фильтрация завершена. Результаты записаны в файл.");
    }

    static Dictionary<string, List<DateTime>> ReadLogFile(string filePath)
    {
        var ipDict = new Dictionary<string, List<DateTime>>();

        foreach (var line in File.ReadLines(filePath))
        {
            var parts = line.Split(':');
            if (parts.Length != 2 || !DateTime.TryParseExact(parts[1].Trim(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var accessTime))
            {
                continue; // Неверный формат, пропустить
            }

            var ip = parts[0].Trim();
            if (!ipDict.ContainsKey(ip))
            {
                ipDict[ip] = new List<DateTime>();
            }
            ipDict[ip].Add(accessTime);
        }

        return ipDict;
    }

    static Dictionary<string, int> FilterIPAddresses(Dictionary<string, List<DateTime>> ipAddresses, DateTime endTime, string district)
    {
        var filteredIPs = new Dictionary<string, int>();

        foreach (var kvp in ipAddresses)
        {
            var ip = kvp.Key;
            var accessTimes = kvp.Value.Where(at => at <= endTime).ToList();

            if (accessTimes.Count > 0)
            {
                if (ip.StartsWith(district))
                {
                    filteredIPs[ip] = accessTimes.Count;
                }
            }
        }

        return filteredIPs;
    }

    static void WriteResults(string filePath, Dictionary<string, int> results)
    {
        using (var writer = new StreamWriter(filePath))
        {
            foreach (var kvp in results)
            {
                writer.WriteLine($"{kvp.Key}: {kvp.Value}");
            }
        }
    }
   static void Log(string _deliveryLog, string message)
    {
    using (var writer = new StreamWriter(_deliveryLog, true))
    {
        writer.WriteLine($"{DateTime.Now}: {message}");
    }
    }
}