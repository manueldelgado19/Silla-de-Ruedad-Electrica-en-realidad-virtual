using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class MyMessageListener : MonoBehaviour
{
    [Header("Referencias de Objetos")]
    public GameObject wheelchair;
    public Camera mainCamera;

    [Header("Configuración de Movimiento")]
    public float movementSpeed = 5.0f;
    public float rotationSpeed = 100.0f;
    public float deadzone = 0.1f;

    [Header("Configuración de Ejes")]
    public bool invertX = false;
    public bool invertY = false;

    [Header("Diagnóstico")]
    public bool enableDebugLogging = true;
    public bool showGizmos = true;

    [Header("Configuración de Registro")]
    public string carpetaGuardado = @"C:\Users\Manuel Delado\Documents";
    public string prefijoArchivo = "datos_joystick";

    // Variables para recolección de datos
    private List<float> tiempos = new List<float>();
    private List<Vector2> inputsJoystick = new List<Vector2>();
    private List<float> angulos = new List<float>();
    private List<float> magnitudes = new List<float>();
    private List<float> posicionX = new List<float>();
    private List<float> posicionZ = new List<float>();
    private List<float> rotacionY = new List<float>();
    private List<string> acciones = new List<string>();
    private float tiempoInicio;

    // Variables de control
    private string debugLogPath;
    private float lastMessageTime;
    private const float MESSAGE_TIMEOUT = 3.0f;
    private Vector2 lastJoystickInput;
    private bool isConnected;

    void Start()
    {
        if (!wheelchair)
        {
            Debug.LogError("¡Error: No se ha asignado el GameObject de la silla de ruedas!");
            enabled = false;
            return;
        }

        if (!mainCamera)
        {
            mainCamera = Camera.main;
            if (!mainCamera)
            {
                Debug.LogError("¡Error: No se encontró ninguna cámara!");
                enabled = false;
                return;
            }
        }

        tiempoInicio = Time.time;
        InitializeDebugLog();
        LogMessage("Sistema iniciado - Esperando datos del joystick...");
    }

    void InitializeDebugLog()
    {
        debugLogPath = Path.Combine(Application.dataPath, "wheelchair_debug.txt");
        lastMessageTime = Time.time;
        try
        {
            File.WriteAllText(debugLogPath, $"=== Inicio de sesión: {DateTime.Now} ===\n");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al crear archivo de log: {e.Message}");
            enableDebugLogging = false;
        }
    }

    void Update()
    {
        if (Time.time - lastMessageTime > MESSAGE_TIMEOUT && isConnected)
        {
            LogWarning($"¡No se han recibido datos del joystick en {MESSAGE_TIMEOUT} segundos!");
            isConnected = false;
        }

        if (lastJoystickInput.magnitude > deadzone)
        {
            ApplyMovement(lastJoystickInput);
        }
    }

    void OnMessageArrived(string msg)
    {
        lastMessageTime = Time.time;
        isConnected = true;
        LogMessage($"Datos recibidos: {msg}");

        if (ProcessJoystickData(msg, out Vector2 joystickInput))
        {
            lastJoystickInput = joystickInput;
            RegistrarDatos(msg, joystickInput);
        }
    }

    bool ProcessJoystickData(string msg, out Vector2 joystickInput)
    {
        joystickInput = Vector2.zero;
        string[] data = msg.Split(',');

        if (data.Length < 2)
        {
            LogError($"Formato de mensaje inválido. Se esperan 2 valores, recibidos: {data.Length}");
            return false;
        }

        try
        {
            float angle = float.Parse(data[0]);
            float magnitude = float.Parse(data[1]);

            if (invertX) angle = 360 - angle;
            if (invertY) magnitude *= -1;

            angle = NormalizeAngle(angle);

            float rad = angle * Mathf.Deg2Rad;
            joystickInput.x = Mathf.Cos(rad) * magnitude / 100f;
            joystickInput.y = Mathf.Sin(rad) * magnitude / 100f;

            // Invertir el eje X para cambiar la dirección de giro
            joystickInput.x *= -1;

            LogMessage($"Procesado - Ángulo: {angle:F2}° Magnitud: {magnitude:F2}% -> X: {joystickInput.x:F2} Y: {joystickInput.y:F2}");
            return true;
        }
        catch (Exception e)
        {
            LogError($"Error al procesar datos del joystick: {e.Message}");
            return false;
        }
    }

    void RegistrarDatos(string mensajeOriginal, Vector2 input)
    {
        string[] data = mensajeOriginal.Split(',');
        float angulo = float.Parse(data[0]);
        float magnitud = float.Parse(data[1]);

        // Registrar tiempo
        tiempos.Add(Time.time - tiempoInicio);

        // Registrar input del joystick
        inputsJoystick.Add(input);

        // Registrar ángulo y magnitud
        angulos.Add(angulo);
        magnitudes.Add(magnitud);

        // Registrar solo las posiciones y rotaciones relevantes
        posicionX.Add(wheelchair.transform.position.x);
        posicionZ.Add(wheelchair.transform.position.z);
        rotacionY.Add(wheelchair.transform.eulerAngles.y);

        // Determinar y registrar la acción con direcciones invertidas
        string accion = DeterminarAccion(input);
        acciones.Add(accion);
    }

    void ApplyMovement(Vector2 input)
    {
        if (!wheelchair) return;

        // Aplicar rotación y movimiento simultáneamente
        float rotationAmount = input.x * rotationSpeed * Time.deltaTime;
        wheelchair.transform.Rotate(0, rotationAmount, 0);

        float moveAmount = input.y * movementSpeed * Time.deltaTime;
        Vector3 movement = wheelchair.transform.right * (-moveAmount);
        wheelchair.transform.position += movement;

        LogMessage($"Movimiento aplicado - Rotación: {rotationAmount:F2}, Movimiento: {moveAmount:F2}");
    }

    string DeterminarAccion(Vector2 input)
    {
        if (input.magnitude <= deadzone) return "Detenido";

        List<string> accionesActuales = new List<string>();

        if (input.y > deadzone) accionesActuales.Add("Avanzando");
        else if (input.y < -deadzone) accionesActuales.Add("Retrocediendo");

        // Invertir la detección de dirección para el registro
        if (input.x > deadzone) accionesActuales.Add("Girando Izquierda");
        else if (input.x < -deadzone) accionesActuales.Add("Girando Derecha");

        return string.Join(" + ", accionesActuales);
    }

    public void GuardarDatosEnCSV()
    {
        StringBuilder csv = new StringBuilder();

        // Cabecera simplificada
        csv.AppendLine("Tiempo,Input_X,Input_Y,Angulo,Magnitud,Posicion_X,Posicion_Z,Rotacion_Y,Accion");

        // Datos simplificados
        for (int i = 0; i < tiempos.Count; i++)
        {
            csv.AppendLine(
                $"{tiempos[i]:F3}," +
                $"{inputsJoystick[i].x:F6},{inputsJoystick[i].y:F6}," +
                $"{angulos[i]:F2},{magnitudes[i]:F2}," +
                $"{posicionX[i]:F6},{posicionZ[i]:F6}," +
                $"{rotacionY[i]:F2}," +
                $"{acciones[i]}"
            );
        }

        GuardarArchivo(csv.ToString());
    }

    void GuardarArchivo(string contenido)
    {
        bool archivoGuardado = false;
        int intentos = 0;
        string rutaArchivo = "";

        while (!archivoGuardado && intentos < 5)
        {
            try
            {
                rutaArchivo = ObtenerSiguienteNombreArchivo(carpetaGuardado, prefijoArchivo, ".csv");
                File.WriteAllText(rutaArchivo, contenido);
                archivoGuardado = true;
                Debug.Log($"Datos guardados exitosamente en: {rutaArchivo}");
            }
            catch (IOException)
            {
                intentos++;
                System.Threading.Thread.Sleep(100);
            }
        }

        if (!archivoGuardado)
        {
            string fechaHora = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            rutaArchivo = Path.Combine(carpetaGuardado, $"{prefijoArchivo}_{fechaHora}.csv");
            File.WriteAllText(rutaArchivo, contenido);
            Debug.Log($"Datos guardados con timestamp en: {rutaArchivo}");
        }
    }

    string ObtenerSiguienteNombreArchivo(string carpeta, string prefijo, string extension)
    {
        int numero = 1;
        string nombreArchivo;
        do
        {
            nombreArchivo = Path.Combine(carpeta, $"{prefijo}{numero}{extension}");
            numero++;
        } while (File.Exists(nombreArchivo));
        return nombreArchivo;
    }

    void OnConnectionEvent(bool success)
    {
        isConnected = success;
        LogMessage($"Estado de conexión: {(success ? "Conectado" : "Desconectado")}");
    }

    float NormalizeAngle(float angle)
    {
        angle = angle % 360;
        return angle < 0 ? angle + 360 : angle;
    }

    void LogMessage(string message)
    {
        if (!enableDebugLogging) return;

        string timeStamp = DateTime.Now.ToString("HH:mm:ss.fff");
        string logMessage = $"[{timeStamp}] {message}\n";

        Debug.Log(logMessage);
        try
        {
            File.AppendAllText(debugLogPath, logMessage);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al escribir en el log: {e.Message}");
        }
    }

    void LogError(string message)
    {
        LogMessage($"ERROR: {message}");
        Debug.LogError(message);
    }

    void LogWarning(string message)
    {
        LogMessage($"ADVERTENCIA: {message}");
        Debug.LogWarning(message);
    }

    void OnDrawGizmos()
    {
        if (!showGizmos || !wheelchair) return;

        // Dibujar dirección de movimiento
        Gizmos.color = Color.blue;
        Vector3 position = wheelchair.transform.position;
        Gizmos.DrawLine(position, position + wheelchair.transform.forward * 2);

        // Dibujar input del joystick
        if (lastJoystickInput.magnitude > deadzone)
        {
            Gizmos.color = Color.green;
            Vector3 inputDirection = new Vector3(lastJoystickInput.x, 0, lastJoystickInput.y);
            Gizmos.DrawLine(position, position + inputDirection * 2);
        }
    }

    void OnDisable()
    {
        GuardarDatosEnCSV();
    }
}
