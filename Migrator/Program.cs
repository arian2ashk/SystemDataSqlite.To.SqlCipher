using System;
using System.Data.SQLite;
using System.IO;
using CommandLine;
using Microsoft.Data.Sqlite;

namespace Migrator
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser(with => with.EnableDashDash = true);
            parser.ParseArguments<Options>(args).WithParsed(options =>
            {
                Console.WriteLine($"Start decrypting {options.Path}");
                DecryptSystem(new SQLiteConnectionStringBuilder { DataSource = options.Path }, options.Password);
                Console.WriteLine($"System.Data.SQLite database is now decrypted");
                var newPassword = string.IsNullOrWhiteSpace(options.NewPassword)
                    ? options.Password
                    : options.NewPassword;
                EncryptToMicrosoft(new SqliteConnectionStringBuilder { DataSource = options.Path }, newPassword);
                Console.WriteLine($"Database is now encrypted with SqlCipher");
            });
        }

        private static void DecryptSystem(SQLiteConnectionStringBuilder connectionStringBuilder, string password)
        {
            var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = $"PRAGMA key = {password};";
            command.ExecuteNonQuery();

            using var command2 = connection.CreateCommand();
            command2.CommandText = "PRAGMA rekey = '';";
            command2.ExecuteNonQuery();
            connection.Close();
        }

        private static void EncryptToMicrosoft(SqliteConnectionStringBuilder connectionStringBuilder, string password)
        {
            var databasePath = connectionStringBuilder["Data Source"]?.ToString() ?? "";
            var databaseName = Path.GetFileNameWithoutExtension(databasePath);
            var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
            var databaseTemp = $"{databaseName}_Temp.db";
            File.Delete(databaseTemp);
            var query =
                @$"ATTACH DATABASE '{databaseTemp}' AS encrypted KEY '{password}'; SELECT sqlcipher_export('encrypted'); DETACH DATABASE encrypted;";

            connection.Open();
            using var cmd = new SqliteCommand(query, connection);
            cmd.ExecuteNonQuery();
            connection.Close();

            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }

            File.Move(databaseTemp, databasePath);
        }
    }
}
