# APObot Frontend

Interfaz web del sistema **APObot para farmacia**, desarrollada con Blazor y Tailwind CSS. El objetivo del proyecto es reproducir de forma fiel, clara y mantenible las pantallas del display táctil de APObot a partir de la documentación gráfica incluida en el repositorio.

> Estado: frontend en desarrollo. Actualmente utiliza datos de demostración y todavía no integra un backend ni una API real.

## Descripción

APObot es una interfaz orientada a la gestión visual de un sistema automatizado de farmacia. El proyecto está diseñado para una pantalla táctil 16:9 y prioriza la legibilidad, los controles grandes y una navegación sencilla para entornos de trabajo.

La aplicación permitirá representar progresivamente:

- Resumen general del sistema y su capacidad.
- Estado de cámaras y módulos.
- Últimas salidas y movimientos.
- Avisos, incidencias y prioridades.
- Consulta y gestión de stock.
- Detalle de productos.
- Flujos de dispensación, pedidos y carga.
- Estadísticas operativas.

## Alcance actual

- Proyecto centrado exclusivamente en la interfaz de usuario.
- Datos mock hasta que exista un contrato de API.
- Sin autenticación, persistencia ni integración backend por ahora.
- Diseño basado en los documentos de referencia disponibles en `resources/`.

Referencias visuales principales:

- `resources/APObot - Ficheros display software - parte 1/APObot - Interfaz display soft Zgz.pdf`
- `resources/APObot - Ficheros display software - parte 1/Logo, iconos y colores/APObot - Gama colores.pdf`

## Tecnologías

- **.NET 10**
- **Blazor Web App**
- **C#**
- **Razor Components**
- **Tailwind CSS 4**
- **HTML5 y CSS3**
- **Montserrat** como tipografía principal
- Assets SVG y PNG propios de APObot

## Paleta visual

| Uso | Color |
|---|---|
| Azul cian principal | `#3cb2dd` |
| Azul oscuro / navy | `#1f2144` |
| Coral | `#e1806f` |
| Fondo gris claro | `#ececec` |
| Gradiente azul | `#3cb2dd` → `#9bd4e6` |
| Gradiente coral | `#d86467` → `#f6b683` |

## Requisitos

Antes de comenzar necesitas:

- .NET 10 SDK
- Node.js LTS y npm
- Git
- Visual Studio, Visual Studio Code o Codex

Comprueba las instalaciones:

```bash
dotnet --version
node --version
npm --version
git --version
```

## Instalación y ejecución

Clona el repositorio:

```bash
git clone https://github.com/lruizap/APO-BOT.git
cd APO-BOT/APO-BOT
```

Instala las dependencias frontend:

```bash
npm install
```

Genera los estilos de Tailwind:

```bash
npm run css:build
```

Ejecuta la aplicación:

```bash
dotnet run
```

Para desarrollo con recarga automática de estilos, utiliza dos terminales:

```bash
npm run css:watch
```

```bash
dotnet watch
```

## Comandos principales

| Comando | Descripción |
|---|---|
| `npm install` | Instala las dependencias de Tailwind |
| `npm run css:build` | Genera y minimiza el CSS |
| `npm run css:watch` | Regenera el CSS al detectar cambios |
| `dotnet build` | Compila y valida el proyecto |
| `dotnet run` | Ejecuta la aplicación |
| `dotnet watch` | Ejecuta con recarga durante el desarrollo |

## Estructura general

```text
APO-BOT/
├── APO-BOT/
│   ├── Components/          # Componentes Razor, páginas y layouts
│   ├── Styles/              # Archivo fuente de Tailwind
│   ├── wwwroot/             # CSS generado, imágenes, iconos y fuentes
│   ├── Program.cs           # Configuración y punto de entrada
│   ├── APO-BOT.csproj       # Configuración del proyecto .NET
│   └── package.json         # Scripts y dependencias frontend
├── resources/               # PDFs, guías y recursos visuales de referencia
├── README.md
└── .gitignore
```

## Reglas de diseño

- Mantener el formato 16:9 y usar 1920 × 1080 como referencia principal.
- Utilizar barra superior cian, navegación lateral navy, fondo gris y tarjetas blancas.
- No introducir nuevos colores sin contrastarlos con la guía gráfica.
- Usar los iconos oficiales disponibles en `wwwroot/assets/apobot`.
- Mantener Montserrat como tipografía principal.
- Priorizar controles grandes, claros y adecuados para uso táctil.
- Conservar radios visuales contenidos, próximos a 8 px.
- Mantener contraste suficiente y estados visibles para selección, error y prioridad.

## Flujo de trabajo recomendado

1. Revisar la pantalla objetivo en los PDF de `resources/`.
2. Crear o reutilizar componentes Razor.
3. Aplicar estilos con Tailwind usando los tokens visuales del proyecto.
4. Utilizar datos mock realistas mientras no exista una API.
5. Ejecutar `npm run css:build`.
6. Ejecutar `dotnet build`.
7. Comparar visualmente el resultado con la referencia antes de cerrar la tarea.
8. Realizar los cambios en una rama y abrir un pull request.

Consulta [CONTRIBUTING.md](CONTRIBUTING.md) para las convenciones de colaboración.

## Estado del desarrollo

### Disponible

- Estructura inicial de la aplicación Blazor.
- Identidad visual APObot.
- Layout principal y navegación lateral.
- Pantalla inicial o dashboard.
- Tarjetas de capacidad, cámaras, últimas salidas, avisos y prioridad.
- Integración de Tailwind CSS.

### Próximas pantallas

- [ ] Stock
- [ ] Detalle de producto
- [ ] Dispensación
- [ ] Pedidos
- [ ] Carga
- [ ] Estadísticas
- [ ] Estados vacíos, carga y error
- [ ] Diseño adaptable para distintas resoluciones del display
- [ ] Integración con API y datos reales

## Calidad y buenas prácticas

Antes de abrir un pull request:

```bash
npm run css:build
dotnet build
```

No deben subirse al repositorio:

- `bin/`
- `obj/`
- `node_modules/`
- Archivos locales del IDE
- Logs o configuraciones privadas

## Licencia

El repositorio no tiene actualmente una licencia pública definida. Todo el contenido y los recursos visuales deben considerarse reservados hasta que el propietario añada una licencia explícita.
