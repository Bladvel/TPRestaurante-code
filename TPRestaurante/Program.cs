using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TPRestaurante
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            if (!DatabaseExists())
            {
                Console.WriteLine("Iniciando configuración de la base de datos...");
                InitializeDatabase();
            }



            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMDI());
        }


        private static bool DatabaseExists()
        {
            string connectionString = "Server=localhost\\SQLEXPRESS;Database=SISFOOD;Integrated Security=True;";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return true; // La conexión fue exitosa, la base de datos existe
                }
            }
            catch (SqlException)
            {
                return false; // La conexión falló, la base de datos no existe
            }
        }


        public static void InitializeDatabase()
        {
            string masterConnectionString = $"Server={Environment.MachineName}\\SQLEXPRESS;Database=master;Integrated Security=True;";
            string sisfoodConnectionString = $"Server={Environment.MachineName}\\SQLEXPRESS;Database=SISFOOD;Integrated Security=True;";
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string scriptPath = Path.Combine(appDirectory, "Resources", "scriptTpRestaurante.sql");

            if (File.Exists(scriptPath))
            {
                // Leer el contenido completo del archivo SQL sin modificarlo
                string script = File.ReadAllText(scriptPath);

                // Dividir el script en bloques separados por 'GO'
                string[] commandTexts = script.Split(new[] { "\nGO\n", "\rGO\r", "\r\nGO\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                try
                {
                    // Paso 1: Crear la base de datos 'SISFOOD' usando la conexión a 'master'
                    using (SqlConnection connection = new SqlConnection(masterConnectionString))
                    {
                        connection.Open();
                        string createDatabaseCommand = "IF DB_ID('SISFOOD') IS NULL CREATE DATABASE SISFOOD;";

                        using (SqlCommand command = new SqlCommand(createDatabaseCommand, connection))
                        {
                            command.ExecuteNonQuery();
                            Console.WriteLine("Base de datos 'SISFOOD' creada exitosamente.");
                        }
                    }

                    // Paso 2: Conectar a la base de datos 'SISFOOD' y ejecutar el resto del script en bloques separados por 'GO'
                    using (SqlConnection connection = new SqlConnection(sisfoodConnectionString))
                    {
                        connection.Open();

                        foreach (string commandText in commandTexts)
                        {
                            if (!string.IsNullOrWhiteSpace(commandText))
                            {
                                try
                                {
                                    using (SqlCommand command = new SqlCommand(commandText, connection))
                                    {
                                        command.ExecuteNonQuery();
                                        Console.WriteLine("Lote ejecutado exitosamente.");
                                    }
                                }
                                catch (SqlException ex)
                                {
                                    Console.WriteLine("Error al ejecutar lote de SQL:");
                                    foreach (SqlError error in ex.Errors)
                                    {
                                        Console.WriteLine($"  - {error.Message} (Error Code: {error.Number})");
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al crear la base de datos o ejecutar el script: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Archivo SQL no encontrado.");
            }
        }






















    }










}
