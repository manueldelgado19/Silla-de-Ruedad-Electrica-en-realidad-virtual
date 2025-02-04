using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;

public class GazeStandardDeviation : MonoBehaviour
{
    // Propiedades públicas para el DataCombiner
    public Vector2 UltimaDesviacionGaze
    {
        get
        {
            if (desviacionesEstandar.Count > 0)
                return desviacionesEstandar[desviacionesEstandar.Count - 1];
            return Vector2.zero;
        }
    }

    public Vector2 UltimaDesviacionGazeNormalizada
    {
        get
        {
            if (desviacionesNormalizadas.Count > 0)
                return desviacionesNormalizadas[desviacionesNormalizadas.Count - 1];
            return Vector2.zero;
        }
    }

    private LineRenderer gazeRayLine;
    private Vector3 currentDirection;
    private float deltaTime = 0.2f;
    private float timer = 0f;

    private Queue<Vector2> gazeDirectionQueue = new Queue<Vector2>();
    private const int QUEUE_SIZE = 10;

    private const float MAX_DEVIATION = 100f;

    private List<float> tiempos = new List<float>();
    private List<Vector2> desviacionesEstandar = new List<Vector2>();
    private List<Vector2> desviacionesNormalizadas = new List<Vector2>();

    void Start()
    {
        gazeRayLine = GetComponent<LineRenderer>();
        if (gazeRayLine == null)
        {
            Debug.LogError("No se encontró el Line Renderer!");
            return;
        }

        for (int i = 0; i < QUEUE_SIZE; i++)
        {
            gazeDirectionQueue.Enqueue(Vector2.zero);
        }
    }

    void Update()
    {
        if (gazeRayLine == null) return;

        timer += Time.deltaTime;

        if (timer >= deltaTime)
        {
            UpdateGazeDirection();
            Vector2 stdDev = CalculateStandardDeviation();

            if (stdDev.magnitude > 0.0001f)
            {
                Vector2 normalizedStdDev = new Vector2(
                    NormalizeValue(stdDev.x),
                    NormalizeValue(stdDev.y)
                );

                tiempos.Add(Time.time);
                desviacionesEstandar.Add(stdDev);
                desviacionesNormalizadas.Add(normalizedStdDev);

                Debug.Log($"StdDev: {stdDev}, Normalized: {normalizedStdDev}");
            }

            timer = 0f;
        }
    }

    void UpdateGazeDirection()
    {
        Vector3[] positions = new Vector3[2];
        gazeRayLine.GetPositions(positions);
        currentDirection = (positions[1] - positions[0]).normalized;

        if (gazeDirectionQueue.Count >= QUEUE_SIZE)
        {
            gazeDirectionQueue.Dequeue();
        }
        gazeDirectionQueue.Enqueue(new Vector2(currentDirection.x, currentDirection.y));
    }

    Vector2 CalculateStandardDeviation()
    {
        if (gazeDirectionQueue.Count == 0) return Vector2.zero;

        Vector2 mean = Vector2.zero;
        foreach (Vector2 dir in gazeDirectionQueue)
        {
            mean += dir;
        }
        mean /= gazeDirectionQueue.Count;

        Vector2 variance = Vector2.zero;
        Vector2 direction = Vector2.zero; // Para mantener el signo

        foreach (Vector2 dir in gazeDirectionQueue)
        {
            Vector2 diff = dir - mean;
            variance.x += diff.x * diff.x;
            variance.y += diff.y * diff.y;
            direction += dir; // Acumulamos la dirección
        }

        // Determinamos el signo basado en la dirección predominante
        float signX = Mathf.Sign(direction.x);
        float signY = Mathf.Sign(direction.y);

        if (gazeDirectionQueue.Count > 1)
            variance /= (gazeDirectionQueue.Count - 1);

        // Aplicamos el signo a la desviación estándar
        return new Vector2(
            signX * Mathf.Sqrt(variance.x),
            signY * Mathf.Sqrt(variance.y)
        );
    }

    float NormalizeValue(float value)
    {
        // Normalización a [-1,1]
        return Mathf.Clamp(value / MAX_DEVIATION, -1f, 1f);
    }

    public void GuardarDatosEnCSV()
    {
        StringBuilder csv = new StringBuilder();
        csv.AppendLine("Tiempo,DesviacionEstandar_X,DesviacionEstandar_Y,DesviacionNormalizada_X,DesviacionNormalizada_Y");

        for (int i = 0; i < desviacionesEstandar.Count; i++)
        {
            csv.AppendLine($"{tiempos[i]:F3},{desviacionesEstandar[i].x:F6},{desviacionesEstandar[i].y:F6}," +
                          $"{desviacionesNormalizadas[i].x:F6},{desviacionesNormalizadas[i].y:F6}");
        }

        string carpeta = @"C:\Users\Manuel Delado\Documents";
        string prefijo = "desviacion_estandar_gaze";
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