using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;

public class GazeDataLogger : MonoBehaviour
{
    // Propiedades públicas para el DataCombiner
    public Vector2 UltimaVelocidadGaze
    {
        get { return velocidades.Count > 0 ? velocidades[velocidades.Count - 1] : Vector2.zero; }
    }

    public Vector2 UltimaVelocidadGazeNormalizada
    {
        get { return velocidadesNormalizadas.Count > 0 ? velocidadesNormalizadas[velocidadesNormalizadas.Count - 1] : Vector2.zero; }
    }

    private LineRenderer gazeRayLine;
    private Vector3 currentDirection;
    private Vector3 previousDirection;
    private float deltaTime = 0.2f;
    private float timer = 0f;
    private Queue<Vector2> gazeVelocities = new Queue<Vector2>();
    private const int WINDOW_SIZE = 5;
    private const float MAX_VELOCITY_X = 110f;
    private const float MAX_VELOCITY_Y = 90f;
    private const float MOVEMENT_THRESHOLD = 0.001f;
    private List<float> tiempos = new List<float>();
    private List<Vector2> velocidades = new List<Vector2>();
    private List<Vector2> velocidadesNormalizadas = new List<Vector2>();

    void Start()
    {
        gazeRayLine = GetComponent<LineRenderer>();
        if (gazeRayLine == null)
        {
            Debug.LogError("¡No se encontró el Line Renderer!");
            enabled = false;
            return;
        }
        InitializeDirections();
        InitializeQueues();
        Debug.Log("Sistema de seguimiento de mirada iniciado");
    }

    void InitializeQueues()
    {
        gazeVelocities.Clear();
        for (int i = 0; i < WINDOW_SIZE; i++)
        {
            gazeVelocities.Enqueue(Vector2.zero);
        }
    }

    void InitializeDirections()
    {
        Vector3[] positions = new Vector3[2];
        gazeRayLine.GetPositions(positions);
        if (positions[0] != positions[1])
        {
            previousDirection = (positions[1] - positions[0]).normalized;
            currentDirection = previousDirection;
        }
        else
        {
            previousDirection = Vector3.forward;
            currentDirection = Vector3.forward;
        }
    }

    void Update()
    {
        if (gazeRayLine == null) return;

        timer += Time.deltaTime;
        if (timer >= deltaTime)
        {
            if (UpdateGazeDirection())
            {
                Vector2 gazeAngularVelocity = CalculateGazeVelocity();
                Vector2 filteredVelocity = ApplyMovingAverageFilter(gazeVelocities, gazeAngularVelocity);

                if (Mathf.Abs(filteredVelocity.x) > MOVEMENT_THRESHOLD || Mathf.Abs(filteredVelocity.y) > MOVEMENT_THRESHOLD)
                {
                    Vector2 normalizedVelocity = new Vector2(
                        NormalizeValue(filteredVelocity.x, MAX_VELOCITY_X),
                        NormalizeValue(filteredVelocity.y, MAX_VELOCITY_Y)
                    );

                    tiempos.Add(Time.time);
                    velocidades.Add(filteredVelocity);
                    velocidadesNormalizadas.Add(normalizedVelocity);

                    Debug.Log($"Velocidad - X: {filteredVelocity.x:F3}, Y: {filteredVelocity.y:F3}, " +
                            $"Norma X: {normalizedVelocity.x:F3}, Norma Y: {normalizedVelocity.y:F3}");
                }
                previousDirection = currentDirection;
            }
            timer = 0f;
        }
    }

    bool UpdateGazeDirection()
    {
        Vector3[] positions = new Vector3[2];
        gazeRayLine.GetPositions(positions);
        if (positions[0] == positions[1]) return false;
        currentDirection = (positions[1] - positions[0]).normalized;
        return true;
    }

    Vector2 CalculateGazeVelocity()
    {
        Vector3 currentAngles = Quaternion.LookRotation(currentDirection).eulerAngles;
        Vector3 previousAngles = Quaternion.LookRotation(previousDirection).eulerAngles;
        float deltaX = Mathf.DeltaAngle(previousAngles.y, currentAngles.y);
        float deltaY = Mathf.DeltaAngle(previousAngles.x, currentAngles.x);
        return new Vector2(deltaX, deltaY) / deltaTime;
    }

    Vector2 ApplyMovingAverageFilter(Queue<Vector2> velocityQueue, Vector2 newVelocity)
    {
        if (velocityQueue.Count >= WINDOW_SIZE)
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

    float NormalizeValue(float value, float maxVelocity)
    {
        return Mathf.Clamp(value / maxVelocity, -1f, 1f);
    }

    public void GuardarDatosEnCSV()
    {
        if (tiempos.Count == 0)
        {
            Debug.LogWarning("No hay datos para guardar");
            return;
        }

        StringBuilder csv = new StringBuilder();
        csv.AppendLine("Tiempo,VelocidadGaze_X,VelocidadGaze_Y,VelocidadNormalizada_X,VelocidadNormalizada_Y");

        for (int i = 0; i < velocidades.Count; i++)
        {
            csv.AppendLine($"{tiempos[i]:F3},{velocidades[i].x:F6},{velocidades[i].y:F6}," +
                          $"{velocidadesNormalizadas[i].x:F6},{velocidadesNormalizadas[i].y:F6}");
        }

        string carpeta = @"C:\Users\Manuel Delado\Documents";
        string prefijo = "velocidad_mirada";
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

    public Vector2 GetCurrentVelocity()
    {
        return velocidades.Count > 0 ? velocidades[velocidades.Count - 1] : Vector2.zero;
    }

    public Vector2 GetCurrentNormalizedVelocity()
    {
        return velocidadesNormalizadas.Count > 0 ? velocidadesNormalizadas[velocidadesNormalizadas.Count - 1] : Vector2.zero;
    }

    public void LimpiarDatos()
    {
        tiempos.Clear();
        velocidades.Clear();
        velocidadesNormalizadas.Clear();
        gazeVelocities.Clear();
        InitializeQueues();
    }
}