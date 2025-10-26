# 🍺 IntroASP - Ecosistema .NET Integrado
### ASP.NET MVC + WCF SOAP + Worker Service + Console Scheduler + API REST + API Externa  

Este proyecto es una solución integral construida en **.NET Framework** y **.NET Core**, que combina distintos tipos de aplicaciones y servicios para demostrar la interoperabilidad dentro del ecosistema .NET.  

Incluye:
- Una aplicación **ASP.NET MVC** que consume un **servicio WCF SOAP**.
- Un **Worker Service** que ejecuta tareas en segundo plano.
- Una **aplicación de consola** para programar tareas mediante **Windows Task Scheduler**.
- Un **servicio REST API** construido en .NET.
- Integración con una **API externa** para mostrar información adicional (banderas de países).

---

## 🧱 Estructura de la solución
📦 IntroASP-Solution
│
├── 💧 BeerSOAPService/ → Servicio WCF (SOAP)
│ ├── IService.cs → Contrato del servicio
│ ├── Service.svc → Endpoint principal
│ ├── Service.svc.cs → Implementación CRUD de cervezas
│ ├── Data/ → Modelos de Entity Framework (Beer, Brand, PubContext)
│ └── Web.config → Configuración de bindings y endpoints
│
├── 🧩 IntroASP/ → Aplicación ASP.NET MVC
│ ├── Controllers/ → Controladores (BeerSoapController, ApiController, etc.)
│ ├── Views/ → Vistas Razor (Index, Create, etc.)
│ ├── Connected Services/ → Referencia al servicio WCF
│ ├── Models/ → Modelos de presentación
│ ├── wwwroot/ → Archivos estáticos
│ └── appsettings.json
│
├── ⚙️ BeerCsWorker/ → Worker Service (.NET Core)
│ ├── Worker.cs → Lógica del servicio en segundo plano
│ ├── Program.cs → Configuración y ejecución del servicio
│ └── appsettings.json → Configuración de intervalos y endpoints
│
├── 🧮 BeerCsvTask/ → Aplicación de consola (Task Scheduler)
│ ├── Program.cs → Tarea programada que consume APIs o genera reportes
│ ├── Services/ → Conexión a la API REST / SOAP
│ └── Logs/ → Carpeta de registros generados
│
├── 🌐 BeerAPI/ → API REST desarrollada en .NET
│ ├── Controllers/ → Endpoints REST (ej. /api/beers)
│ ├── Models/ → Modelos de datos
│ ├── Data/ → DbContext y configuración EF
│ └── Program.cs → Configuración de endpoints y Swagger

🧑‍💻 Autor
Jesús Ignacio Rostro Díaz
Estudiante de Ingeniería en Desarrollo de software
Centro de Enseñanza Técnica Industrial
📍 Guadalajara, Jalisco, México
🔗 GitHub @xZloy
