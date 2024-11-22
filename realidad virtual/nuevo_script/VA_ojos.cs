using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;

public class EyeAngularVelocityCalculator : MonoBehaviour
{
    // Variables para los ojos
    private Vector2 currentEyeAngles = Vector2.zero;
    private Vector2 previousEyeAngles = Vector2.zero;

    // Frecuencia de muestreo (en segundos)
    private float deltaTime = 0.2f;
    private float timer = 0f;

    // Cola para promedio móvil (5 puntos)
    private Queue<Vector2> eyeAngularVelocities = new Queue<Vector2>();

    // Variables para normalización
    private Vector2 minEyeVelocity = new Vector2(float.MaxValue, float.MaxValue);
    private Vector2 maxEyeVelocity = new Vector2(float.MinValue, float.MinValue);

    // Listas para almacenar los datos
    private List<float> tiempos = new List<float>();
    private List<Vector2> velocidades = new List<Vector2>();
    private List<Vector2> velocidadesNormalizadas = new List<Vector2>();

    // Referencias para el eye tracking
    public Transform leftEye;
    public Transform rightEye;

    void Start()
    {
        if (leftEye == null || rightEye == null)
        {
            Debug.LogError("Se requieren referencias a los ojos. Por favor asignarlas en el inspector.");
            enabled = false;
            return;
        }
        Debug.Log("Sistema de eye tracking iniciado");
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= deltaTime)
        {
            // Actualizar ángulos de los ojos
            UpdateEyeAngles();

            // Calcular velocidad angular
            Vector2 eyeAngularVelocity = CalculateAngularVelocity(previousEyeAngles, currentEyeAngles);

            // Aplicar filtro de promedio móvil
            Vector2 filteredEyeAngularVelocity = ApplyMovingAverageFilter(eyeAngularVelocities, eyeAngularVelocity);

            // Normalizar velocidad
            UpdateNormalizationRanges(filteredEyeAngularVelocity);
            Vector2 normalizedVelocity = NormalizeVelocity(filteredEyeAngularVelocity, minEyeVelocity, maxEyeVelocity);

            // Guardar datos en las listas
            tiempos.Add(Time.time);
            velocidades.Add(filteredEyeAngularVelocity);
            velocidadesNormalizadas.Add(normalizedVelocity);

            // Actualizar valores previos
            previousEyeAngles = currentEyeAngles;

            timer = 0f;
        }
    }

    void UpdateEyeAngles()
    {
        // Calcular el promedio de la dirección de ambos ojos
        Vector3 leftRotation = leftEye.eulerAngles;
        Vector3 rightRotation = rightEye.eulerAngles;

        // Promedio de rotación de ambos ojos
        Vector3 averageRotation = new Vector3(
            Mathf.LerpAngle(leftRotation.x, rightRotation.x, 0.5f),
            Mathf.LerpAngle(leftRotation.y, rightRotation.y, 0.5f),
            0f
        );

        currentEyeAngles = new Vector2(averageRotation.y, averageRotation.x); // Yaw (horizontal), Pitch (vertical)
    }

    Vector2 CalculateAngularVelocity(Vector2 previousAngles, Vector2 currentAngles)
    {
        Vector2 deltaAngles = currentAngles - previousAngles;

        // Manejar el caso especial de cruce de 360/0 grados
        if (Mathf.Abs(deltaAngles.x) > 180f)
            deltaAngles.x = deltaAngles.x > 0 ? deltaAngles.x - 360f : deltaAngles.x + 360f;
        if (Mathf.Abs(deltaAngles.y) > 180f)
            deltaAngles.y = deltaAngles.y > 0 ? deltaAngles.y - 360f : deltaAngles.y + 360f;

        return deltaAngles / deltaTime;
    }

    Vector2 ApplyMovingAverageFilter(Queue<Vector2> velocityQueue, Vector2 newVelocity)
    {
        if (velocityQueue.Count >= 5)
        {
            velocityQueue.Dequeue();
        }
        velocityQueue.Enqueue(newVelocity);

        Vector2 sum = Vector2.zero;
        foreach (Vector2 velocity in velocityQueue)
        {
            sum += velocity;
        }
        return sum / velocityQueue.Count;
    }

    float NormalizeValue(float value, float min, float max)
    {
        if (Mathf.Approximately(max, min)) return 0;
        return Mathf.Clamp01((value - min) / (max - min));
    }

    void UpdateNormalizationRanges(Vector2 eyeVelocity)
    {
        minEyeVelocity.x = Mathf.Min(minEyeVelocity.x, eyeVelocity.x);
        minEyeVelocity.y = Mathf.Min(minEyeVelocity.y, eyeVelocity.y);
        maxEyeVelocity.x = Mathf.Max(maxEyeVelocity.x, eyeVelocity.x);
        maxEyeVelocity.y = Mathf.Max(maxEyeVelocity.y, eyeVelocity.y);
    }

    Vector2 NormalizeVelocity(Vector2 velocity, Vector2 min, Vector2 max)
    {
        return new Vector2(
            NormalizeValue(velocity.x, min.x, max.x),
            NormalizeValue(velocity.y, min.y, max.y)
        );
    }

    public void GuardarDatosEnCSV()
    {
        StringBuilder csv = new StringBuilder();

        // Agrega la cabecera al archivo CSV
        csv.AppendLine("Tiempo,VelocidadAngular_X,VelocidadAngular_Y,VelocidadNormalizada_X,VelocidadNormalizada_Y");

        // Recorre todas las velocidades registradas
        for (int i = 0; i < velocidades.Count; i++)
        {
            csv.AppendLine($"{tiempos[i]:F3},{velocidades[i].x:F6},{velocidades[i].y:F6}," +
                          $"{velocidadesNormalizadas[i].x:F6},{velocidadesNormalizadas[i].y:F6}");
        }

        // Define la ruta de la carpeta donde se guardará el archivo
        string carpeta = @"C:\Users\Manuel Delado\Documents";
        string prefijo = "velocidad_angular_ojos";
        string extension = ".csv";

        bool archivoGuardado = false;
        int intentos = 0;
        string rutaArchivo = "";

        while (!archivoGuardado && intentos < 5)
        {
            try
            {
                rutaArchivo = ObtenerSiguienteNombreArchivo(carpeta, prefijo, extension);
                File.WriteAllText(rutaArchivo, csv.ToString());
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
            string fechaHora = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            rutaArchivo = Path.Combine(carpeta, $"{prefijo}_{fechaHora}{extension}");
            File.WriteAllText(rutaArchivo, csv.ToString());
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

    void OnDisable()
    {
        GuardarDatosEnCSV();
    }
}