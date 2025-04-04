Descripción del Proyecto

Este proyecto implementa un sistema de generación y seguimiento de rutas en Unity utilizando LineRenderer y algoritmos de Bezier. Está diseñado para evaluar la desviación en el movimiento de una silla de ruedas en un entorno virtual, permitiendo la captura de datos precisos para su análisis.

1. Requisitos

Unity 2021.3 o superior

C#
Biblioteca LineRenderer
Dispositivo compatible con VR (opcional)

2. Instalacion

Arrastrar los archivos que se encuentran aqui cs al repositorio.

4. Uso del Proyecto

Ejecuta la escena principal en Unity.
Coloca la silla de ruedas en la escena y define su punto de inicio.
Observa la generación automática de rutas y el cálculo de desviación.
Analiza los datos de desviación en tiempo real dentro del Editor de Unity.
Guarda los datos generados en un archivo CSV para su posterior análisis.

4. Scripts Principales

4.1. CustomCurve.cs

Define anclajes y puntos de control para generar curvas de Bezier.
Utiliza LineRenderer para visualizar la trayectoria en tiempo real.
Permite ajustar el suavizado y el ancho de la línea mediante parámetros configurables.

4.2. RutaManager.cs

Registra la posición real e ideal de la silla de ruedas.
Calcula la desviación con respecto a rutas predefinidas en la escena.
Guarda los datos en un archivo CSV automáticamente al finalizar la simulación.

4.3. zonas_visual.cs

Detecta cuando la silla de ruedas entra en zonas específicas dentro de la escena.
Registra tareas y comandos según la zona activada.
Interactúa con DataCombiner para actualizar registros de actividad.

4.3.  Autopathfolower.cs

Detecta la ruta que tiene que seguir y mediante procesos internos realiza el recorrido automaticamente siguiendo la ruta (linea)

4.4 EtapaAudioManager.cs

Ya que el experimento lleva diferentes audios debido a que son diferentes las etapas que tiene que realizar el usurio, este e sun gestor en el cual se le asigna el audio que se quiera escuchar en la etapa, esto va de la mano con el selector de participante , ya que este tiene que leer  de este cual es la etapa seleccionada

5. Guardado de Datos en CSV

Los datos de posición y desviación se registran automáticamente cada cierto intervalo de tiempo.
Se guardan en archivos CSV en C:\Users\Manuel Delgado\Documents. (aqui poner la ruda en donde quieras que se guarde el archivo )
En caso de archivos duplicados, se asigna un sufijo numérico o timestamp para evitar sobrescribir datos.
El CSV incluye columnas con Tiempo, Posición Real (X,Y,Z), Posición Ideal (X,Z), y Desviación.

6. Consideraciones Finales

Asegúrate de revisar el RutaManager.cs para configurar el intervalo de muestreo de datos.

Si se requiere un ajuste en la visualización de las rutas, modifica los valores de suavizado en CustomCurve.cs.

Para depuración, activa la opción de Debug.Log() en RutaManager.cs y zonas_visual.cs para visualizar registros en la consola de Unity.
