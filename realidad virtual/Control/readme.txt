CONTROL POR TECLADO: El sistema está configurado para responder a las siguientes teclas:

W: Avance frontal
S: Retroceso
A: Giro a la izquierda
D: Giro a la derecha


IMPLEMENTACIÓN DE JOYSTICK FÍSICO: Hardware necesario:
Microcontrolador STM32
Joystick analógico
Cable USB para conexión
Abrir Arduino IDE
Cargar el código "Joystick_Arduino"
En el menú Herramientas:
Seleccionar el modelo correcto de STM32
Configurar los parámetros según el microcontrolador
Compilar y cargar el código al microcontrolador
Descripción
Este proyecto implementa un puente de comunicación en tiempo real entre un Arduino y Unity utilizando WebSockets. Su propósito es recibir datos del joystick conectado al Arduino a través del puerto serie y transmitirlos a Unity, permitiendo su integración en entornos de realidad virtual, simulaciones o sistemas de control interactivos.

Funcionamiento
Detección y conexión con Arduino

Busca automáticamente el puerto donde está conectado el Arduino.
Establece una comunicación serie con una velocidad de 9600 baudios.
Si el dispositivo se desconecta, intenta reconectarlo de manera automática.
Inicialización del servidor WebSocket

Inicia un servidor en ws://localhost:8080.
Permite múltiples clientes conectados simultáneamente.
Maneja la conexión y desconexión de clientes en tiempo real.
Recepción y transmisión de datos

Lee datos del Arduino en formato:
Ángulo,Magnitud

Valida que los datos sean correctos (0°-360° para ángulo y 0%-100% para magnitud).
Si los datos son válidos, los transmite a todos los clientes WebSocket conectados (incluyendo Unity).
Se muestra en consola el estado de la conexión y los valores transmitidos.
Registro de eventos y errores

Se genera un archivo de log con los eventos de conexión, desconexión y errores.
Maneja errores de comunicación serial y WebSocket para garantizar estabilidad.
Requisitos
Hardware
Arduino con un joystick analógico u otro sensor que envíe datos en formato Ángulo,Magnitud.
Software
Python 3.8+
Bibliotecas necesarias:
pip install asyncio websockets pyserial
Unity (para recibir los datos a través de WebSockets).
Instrucciones de Uso: 
Conectar el Arduino a la PC y asegurarse de que esté enviando datos en formato Ángulo,Magnitud por el puerto serie.
Ejecutar el script en la PC donde está conectado el Arduino:
python bridge.py

Conectar un cliente WebSocket en Unity

Unity debe establecer una conexión con el servidor WebSocket en ws://localhost:8080 para recibir los datos del joystick en tiempo real.
Instalar el plugin WebSocket en Unity

Es necesario un plugin de WebSocket compatible con la versión de Unity utilizada.
El archivo .dll adecuado se encuentra en la carpeta websocket/lib.
Si se usa la versión .NET Standard, se debe entrar en la carpeta standard y copiar el archivo .dll.
Este archivo debe colocarse dentro de la carpeta Plugins en Unity para que el sistema lo reconozca correctamente.

Crear el script en Unity
En Unity, se debe crear un nuevo script en C# llamado MyMessage.cs.
Se debe pegar el código correspondiente dentro del script.
Si el plugin WebSocket se agregó correctamente, no deberían aparecer errores de compilación.
Asignar el script al objeto en Unity
Una vez validado que el script no tiene errores, se debe adjuntar al GameObject que representará al usuario o entidad que se desea mover en la escena. 
