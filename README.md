# EduCode Web MVC — Versión 3.0.4

Aplicación ASP.NET Core MVC independiente. Consume EduCode API mediante HTTP/HTTPS, JSON y JWT; no accede directamente a Entity Framework ni a SQL Server.

## Experiencia 3.0

- Navegación y permisos diferenciados para Alumno, Docente y Administrador.
- Portal docente con grupos propios, actividad reciente, señales por tema y exportación CSV.
- Catálogo docente de solo lectura, sin envío de soluciones, pistas ni historial individual.
- Propuestas de ejercicios sujetas a revisión administrativa.
- Registro docente mediante invitación, con vigencia, restricción opcional de correo y revocación.
- Administración de grupos e inscripciones.
- Panel administrativo con métricas globales, estado real de la API/base y auditoría reciente.

El portal docente no incorpora corrección manual, comentarios sobre entregas ni calificación de alumnos.

## Funcionalidades

- Inicio público responsive.
- Registro, login, sesión y logout.
- Dashboard del alumno.
- Recorrido por 8 unidades y 56 ejercicios, con Array y Lista como temas independientes.
- Editor JavaScript, pistas e historial.
- Evaluación local inmediata.
- Panel administrativo de contenidos, analítica y estructura académica.
- Tutor IA conversacional especializado en Programación 1.
- Página principal actualizada con el recorrido, Array y Lista por separado, gamificación, perfil y sistema de opiniones.


## Perfil y opiniones 2.9.6

- Acceso a **Mi cuenta** desde el navbar y el dashboard.
- Nombre y apellido visibles como datos de registro no editables.
- Edición de nombre de usuario para alumnos y docentes únicamente cuando es único, con un máximo de dos cambios efectivos en una ventana móvil de 15 días. La identidad administrativa permanece fija.
- El avatar y la foto pueden actualizarse aunque el límite temporal del nombre de usuario esté agotado.
- Selector de ocho avatares temáticos.
- Carga opcional de foto JPG, PNG o WebP de hasta 2 MB.
- Validación de MIME y firma real del archivo antes de guardarlo.
- Previsualización local y eliminación segura de la foto anterior.
- Formulario de opinión para alumnos y docentes, con categoría, estrellas, comentario y consentimiento de publicación. Administración conserva solo la bandeja de supervisión.
- Historial personal con estado de revisión.
- Bandeja exclusiva de Administración para clasificar el feedback.
- Avatar o foto visibles en el encabezado, el ranking y las opiniones.

Las imágenes se guardan en `wwwroot/uploads/perfiles` con nombres internos asociados al identificador del usuario. En un despliegue con varias instancias deberá reemplazarse el almacenamiento local por un servicio compartido.

## Comunidad y progreso 2.9.0

- Nueva vista `Comunidad/Index` para ranking, rachas e insignias.
- Resumen compacto en el dashboard.
- Aviso visual al desbloquear una insignia en la pantalla de resultado.
- Control para participar u ocultarse del ranking sin borrar el progreso.


## Tutor IA 2.9.4

La experiencia incluye:

- Acceso desde el navbar y el panel del alumno.
- Alcance visible con Array y Lista como temas separados.
- Chat responsive con historial en `sessionStorage` aislado por `UsuarioId` y validación de propietario.
- Envío a la API únicamente de mensajes del alumno; el servidor vuelve a limpiarlos y validarlos.
- Preguntas sugeridas y continuidad contextual controlada.
- Mensajes diferenciados para rechazo, aclaración y respaldo local.
- Contador exacto de caracteres y validación del máximo de 800.
- Renderizado seguro mediante `textContent` y nodos DOM, sin `innerHTML` para mensajes o controles dinámicos.
- Indicador de escritura y prevención de doble envío desde la interfaz.
- Cabeceras `nosniff`, anti-framing, política de referentes, permisos restringidos y CSP.
- Opción `Nueva conversación` sin afectar el historial de ejercicios.
- Enlaces internos seguros hacia el ejercicio recomendado o el recorrido.
- Las solicitudes de solución completa reciben pistas y una invitación a presentar un intento propio.
- Las solicitudes de práctica utilizan ejercicios reales del catálogo y no consignas inventadas por la IA.

## Ejecución

Para el inicio normal, ejecutar `..\INICIAR_EDUCODE.bat`. En Development la Web MVC consume la API por `http://localhost:5168`, evitando dependencias del certificado HTTPS local.

```cmd
dotnet restore
dotnet run --project ".\src\EduCode.WebMVC\EduCode.WebMVC.csproj"
```

Abrir: `https://localhost:7180`

La URL base general se configura en `src/EduCode.WebMVC/appsettings.json` y la URL local de Development en `appsettings.Development.json`. El timeout del cliente contempla respuestas de proveedores externos y sus reintentos.


## Corrección 2.9.3 — aislamiento de conversaciones

- La clave del historial dejó de ser global para el navegador.
- Cada alumno utiliza `educode.tutor.historial.2.9.4.usuario.{UsuarioId}`.
- El contenido persistido incluye el propietario y se descarta si no coincide con la sesión autenticada.
- Las claves globales de versiones anteriores se eliminan al abrir el Tutor para evitar filtraciones entre cuentas.
- `Nueva conversación` borra únicamente el historial del alumno activo.

El historial continúa siendo local a la pestaña y al navegador. No se sincroniza entre dispositivos ni se guarda todavía en la base de datos.


## Corrección 2.9.4 — presentación de errores

- El alumno recibe mensajes claros sin referencias a API, puertos, endpoints o proveedores.
- Las respuestas se identifican como `Tutor EduCode` o `Tutor EduCode · IA`.
- Los errores de conexión y demora indican únicamente qué ocurrió y qué acción puede realizar el alumno.
- Los detalles técnicos permanecen disponibles en logs para el desarrollador.
- El cargador general ahora muestra `Procesando tu solicitud…`.
