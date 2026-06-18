using System;
using System.IO;
using System.Data;
using System.Data.SQLite;
using OfficeOpenXml;

namespace UniversalDbManager
{
    class Program
    {
        private static string dbPath = "Db.db";
        static void Main(string[] args)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            LoadConfig();

            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"=== УНИВИРСАЛЬНЫЙ МЕНЕДЖЕР БД ===");
                Console.ResetColor();
                Console.WriteLine("Доступные команды:");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(" view [таблица] - Просмотр любой таблицы");
                Console.WriteLine(" excel [таблица] - Экспорт любой таблицы в Excel");
                Console.ResetColor();
                Console.WriteLine(" add - Добавить новую запись");
                Console.WriteLine(" del - Удалить запись по ID");
                Console.WriteLine(" test - Комплексная проверка структуры БД");
                Console.WriteLine(" exit - Выход");
                Console.Write("\nВведите команду: ");

                string input = Console.ReadLine()?.Trim().ToLower();
                if (string.IsNullOrEmpty(input)) continue;
                
                //Пока только для Coupons
                if (input == "add") { Add(); continue; }
                if (input == "del") { Delete(); continue; }
                if (input == "test") { Test(); continue; }
                if (input == "exit") break;

                string[] parts = input.Split(' ', 2);
                if (parts.Length == 2)
                {
                    string action = parts[0];
                    string tableName = parts[1];

                    if (action == "viwe")
                    {
                        ViewTableUniversal(tableName);
                        continue;
                    }

                    if (action == "excel")
                    {
                        ExportToExcelUniversal(tableName);
                        continue;
                    }

                }
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Неизвестная команда");
                Console.ResetColor();
                Console.ReadKey();  
            }
        }

        private static void LoadConfig()
        {
            string iniPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
            if (File.Exists(iniPath))
            {
                foreach (var line in File.ReadAllLines(iniPath))
                {
                    if (line.StartsWith("Path", StringComparison.OrdinalIgnoreCase) && line.Contains("="))
                    {
                        dbPath = line.Split('=')[1].Trim();
                    }
                }
            }
        }
    }
}