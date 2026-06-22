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
            Environment.SetEnvironmentVariable("EPPlusLicenseContext", "NonCommercial");
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

                string inputLower = input.ToLower();

                //Пока только для Coupons
                if (inputLower == "add") { Add(); continue; }
                if (inputLower == "del") { Delete(); continue; }
                if (inputLower == "test") { Test(); continue; }
                if (inputLower == "exit") break;

                string[] parts = input.Split(' ', 2);
                Console.ReadKey();
                if (parts.Length >= 2)
                {
                    string action = parts[0];
                    string tableName = parts[1];

                    if (action == "view")
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
                    // string sql = "INSERT INTO Coupons (Nom, Sum, Transmitted) VALUES (@nom, @sum, 0)";
                    string sql = "INSERT INTO Coupons (Nom, Sum) VALUES (@nom, @sum)";
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
        private static void Delete()
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

        // ТЕСТ
        private static void Test()
        {
            Console.Clear();
            Console.WriteLine("=== ТЕСТ ПРОВЕРКИ ЦЕЛОСТНОСТИ СТРУКТУРЫ БД ===");

            string[] tablesToCheck = { "Coupons", "Goods", "GoodsPrices", "Goodsleftovers", "GChecks", "Shifts" };

            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[OK] Соединение с файлом базы данных успешно установлено.\n");
                    Console.ResetColor();

                    foreach (var table in tablesToCheck)
                    {
                        string sql = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{table}';";
                        using (var cmd = new SQLiteCommand(sql, conn))
                        {
                            var res = cmd.ExecuteScalar();
                            if (res != null)
                            {
                                Console.Write($"Проверка таблицы {table,-16} -> ");
                                Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("[СУЩЕСТВУЕТ]"); Console.ResetColor();
                            }
                            else
                            {
                                Console.Write($"Проверка таблицы {table,-16} -> ");
                                Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("[ОТСУТСТВУЕТ!]"); Console.ResetColor();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[КРИТИЧЕСКАЯ ОШИБКА ТЕСТА]: {ex.Message}");
                Console.ResetColor();
            }
            Console.WriteLine("\nТестирование завершено. Нажмите любую клавишу...");
            Console.ReadKey();
        }

        // ПРОСМОТР ЛЮБОЙ ТАБЛИЦЫ 
        private static void ViewTableUniversal(string tableName)
        {
            Console.Clear();
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    // Параметризация имени таблицы в чистом SQL невозможна, поэтому экранируем имя для защиты от SQL-инъекций
                    string sql = $"SELECT * FROM [{tableName}]";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    using (var adapter = new SQLiteDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        if (dt.Rows.Count == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"Таблица '{tableName}' пуста или не существует.");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"--- СОДЕРЖИМОЕ ТАБЛИЦЫ: {tableName.ToUpper()} (Строк: {dt.Rows.Count}) ---");
                            Console.ResetColor();

                            // Выводим шапку таблицы на основе структуры БД
                            foreach (DataColumn column in dt.Columns)
                            {
                                Console.Write($"{column.ColumnName,-15}\t");
                            }
                            Console.WriteLine("\n" + new string('-', dt.Columns.Count * 20));

                            // Выводим данные строк
                            foreach (DataRow row in dt.Rows)
                            {
                                foreach (var item in row.ItemArray)
                                {
                                    // На всякий случай обрезаем длинные строки для консоли, чтобы не ломать разметку
                                    // (В дальнейшем можно отредактировать ещё)
                                    string val = item?.ToString() ?? "NULL";
                                    if (val.Length > 14) val = val.Substring(0, 11) + "...";
                                    Console.Write($"{val,-15}\t");
                                }
                                Console.WriteLine();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка доступа к таблице '{tableName}': {ex.Message}");
                Console.ResetColor();
            }
            Console.WriteLine("\nНажмите любую клавишу...");
            Console.ReadKey();
        }

        // ЭКСПОРТ В EXCEL
        private static void ExportToExcelUniversal(string tableName)
        {
            Console.Clear();
            // Сохраняем в CSV, который Excel откроет по умолчанию
            string fileName = $"{tableName}_export.csv";

            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    string sql = $"SELECT * FROM [{tableName}]";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    using (var adapter = new SQLiteDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        if (dt.Rows.Count == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"Таблица '{tableName}' пуста или не существует. Нечего экспортировать.");
                            Console.ResetColor();
                            Console.ReadKey();
                            return;
                        }

                        // Используем StreamWriter для записи файла
                        // чтобы Excel сразу правильно понял русские буквы
                        using (var writer = new StreamWriter(fileName, false, System.Text.Encoding.UTF8))
                        {
                            // Записываем заголовки колонок. В качестве разделителя используем точку с запятой (стандарт для Excel)
                            List<string> columns = new List<string>();
                            foreach (DataColumn column in dt.Columns)
                            {
                                columns.Add(column.ColumnName);
                            }
                            writer.WriteLine(string.Join(";", columns));

                            // Записываем строки с данными
                            foreach (DataRow row in dt.Rows)
                            {
                                List<string> fields = new List<string>();
                                foreach (var item in row.ItemArray)
                                {
                                    string field = item?.ToString() ?? "";
                                    // Если внутри текста есть точка с запятой или кавычки, экранируем их
                                    if (field.Contains(";") || field.Contains("\"") || field.Contains("\n"))
                                    {
                                        field = "\"" + field.Replace("\"", "\"\"") + "\"";
                                    }
                                    fields.Add(field);
                                }
                                writer.WriteLine(string.Join(";", fields));
                            }
                        }

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Успешно! Таблица '{tableName}' экспортирована в файл: {fileName}");
                        Console.ResetColor();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка при экспорте таблицы '{tableName}': {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\nНажмите любую клавишу...");
            Console.ReadKey();
        }
    }
}