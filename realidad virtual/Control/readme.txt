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

En Unity, conectar un cliente WebSocket a ws://localhost:8080 para recibir los datos del joystick en tiempo real.7
El programa en unity necesita un pluggin, este es websocket y se agrega a unity. 
Dentro de unity creamos un programa que se llame mymessage y pegamos el codigo, deberia salir sin errores si agregamos el webscoket a pluggins. 
si sale sin errores lo agregamos al objeto, usuario que se quiera mover. 
