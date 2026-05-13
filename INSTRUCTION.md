# INSTRUCTION - APIFORMS v1.0.0

## 1) Requisitos
- .NET SDK 8
- Node.js 18+ y npm
- MongoDB (local o Atlas)
- Git

## 2) Configuración de Base de Datos (MongoDB)
Archivo a configurar:
- `src/APIForms.Api/appsettings.json`

Claves:
- `Mongo:ConnectionString`
- `Mongo:Database`

Ejemplo:
```json
{
  "Mongo": {
    "ConnectionString": "mongodb://localhost:27017",
    "Database": "apiforms"
  }
}
```

Si usas MongoDB Atlas:
- `ConnectionString`: cadena `mongodb+srv://...`
- Verifica whitelist de IP y usuario/rol con permisos en la BD.

## 3) Configuración JWT
En el mismo `appsettings.json`:
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:Key`

Ejemplo:
```json
{
  "Jwt": {
    "Issuer": "APIFORMS",
    "Audience": "APIFORMS_CLIENT",
    "Key": "CAMBIA_ESTA_CLAVE_POR_UNA_SEGURA_DE_32+_CARACTERES"
  }
}
```

## 4) Levantar el backend
```bash
cd src
dotnet restore APIForms.sln
dotnet build APIForms.sln
dotnet run --project APIForms.Api/APIForms.Api.csproj --urls http://localhost:5000
```

## 5) Levantar el frontend
```bash
cd apiforms-web
npm install
npm start
```

URLs:
- Frontend: `http://localhost:4200`
- Backend: `http://localhost:5000`

## 6) Publicar primera versión a GitHub

### Inicializar y versionar
```bash
git init
git add .
git commit -m "chore: first release v1.0.0"
git branch -M main
git tag -a v1.0.0 -m "First stable version"
```

### Conectar repositorio remoto
```bash
git remote add origin https://github.com/apiforms-org/apiforms.git
```

### Autenticación recomendada (segura)
Usa un **Personal Access Token (PAT)** de GitHub (no contraseña de cuenta).

### Subir `main` y tag
```bash
git push -u origin main
git push origin v1.0.0
```

## 7) Notas de seguridad
- No guardes credenciales en texto plano en el repo.
- Usa variables de entorno o secretos del entorno de despliegue para producción.
- Evita compartir usuario/contraseña de GitHub; usa PAT con permisos mínimos.
