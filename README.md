# APObot Frontend

Interfaz responsive del display APObot para farmacia, desarrollada con Blazor y Tailwind CSS a partir de las pantallas, colores, iconos y tipografias incluidas en `resources`.

El producto principal de este repositorio es el frontend. `demo/APO-BOT.DemoApi` proporciona una API local con SQLite para probar la interfaz de extremo a extremo; no sustituye al backend de produccion.

## Funcionalidad disponible

- Pantallas de Inicio, Stock, Pedidos, Carga, Estadisticas, Avisos y Ajustes.
- Adaptacion para escritorio, portatiles, tabletas y moviles.
- Assets oficiales APObot y tipografia Montserrat.
- Cliente HTTP tipado y contratos preparados para integrar el backend real.
- Estados de carga, error y ausencia de datos.
- Graficos alimentados por API.
- API de demostracion y SQLite regenerable con datos de prueba.
- Aviso de comprobacion que se reactiva cada vez que arranca la API demo.

## Tecnologias

- .NET 10 y Blazor Web App con renderizado interactivo de servidor.
- C# y Razor Components.
- Tailwind CSS 4.
- SQLite con Entity Framework Core en el entorno demo.
- Montserrat y recursos graficos oficiales de APObot.

## Requisitos

- Git.
- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0).
- Node.js 20 o posterior y npm.
- Windows 10/11 para ejecutar los scripts incluidos sin adaptaciones.

Comprueba las instalaciones:

```powershell
git --version
dotnet --version
node --version
npm --version
```

## Descargar y probar el proyecto completo

1. Clona el repositorio y entra en la carpeta:

```powershell
git clone https://github.com/lruizap/APO-BOT.git
cd APO-BOT
```

2. Instala Tailwind y genera el CSS:

```powershell
npm ci --prefix .\APO-BOT
npm run css:build --prefix .\APO-BOT
```

3. Crea desde cero la base SQLite con todos los datos de prueba:

```powershell
.\scripts\database\create-demo-database.bat
```

La base tambien se crea automaticamente si no existe al arrancar la API.

4. Arranca la API y el frontend juntos:

```powershell
.\scripts\start-demo.bat
```

Alternativa desde PowerShell:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\start-demo.ps1
```

5. Abre las direcciones locales:

- Frontend: `http://localhost:5168`
- API demo: `http://localhost:5090`
- Estado de API y SQLite: `http://localhost:5090/health`

Deten el entorno con `Ctrl+C`. El script cierra tambien la API que haya iniciado.

## Base de datos de demostracion

La base se genera en:

```text
demo/APO-BOT.DemoApi/Data/apobot-demo.db
```

El archivo no se sube a GitHub. `scripts/database/create-demo-database.bat` elimina cualquier base local anterior y reconstruye tablas, configuracion, productos, stock, pedidos, carga, historicos, graficos y avisos.

Para restaurar los datos mientras la API esta en marcha:

```powershell
Invoke-RestMethod -Method Post http://localhost:5090/api/v1/demo/reset
```

La API reactiva al iniciar el aviso `Aviso de comprobacion`, incluso si se resolvio en una ejecucion anterior.

## Ejecucion por separado

API demo:

```powershell
dotnet run --project .\demo\APO-BOT.DemoApi --urls http://localhost:5090
```

Frontend:

```powershell
dotnet run --project .\APO-BOT --urls http://localhost:5168
```

La configuracion local apunta a `http://localhost:5090`. Para integrar otro backend, modifica `Api:Enabled`, `Api:BaseUrl` y `Api:TimeoutSeconds` o usa variables de entorno como `Api__BaseUrl`.

## Desarrollo de estilos

Modo observacion:

```powershell
npm run css:watch --prefix .\APO-BOT
```

Antes de entregar cambios:

```powershell
npm run css:build --prefix .\APO-BOT
dotnet build .\APO-BOT\APO-BOT.csproj
dotnet build .\demo\APO-BOT.DemoApi\APO-BOT.DemoApi.csproj
```

## Rutas

| Ruta | Contenido |
|---|---|
| `/` | Capacidad, entradas, salidas, camaras, avisos y prioridad |
| `/stock` | Inventario, detalle de producto y dispensacion |
| `/pedidos` | Listado y estado del pedido |
| `/carga` | Configuracion, sesion de carga y rechazos |
| `/estadisticas` | Historicos y graficos de actividad y capacidad |
| `/avisos` | Avisos activos, resueltos y accion de resolucion |
| `/ajustes` | Identificacion, pantalla y avisos sonoros |

## Estructura

```text
APO-BOT/
|-- APO-BOT/                    # Aplicacion Blazor
|   |-- Components/             # Layout, paginas y componentes
|   |-- Infrastructure/Api/     # Cliente HTTP y configuracion
|   |-- Models/                 # Contratos del backend
|   |-- Styles/                 # Fuente de Tailwind
|   `-- wwwroot/                # CSS, fuentes e imagenes oficiales
|-- demo/APO-BOT.DemoApi/       # API local y persistencia SQLite
|-- docs/backend-integration.md # Contrato de integracion
|-- resources/                  # Documentacion grafica original
`-- scripts/                    # Arranque y creacion de base demo
```

## Referencias visuales

- Pantallas: `resources/APObot - Ficheros display software - parte 1/APObot - Interfaz display soft Zgz.pdf`.
- Colores: `resources/APObot - Ficheros display software - parte 1/Logo, iconos y colores/APObot - Gama colores.pdf`.
- Assets usados por la app: `APO-BOT/wwwroot/assets/apobot`.
- Contrato de integracion: `docs/backend-integration.md`.

La interfaz no debe incluir datos operativos de produccion ni colores, logos o iconos ajenos a la guia. Los datos visibles durante la demostracion proceden de SQLite a traves de la API local.

## Licencia

El repositorio no tiene una licencia publica definida. El codigo y los recursos visuales deben considerarse reservados hasta que el propietario indique lo contrario.
