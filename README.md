# APObot Frontend

Interfaz frontend del display APObot para farmacia. El objetivo del repositorio es reproducir con la maxima fidelidad posible las pantallas y la guia grafica incluidas en `resources`.

## Alcance

- Proyecto exclusivamente de interfaz.
- Sin integracion backend por ahora.
- Los datos visibles en pantalla son mockups hasta que exista contrato de API.
- La referencia visual principal es `resources/APObot - Ficheros display software - parte 1/APObot - Interfaz display soft Zgz.pdf`.
- La guia de color principal es `resources/APObot - Ficheros display software - parte 1/Logo, iconos y colores/APObot - Gama colores.pdf`.

## Stack

- .NET/Blazor para componentes de interfaz.
- Tailwind CSS para estilos utilitarios y tokens visuales.
- Montserrat como tipografia base.
- Assets SVG/PNG propios de APObot servidos desde `APO-BOT/wwwroot/assets/apobot`.

## Comandos

Desde `APO-BOT/`:

```bash
npm install
npm run css:build
dotnet run
```

Durante desarrollo de estilos:

```bash
npm run css:watch
```

## Reglas visuales

- Mantener layout 16:9 como referencia base del display: 1920 x 1080.
- Usar barra superior cian, navegacion lateral navy, fondo gris claro y tarjetas blancas.
- No introducir paletas nuevas sin contrastarlas con la guia.
- Usar iconos oficiales desde `wwwroot/assets/apobot`.
- Usar Montserrat en toda la interfaz.
- Priorizar controles grandes y legibles para uso tactil.
- Las tarjetas y botones deben conservar radios contenidos, cercanos a 8 px.

## Colores APObot

- Azul cian: `#3cb2dd`
- Azul oscuro: `#1f2144`
- Coral: `#e1806f`
- Fondo gris claro: `#ececec`
- Degradado azul: `#3cb2dd` a `#9bd4e6`
- Degradado coral: `#d86467` a `#f6b683`

## Flujo de trabajo

1. Revisar la pantalla objetivo en los PDFs de `resources`.
2. Crear componentes en Blazor con Tailwind.
3. Usar datos mock con nombres realistas de farmacia.
4. Ejecutar `npm run css:build`.
5. Ejecutar `dotnet build`.
6. Comparar visualmente contra la pantalla de referencia antes de cerrar la tarea.

## Estado actual

La pantalla inicial reproduce la vista de inicio/dashboard: capacidad, camaras, ultimas salidas, avisos, prioridad y navegacion lateral. Las siguientes pantallas a construir son Stock, detalle de producto, dispensacion, Pedidos, Carga y Estadisticas.
