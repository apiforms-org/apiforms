# APIFORMS - Documentación Principal del Proyecto

APIFORMS es una plataforma para crear formularios, exponerlos como APIs CRUD y aplicar reglas de transformación/validación con SmartQL antes de persistir datos.

## Título del proyecto
**APIFORMS: Formularios -> APIs -> Flujos**

## Características
- Creación de formularios dinámicos por tenant.
- Publicación de formularios para consumo público.
- APIs CRUD automáticas por formulario.
- Autenticación JWT y control por tenant.
- Subscription Keys por formulario (múltiples keys activas con nombre por cliente).
- Políticas SmartQL persistidas en backend.
- Gateway SmartQL previo al guardado (`form.submit`, `api.create`, `api.update`).
- Búsqueda avanzada por respuestas (`field/value`, query múltiple, `filters=campo:valor`).
- UI administrativa para formularios, respuestas, políticas, permisos y auth de consumo.

## Arquitectura
Monorepo con backend .NET 8 y frontend Angular.

### Backend
- `src/APIForms.Api`: capa HTTP, controllers, auth, middleware.
- `src/APIForms.Application`: servicios de negocio, DTOs, interfaces.
- `src/APIForms.Domain`: entidades de dominio.
- `src/APIForms.Infrastructure`: repositorios Mongo y DI.

### Frontend
- `apiforms-web`: Angular standalone.
- Features principales:
  - `auth`
  - `apiforms/forms-list`
  - `apiforms/form-builder`
  - `apiforms/api-settings`
  - `apiforms/policies`
  - `apiforms/form-responses`

## Flujo funcional
1. Usuario se registra/inicia sesión.
2. Crea formulario con campos y reglas de required/tipo.
3. Publica formulario.
4. Consume datos por API (`/data`) o formulario público.
5. Si existen políticas SmartQL activas para el evento, se ejecutan antes de guardar.
6. Si todas pasan, persistencia en Mongo.
7. Si alguna política rechaza, devuelve error y no persiste.

## Endpoints clave

### Auth
- `POST /api/auth/register`
- `POST /api/auth/login`

### Formularios
- `GET /api/forms`
- `POST /api/forms`
- `GET /api/forms/{id}`
- `PUT /api/forms/{id}`
- `DELETE /api/forms/{id}`
- `POST /api/forms/{id}/publish`
- `POST /api/forms/{id}/unpublish`

### Datos de formulario
Base: `/api/forms/{formId}/{slug}/data`
- `GET /`
- `POST /`
- `GET /{id}`
- `PUT /{id}`
- `DELETE /{id}`
- `GET /search?field=nombre&value=diego`
- `GET /search-by-question?question=hola&nombre=diego`
- `GET /search-by-question?question=hola&filters=nombre:diego,ciudad:cali`

Notas:
- `question` en `search-by-question` es opcional.
- Acepta múltiples filtros por `&campo=valor` y por `filters=campo:valor,...`.

### Form Permissions
Base: `/api/form-permissions/{formId}`
- `GET`
- `PUT`

### Form Auth / Subscription Keys
Base: `/api/form-auth/{formId}`
- `GET`
- `PUT`
- `GET /keys`
- `POST /keys` body `{ "name": "cliente-acme" }`
- `DELETE /keys/{keyId}`

Notas:
- Se permiten múltiples keys activas por formulario.
- Validación por hash y tenant.

### SmartQL Policies
Base: `/api/smartql-policies`
- `GET /{formId}/{policyId}`
- `PUT /{formId}`

Body `PUT`:
```json
{
  "policyId": "smartql_form_default",
  "event": "ON api.create",
  "smartQl": "ON api.create\nREQUIRE input.nombre\nRETURN input",
  "enabled": true,
  "priority": 100
}
```

## SmartQL (estado actual)
SmartQL no es SQL. Es un DSL de reglas para validar/transformar payloads.

### Soporte implementado
- `ON ...`
- `REQUIRE input.campo`
- `SET input.campo = UPPER(...) | LOWER(...) | TRIM(...) | "literal" | input.otroCampo`
- `IF input.campo IS_EMPTY THEN REJECT "mensaje"`
- `IF input.campo NOT_MATCH /regex/ THEN REJECT "mensaje"`
- `REJECT "mensaje"`
- `RETURN input`

### Eventos evaluados
- `form.submit`
- `api.create`
- `api.update`

### Comportamiento de la caja Gateway
- Sin políticas activas: transparente (passthrough).
- Con políticas activas: ejecuta en orden de prioridad.
- Cualquier rechazo detiene guardado.

## Seguridad
- JWT obligatorio en rutas autenticadas.
- Tenant aislado por claim `tenantId`.
- `x-api-key` opcional por formulario (si `requireSubscriptionKey=true`).
- Hash SHA256 para almacenar keys.
- Middleware de excepciones homogéneo en JSON.

## Configuración
Archivo: `src/APIForms.Api/appsettings.json`
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:Key`
- `Mongo:ConnectionString`
- `Mongo:Database`

## Frontend - UX actual
- Landing pública estilo dark/neón con navegación y panel lateral animado para:
  - Características
  - Documentación
  - Plantillas
  - Blog
- Hero con login rápido (`Entrar` / `Registrarse`).
- Área logueada separada (`/apiforms/*`) con cabecera propia y acceso a proyectos.
- Diseño actualizado para contraste en paneles, tarjetas y modales sobre fondo oscuro.

## Plantillas
Actualmente las “Plantillas” en landing son contenido informativo lateral.
Siguiente evolución recomendada:
- Endpoint para catálogo de plantillas.
- Clonado de formulario desde plantilla.
- Versionado y marketplace interno de plantillas.

## Documentación operativa

### Levantar local
1. Backend:
   - `cd src`
   - `dotnet restore APIForms.sln`
   - `dotnet build APIForms.sln`
   - `dotnet run --project APIForms.Api/APIForms.Api.csproj --urls http://localhost:5000`
2. Frontend:
   - `cd apiforms-web`
   - `npm install`
   - `npm start`

### URLs
- Frontend: `http://localhost:4200`
- Backend: `http://localhost:5000`

## Diagnóstico rápido
- `404` al guardar SmartQL: backend viejo corriendo sin `SmartQlPoliciesController`.
- `401` en SmartQL: endpoint existe pero token/sesión inválida.
- `status 0` en frontend: backend caído o problema de red/CORS.

## Estado de madurez por módulo
- Formularios CRUD: funcional.
- Publicación y consumo de datos: funcional.
- Permisos CRUD/público: funcional.
- Auth JWT: funcional.
- Subscription keys múltiples: funcional.
- SmartQL policies persistidas: funcional.
- SmartQL engine básico: funcional.
- Conectores avanzados SmartQL (HTTP/SFTP/DB externos): pendiente.

## Roadmap recomendado
1. SmartQL `CALL http` con timeout/retry/circuit breaker.
2. Catálogo de conectores (`http`, `sftp`, `db`) por tenant con secretos cifrados.
3. Historial de ejecución de políticas (auditoría).
4. Sandbox de prueba de políticas (dry-run).
5. Catálogo real de plantillas y clonación.
6. Mejoras de presupuesto CSS y refactor de estilos compartidos.

## Convenciones de desarrollo
- DTOs en `Application/DTOs`.
- Interfaces de repositorio en `Application/Interfaces`.
- Controllers delgados; lógica en services.
- Persistencia vía repositories.
- Al agregar endpoint, actualizar frontend asociado y documentación.

## Notas importantes de sesión
- El diseño visual evolucionó a tema dark/neón.
- La landing pública y el área logueada están separadas por ruta.
- El login principal ahora está integrado en el panel del hero.
- La pantalla `/login` mantiene bloque visual de formulario de contacto.

