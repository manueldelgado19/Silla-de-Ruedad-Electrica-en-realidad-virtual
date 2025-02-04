using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;
using System.Linq;

public class NormalizedGazeDwellTimeCalculator : MonoBehaviour
{
    // Propiedades públicas para el DataCombiner
    public Vector2 UltimaPermanenciaGaze
    {
        get
        {
            if (tiempoPermanenciaX.Count > 0 && tiempoPermanenciaY.Count > 0)
                return new Vector2(tiempoPermanenciaX[tiempoPermanenciaX.Count - 1],
                                 tiempoPermanenciaY[tiempoPermanenciaY.Count - 1]);
            return Vector2.zero;
        }
    }

    public Vector2 UltimaPermanenciaGazeNormalizada
    {
        get
        {
            if (tiempoPermanenciaNormalizadoX.Count > 0 && tiempoPermanenciaNormalizadoY.Count > 0)
                return new Vector2(tiempoPermanenciaNormalizadoX[tiempoPermanenciaNormalizadoX.Count - 1],
                                 tiempoPermanenciaNormalizadoY[tiempoPermanenciaNormalizadoY.Count - 1]);
            return Vector2.zero;
        }
    }

    private class AdaptiveKalmanFilter
    {
        private float Q;
        private float R;
        private float P = 1;
        private float X = 0;
        private float K = 0;
        private const float MIN_Q = 0.0001f;
        private const float MAX_Q = 0.01f;

        public void UpdateNoise(float variance)
        {
            Q = Mathf.Clamp(variance * 0.001f, MIN_Q, MAX_Q);
            R = Mathf.Clamp(Mathf.Log(1 + Q * 50f) * 0.5f, 0.01f, 1.0f);
        }

        public float Update(float measurement)
        {
            P = P + Q;
            K = P / (P + R);
            X = X + K * (measurement - X);
            P = (1 - K) * P;
            return X;
        }

        public void Reset(float initialValue)
        {
            X = initialValue;
            P = 1;
            Q = MIN_Q;
            R = Mathf.Log(1 + Q * 50f) * 0.5f;
        }
    }

    // Buffer circular optimizado con manejo seguro de tipos
    private class OptimizedCircularBuffer<T>
    {
        private readonly T[] buffer;
        private readonly List<float> sortedValues;
        private int start;
        private int count;
        private float runningSum;
        private float runningSumSquares;
        private readonly int capacity;

        public OptimizedCircularBuffer(int capacity)
        {
            this.capacity = capacity;
            buffer = new T[capacity];
            sortedValues = new List<float>(capacity);
            start = 0;
            count = 0;
            runningSum = 0;
            runningSumSquares = 0;
        }

        private float ConvertToFloat(T item)
        {
            if (item is float f)
                return f;
            if (item is Vector3 v)
                return v.magnitude;
            throw new ArgumentException($"Tipo no soportado en el buffer: {typeof(T).Name}");
        }

        public void Add(T item)
        {
            float value = ConvertToFloat(item);

            if (count < capacity)
            {
                buffer[(start + count) % capacity] = item;
                sortedValues.Add(value);
                count++;
            }
            else
            {
                float oldValue = ConvertToFloat(buffer[start]);
                int index = sortedValues.BinarySearch(oldValue);
                if (index >= 0)
                    sortedValues.RemoveAt(index);

                buffer[start] = item;
                start = (start + 1) % capacity;

                index = sortedValues.BinarySearch(value);
                if (index < 0) index = ~index;
                sortedValues.Insert(index, value);

                runningSum = runningSum - oldValue + value;
                runningSumSquares = runningSumSquares - (oldValue * oldValue) + (value * value);
            }
        }

        public float GetVariance()
        {
            if (count < 2) return 0;
            float mean = runningSum / count;
            return (runningSumSquares / count) - (mean * mean);
        }

        public (float Q1, float Q3) GetQuartiles()
        {
            if (count < 4)
                return (0, 0);

            var sorted = sortedValues.OrderBy(x => x).ToList();
            int q1Index = count / 4;
            int q3Index = (count * 3) / 4;

            return (sorted[q1Index], sorted[q3Index]);
        }

        public float GetMean() => count > 0 ? runningSum / count : 0;
        public IEnumerable<T> GetValues() => buffer.Take(count);
        public int Count => count;
        public void Clear()
        {
            count = 0;
            start = 0;
            runningSum = 0;
            runningSumSquares = 0;
            sortedValues.Clear();
        }
    }

    // Variables para la mirada
    [SerializeField] private float deltaTime = 0.2f;
    private LineRenderer gazeRayLine;
    private Vector3 currentGazeDirection;
    private Vector3 previousGazeDirection;
    private float timer;

    // Tiempo de permanencia
    private float dwellTimeX;
    private float dwellTimeY;

    // Constantes
    private const float RANGE_THRESHOLD = 0.1f;
    private const float MOVEMENT_THRESHOLD = 0.001f;
    private const int BUFFER_SIZE = 5;

    // Sistemas de filtrado y análisis
    private readonly AdaptiveKalmanFilter kalmanX = new AdaptiveKalmanFilter();
    private readonly AdaptiveKalmanFilter kalmanY = new AdaptiveKalmanFilter();
    private readonly AdaptiveKalmanFilter kalmanZ = new AdaptiveKalmanFilter();

    private OptimizedCircularBuffer<float> velocityBufferX;
    private OptimizedCircularBuffer<float> velocityBufferY;
    private OptimizedCircularBuffer<float> dwellTimeBufferX;
    private OptimizedCircularBuffer<float> dwellTimeBufferY;
    private OptimizedCircularBuffer<Vector3> directionBuffer;

    // Almacenamiento de datos
    private readonly List<float> tiempos = new List<float>();
    private readonly List<float> tiempoPermanenciaX = new List<float>();
    private readonly List<float> tiempoPermanenciaY = new List<float>();
    private readonly List<float> tiempoPermanenciaNormalizadoX = new List<float>();
    private readonly List<float> tiempoPermanenciaNormalizadoY = new List<float>();

    private void Start()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        try
        {
            gazeRayLine = GetComponent<LineRenderer>();
            if (gazeRayLine == null)
                throw new MissingComponentException("LineRenderer no encontrado");

            velocityBufferX = new OptimizedCircularBuffer<float>(BUFFER_SIZE);
            velocityBufferY = new OptimizedCircularBuffer<float>(BUFFER_SIZE);
            dwellTimeBufferX = new OptimizedCircularBuffer<float>(BUFFER_SIZE);
            dwellTimeBufferY = new OptimizedCircularBuffer<float>(BUFFER_SIZE);
            directionBuffer = new OptimizedCircularBuffer<Vector3>(BUFFER_SIZE);

            InitializeGazeDirection();
            Debug.Log("Sistema de análisis de mirada inicializado correctamente");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al inicializar el sistema: {e.Message}");
            enabled = false;
        }
    }

    private void InitializeGazeDirection()
    {
        UpdateGazeDirection();
        previousGazeDirection = currentGazeDirection;

        kalmanX.Reset(currentGazeDirection.x);
        kalmanY.Reset(currentGazeDirection.y);
        kalmanZ.Reset(currentGazeDirection.z);
    }

    private bool IsOutlier(float value, OptimizedCircularBuffer<float> buffer)
    {
        if (buffer.Count < 4) return false;

        var (q1, q3) = buffer.GetQuartiles();
        float iqr = q3 - q1;

        float adaptiveIqrFactor = Mathf.Clamp(1.2f + (buffer.GetVariance() * 5f), 1.2f, 2.5f);
        float lowerBound = q1 - (adaptiveIqrFactor * iqr);
        float upperBound = q3 + (adaptiveIqrFactor * iqr);

        return value < lowerBound || value > upperBound;
    }

    private Vector3 SmoothTransition(Vector3 newDirection)
    {
        float varX = velocityBufferX.GetVariance();
        float varY = velocityBufferY.GetVariance();
        float variance = (varX + varY) * 0.5f;

        float alpha = Mathf.Clamp(0.15f / (1 + variance * 8f), 0.05f, 0.25f);

        Vector3 smoothed = Vector3.Lerp(previousGazeDirection, newDirection, alpha);
        return (smoothed.magnitude > 0.95f) ? smoothed : smoothed.normalized;
    }

    private void UpdateGazeDirection()
    {
        Vector3[] positions = new Vector3[2];
        gazeRayLine.GetPositions(positions);
        Vector3 rawDirection = (positions[1] - positions[0]).normalized;

        Vector3 kalmanFiltered = new Vector3(
            kalmanX.Update(rawDirection.x),
            kalmanY.Update(rawDirection.y),
            kalmanZ.Update(rawDirection.z)
        );

        currentGazeDirection = SmoothTransition(kalmanFiltered);
        directionBuffer.Add(currentGazeDirection);
    }

    private void Update()
    {
        if (!enabled || gazeRayLine == null) return;

        timer += Time.deltaTime;
        if (timer >= deltaTime)
        {
            ProcessGazeData();
            timer = 0f;
        }
    }

    private void ProcessGazeData()
    {
        try
        {
            UpdateGazeDirection();

            float velocityX = (currentGazeDirection.x - previousGazeDirection.x) / deltaTime;
            float velocityY = (currentGazeDirection.y - previousGazeDirection.y) / deltaTime;

            velocityBufferX.Add(velocityX);
            velocityBufferY.Add(velocityY);

            kalmanX.UpdateNoise(velocityBufferX.GetVariance());
            kalmanY.UpdateNoise(velocityBufferY.GetVariance());

            if (!IsOutlier(velocityX, velocityBufferX) && !IsOutlier(velocityY, velocityBufferY))
            {
                CalculateDwellTime();

                float deltaX = Mathf.Abs(currentGazeDirection.x - previousGazeDirection.x);
                float deltaY = Mathf.Abs(currentGazeDirection.y - previousGazeDirection.y);

                if (deltaX > MOVEMENT_THRESHOLD || deltaY > MOVEMENT_THRESHOLD)
                {
                    float directionX = Mathf.Sign(currentGazeDirection.x - previousGazeDirection.x);
                    float directionY = Mathf.Sign(currentGazeDirection.y - previousGazeDirection.y);

                    float signedDwellTimeX = dwellTimeX * directionX;
                    float signedDwellTimeY = dwellTimeY * directionY;

                    float normalizedX = NormalizeDwellTime(signedDwellTimeX, dwellTimeBufferX);
                    float normalizedY = NormalizeDwellTime(signedDwellTimeY, dwellTimeBufferY);

                    SaveDataPoint(signedDwellTimeX, signedDwellTimeY, normalizedX, normalizedY);
                }
            }

            previousGazeDirection = currentGazeDirection;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error en ProcessGazeData: {e.Message}");
        }
    }

    private void CalculateDwellTime()
    {
        float deltaX = Mathf.Abs(currentGazeDirection.x - previousGazeDirection.x);
        float deltaY = Mathf.Abs(currentGazeDirection.y - previousGazeDirection.y);

        bool isActuallyMovingX = deltaX > MOVEMENT_THRESHOLD;
        bool isActuallyMovingY = deltaY > MOVEMENT_THRESHOLD;

        if (isActuallyMovingX && deltaX <= RANGE_THRESHOLD)
        {
            dwellTimeX += deltaTime;
            dwellTimeBufferX.Add(dwellTimeX);
        }
        else
        {
            dwellTimeX = 0f;
            dwellTimeBufferX.Clear();
        }

        if (isActuallyMovingY && deltaY <= RANGE_THRESHOLD)
        {
            dwellTimeY += deltaTime;
            dwellTimeBufferY.Add(dwellTimeY);
        }
        else
        {
            dwellTimeY = 0f;
            dwellTimeBufferY.Clear();
        }
    }

    private float NormalizeDwellTime(float dwellTime, OptimizedCircularBuffer<float> dwellTimeBuffer)
    {
        float dynamicMaxDwell = Mathf.Max(1.5f, dwellTimeBuffer.GetMean() * 1.5f);
        return Mathf.Clamp(dwellTime / dynamicMaxDwell, -1f, 1f);
    }

    private void SaveDataPoint(float dwellX, float dwellY, float normX, float normY)
    {
        tiempos.Add(Time.time);
        tiempoPermanenciaX.Add(dwellX);
        tiempoPermanenciaY.Add(dwellY);
        tiempoPermanenciaNormalizadoX.Add(normX);
        tiempoPermanenciaNormalizadoY.Add(normY);
    }
    public void GuardarDatosEnCSV()
    {
        if (tiempos.Count == 0)
        {
            Debug.LogWarning("No hay datos para guardar");
            return;
        }

        StringBuilder csv = new StringBuilder();
        csv.AppendLine("Tiempo,Permanencia_X,Permanencia_Y,PermanenciaNormalizada_X,PermanenciaNormalizada_Y");

        for (int i = 0; i < tiempos.Count; i++)
        {
            csv.AppendLine($"{tiempos[i]:F3},{tiempoPermanenciaX[i]:F3},{tiempoPermanenciaY[i]:F3}," +
                          $"{tiempoPermanenciaNormalizadoX[i]:F3},{tiempoPermanenciaNormalizadoY[i]:F3}");
        }

        string carpeta = @"C:\Users\Manuel Delado\Documents";
        string prefijo = "dwell_time_gaze";
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