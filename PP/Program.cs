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
        private static SQLiteConnection GetConnection() => new SQLiteConnection($"Data Source={dbPath};Version=3;");

        // УПРАВЛЕНИЕ КУПОНАМИ - ДОБАВЛЕНИЕ
        private static void Add()
        {
            Console.Clear();
            Console.WriteLine("=== ДОБАВЛЕНИЕ КУПОНА ===");
            Console.Write("Введите номер купона (Nom): ");
            string nom = Console.ReadLine();
            Console.Write("Введите сумму (Sum): ");
            if (!double.TryParse(Console.ReadLine(), out double sum)) { Console.WriteLine("Ошибка ввода."); Console.ReadKey(); return; }

            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    string sql = "INSERT INTO Coupons (Nom, Sum, Transmitted) VALUES (@nom, @sum, 0)";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@nom", nom);
                        cmd.Parameters.AddWithValue("@sum", sum);
                        cmd.ExecuteNonQuery();
                    }
                }
                Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("Купон добавлен.");
            }
            catch (Exception ex) { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine(ex.Message); }
            Console.ResetColor(); Console.ReadKey();
        }

        // УПРАВЛЕНИЕ КУПОНАМИ - УДАЛЕНИЕ
        private static void DeleteCoupon()
        {
            Console.Clear();
            Console.WriteLine("=== УДАЛЕНИЕ КУПОНА ===");
            Console.Write("Введите ID купона: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) return;

            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    string sql = "DELETE FROM Coupons WHERE id = @id";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        int r = cmd.ExecuteNonQuery();
                        Console.WriteLine(r > 0 ? "Успешно удалено." : "ID не найден.");
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.ReadKey();
        }
    }
}