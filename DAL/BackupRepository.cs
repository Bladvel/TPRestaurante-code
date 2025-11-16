using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class BackupRepository
    {

        Access access = new Access();

        public int CreateBackup(string ruta)
        {
            List<SqlParameter> parameters = new List<SqlParameter>()
            {
                access.CreateParameter("@ruta", ruta)
            };

            access.Open();
            int resultado = access.WriteScalar("GENERAR_BACKUP", parameters);
            access.Close();

            return resultado;
        }

        public void RestoreBackup(string ruta)
        {
            //SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            //builder.DataSource = "DESKTOP-DAN";
            //builder.InitialCatalog = "TpRESTAURANTE";
            //builder.IntegratedSecurity = true;


            string connectionStringTemplate = ConfigurationManager.ConnectionStrings["Principal"].ConnectionString;

            // Reemplazar el marcador de posición con el nombre real de la máquina
            string strConn = connectionStringTemplate.Replace("{ServerName}", Environment.MachineName);

            using (SqlConnection conexion = new SqlConnection(strConn))
            {
                conexion.Open();

                SqlCommand cmd1 = new SqlCommand("ALTER DATABASE [SISFOOD] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", conexion);
                cmd1.ExecuteNonQuery();

                SqlCommand cmd2 = new SqlCommand("USE MASTER RESTORE DATABASE [SISFOOD] FROM DISK = N'" + ruta + @"' WITH FILE = 1, NOUNLOAD, REPLACE, STATS = 10;", conexion);
                cmd2.ExecuteNonQuery();

                SqlCommand cmd3 = new SqlCommand("ALTER DATABASE [SISFOOD] SET MULTI_USER", conexion);
                cmd3.ExecuteNonQuery();

                conexion.Close();
            }
        }
    }
}
