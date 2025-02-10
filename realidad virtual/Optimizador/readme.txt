Cuando el ambiente tiene una alta resolución, se tiende a tener una alta cantidad de polígonos. Esta cantidad de polígonos se puede observar en la ventana de Juego en la sección de Estadísticas, en Tris.
Pero no todos los objetos tienen la misma cantidad de polígonos, algunos objetos tienen menos o mayor cantidad. Por ello se hizo una herramienta que analiza los objetos que se le adjuntan.
Teniendo el código en tu escena al guardar (Ctrl+S), al abrir Ventana ( WIindow ) , la cual se encuentra en la barra superior, saldrá una opción que dirá "Analizador de Malla".
De esta manera sabras cuales son aquellos obejtos que necesitan simplificar malla.
para semiplicar malla se hace lo siguiente :
para empezar se necesaira meshsimplifer como paquete esto es lo que hara jalar el progrma con nombre polignos_reducir.
Abrir el Administrador de paquetes en Unity (Ventana > Administrador de paquetes)
Hacer clic en el botón '+' en la esquina superior izquierda
Seleccionar "Agregar paquete desde la URL de git"
Pegue esta URL: https://github.com/Whinarn/UnityMeshSimplifier.git
Teniendo el código poligonos_reducir en Unity, se adjunta este a un objeto vacío. Automáticamente saldrá un menú en el cual se le agregan aquellos objetos que se busca simplificar los polígonos en los objetos.
Se debe agregar el sript boton poligonos el cual lo que hace es simplemente ser un boton para iniciar todo el proceso. 


INSTANCIA DE GPU: Una herramienta para Unity que ayuda a configurar Instancia de GPU de manera automática es la siguiente:
Función: GPU Instancing permite renderizar Múltiples copias del mismo objeto de manera más eficiente, mejorando el rendimiento de la simulación cuando tienes muchos objetos idénticos en la escena.

Pases para usarlo:

Crear un objeto vacío en la escena.
Adjuntar este script al objeto
(Opcional) Asignar una carpeta raíz para procesar
Usar el botón "INICIAR PROCESO DE INSTANCIA DE GPU"
Revisar los resultados en la consola de Unity
