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
                Console.ForegroundColor= ConsoleColor.Yellow;
                Console.WriteLine(" view [таблица] - Просмотр любой таблицы");
                Console.WriteLine(" excel [таблица] - Экспорт любой таблицы в Excel");
                Console.ResetColor();
                Console.WriteLine(" add - Добавить новую запись");
                Console.WriteLine(" del - Удалить запись по ID");
                Console.WriteLine(" test - Комплексная проверка структуры БД");
                Console.WriteLine(" exit - Выход");
                Console.Write("\nВведите команду: ");
            }
    }
}
}