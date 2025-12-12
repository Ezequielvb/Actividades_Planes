# Actividades por Ciudad - Proyecto Final UD2

## Tema del Proyecto

**Gestión de Actividades por Ciudad**: Aplicación WPF que permite gestionar ciudades y sus actividades turísticas, permitiendo asociar múltiples actividades a cada ciudad mediante una relación N:M.

## Descripción de las Tablas

### 1. Tabla `ciudad` (Tabla Principal)
- **id**: INTEGER PRIMARY KEY AUTOINCREMENT
- **nombre**: TEXT NOT NULL - Nombre de la ciudad
- **poblacion**: INTEGER - Población de la ciudad
- **pais**: TEXT - País donde se encuentra la ciudad
- **fechaFundacion**: TEXT - Fecha de fundación de la ciudad

### 2. Tabla `actividad` (Tabla Secundaria)
- **id**: INTEGER PRIMARY KEY AUTOINCREMENT
- **nombre**: TEXT NOT NULL - Nombre de la actividad
- **precio**: REAL - Precio de la actividad en euros
- **duracion**: INTEGER - Duración en minutos
- **tipo**: TEXT - Tipo de actividad (Cultural, Recreativo, Gastronomía)
- **estado**: TEXT - Estado de disponibilidad (Disponible, No Disponible)

### 3. Tabla `actividadciudad` (Tabla Intermedia N:M)
- **id**: INTEGER PRIMARY KEY AUTOINCREMENT
- **ciudadid**: INTEGER NOT NULL - Referencia a ciudad(id)
- **actividadid**: INTEGER NOT NULL - Referencia a actividad(id)
- **FOREign KEY**: Relaciones con CASCADE DELETE

## Relaciones entre Tablas

- **Ciudad ↔ Actividad**: Relación N:M (Muchas ciudades pueden tener muchas actividades)
- La tabla intermedia `actividadciudad` permite asociar múltiples actividades a cada ciudad
- Al eliminar una ciudad o actividad, se eliminan automáticamente las relaciones asociadas (CASCADE DELETE)

## Funcionalidades Implementadas

### CRUD Completo
- ✅ **Crear**: Permite agregar nuevas ciudades y actividades con todos sus campos
- ✅ **Leer**: Lista todas las ciudades y actividades, mostrando actividades asociadas por ciudad
- ✅ **Actualizar**: Modifica ciudades y actividades existentes
- ✅ **Eliminar**: Elimina ciudades y actividades con confirmación mediante MessageBox

### Listados Sincronizados
- ✅ Listado principal de ciudades (ListBox izquierdo)
- ✅ Listado de actividades asociadas a la ciudad seleccionada (ListBox central)
- ✅ Listado general de todas las actividades (ListBox derecho)
- ✅ Sincronización automática mediante evento `SelectionChanged`

### Filtros y Búsqueda
- ✅ **Búsqueda por texto**: Busca actividades por nombre, tipo o estado
- ✅ **Filtro por tipo**: Filtra actividades por tipo (Cultural, Recreativo, Gastronomía)
- ✅ **Filtro por estado**: Filtra actividades por estado (Disponible, No Disponible)
- ✅ Los filtros se pueden combinar para búsquedas más específicas

### Validaciones
- ✅ Validación de campos obligatorios (nombre de ciudad y actividad)
- ✅ Validación de tipos numéricos (población, precio, duración)
- ✅ Mensajes de confirmación antes de eliminar
- ✅ Mensajes informativos al realizar operaciones CRUD

### Consultas SQL con Parámetros
- ✅ Todas las consultas utilizan parámetros SQL (@nombre, @id, etc.)
- ✅ Protección contra inyección SQL
- ✅ Consultas optimizadas con JOIN para relaciones

## Diseño Visual

### Colores
- **Fondo principal**: #F5F5F5 (Gris claro)
- **Panel Ciudades**: LightBlue con borde SteelBlue
- **Panel Actividades Asociadas**: LightYellow con borde Orange
- **Panel Todas las Actividades**: LightGreen con borde Green
- **Botones**:
  - Verde para crear
  - Azul para actualizar
  - Rojo para eliminar
  - Naranja para agregar asociaciones

### Iconos
- ✅ Iconos Segoe MDL2 Assets en:
  - Título principal (ícono de ciudad)
  - Encabezados de secciones
  - Botones (crear, actualizar, eliminar)
  - Items de listas (ListBox)

### Distribución
- ✅ Grid con 3 columnas principales
- ✅ Márgenes consistentes (Margin="5" y Margin="10")
- ✅ Espaciado adecuado entre controles
- ✅ Altura de ventana: 750px, Ancho: 1200px

### Tipografía
- ✅ Título principal: FontSize="20", FontWeight="Bold"
- ✅ Encabezados de sección: FontSize="14", FontWeight="Bold"
- ✅ Campos de texto: FontSize="13"
- ✅ Texto de estado: FontSize="12"

### Experiencia de Usuario
- ✅ Tooltips informativos en todos los controles principales
- ✅ Mensajes claros mediante MessageBox
- ✅ Barra de estado en la parte inferior
- ✅ Interfaz intuitiva y fácil de usar

## Estructura del Proyecto

```
ActividadesCiudad/
├── Models/
│   ├── Ciudad.cs
│   └── Actividad.cs
├── Services/
│   └── DatabaseService.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── App.xaml
├── App.xaml.cs
├── actividades.db
└── README.md
```

## Instrucciones para Ejecutar

1. Abrir el proyecto en Visual Studio 2022 o superior
2. Asegurarse de tener .NET 9.0 instalado
3. Compilar el proyecto (F5 o Ctrl+F5)
4. La base de datos se creará automáticamente en la carpeta de salida
5. Si la base de datos ya existe, se cargarán los datos existentes

## Notas Importantes

- La base de datos SQLite se crea automáticamente si no existe
- Los datos de ejemplo se insertan solo si la base de datos está vacía
- Las migraciones de columnas se realizan automáticamente si faltan campos
- La aplicación valida que no se inserten registros duplicados en relaciones N:M

## Capturas de Pantalla

### Listado Principal
La aplicación muestra tres paneles:
- **Izquierda**: Lista de ciudades con campos de edición
- **Centro**: Actividades asociadas a la ciudad seleccionada
- **Derecha**: Todas las actividades con filtros y búsqueda

  <img width="1885" height="992" alt="image" src="https://github.com/user-attachments/assets/8761b817-637e-4a8b-876e-75d8592c4b9e" />


### Panel de Edición
Cada panel incluye:
- Campos de texto para nombre
- Campos numéricos (población, precio, duración)
- ComboBox para tipo y estado
- Botones CRUD con iconos
  <img width="599" height="324" alt="image" src="https://github.com/user-attachments/assets/f34d59d2-3932-4aff-983c-cc5ccbe46f90" />
  <img width="601" height="376" alt="image" src="https://github.com/user-attachments/assets/dd601778-d311-4ee8-93b9-56654687e2b0" />


### Filtrado y Búsqueda
- Barra de búsqueda en tiempo real
- ComboBox para filtrar por tipo
- ComboBox para filtrar por estado
- Los filtros se combinan automáticamente
  <img width="599" height="400" alt="image" src="https://github.com/user-attachments/assets/aa715429-4e5f-4e86-b730-208e515af3e9" />
  <img width="604" height="402" alt="image" src="https://github.com/user-attachments/assets/e6df3603-0017-406b-b9af-e1e69e6190a6" />
  <img width="596" height="393" alt="image" src="https://github.com/user-attachments/assets/a6f2b174-5636-4a2a-9d88-4db2c2290475" />



### CRUD en Funcionamiento
- Mensajes de confirmación al eliminar
- Mensajes de éxito al crear/actualizar
- Validaciones antes de guardar
- Actualización automática de listas

## Justificación del Diseño

### Colores
Los colores elegidos (azul, amarillo, verde) representan diferentes secciones de la aplicación y facilitan la identificación visual de cada panel. Los colores suaves no fatigan la vista y mantienen una apariencia profesional.

### Iconos
Los iconos Segoe MDL2 Assets proporcionan una interfaz moderna y consistente con el estilo de Windows. Ayudan a identificar rápidamente las acciones disponibles.

### Distribución
La distribución en 3 columnas permite ver simultáneamente:
- Las ciudades disponibles
- Las actividades de una ciudad específica
- Todas las actividades con opciones de filtrado

Esta organización facilita la gestión de relaciones N:M de manera intuitiva.

## Tecnologías Utilizadas

- **.NET 9.0**: Framework de desarrollo
- **WPF (Windows Presentation Foundation)**: Interfaz gráfica
- **SQLite**: Base de datos relacional
- **Microsoft.Data.Sqlite**: Proveedor de datos para SQLite
- **C#**: Lenguaje de programación

## Autor

Ezequiel Vargas Berrocal

