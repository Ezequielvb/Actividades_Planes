using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ActividadesCiudad.Services;
using Microsoft.Data.Sqlite;

namespace ActividadesCiudad;

public partial class MainWindow : Window
{
    private readonly DatabaseService db = new DatabaseService();

    public MainWindow()
    {
        InitializeComponent();
        InicializarBD();
        CargarDatos();
        ActualizarEstado("Listo");
    }

    private void InicializarBD()
    {
        try
        {
            // Buscar BD en la raíz del proyecto (subir 3 niveles desde bin/Debug/net9.0-windows)
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            string dbPath;
            
            if (exeDir.Contains("bin") && (exeDir.Contains("Debug") || exeDir.Contains("Release")))
            {
                var dir = new DirectoryInfo(exeDir);
                for (int i = 0; i < 3 && dir.Parent != null; i++)
                {
                    dir = dir.Parent;
                }
                dbPath = Path.Combine(dir.FullName, "actividades.db");
            }
            else
            {
                dbPath = Path.Combine(exeDir, "actividades.db");
            }
            
            using var conn = new SqliteConnection($"Data Source={dbPath};");
            conn.Open();
            
            using var cmd = new SqliteCommand(@"
                CREATE TABLE IF NOT EXISTS ciudad (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    nombre TEXT NOT NULL,
                    poblacion INTEGER,
                    pais TEXT,
                    fechaFundacion TEXT
                );
                CREATE TABLE IF NOT EXISTS actividad (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    nombre TEXT NOT NULL,
                    precio REAL,
                    duracion INTEGER,
                    tipo TEXT,
                    estado TEXT
                );
                CREATE TABLE IF NOT EXISTS actividadciudad (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ciudadid INTEGER NOT NULL,
                    actividadid INTEGER NOT NULL,
                    FOREIGN KEY (ciudadid) REFERENCES ciudad(id) ON DELETE CASCADE,
                    FOREIGN KEY (actividadid) REFERENCES actividad(id) ON DELETE CASCADE
                );
            ", conn);
            cmd.ExecuteNonQuery();

            // Migración: agregar columnas nuevas si la BD ya existía sin ellas
            try
            {
                using var alterCmd = new SqliteCommand(@"
                    ALTER TABLE ciudad ADD COLUMN poblacion INTEGER;
                    ALTER TABLE ciudad ADD COLUMN pais TEXT;
                    ALTER TABLE ciudad ADD COLUMN fechaFundacion TEXT;
                ", conn);
                alterCmd.ExecuteNonQuery();
            }
            catch { }

            try
            {
                using var alterCmd2 = new SqliteCommand(@"
                    ALTER TABLE actividad ADD COLUMN precio REAL;
                    ALTER TABLE actividad ADD COLUMN duracion INTEGER;
                    ALTER TABLE actividad ADD COLUMN tipo TEXT;
                    ALTER TABLE actividad ADD COLUMN estado TEXT;
                ", conn);
                alterCmd2.ExecuteNonQuery();
            }
            catch { }

            using var countCmd = new SqliteCommand("SELECT COUNT(*) FROM ciudad", conn);
            if (Convert.ToInt64(countCmd.ExecuteScalar()) == 0)
            {
                using var insertCmd = new SqliteCommand(@"
                    INSERT INTO ciudad (nombre, poblacion, pais, fechaFundacion) VALUES ('Madrid', 3223000, 'España', '1085');
                    INSERT INTO ciudad (nombre, poblacion, pais, fechaFundacion) VALUES ('Barcelona', 1636762, 'España', '15 a.C.');
                    INSERT INTO ciudad (nombre, poblacion, pais, fechaFundacion) VALUES ('Valencia', 789744, 'España', '138 a.C.');
                    INSERT INTO actividad (nombre, precio, duracion, tipo, estado) VALUES ('Museo', 12.50, 120, 'Cultural', 'Disponible');
                    INSERT INTO actividad (nombre, precio, duracion, tipo, estado) VALUES ('Parque', 0.00, 60, 'Recreativo', 'Disponible');
                    INSERT INTO actividad (nombre, precio, duracion, tipo, estado) VALUES ('Teatro', 25.00, 90, 'Cultural', 'Disponible');
                    INSERT INTO actividad (nombre, precio, duracion, tipo, estado) VALUES ('Restaurante', 45.00, 90, 'Gastronomía', 'Disponible');
                    INSERT INTO actividad (nombre, precio, duracion, tipo, estado) VALUES ('Monumento', 5.00, 30, 'Cultural', 'Disponible');
                    INSERT INTO actividad (nombre, precio, duracion, tipo, estado) VALUES ('Playa', 0.00, 180, 'Recreativo', 'Disponible');
                    INSERT INTO actividadciudad (ciudadid, actividadid) VALUES (1, 1);
                    INSERT INTO actividadciudad (ciudadid, actividadid) VALUES (1, 2);
                    INSERT INTO actividadciudad (ciudadid, actividadid) VALUES (2, 6);
                    INSERT INTO actividadciudad (ciudadid, actividadid) VALUES (2, 3);
                ", conn);
                insertCmd.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al inicializar BD:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            ActualizarEstado("Error al inicializar la base de datos");
        }
    }

    private void CargarDatos()
    {
        try
        {
            var ciudades = db.ObtenerCiudades();
            ListaCiudades.ItemsSource = ciudades.DefaultView;

            CargarActividades();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar datos:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CargarActividades()
    {
        try
        {
            var actividades = db.ObtenerActividades();
            if (ListaActividades != null)
            {
                ListaActividades.ItemsSource = actividades.DefaultView;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar actividades:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ActualizarEstado(string mensaje)
    {
        txtEstado.Text = mensaje;
    }

    private void ListaCiudades_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListaCiudades.SelectedItem is DataRowView row)
        {
            int ciudadId = Convert.ToInt32(row["id"]);
            ListaActividadesAsociadas.ItemsSource =
                db.ObtenerActividadesCiudad(ciudadId).DefaultView;
            txtCiudad.Text = row["nombre"]?.ToString() ?? string.Empty;
            txtPoblacion.Text = row["poblacion"]?.ToString() ?? string.Empty;
            txtPais.Text = row["pais"]?.ToString() ?? string.Empty;
            txtFechaFundacion.Text = row["fechaFundacion"]?.ToString() ?? string.Empty;
            ActualizarEstado($"Ciudad seleccionada: {row["nombre"]}");
        }
        else
        {
            ListaActividadesAsociadas.ItemsSource = null;
            txtCiudad.Clear();
            txtPoblacion.Clear();
            txtPais.Clear();
            txtFechaFundacion.Clear();
        }
    }

    private void ListaActividades_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListaActividades.SelectedItem is DataRowView row)
        {
            txtActividad.Text = row["nombre"]?.ToString() ?? string.Empty;
            txtPrecio.Text = row["precio"]?.ToString() ?? string.Empty;
            txtDuracion.Text = row["duracion"]?.ToString() ?? string.Empty;
            
            // Sincronizar ComboBox con el valor de la BD
            string? tipo = row["tipo"]?.ToString();
            foreach (ComboBoxItem item in cmbTipo.Items)
            {
                if (item.Content.ToString() == tipo)
                {
                    cmbTipo.SelectedItem = item;
                    break;
                }
            }
            
            string? estado = row["estado"]?.ToString();
            foreach (ComboBoxItem item in cmbEstado.Items)
            {
                if (item.Content.ToString() == estado)
                {
                    cmbEstado.SelectedItem = item;
                    break;
                }
            }
            
            ActualizarEstado($"Actividad seleccionada: {row["nombre"]}");
        }
        else
        {
            txtActividad.Clear();
            txtPrecio.Clear();
            txtDuracion.Clear();
            cmbTipo.SelectedIndex = -1;
            cmbEstado.SelectedIndex = -1;
        }
    }


    private void AgregarCiudad_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtCiudad.Text))
        {
            MessageBox.Show("Por favor, ingresa un nombre de ciudad.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            string nombreCiudad = txtCiudad.Text.Trim();
            // Campos opcionales: validar y convertir solo si tienen valor
            int? poblacion = null;
            if (!string.IsNullOrWhiteSpace(txtPoblacion.Text) && int.TryParse(txtPoblacion.Text, out int pop))
                poblacion = pop;
            
            string? pais = string.IsNullOrWhiteSpace(txtPais.Text) ? null : txtPais.Text.Trim();
            string? fechaFundacion = string.IsNullOrWhiteSpace(txtFechaFundacion.Text) ? null : txtFechaFundacion.Text.Trim();
            
            db.EjecutarComando(
                "INSERT INTO ciudad (nombre, poblacion, pais, fechaFundacion) VALUES (@nombre, @poblacion, @pais, @fechaFundacion)",
                ("@nombre", nombreCiudad),
                ("@poblacion", poblacion ?? (object)DBNull.Value),
                ("@pais", pais ?? (object)DBNull.Value),
                ("@fechaFundacion", fechaFundacion ?? (object)DBNull.Value)
            );
            CargarDatos();
            txtCiudad.Clear();
            txtPoblacion.Clear();
            txtPais.Clear();
            txtFechaFundacion.Clear();
            ActualizarEstado($"Ciudad '{nombreCiudad}' agregada correctamente");
            MessageBox.Show("Ciudad agregada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al agregar ciudad:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            ActualizarEstado("Error al agregar ciudad");
        }
    }

    private void ActualizarCiudad_Click(object sender, RoutedEventArgs e)
    {
        if (ListaCiudades.SelectedValue == null)
        {
            MessageBox.Show("Selecciona una ciudad para actualizar.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(txtCiudad.Text))
        {
            MessageBox.Show("Por favor, ingresa un nombre de ciudad.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            int ciudadId = Convert.ToInt32(ListaCiudades.SelectedValue);
            string nombreAnterior = ((DataRowView)ListaCiudades.SelectedItem)?["nombre"]?.ToString() ?? string.Empty;

            int? poblacion = null;
            if (!string.IsNullOrWhiteSpace(txtPoblacion.Text) && int.TryParse(txtPoblacion.Text, out int pop))
                poblacion = pop;
            
            string? pais = string.IsNullOrWhiteSpace(txtPais.Text) ? null : txtPais.Text.Trim();
            string? fechaFundacion = string.IsNullOrWhiteSpace(txtFechaFundacion.Text) ? null : txtFechaFundacion.Text.Trim();

            db.EjecutarComando(
                "UPDATE ciudad SET nombre = @nombre, poblacion = @poblacion, pais = @pais, fechaFundacion = @fechaFundacion WHERE id = @id",
                ("@nombre", txtCiudad.Text.Trim()),
                ("@poblacion", poblacion ?? (object)DBNull.Value),
                ("@pais", pais ?? (object)DBNull.Value),
                ("@fechaFundacion", fechaFundacion ?? (object)DBNull.Value),
                ("@id", ciudadId)
            );

            CargarDatos();
            ActualizarEstado($"Ciudad '{nombreAnterior}' actualizada a '{txtCiudad.Text.Trim()}'");
            MessageBox.Show("Ciudad actualizada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al actualizar ciudad:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            ActualizarEstado("Error al actualizar ciudad");
        }
    }

    private void EliminarCiudad_Click(object sender, RoutedEventArgs e)
    {
        if (ListaCiudades.SelectedValue == null)
        {
            MessageBox.Show("Selecciona primero una ciudad.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var resultado = MessageBox.Show(
            "¿Estás seguro de que deseas eliminar esta ciudad?\n\nEsto también eliminará todas sus actividades asociadas.",
            "Confirmar eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (resultado != MessageBoxResult.Yes)
            return;

        try
        {
            int ciudadId = Convert.ToInt32(ListaCiudades.SelectedValue);
            string nombreCiudad = ((DataRowView)ListaCiudades.SelectedItem)?["nombre"]?.ToString() ?? string.Empty;

            db.EjecutarComando(
                "DELETE FROM ciudad WHERE id = @id",
                ("@id", ciudadId)
            );

            CargarDatos();
            ListaActividadesAsociadas.ItemsSource = null;
            txtCiudad.Clear();
            txtPoblacion.Clear();
            txtPais.Clear();
            txtFechaFundacion.Clear();
            ActualizarEstado($"Ciudad '{nombreCiudad}' eliminada correctamente");
            MessageBox.Show("Ciudad eliminada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al eliminar ciudad:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            ActualizarEstado("Error al eliminar ciudad");
        }
    }

    private void AgregarActividad_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtActividad.Text))
        {
            MessageBox.Show("Por favor, ingresa un nombre de actividad.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            string nombreActividad = txtActividad.Text.Trim();
            
            // Validar y convertir campos numéricos opcionales
            double? precio = null;
            if (!string.IsNullOrWhiteSpace(txtPrecio.Text) && double.TryParse(txtPrecio.Text, out double pr))
                precio = pr;
            
            int? duracion = null;
            if (!string.IsNullOrWhiteSpace(txtDuracion.Text) && int.TryParse(txtDuracion.Text, out int dur))
                duracion = dur;
            
            // Obtener valores de ComboBox (pueden ser null si no hay selección)
            string? tipo = (cmbTipo.SelectedItem as ComboBoxItem)?.Content?.ToString();
            string? estado = (cmbEstado.SelectedItem as ComboBoxItem)?.Content?.ToString();
            
            db.EjecutarComando(
                "INSERT INTO actividad (nombre, precio, duracion, tipo, estado) VALUES (@nombre, @precio, @duracion, @tipo, @estado)",
                ("@nombre", nombreActividad),
                ("@precio", precio ?? (object)DBNull.Value),
                ("@duracion", duracion ?? (object)DBNull.Value),
                ("@tipo", tipo ?? (object)DBNull.Value),
                ("@estado", estado ?? (object)DBNull.Value)
            );
            
            if (txtBusqueda != null) txtBusqueda.Clear();
            if (cmbFiltroTipo != null) cmbFiltroTipo.SelectedIndex = 0;
            if (cmbFiltroEstado != null) cmbFiltroEstado.SelectedIndex = 0;
            
            CargarActividades();
            
            txtActividad.Clear();
            txtPrecio.Clear();
            txtDuracion.Clear();
            if (cmbTipo != null) cmbTipo.SelectedIndex = -1;
            if (cmbEstado != null) cmbEstado.SelectedIndex = -1;
            
            ActualizarEstado($"Actividad '{nombreActividad}' agregada correctamente");
            MessageBox.Show("Actividad agregada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al agregar actividad:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            ActualizarEstado("Error al agregar actividad");
        }
    }

    private void ActualizarActividad_Click(object sender, RoutedEventArgs e)
    {
        if (ListaActividades.SelectedValue == null)
        {
            MessageBox.Show("Selecciona una actividad para actualizar.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(txtActividad.Text))
        {
            MessageBox.Show("Por favor, ingresa un nombre de actividad.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            int actividadId = Convert.ToInt32(ListaActividades.SelectedValue);
            string nombreAnterior = ((DataRowView)ListaActividades.SelectedItem)?["nombre"]?.ToString() ?? string.Empty;

            double? precio = null;
            if (!string.IsNullOrWhiteSpace(txtPrecio.Text) && double.TryParse(txtPrecio.Text, out double pr))
                precio = pr;
            
            int? duracion = null;
            if (!string.IsNullOrWhiteSpace(txtDuracion.Text) && int.TryParse(txtDuracion.Text, out int dur))
                duracion = dur;
            
            string? tipo = (cmbTipo.SelectedItem as ComboBoxItem)?.Content?.ToString();
            string? estado = (cmbEstado.SelectedItem as ComboBoxItem)?.Content?.ToString();

            db.EjecutarComando(
                "UPDATE actividad SET nombre = @nombre, precio = @precio, duracion = @duracion, tipo = @tipo, estado = @estado WHERE id = @id",
                ("@nombre", txtActividad.Text.Trim()),
                ("@precio", precio ?? (object)DBNull.Value),
                ("@duracion", duracion ?? (object)DBNull.Value),
                ("@tipo", tipo ?? (object)DBNull.Value),
                ("@estado", estado ?? (object)DBNull.Value),
                ("@id", actividadId)
            );

            AplicarFiltros();
            ActualizarEstado($"Actividad '{nombreAnterior}' actualizada a '{txtActividad.Text.Trim()}'");
            MessageBox.Show("Actividad actualizada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al actualizar actividad:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            ActualizarEstado("Error al actualizar actividad");
        }
    }

    private void EliminarActividad_Click(object sender, RoutedEventArgs e)
    {
        if (ListaActividades.SelectedValue == null)
        {
            MessageBox.Show("Selecciona primero una actividad.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var resultado = MessageBox.Show(
            "¿Estás seguro de que deseas eliminar esta actividad?\n\nEsto también eliminará todas sus asociaciones con ciudades.",
            "Confirmar eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (resultado != MessageBoxResult.Yes)
            return;

        try
        {
            int actividadId = Convert.ToInt32(ListaActividades.SelectedValue);
            string nombreActividad = ((DataRowView)ListaActividades.SelectedItem)?["nombre"]?.ToString() ?? string.Empty;

            db.EjecutarComando(
                "DELETE FROM actividad WHERE id = @id",
                ("@id", actividadId)
            );

            AplicarFiltros();
            
            txtActividad.Clear();
            txtPrecio.Clear();
            txtDuracion.Clear();
            if (cmbTipo != null) cmbTipo.SelectedIndex = -1;
            if (cmbEstado != null) cmbEstado.SelectedIndex = -1;
            
            if (ListaCiudades.SelectedValue != null)
            {
                int ciudadId = Convert.ToInt32(ListaCiudades.SelectedValue);
                ListaActividadesAsociadas.ItemsSource = db.ObtenerActividadesCiudad(ciudadId).DefaultView;
            }

            ActualizarEstado($"Actividad '{nombreActividad}' eliminada correctamente");
            MessageBox.Show("Actividad eliminada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al eliminar actividad:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            ActualizarEstado("Error al eliminar actividad");
        }
    }

    private void AgregarActividadCiudad_Click(object sender, RoutedEventArgs e)
    {
        if (ListaCiudades.SelectedValue == null || ListaActividades.SelectedValue == null)
        {
            MessageBox.Show("Selecciona una ciudad y una actividad para asociar.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        int ciudadId = Convert.ToInt32(ListaCiudades.SelectedValue);
        int actividadId = Convert.ToInt32(ListaActividades.SelectedValue);

        try
        {
            // Verificar que la actividad no esté ya asociada (evitar duplicados)
            var actividadesCiudad = db.ObtenerActividadesCiudad(ciudadId);
            foreach (DataRowView row in actividadesCiudad.DefaultView)
            {
                if (Convert.ToInt32(row["id"]) == actividadId)
                {
                    MessageBox.Show("Esta actividad ya está asociada a la ciudad seleccionada.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            db.EjecutarComando(
                "INSERT INTO actividadciudad (ciudadid, actividadid) VALUES (@ciudadid, @actividadid)",
                ("@ciudadid", ciudadId),
                ("@actividadid", actividadId)
            );

            ListaActividadesAsociadas.ItemsSource = db.ObtenerActividadesCiudad(ciudadId).DefaultView;
            ActualizarEstado("Actividad asociada a la ciudad correctamente");
            MessageBox.Show("Actividad asociada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al agregar actividad:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            ActualizarEstado("Error al asociar actividad");
        }
    }

    private void QuitarActividad_Click(object sender, RoutedEventArgs e)
    {
        if (ListaCiudades.SelectedValue == null || ListaActividadesAsociadas.SelectedValue == null)
        {
            MessageBox.Show("Selecciona una ciudad y una actividad asociada para quitar.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        int ciudadId = Convert.ToInt32(ListaCiudades.SelectedValue);
        int actividadId = Convert.ToInt32(ListaActividadesAsociadas.SelectedValue);

        try
        {
            db.EjecutarComando(
                "DELETE FROM actividadciudad WHERE ciudadid = @ciudadid AND actividadid = @actividadid",
                ("@ciudadid", ciudadId),
                ("@actividadid", actividadId)
            );

            ListaActividadesAsociadas.ItemsSource = db.ObtenerActividadesCiudad(ciudadId).DefaultView;
            ActualizarEstado("Actividad desasociada de la ciudad correctamente");
            MessageBox.Show("Actividad desasociada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al quitar actividad:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            ActualizarEstado("Error al desasociar actividad");
        }
    }

    private void txtBusqueda_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (ListaActividades != null && txtBusqueda != null)
            AplicarFiltros();
    }

    private void cmbFiltroTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListaActividades != null && cmbFiltroTipo != null)
            AplicarFiltros();
    }

    private void cmbFiltroEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListaActividades != null && cmbFiltroEstado != null)
            AplicarFiltros();
    }

    private void AplicarFiltros()
    {
        if (ListaActividades == null || txtBusqueda == null || cmbFiltroTipo == null || cmbFiltroEstado == null)
            return;

        try
        {
            DataTable todasActividades = db.ObtenerActividades();
            DataTable resultado = todasActividades.Clone();
            
            string busqueda = txtBusqueda.Text?.Trim() ?? string.Empty;
            string tipoFiltro = (cmbFiltroTipo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todos";
            string estadoFiltro = (cmbFiltroEstado.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todos";

            // Filtrar fila por fila: debe cumplir búsqueda Y tipo Y estado
            foreach (DataRow row in todasActividades.Rows)
            {
                bool cumpleBusqueda = true;
                bool cumpleTipo = true;
                bool cumpleEstado = true;

                // Búsqueda: busca en nombre, tipo o estado (case-insensitive)
                if (!string.IsNullOrWhiteSpace(busqueda))
                {
                    string busquedaLower = busqueda.ToLower();
                    string nombre = (row["nombre"]?.ToString() ?? "").ToLower();
                    string tipo = (row["tipo"]?.ToString() ?? "").ToLower();
                    string estado = (row["estado"]?.ToString() ?? "").ToLower();
                    
                    cumpleBusqueda = nombre.Contains(busquedaLower) || 
                                    tipo.Contains(busquedaLower) || 
                                    estado.Contains(busquedaLower);
                }

                // Filtro por tipo: exacto
                if (tipoFiltro != "Todos")
                {
                    string tipoRow = row["tipo"]?.ToString() ?? "";
                    cumpleTipo = tipoRow == tipoFiltro;
                }

                // Filtro por estado: exacto
                if (estadoFiltro != "Todos")
                {
                    string estadoRow = row["estado"]?.ToString() ?? "";
                    cumpleEstado = estadoRow == estadoFiltro;
                }

                // Si cumple todos los criterios, agregar al resultado
                if (cumpleBusqueda && cumpleTipo && cumpleEstado)
                {
                    resultado.ImportRow(row);
                }
            }

            ListaActividades.ItemsSource = resultado.DefaultView;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al aplicar filtros: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            try
            {
                CargarActividades();
            }
            catch { }
        }
    }
}
