using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;
using ActividadesCiudad.Models;

namespace ActividadesCiudad.Services;

public class DatabaseService
{
    private static string GetDatabasePath()
    {
        string exeDir = AppDomain.CurrentDomain.BaseDirectory;
        
        if (exeDir.Contains("bin") && (exeDir.Contains("Debug") || exeDir.Contains("Release")))
        {
            var dir = new DirectoryInfo(exeDir);
            for (int i = 0; i < 3 && dir.Parent != null; i++)
            {
                dir = dir.Parent;
            }
            return Path.Combine(dir.FullName, "actividades.db");
        }
        
        return Path.Combine(exeDir, "actividades.db");
    }
    
    private readonly string connectionString = $"Data Source={GetDatabasePath()};";

    public DataTable ObtenerCiudades()
    {
        return EjecutarConsulta("SELECT id, nombre, poblacion, pais, fechaFundacion FROM ciudad ORDER BY nombre");
    }

    public DataTable ObtenerActividades()
    {
        return EjecutarConsulta("SELECT id, nombre, precio, duracion, tipo, estado FROM actividad ORDER BY nombre");
    }

    public DataTable BuscarActividades(string textoBusqueda)
    {
        const string sql = "SELECT id, nombre, precio, duracion, tipo, estado FROM actividad " +
                          "WHERE nombre LIKE @busqueda OR tipo LIKE @busqueda OR estado LIKE @busqueda " +
                          "ORDER BY nombre";
        return EjecutarConsulta(sql, ("@busqueda", $"%{textoBusqueda}%"));
    }

    public DataTable FiltrarActividadesPorTipo(string tipo)
    {
        const string sql = "SELECT id, nombre, precio, duracion, tipo, estado FROM actividad " +
                          "WHERE tipo = @tipo ORDER BY nombre";
        return EjecutarConsulta(sql, ("@tipo", tipo));
    }

    public DataTable FiltrarActividadesPorEstado(string estado)
    {
        const string sql = "SELECT id, nombre, precio, duracion, tipo, estado FROM actividad " +
                          "WHERE estado = @estado ORDER BY nombre";
        return EjecutarConsulta(sql, ("@estado", estado));
    }

    public DataTable ObtenerActividadesCiudad(int ciudadId)
    {
        const string sql =
            "SELECT a.id, a.nombre " +
            "FROM actividad a " +
            "INNER JOIN actividadciudad ac ON a.id = ac.actividadid " +
            "WHERE ac.ciudadid = @ciudadid " +
            "ORDER BY a.id";
        return EjecutarConsulta(sql, ("@ciudadid", ciudadId));
    }

    public void EjecutarComando(string sql, params (string nombre, object? valor)[] parametros)
    {
        using var conn = new SqliteConnection(connectionString);
        conn.Open();
        using var cmd = new SqliteCommand(sql, conn);
        
        if (parametros != null)
        {
            foreach (var p in parametros)
            {
                cmd.Parameters.AddWithValue(p.nombre, p.valor ?? DBNull.Value);
            }
        }

        cmd.ExecuteNonQuery();
    }

    private DataTable EjecutarConsulta(string sql, params (string nombre, object? valor)[] parametros)
    {
        var dt = new DataTable();

        using var conn = new SqliteConnection(connectionString);
        conn.Open();
        using var cmd = new SqliteCommand(sql, conn);
        
        if (parametros != null)
        {
            foreach (var p in parametros)
            {
                cmd.Parameters.AddWithValue(p.nombre, p.valor ?? DBNull.Value);
            }
        }

        using var reader = cmd.ExecuteReader();
        
        if (reader.HasRows)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                dt.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
            }

            while (reader.Read())
            {
                var row = dt.NewRow();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[i] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
                }
                dt.Rows.Add(row);
            }
        }
        else
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                dt.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
            }
        }

        return dt;
    }
}

