using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace BussinessErp.Helpers
{
    /// <summary>
    /// Centralized database connection management with auto-DB creation.
    /// </summary>
    public static class DatabaseHelper
    {
        private static readonly string _connString =
            @"Server=DESKTOP-PELPG2T\SQLEXPRESS;Database=BussinessDB;Trusted_Connection=True;TrustServerCertificate=True;";

        private static readonly string _masterConnString =
            @"Server=DESKTOP-PELPG2T\SQLEXPRESS;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";

        public static string ConnectionString => _connString;

        /// <summary>
        /// Creates a new open SqlConnection asynchronously.
        /// </summary>
        public static async Task<SqlConnection> GetConnectionAsync()
        {
            var conn = new SqlConnection(_connString);
            await conn.OpenAsync();
            return conn;
        }

        public static bool IsInitialized { get; private set; } = false;

        /// <summary>
        /// Ensures the database and schema exist. Call once at startup.
        /// </summary>
        public static async Task InitializeDatabaseAsync()
        {
            if (IsInitialized) return;

            try
            {
                // Fast path: If DB exists and has users, we consider it initialized
                if (await HasSeedDataAsync())
                {
                    IsInitialized = true;
                    AppLogger.Info("Database already initialized (Fast Path).");
                    return;
                }

                // Step 1: Create the database if it doesn't exist
                using (var masterConn = new SqlConnection(_masterConnString))
                {
                    await masterConn.OpenAsync();
                    using (var cmd = masterConn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'BussinessDB')
                            BEGIN
                                CREATE DATABASE [BussinessDB];
                            END";
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                AppLogger.Info("Database existence verified.");

                // Step 2: Run the setup SQL script
                string sqlDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SQL");
                string setupFile = Path.Combine(sqlDir, "DatabaseSetup.sql");
                string seedFile = Path.Combine(sqlDir, "SeedData.sql");

                if (File.Exists(setupFile))
                {
                    await ExecuteSqlFileAsync(setupFile);
                    AppLogger.Info("DatabaseSetup.sql executed successfully.");
                }

                // Step 3: Run seed data
                if (File.Exists(seedFile))
                {
                    await ExecuteSqlFileAsync(seedFile);
                    AppLogger.Info("SeedData.sql execution attempted.");
                }

                IsInitialized = true;
            }
            catch (Exception ex)
            {
                AppLogger.Error("Database initialization failed: " + ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// Checks if the database already has seed data.
        /// </summary>
        private static async Task<bool> HasSeedDataAsync()
        {
            try
            {
                using (var conn = await GetConnectionAsync())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM [dbo].[Users]";
                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result) > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Executes a SQL file, splitting on GO batches.
        /// </summary>
        private static async Task ExecuteSqlFileAsync(string filePath)
        {
            string script = File.ReadAllText(filePath);
            string[] batches = System.Text.RegularExpressions.Regex.Split(script, @"^\s*GO\s*$",
                System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            using (var conn = await GetConnectionAsync())
            {
                foreach (string batch in batches)
                {
                    string trimmed = batch.Trim();
                    if (string.IsNullOrEmpty(trimmed)) continue;

                    // Skip USE statements as we're already connected to the right DB
                    if (trimmed.StartsWith("USE ", StringComparison.OrdinalIgnoreCase)) continue;

                    try
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandTimeout = 120;
                            cmd.CommandText = trimmed;
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Warn($"SQL batch warning: {ex.Message}");
                        // Continue with next batch — some may fail on "IF NOT EXISTS" checks
                    }
                }
            }
        }

        /// <summary>
        /// Executes a non-query command asynchronously.
        /// </summary>
        public static async Task<int> ExecuteNonQueryAsync(string sql, params SqlParameter[] parameters)
        {
            using (var conn = await GetConnectionAsync())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Executes a scalar query asynchronously.
        /// </summary>
        public static async Task<object> ExecuteScalarAsync(string sql, params SqlParameter[] parameters)
        {
            using (var conn = await GetConnectionAsync())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                return await cmd.ExecuteScalarAsync();
            }
        }

        public static async Task BackupDatabaseAsync(string filePath)
        {
            string dbName = "BussinessDB";
            string sql = $@"BACKUP DATABASE [{dbName}] TO DISK = @path WITH FORMAT, MEDIANAME = 'Z_SQLServerBackups', NAME = 'Full Backup of {dbName}';";
            
            using (var conn = new SqlConnection(_masterConnString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@path", filePath);
                    cmd.CommandTimeout = 300; // 5 minutes for backup
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public static async Task RestoreDatabaseAsync(string filePath)
        {
            string dbName = "BussinessDB";
            // Kick users off and restore
            string sql = $@"
                USE [master];
                ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                RESTORE DATABASE [{dbName}] FROM DISK = @path WITH REPLACE;
                ALTER DATABASE [{dbName}] SET MULTI_USER;";

            using (var conn = new SqlConnection(_masterConnString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@path", filePath);
                    cmd.CommandTimeout = 300; // 5 minutes for restore
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
