# Contrato de integracion backend

Los componentes de interfaz no incluyen datos operativos incrustados. Todas las entidades se obtienen mediante `IApoBotApiClient` y los DTO de `APO-BOT/Models`.

Para comprobar el contrato de extremo a extremo existe una API local aislada en `demo/APO-BOT.DemoApi`. Persiste datos semilla en SQLite y no forma parte del backend de produccion.

## Configuracion

La API permanece desactivada por defecto. Puede habilitarse con configuracion JSON o variables de entorno:

```json
{
  "Api": {
    "Enabled": true,
    "BaseUrl": "https://api.example.com/",
    "TimeoutSeconds": 30
  }
}
```

```powershell
$env:Api__Enabled = "true"
$env:Api__BaseUrl = "https://api.example.com/"
$env:Api__TimeoutSeconds = "30"
```

`BaseUrl` debe ser una URL absoluta. Cuando la integracion no esta habilitada, las pantallas muestran estados vacios y los comandos permanecen deshabilitados.

## Autenticacion

`ApiAuthenticationHandler` solicita un token a `IApiAccessTokenProvider` y, si existe, envia `Authorization: Bearer <token>`. El registro actual usa `EmptyApiAccessTokenProvider`, que no proporciona credenciales.

El proyecto que integre identidad debe implementar `IApiAccessTokenProvider` y sustituir ese registro en `Program.cs`. Los tokens no deben almacenarse en `appsettings.json` ni versionarse.

## Convenciones

- JSON en `camelCase`.
- Enumeraciones como cadenas en `camelCase`.
- Fechas y horas en ISO 8601, preferiblemente UTC.
- Fechas sin hora en formato `YYYY-MM-DD`.
- Identificadores tratados como cadenas opacas.
- Listados paginados con `{ items, page, pageSize, totalCount }`.
- Respuestas de error en Problem Details ampliado con `code` y `errors` cuando proceda.
- `GET` opcionales devuelven `204 No Content` o `404 Not Found` cuando no existe un recurso activo.

Ejemplo de error:

```json
{
  "type": "https://api.example.com/problems/validation",
  "title": "Solicitud no valida",
  "status": 400,
  "detail": "Revise los campos indicados.",
  "code": "validation_error",
  "errors": {
    "quantity": ["Debe ser mayor que cero."]
  }
}
```

## Endpoints

| Metodo | Ruta | Uso |
| --- | --- | --- |
| `GET` | `/api/v1/system/context` | Farmacia, usuario, hora de servidor, luz y numero de alertas. |
| `PUT` | `/api/v1/system/light` | Cambiar luz con `{ enabled }`. |
| `POST` | `/api/v1/system/actions` | Ejecutar `{ action: "pause" | "powerOff" }`. |
| `GET` | `/api/v1/dashboard` | Capacidad, entradas, dispensaciones, alertas, prioridad y camaras. |
| `PUT` | `/api/v1/dashboard/priority` | Cambiar `{ mode: "dispense" | "load" | "optimize" }`. |
| `GET` | `/api/v1/alerts?includeResolved=true` | Consultar avisos activos y, opcionalmente, resueltos. |
| `PATCH` | `/api/v1/alerts/{id}/resolve` | Resolver un aviso. |
| `GET` | `/api/v1/settings` | Obtener identificacion y preferencias del display. |
| `PUT` | `/api/v1/settings` | Guardar identificacion y preferencias del display. |
| `GET` | `/api/v1/stock` | Inventario filtrado y paginado. |
| `GET` | `/api/v1/stock/{id}` | Detalle de producto. |
| `GET` | `/api/v1/stock/{id}/units` | Envases fisicos de un producto. |
| `GET` | `/api/v1/outputs` | Salidas disponibles. |
| `POST` | `/api/v1/dispensations` | Crear una dispensacion. |
| `GET` | `/api/v1/orders` | Pedidos filtrados y paginados. |
| `GET` | `/api/v1/orders/active` | Pedido activo, si existe. |
| `GET` | `/api/v1/orders/{id}` | Detalle y lineas de un pedido. |
| `GET` | `/api/v1/load/configuration` | Ventanas y baldas disponibles. |
| `GET` | `/api/v1/load/sessions/active` | Sesion de carga activa, si existe. |
| `POST` | `/api/v1/load/sessions` | Iniciar una sesion de carga. |
| `POST` | `/api/v1/load/sessions/{id}/commands` | Pausar, detener o controlar una balda. |
| `GET` | `/api/v1/statistics/history` | Historico correcto y rechazado. |
| `GET` | `/api/v1/statistics/capacity` | Series de actividad y capacidad. |

### Consultas de stock

`GET /api/v1/stock` acepta `code`, `description`, `category`, `hasStock`, `minimumStock`, `observation`, `sortBy`, `descending`, `page` y `pageSize`.

### Dispensaciones

`POST /api/v1/dispensations` recibe:

```json
{
  "productId": "string",
  "outputId": "string",
  "quantity": 1,
  "priority": "high",
  "unitIds": []
}
```

`priority` admite `high`, `medium` y `low`. `unitIds` permite solicitar envases concretos y puede quedar vacio para que el backend seleccione las unidades.

### Carga

El inicio de sesion recibe `deliveryNote`, `supplier`, `deliveryNoteCode`, `windowId` y `shelfId`. Los comandos admitidos son `start`, `pause`, `stop`, `startShelf`, `pauseShelf` y `stopShelf`. Los comandos de balda incluyen `shelfId`.

### Historicos y graficos

`GET /api/v1/statistics/history` acepta `type`, `period`, `productId`, `from`, `to`, `page` y `pageSize`. `type` admite `dispensation`, `load`, `capacity` y `article`; `period` admite `days`, `week` y `month`.

`GET /api/v1/statistics/capacity?period=days` devuelve dos series:

```json
{
  "throughput": [
    { "label": "string", "primaryValue": 0, "secondaryValue": 0, "timestamp": "2026-01-01T00:00:00Z" }
  ],
  "capacity": [
    { "label": "string", "primaryValue": 0, "secondaryValue": 0, "timestamp": "2026-01-01T00:00:00Z" }
  ]
}
```

Los valores de `throughput` representan cargados y dispensados. Los valores de `capacity` representan almacenados y espacio libre. Los componentes escalan los datos recibidos y no generan puntos artificiales.

### Ajustes

`GET /api/v1/settings` y `PUT /api/v1/settings` usan el mismo contrato:

```json
{
  "pharmacyName": "string",
  "userDisplayName": "string",
  "alertSoundEnabled": true,
  "screenBrightness": 85,
  "refreshIntervalSeconds": 30
}
```

`screenBrightness` admite valores entre 20 y 100. `refreshIntervalSeconds` admite valores entre 10 y 300.

## Estados de interfaz

Cada pantalla distingue carga, error, conjunto vacio y datos disponibles. Las operaciones escriben mediante el cliente tipado y vuelven a consultar el recurso afectado cuando es necesario. Los errores de red, timeout y Problem Details se convierten en `ApiException` para impedir que detalles de transporte se filtren a los componentes.

## Utilidades de la API demo

- `GET /health`: estado de la API y version efectiva de SQLite.
- `POST /api/v1/demo/reset`: elimina y vuelve a crear la base con sus datos iniciales.

El perfil `Development` configura `Api:BaseUrl` como `http://localhost:5090/`. En otros entornos la API permanece desactivada hasta que se configure explicitamente.
