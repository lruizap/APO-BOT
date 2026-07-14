# APObot Frontend

Interfaz responsive del display APObot para farmacia, desarrollada con Blazor y Tailwind CSS a partir de las pantallas, colores, iconos y tipografias incluidas en `resources`.

El producto de este repositorio es el frontend. La carpeta `demo/APO-BOT.DemoApi` contiene una API local con SQLite exclusivamente para probar la interfaz de extremo a extremo; no sustituye al backend de produccion.

## Que incluye

- Pantallas de Inicio, Stock, Pedidos, Carga, Estadisticas, Avisos y Ajustes.
- Adaptacion para escritorio 1920 x 1080, portatiles, tabletas y moviles.
- Assets oficiales APObot y tipografia Montserrat.
- Cliente HTTP tipado, contratos de datos y estados de carga, error y ausencia de datos.
- Graficos alimentados por la API, sin valores incrustados en los componentes.
- API de demostracion y base SQLite regenerable con datos de prueba.
- Aviso de comprobacion que vuelve a quedar activo cada vez que arranca la API demo.

## Requisitos

- Git.
- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0).
- Node.js 20 o posterior y npm, solo necesarios para recompilar Tailwind CSS.
- Windows 10/11 para usar los scripts incluidos sin adaptaciones.

Comprueba la instalacion:

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

2. Instala las dependencias de Tailwind y genera el CSS:

```powershell
cd .\APO-BOT
npm ci
npm run css:build
cd ..
```

3. Crea desde cero la base SQLite con el esquema y todos los datos de prueba:

```powershell
.\scripts\database\create-demo-database.bat
```

4. Arranca la API y el frontend juntos:

```powershell
.\scripts\start-demo.ps1
```

Si PowerShell bloquea la ejecucion de scripts:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\start-demo.ps1
```

5. Abre las direcciones locales:

- Frontend: `http://localhost:5168`
- API demo: `http://localhost:5090`
- Estado de la API y SQLite: `http://localhost:5090/health`

Deten el entorno con `Ctrl+C` en la consola del frontend. El script tambien cerrara la API que haya iniciado.

## Base de datos de demostracion

La base se crea en:

```text
demo/APO-BOT.DemoApi/Data/apobot-demo.db
```

El archivo no se sube a GitHub. El script `scripts/database/create-demo-database.bat` borra cualquier base local anterior y ejecuta el sembrador de la API para reconstruir tablas, configuracion, productos, stock, pedidos, carga, historicos, graficos y avisos.

Tambien puedes restaurar los datos mientras la API esta en marcha:

```powershell
Invoke-RestMethod -Method Post http://localhost:5090/api/v1/demo/reset
```

La API reactiva al iniciar el aviso `Aviso de comprobacion`, incluso si se marco como resuelto en una ejecucion anterior. Esto permite comprobar siempre la insignia lateral y la pantalla de Avisos.

## Ejecucion por separado

API demo:

```powershell
dotnet run --project .\demo\APO-BOT.DemoApi --urls http://localhost:5090
```

Frontend:

```powershell
dotnet run --project .\APO-BOT --urls http://localhost:5168
```

El perfil `Development` del frontend apunta a `http://localhost:5090`. En otros entornos configura `Api:Enabled`, `Api:BaseUrl` y `Api:TimeoutSeconds` en `APO-BOT/appsettings*.json`.

## Desarrollo de estilos

Desde `APO-BOT/`:

```powershell
npm run css:watch
```

Antes de entregar cambios:

```powershell
npm run css:build
dotnet build .\APO-BOT.csproj
dotnet build ..\demo\APO-BOT.DemoApi\APO-BOT.DemoApi.csproj
```

## Rutas de la interfaz

- `/`: capacidad, entradas, salidas, camaras, avisos y prioridad.
- `/stock`: inventario, detalle de producto y dispensacion.
- `/pedidos`: listado y estado del pedido.
- `/carga`: configuracion, sesion de carga y rechazos.
- `/estadisticas`: historicos y graficos de actividad/capacidad.
- `/avisos`: avisos activos, resueltos y accion de resolucion.
- `/ajustes`: identificacion, pantalla y avisos sonoros.

## Referencias y arquitectura

- Referencia visual: `resources/APObot - Ficheros display software - parte 1/APObot - Interfaz display soft Zgz.pdf`.
- Guia de color: `resources/APObot - Ficheros display software - parte 1/Logo, iconos y colores/APObot - Gama colores.pdf`.
- Contrato de integracion: `docs/backend-integration.md`.
- Assets de la app: `APO-BOT/wwwroot/assets/apobot`.
- Contratos tipados: `APO-BOT/Models`.
- Cliente HTTP: `APO-BOT/Infrastructure/Api`.

La interfaz no debe contener datos operativos de produccion ni inventar colores, logos o iconos. Los datos visibles durante la demostracion proceden de SQLite a traves de la API local.
