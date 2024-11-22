using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;

public class StandardDeviationCalculator : MonoBehaviour
{
    // Variables para la cabeza
    private Vector2 currentHeadAngles = Vector2.zero;

    // Frecuencia de muestreo (en segundos)
    private float deltaTime = 0.2f;
    private float timer = 0f;

    // Fila para promedio móvil (5 puntos)
    private Queue<Vector2> headAnglesQueue = new Queue<Vector2>();

    // Variables para normalización
    private Vector2 minStandardDeviation = new Vector2(float.MaxValue, float.MaxValue);
    private Vector2 maxStandardDeviation = new Vector2(float.MinValue, float.MinValue);

    // Listas para almacenar los datos
    private List<float> tiempos = new List<float>();
    private List<Vector2> desviacionesEstandar = new List<Vector2>();
    private List<Vector2> desviacionesEstandarNormalizadas = new List<Vector2>();

    void Start()
    {
        Debug.Log("Sistema de cálculo de desviación estándar con normalización iniciado");
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= deltaTime)
        {
            // Actualizar ángulos de la cabeza
            UpdateHeadAngles();

            // Aplicar filtro de promedio móvil y calcular desviación estándar
            Vector2 stdDev = CalculateStandardDeviation(headAnglesQueue, currentHeadAngles);

            // Actualizar rangos de normalización
            UpdateNormalizationRanges(stdDev);

            // Normalizar desviación estándar
            Vector2 normalizedStdDev = NormalizeStandardDeviation(stdDev, minStandardDeviation, maxStandardDeviation);

            // Guardar datos en las listas
            tiempos.Add(Time.time);
            desviacionesEstandar.Add(stdDev);
            desviacionesEstandarNormalizadas.Add(normalizedStdDev);

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

    Vector2 CalculateStandardDeviation(Queue<Vector2> angleQueue, Vector2 newAngles)
    {
        if (angleQueue.Count >= 5)
        {
            angleQueue.Dequeue();
        }
        angleQueue.Enqueue(newAngles);

        // Calcular promedio
        Vector2 mean = Vector2.zero;
        foreach (Vector2 angle in angleQueue)
        {
            mean += angle;
        }
        mean /= angleQueue.Count;

        // Calcular desviación estándar
        Vector2 variance = Vector2.zero;
        foreach (Vector2 angle in angleQueue)
        {
            Vector2 diff = angle - mean;
            variance.x += diff.x * diff.x;
            variance.y += diff.y * diff.y;
        }
        variance /= angleQueue.Count;

        return new Vector2(Mathf.Sqrt(variance.x), Mathf.Sqrt(variance.y));
    }

    void UpdateNormalizationRanges(Vector2 stdDev)
    {
        minStandardDeviation.x = Mathf.Min(minStandardDeviation.x, stdDev.x);
        minStandardDeviation.y = Mathf.Min(minStandardDeviation.y, stdDev.y);
        maxStandardDeviation.x = Mathf.Max(maxStandardDeviation.x, stdDev.x);
        maxStandardDeviation.y = Mathf.Max(maxStandardDeviation.y, stdDev.y);
    }

    Vector2 NormalizeStandardDeviation(Vector2 stdDev, Vector2 min, Vector2 max)
    {
        return new Vector2(
            NormalizeValue(stdDev.x, min.x, max.x),
            NormalizeValue(stdDev.y, min.y, max.y)
        );
    }

    float NormalizeValue(float value, float min, float max)
    {
        if (Mathf.Approximately(max, min)) return 0;
        return Mathf.Clamp01((value - min) / (max - min));
    }

    public void GuardarDatosEnCSV()
    {
        StringBuilder csv = new StringBuilder();

        // Agrega la cabecera al archivo CSV
        csv.AppendLine("Tiempo,DesviacionEstandar_X,DesviacionEstandar_Y,DesviacionNormalizada_X,DesviacionNormalizada_Y");

        // Recorre todas las desviaciones estándar registradas
        for (int i = 0; i < desviacionesEstandar.Count; i++)
        {
            csv.AppendLine($"{tiempos[i]:F3},{desviacionesEstandar[i].x:F6},{desviacionesEstandar[i].y:F6}," +
                          $"{desviacionesEstandarNormalizadas[i].x:F6},{desviacionesEstandarNormalizadas[i].y:F6}");
        }

        // Define la ruta de la carpeta donde se guardará el archivo
        string carpeta = @"C:\Users\Manuel Delado\Documents";
        string prefijo = "desviacion_estandar";
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
