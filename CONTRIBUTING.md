# Guía de contribución

Gracias por colaborar en APObot Frontend. El objetivo de estas reglas es mantener una interfaz coherente con la documentación visual y facilitar la revisión de cambios.

## Preparación

Desde la carpeta `APO-BOT/`:

```bash
npm install
npm run css:build
dotnet build
```

Para desarrollar con recarga automática:

```bash
npm run css:watch
```

```bash
dotnet watch
```

## Flujo de ramas

No trabajes directamente sobre `master`.

Utiliza nombres descriptivos:

```text
feature/stock-screen
feature/product-details
fix/sidebar-navigation
docs/update-readme
refactor/dashboard-components
```

## Commits

Usa mensajes breves y descriptivos, preferiblemente en inglés:

```text
feat: add stock overview screen
fix: correct active navigation state
docs: update setup instructions
refactor: extract dashboard card component
style: align warning cards with design guide
```

## Pull requests

Cada pull request debe:

- Resolver un objetivo concreto.
- Explicar los cambios realizados.
- Indicar cómo se ha probado.
- Incluir capturas cuando cambie la interfaz.
- Evitar mezclar refactorizaciones y funcionalidades no relacionadas.
- Compilar correctamente antes de solicitar revisión.

Comprobación mínima:

```bash
npm run css:build
dotnet build
```

## Criterios visuales

- Consultar primero los documentos de `resources/`.
- Mantener la paleta y tipografía oficiales.
- Diseñar para uso táctil y formato 16:9.
- Reutilizar componentes siempre que sea razonable.
- No construir clases Tailwind dinámicas que el compilador no pueda detectar.
- Usar estados claros para navegación, prioridad, error y selección.
- No introducir librerías visuales adicionales sin justificar su necesidad.

## Organización del código

- Las páginas deben coordinar la interfaz, no concentrar toda la lógica.
- Extraer componentes cuando una sección se repita o tenga responsabilidad propia.
- Mantener nombres claros en componentes, propiedades y métodos.
- Evitar datos de negocio reales o información sensible en los mocks.
- Documentar decisiones poco evidentes mediante comentarios breves.

## Archivos que no deben versionarse

- `bin/`
- `obj/`
- `node_modules/`
- Configuración local del IDE
- Logs
- Secretos, credenciales o cadenas de conexión privadas

## Informe de errores

Al comunicar un error incluye:

- Pantalla o componente afectado.
- Pasos para reproducirlo.
- Comportamiento esperado.
- Comportamiento actual.
- Resolución y navegador utilizados.
- Captura o vídeo cuando sea útil.
