Descripción del Proyecto

Este proyecto implementa un sistema de seguimiento y análisis de la dirección y velocidad angular de la cabeza en Unity. Se utilizan cálculos de ángulos horizontales y verticales en relación con una referencia para clasificar la dirección de la cabeza en diferentes categorías. Además, se analiza la velocidad angular con normalización de datos y se almacenan en archivos CSV para su análisis.

1. Requisitos

Unity 2021.3 o superior
C#
Camera.main
Dispositivo compatible con VR (opcional)

2. Instalación

Toma los .cs que se icnluyen en el proyectos , y ponlos en unity, este es bastanre amigable ya que fucniona en base a la rotacion de la camara, entonces solo necesitara la camara, nada mas 
Configura la referencia del objeto de dirección en el Inspector de Unity.

3. Uso del Proyecto

Ejecuta la escena principal en Unity.
Configura el objeto de referencia (frontReference) en la escena.
El sistema calculará en tiempo real la dirección y velocidad angular de la cabeza.
Los datos se registrarán en listas y se guardarán en archivos CSV al finalizar la simulación.
Usa Debug.Log() para visualizar la información en la consola de Unity si es necesario.

4. Scripts Principales

4.1. HeadDirectionTracker.cs

Calcula los ángulos horizontales y verticales en relación con una referencia frontal.
Determina la dirección de la cabeza en base a umbrales predefinidos.
Almacena la dirección de la cabeza en listas y archivos CSV.
Soporta personalización del intervalo de muestreo y umbrales de ángulo.
4.2. StandardDeviationCalculator.cs

Calcula la desviación estándar de la dirección de la cabeza en tiempo real.
Aplica un filtro de promedio móvil para mejorar la precisión de los datos.
Normaliza los valores de desviación para su comparación.
Almacena los valores en un archivo CSV para análisis posterior

4.3. NormalizedDwellTimeCalculator.cs

Calcula el tiempo de permanencia de la cabeza en diferentes áreas.
Define un umbral de estabilidad para detectar movimientos prolongados en una misma posición.
Normaliza el tiempo de permanencia para facilitar su interpretación.
Guarda los tiempos de permanencia en un archivo CSV para análisis estadístico.

4.4. AngularVelocityCalculator.cs

Calcula la velocidad angular de la cabeza en los ejes horizontal y vertical.
Aplica un filtro de promedio móvil para suavizar las variaciones bruscas.
Normaliza los valores de velocidad angular para su análisis comparativo.
Guarda los valores en un archivo CSV con detalles de tiempo y velocidad angular.

5. Guardado de Datos en CSV

Los datos de dirección, velocidad angular y tiempo de permanencia se registran automáticamente.
Se almacenan en archivos CSV en C:\Users\Manuel Delgado\Documents.
En caso de archivos duplicados, se asigna un sufijo numérico o timestamp para evitar sobrescribir datos.
Los CSV incluyen columnas con Tiempo, Ángulo Horizontal, Ángulo Vertical, Dirección, Velocidad Angular X/Y, Desviación Estándar, y Tiempo de Permanencia.

6. Consideraciones Finales

Configura el intervalo de muestreo en los scripts para ajustar la frecuencia de registro de datos.
Modifica los valores de horizontalThreshold y verticalThreshold en HeadDirectionTracker.cs para ajustar la sensibilidad.
Ajusta los parámetros de normalización en AngularVelocityCalculator.cs y StandardDeviationCalculator.cs si es necesario.
Activa Debug.Log() en los scripts si necesitas verificar el comportamiento en tiempo real.
