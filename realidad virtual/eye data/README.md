Descripción del Proyecto

Este proyecto implementa un sistema de seguimiento y análisis de la dirección y velocidad de la mirada en Unity. Se utilizan LineRenderer y cálculos de desviación para analizar el comportamiento de la mirada en entornos de realidad virtual.

1. Requisitos

Unity 2021.3 o superior

C#

Biblioteca LineRenderer

Dispositivo compatible con VR (opcional)

2. Instalación

Usa los proyectos .cs que se incluyen en el pryecto, una vez los scripts en unity, adjuntarlos a lineredender que tiene los ojos.  
Configura el LineRenderer en los scripts de seguimiento de mirada.

3. Uso del Proyecto

Ejecuta la escena principal en Unity.
Asegúrate de que el LineRenderer esté correctamente configurado.
El sistema registrará la dirección y velocidad de la mirada en tiempo real.
Los datos serán analizados y almacenados en archivos CSV.
Utiliza Debug.Log() para verificar la información en la consola de Unity si es necesario.

4. Scripts Principales

4.1. GazeDataLogger.cs

Registra la dirección y velocidad angular de la mirada.
Aplica un filtro de media móvil para suavizar los datos.
Normaliza los valores de velocidad en un rango predefinido.
Guarda los datos en un archivo CSV al finalizar la simulación.

4.2. GazeDirectionWithArea.cs

Detecta en qué zona de la pantalla se encuentra la mirada.
Clasifica la dirección de la mirada en categorías predefinidas.
Utiliza BoxCollider para definir áreas de interés.
Registra los datos de dirección en un archivo CSV.

4.3. GazeStandardDeviation.cs

Calcula la desviación estándar de la dirección de la mirada.
Utiliza una ventana de datos para analizar variaciones.
Normaliza los valores de desviación para su comparación.
Almacena los valores en un archivo CSV para su análisis posterior.

4.4. NormalizedGazeDwellTimeCalculator.cs

Calcula el tiempo de permanencia de la mirada en diferentes áreas.
Aplica un filtro de Kalman adaptativo para mejorar la precisión.
Utiliza un buffer circular para el análisis de datos en tiempo real.
Guarda los tiempos de permanencia en un archivo CSV para análisis estadístico.

5. Guardado de Datos en CSV

Los datos de velocidad, desviación y dirección de la mirada se registran automáticamente.
Se guardan en archivos CSV en C:\Users\Manuel Delgado\Documents.
En caso de archivos duplicados, se asigna un sufijo numérico o timestamp para evitar sobrescribir datos.
Los CSV incluyen columnas con Tiempo, Velocidad Gaze (X,Y), Desviación Estándar (X,Y), Dirección, y Tiempo de Permanencia.

6. Consideraciones Finales

Configura el intervalo de muestreo en GazeDataLogger.cs para ajustar la frecuencia de registro de datos.
Modifica los valores en GazeDirectionWithArea.cs si necesitas redefinir las zonas de interés.
Si el filtrado de datos es demasiado agresivo o insuficiente, ajusta los parámetros en NormalizedGazeDwellTimeCalculator.cs.
Activa Debug.Log() en los scripts si necesitas verificar el comportamiento en tiempo real.

