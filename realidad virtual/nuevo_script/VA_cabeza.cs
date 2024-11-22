// Información sobre los valores de VelocidadAngular_X (Yaw, eje horizontal):
// - Valores positivos: Indican que la rotación en el eje horizontal está ocurriendo hacia la derecha
//   (girar la cabeza hacia la derecha).
// - Valores negativos: Indican que la rotación en el eje horizontal está ocurriendo hacia la izquierda
//   (girar la cabeza hacia la izquierda).

// Información sobre los valores de VelocidadAngular_Y (Pitch, eje vertical):
// - Valores positivos: Indican que la cabeza (o la cámara) se está inclinando hacia arriba.
// - Valores negativos: Indican que la cabeza (o la cámara) se está inclinando hacia abajo.
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;

public class AngularVelocityCalculator : MonoBehaviour
{
    // Variables para la cabeza
    private Vector2 currentHeadAngles = Vector2.zero;
    private Vector2 previousHeadAngles = Vector2.zero;

    // Frecuencia de muestreo (en segundos)
    private float deltaTime = 0.2f;
    private float timer = 0f;

    // Fila para promedio móvil (5 puntos)
    private Queue<Vector2> headAngularVelocities = new Queue<Vector2>();

    // Variables para normalización
    private Vector2 minHeadVelocity = new Vector2(float.MaxValue, float.MaxValue);
    private Vector2 maxHeadVelocity = new Vector2(float.MinValue, float.MinValue);

    // Listas para almacenar los datos
    private List<float> tiempos = new List<float>();
    private List<Vector2> velocidades = new List<Vector2>();
    private List<Vector2> velocidadesNormalizadas = new List<Vector2>();

    void Start()
    {
        Debug.Log("Sistema de tracking iniciado");
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= deltaTime)
        {
            // Actualizar ángulos de la cabeza
            UpdateHeadAngles();

            // Calcular velocidad angular
            Vector2 headAngularVelocity = CalculateAngularVelocity(previousHeadAngles, currentHeadAngles);

            // Aplicar filtro de promedio móvil
            Vector2 filteredHeadAngularVelocity = ApplyMovingAverageFilter(headAngularVelocities, headAngularVelocity);

            // Normalizar velocidad
            UpdateNormalizationRanges(filteredHeadAngularVelocity);
            Vector2 normalizedVelocity = NormalizeVelocity(filteredHeadAngularVelocity, minHeadVelocity, maxHeadVelocity);

            // Guardar datos en las listas
            tiempos.Add(Time.time);
            velocidades.Add(filteredHeadAngularVelocity);
            velocidadesNormalizadas.Add(normalizedVelocity);

            // Actualizar valores previos
            previousHeadAngles = currentHeadAngles;

            timer = 0f;
        }
    }

    void UpdateHeadAngles()
    {
        if (Camera.main != null)
        {
            Vector3 rotation = Camera.main.transform.eulerAngles;
            currentHeadAngles = new Vector2(rotation.y, rotation.x); // Yaw (horizontal), Pitch (vertical)
        }
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

    void UpdateNormalizationRanges(Vector2 headVelocity)
    {
        minHeadVelocity.x = Mathf.Min(minHeadVelocity.x, headVelocity.x);
        minHeadVelocity.y = Mathf.Min(minHeadVelocity.y, headVelocity.y);
        maxHeadVelocity.x = Mathf.Max(maxHeadVelocity.x, headVelocity.x);
        maxHeadVelocity.y = Mathf.Max(maxHeadVelocity.y, headVelocity.y);
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
        string prefijo = "velocidad_angular";
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