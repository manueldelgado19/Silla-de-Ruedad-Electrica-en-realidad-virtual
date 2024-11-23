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

CONEXIÓN ARDUINO-UNIty
Script "MyMessage" en Unity
Plugin Ardity para la comunicación serial

PASOS PARA LA CONEXIÓN:
Importar Ardity a tu proyecto de Unity, este pluggins se encuentra en la siguiente liga : https://assetstore.unity.com/packages/tools/integration/ardity-arduino-unity-communication-made-easy-123819
si por alguna razon el link no funciona, buscar en asststore, ardity.

Adjuntar el script "MyMessage" al objeto que deseas controlar
Configurar el puerto serial en Unity este es serialcontroller, aqui se debe de adjuntar el maymessage

