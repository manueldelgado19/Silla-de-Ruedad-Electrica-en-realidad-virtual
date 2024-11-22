INSTRUCCIONES DE USO:


1. CONFIGURACIÓN INICIAL:
Crear un objeto vacío en la escena de Unity
Adjuntar este script (permanencia_cabeza.cs) al objeto
Asegurarse que existe una cámara principal en la escena o un xrig 

2. PARÁMETROS AJUSTABLES:
deltaTime (0.2f): Frecuencia de muestreo en segundos
rangeThreshold (5f): Rango en grados para considerar que la cabeza está "estable"

3. FUNCIONAMIENTO:
El script registra los ángulos de la cámara (que representa la cabeza)
Calcula cuánto tiempo permanece la cabeza relativamente estable
Normaliza los tiempos de permanencia para tener valores entre 0 y 1
Guarda los datos automáticamente en un archivo CSV cuando se desactiva el script

4. DATOS REGISTRADOS EN CSV:
Tiempo transcurrido
Tiempo de permanencia en eje X (horizontal)
Tiempo de permanencia en eje Y (vertical) 
Valores normalizados de permanencia en ambos ejes

5. UBICACIÓN DEL ARCHIVO CSV:
Por defecto se guarda en "C:\Users\Manuel Delado\Documents", esta ubicacion se cambia segun la direccion en la que el usario quiera guardar los datos 
Se genera con nombre "tiempo_permanencia" + número incremental , esto es importante ya que no si tienes un dato #130 y los borras , los datos se reinician, osease volvria a ser documento_1

Nota: este es solo en tiempo de permanencia, pero es lo mismo para los demas datos velocidad angular y desviacion, lo que varia es el punto 3. 
