# Requisitos Funcionales (Lo que hará la aplicación)

## Visualización de Hora Mundial:

Permitir consultar la hora actual en diferentes zonas horarias/partes del mundo de manera sencilla.

## Diseño de Tarjetas Desplegables (Flip Cards):

La interfaz principal debe comportarse mediante tarjetas desplegables para organizar las distintas herramientas (reloj, alarmas, temporizador, hora mundial).

## Gestión de Alarmas y Temporizador:

Sistema para configurar, activar y desactivar alarmas personalizadas.

## Incluir una funcionalidad de temporizador totalmente operable.

## Modo Pantalla Completa ("Modo Reloj de Escritorio"):

Capacidad de expandir el reloj para cubrir toda la pantalla sin bloquear el sistema operativo, permitiendo que la PC funcione visualmente como un reloj dedicado.

## Modo Siempre Visible (Always on Top / Widget):

Opción para fijar la aplicación en primer plano sobre otras ventanas, actuando como un widget flotante, con la facilidad de activar o desactivar este comportamiento a gusto del usuario.

## Personalización Visual (Neón):

* Tema oscuro por defecto.

* Activación de un halo o borde de color neón alrededor de la interfaz.

* Selector de colores para modificar el tono del borde neón según la preferencia del usuario.

# Requisitos No Funcionales (Cómo debe rendir y construirse)

## Stack Tecnológico Moderno (.NET Puntero):

Desarrollado utilizando las tecnologías más recientes del ecosistema .NET (como .NET 8/9 y frameworks de interfaz modernos orientados a escritorio como WinUI 3 o MAUI para escritorio, asegurando rendimiento fluido y aceleración por hardware).

## Rendimiento y Ligereza:

Bajo consumo de recursos de CPU y memoria RAM, ideal para mantenerse activo en segundo plano o en primer plano de manera continua sin ralentizar el equipo.

## Experiencia de Usuario (UX/UI):

Interfaz fluida, moderna, con transiciones suaves al desplegar las tarjetas y animaciones limpias en el efecto neón.

## Persistencia de Configuración:

La aplicación debe recordar las preferencias del usuario (alarmas guardadas, zonas horarias favoritas, color de neón seleccionado y estado del modo siempre visible) al cerrar y volver a abrir el programa.

## Compatibilidad de Sistema:

Diseñado nativamente para ejecutarse en entornos Windows modernos, aprovechando las APIs del sistema para el manejo de ventanas flotantes y pantalla completa.