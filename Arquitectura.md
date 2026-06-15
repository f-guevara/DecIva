# Arquitectura – DataGenerator

Sistema de inventario de instrumentos quirúrgicos. Permite cargar archivos Excel/CSV con artículos del proveedor anterior (MC Medizintechnik), generar nuevos códigos automáticamente por categoría, guardarlos en PostgreSQL y exportarlos a Excel.

---

## Arquitectura general

```
Browser
   ↓
Blazor Server (puerto 8080 interno / PORT externo)
   ↓
PostgreSQL 16 (red interna Docker)
```

No hay API separada — todo vive dentro de la aplicación Blazor.

---

## Contenedores

| Contenedor | Imagen | Puerto | Función |
|---|---|---|---|
| ui_datagenerator | datagenerator (build local) | ${PORT}:8080 | UI Blazor + lógica |
| db_datagenerator | postgres:16 | interno | Base de datos |
| npm | jc21/nginx-proxy-manager | 80/443 | Reverse proxy / SSL |

---

## Estructura en el VPS

```
/opt/datagenerator/
├── docker-compose.yml
├── .env              → secretos reales (nunca en git)
└── .env.example      → plantilla sin secretos (en git)
```

Crear la carpeta en un VPS nuevo:
```bash
mkdir -p /opt/datagenerator
```

---

## Variables de entorno

| Variable | Descripción |
|---|---|
| POSTGRES_DB | Nombre de la base de datos (ej: datagenerator) |
| POSTGRES_USER | Usuario PostgreSQL |
| POSTGRES_PASSWORD | Contraseña PostgreSQL — sin caracteres especiales |
| PORT | Puerto expuesto en el VPS (ej: 9510) |
| DB_CONNECTION | Cadena de conexión completa — debe coincidir con las variables de arriba |

> La contraseña de PostgreSQL NO debe tener caracteres especiales (va dentro de la cadena de conexión).

---

## Proceso: deploy en VPS nuevo

### 1. Clonar el repo
```bash
git clone https://github.com/f-guevara/DataGenerator.git /opt/datagenerator
cd /opt/datagenerator
cp .env.example .env
nano .env   # rellenar con valores reales
```

### 2. Arrancar
```bash
docker compose up -d --build
docker logs ui_datagenerator -f   # verificar migración y startup
```

La app crea la base de datos y ejecuta las migraciones automáticamente al arrancar.

### 3. Conectar NPM a la red
```bash
docker network connect datagenerator_network npm
```

### 4. Configurar Proxy Host en NPM
- **Dominio:** `datagenerator.zifros.de`
- **Forward Hostname:** `ui_datagenerator`
- **Forward Port:** `8080`
- **Websockets Support:** ON — obligatorio para Blazor Server
- **Block Common Exploits:** ON
- **SSL:** Let's Encrypt, Force SSL, HTTP/2, HSTS

---

## Actualizaciones de código

```bash
# En Windows
git add .
git commit -m "descripción del cambio"
git push

# En VPS
cd /opt/datagenerator
git pull
docker compose up -d --build
```

---

## Credenciales iniciales

| Campo | Valor |
|---|---|
| URL | https://datagenerator.zifros.de |
| Admin email | admin@medizintechnik.com |
| Admin password | (el configurado en el primer deploy — cámbialo desde la app) |

> El admin está sembrado automáticamente al primer arranque. Cambiar la contraseña desde **Change Password** en el menú lateral tras el primer login.

---

## Lógica de códigos de artículo

El sistema detecta la categoría del instrumento desde el nombre del producto y asigna un prefijo automático:

| Prefijo | Categoría |
|---|---|
| FRC | Forceps, pinzas de apósitos, swab forceps |
| SCS | Tijeras (Metzenbaum, Mayo, etc.) |
| NHL | Portaagujas |
| CLM | Clamps, pinzas hemostáticas (Kocher, Mosquito, Kelly) |
| RTR | Retractores, ganchos |
| DLT | Dilatadores (Hegar, Pratt) |
| CAN | Cánulas, trocares |
| SPL | Espéculos |
| PRB | Sondas, directores |
| SCL | Mangos de bisturí |
| HKS | Ganchos dérmicos |
| OSTeo | Instrumental óseo (osteótomos, gubias, raspas) |
| GEN | General (sin categoría detectada) |

Los códigos se generan como `PREFIJO-NNNNN` (ej: `FRC-00042`), con contador independiente por categoría y continuidad entre importaciones.

---

## Troubleshooting

| Error | Causa | Solución |
|---|---|---|
| Contenedor no visible desde NPM | Red Docker diferente | `docker network connect datagenerator_network npm` |
| password authentication failed | Contraseña en DB_CONNECTION no coincide con POSTGRES_PASSWORD | Verificar .env — sin caracteres especiales en la contraseña |
| Blazor no carga / pantalla en blanco | Websockets no habilitados en NPM | Activar Websockets Support en el Proxy Host |
| Migration error al arrancar | Base de datos no disponible aún | Esperar que el contenedor db esté healthy; reiniciar ui si es necesario |
| Excel no descarga | JS `downloadFile` bloqueado | Verificar que el navegador no bloquea descargas del dominio |
