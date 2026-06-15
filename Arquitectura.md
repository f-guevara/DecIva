# Arquitectura – DecIva

Herramienta web para contribuyentes que declaran IVA con **facturas de consumidor final** y **comprobantes de crédito fiscal (CCF)**. Calcula las casillas del formulario **F07** y genera un PDF de referencia para copiar en el portal del Ministerio de Hacienda.

---

## Arquitectura general

```
Browser
   ↓
Blazor Server (puerto 8080 interno / PORT externo)
   ↓
PostgreSQL 16 (red interna Docker)
```

No hay API separada — la UI, el cálculo de casillas y la generación del PDF viven en la misma aplicación Blazor.

---

## Contenedores

| Contenedor | Imagen | Puerto | Función |
|---|---|---|---|
| ui_deciva | deciva (build local) | ${PORT}:8080 | UI Blazor + cálculo IVA + PDF |
| db_deciva | postgres:16 | interno | Base de datos (Identity) |
| npm | jc21/nginx-proxy-manager | 80/443 | Reverse proxy / SSL |

---

## Estructura en el VPS

```
/opt/deciva/
├── docker-compose.yml
├── Dockerfile
├── .env              → secretos reales (nunca en git)
└── .env.example      → plantilla sin secretos (en git)
```

Crear la carpeta en un VPS nuevo:
```bash
mkdir -p /opt/deciva
```

---

## Variables de entorno

| Variable | Descripción |
|---|---|
| POSTGRES_DB | Nombre de la base de datos (ej: deciva) |
| POSTGRES_USER | Usuario PostgreSQL |
| POSTGRES_PASSWORD | Contraseña PostgreSQL — sin caracteres especiales |
| PORT | Puerto expuesto en el VPS (ej: 9512) |
| DB_CONNECTION | Cadena de conexión PostgreSQL completa |

> La contraseña de PostgreSQL NO debe tener caracteres especiales (va dentro de la cadena de conexión).

> **Desarrollo local:** el proyecto usa SQL Server LocalDB del template de Identity. Para deploy en VPS migrar a PostgreSQL (mismo patrón que DataGenerator).

---

## Proceso: deploy en VPS nuevo

### 1. Clonar el repo
```bash
git clone https://github.com/f-guevara/DecIva.git /opt/deciva
cd /opt/deciva
cp .env.example .env
nano .env   # rellenar con valores reales
```

### 2. Arrancar
```bash
docker compose up -d --build
docker logs ui_deciva -f   # verificar migración y startup
```

### 3. Conectar NPM a la red
```bash
docker network connect deciva_network npm
```

### 4. Configurar Proxy Host en NPM
- **Dominio:** `deciva.zifros.de` (o el subdominio que corresponda)
- **Forward Hostname:** `ui_deciva`
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
cd /opt/deciva
git pull
docker compose up -d --build
```

---

## Lógica de negocio: declaración IVA simplificada

La app cubre contribuyentes que solo usan **CCF** y **facturas consumidor final**.

### Casillas principales

| Sección | Casillas | Descripción |
|---|---|---|
| Ventas | 85, 86, 87, 105 | Ventas CCF, facturas, devoluciones |
| Compras | 80, 81, 100 | Compras CCF y devoluciones |
| Créditos / débitos | 110, 130, 135, 140, 145, 150 | IVA 13% compras y ventas |
| Resultado | 155, 160 | Saldo a favor o impuesto determinado |
| Anticipo a cuenta | 165, 171, 166, 168 | Adelanto 2% (ver abajo) |

### IVA 13% vs anticipo a cuenta 2%

Son conceptos **distintos**:

| Concepto | Tasa | Qué es |
|---|---|---|
| IVA | 13% | Impuesto sobre ventas y crédito sobre compras |
| Anticipo a cuenta | 2% | Adelanto que Hacienda recibe sobre operaciones con CCF |

**Reglas que aplica la app:**

1. Las ventas y compras se declaran con **IVA 13% normal** en casillas 85/86/80.
2. El anticipo del **2% NO sustituye al IVA** — es un adelanto aparte, calculado sobre la base gravable sin IVA.
3. **Anticipo recibido en ventas** (casilla **171**): cuando sus clientes pagaron el 2% sobre compras a usted con CCF. Reduce el impuesto → casilla **168** = 160 − 166.
4. **Anticipo pagado en compras** (casilla **165**): cuando usted pagó el 2% al comprar con CCF. Se declara en la **sección E** del F07, aparte del impuesto de operaciones.
5. Las **facturas consumidor final** normalmente **no** tienen anticipo a cuenta.

### Ingreso de montos (Sin IVA / Con IVA)

| Documento | Modo recomendado | Motivo |
|---|---|---|
| CCF (ventas/compras) | Sin IVA | El CCF muestra base gravada e IVA separados |
| Factura consumidor final | Con IVA | El ticket suele traer un solo total |

### Salida

La app genera un **PDF de referencia** (no es la declaración oficial). El contribuyente copia las casillas en el portal de Hacienda.

---

## Estructura del código

```
DecIva/
├── Components/
│   ├── Pages/Declaracion.razor    → formulario principal
│   └── Shared/CampoMontoIva.razor → selector Sin IVA / Con IVA
├── Models/
│   ├── EntradaDeclaracion.cs      → datos del usuario
│   └── ResultadoDeclaracion.cs    → casillas calculadas
├── Services/
│   ├── CalculadoraIvaService.cs   → fórmulas F07
│   ├── IvaUtilidades.cs           → IVA 13%, anticipo 2%
│   └── GeneradorPdfDeclaracionService.cs
└── wwwroot/js/deciva.js           → descarga del PDF
```

---

## Archivos que NO van en git

| Patrón | Motivo |
|---|---|
| `*.pdf` | Declaraciones y muestras con datos de contribuyentes |
| `.env` | Secretos de producción |
| `bin/`, `obj/`, `.vs/` | Artefactos de build |

---

## Troubleshooting

| Error | Causa | Solución |
|---|---|---|
| Contenedor no visible desde NPM | Red Docker diferente | `docker network connect deciva_network npm` |
| Blazor no carga / pantalla en blanco | Websockets no habilitados en NPM | Activar Websockets Support en el Proxy Host |
| password authentication failed | Contraseña en DB_CONNECTION no coincide | Verificar .env — sin caracteres especiales |
| PDF no descarga | JS bloqueado | Verificar que `deciva.js` carga y el navegador permite descargas |
| Casilla 168 no coincide con Hacienda | Retenciones/percepciones 1% no incluidas | La app simplificada solo contempla anticipo 2%; ampliar si el contribuyente tiene retenciones |

---

## Notas importantes

- El PDF generado **no es** la declaración oficial — solo una guía de casillas.
- Nunca commitear `.env` ni archivos PDF.
- La imagen Docker se construye en el VPS (`docker compose up --build`).
- TZ recomendado en contenedor: `America/El_Salvador`.
