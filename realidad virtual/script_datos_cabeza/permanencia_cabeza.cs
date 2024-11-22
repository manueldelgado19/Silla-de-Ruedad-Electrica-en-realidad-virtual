using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;

public class NormalizedDwellTimeCalculator : MonoBehaviour
{
    // Variables para la cabeza
    private Vector2 currentHeadAngles = Vector2.zero;
    private Vector2 previousHeadAngles = Vector2.zero; // Ángulos previos de la cabeza

    // Frecuencia de muestreo (en segundos)
    private float deltaTime = 0.2f;
    private float timer = 0f;

    // Tiempo de permanencia acumulado
    private float dwellTimeX = 0f;
    private float dwellTimeY = 0f;

    // Última posición registrada dentro del rango
    private bool isWithinRangeX = false;
    private bool isWithinRangeY = false;

    // Rango de valores para considerar estabilidad
    private float rangeThreshold = 5f;

    // Valores máximos de permanencia para normalización
    private float maxDwellTimeX = 0f;
    private float maxDwellTimeY = 0f;

    // Listas para almacenar los datos
    private List<float> tiempos = new List<float>();
    private List<float> tiempoPermanenciaX = new List<float>();
    private List<float> tiempoPermanenciaY = new List<float>();
    private List<float> tiempoPermanenciaNormalizadoX = new List<float>();
    private List<float> tiempoPermanenciaNormalizadoY = new List<float>();

    void Start()
    {
        Debug.Log("Sistema de cálculo de tiempo de permanencia con normalización iniciado");
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= deltaTime)
        {
            // Actualizar ángulos de la cabeza
            UpdateHeadAngles();

            // Calcular tiempo de permanencia
            CalculateDwellTime();

            // Actualizar valores máximos para normalización
            UpdateMaxDwellTimes();

            // Normalizar tiempos de permanencia
            float normalizedDwellTimeX = NormalizeDwellTime(dwellTimeX, maxDwellTimeX);
            float normalizedDwellTimeY = NormalizeDwellTime(dwellTimeY, maxDwellTimeY);

            // Guardar datos en las listas
            tiempos.Add(Time.time);
            tiempoPermanenciaX.Add(dwellTimeX);
            tiempoPermanenciaY.Add(dwellTimeY);
            tiempoPermanenciaNormalizadoX.Add(normalizedDwellTimeX);
            tiempoPermanenciaNormalizadoY.Add(normalizedDwellTimeY);

            // Actualizar ángulos previos
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

    void CalculateDwellTime()
    {
        // Verificar si el ángulo actual está dentro del rango en el eje X
        if (Mathf.Abs(currentHeadAngles.x - previousHeadAngles.x) <= rangeThreshold)
        {
            isWithinRangeX = true;
            dwellTimeX += deltaTime;
        }
        else
        {
            isWithinRangeX = false;
            dwellTimeX = 0f; // Reiniciar si está fuera del rango
        }

        // Verificar si el ángulo actual está dentro del rango en el eje Y
        if (Mathf.Abs(currentHeadAngles.y - previousHeadAngles.y) <= rangeThreshold)
        {
            isWithinRangeY = true;
            dwellTimeY += deltaTime;
        }
        else
        {
            isWithinRangeY = false;
            dwellTimeY = 0f; // Reiniciar si está fuera del rango
        }
    }

    void UpdateMaxDwellTimes()
    {
        maxDwellTimeX = Mathf.Max(maxDwellTimeX, dwellTimeX);
        maxDwellTimeY = Mathf.Max(maxDwellTimeY, dwellTimeY);
    }

    float NormalizeDwellTime(float dwellTime, float maxDwellTime)
    {
        if (Mathf.Approximately(maxDwellTime, 0)) return 0f;
        return Mathf.Clamp01(dwellTime / maxDwellTime);
    }

    public void GuardarDatosEnCSV()
    {
        StringBuilder csv = new StringBuilder();

        // Agrega la cabecera al archivo CSV
        csv.AppendLine("Tiempo,Permanencia_X,Permanencia_Y,PermanenciaNormalizada_X,PermanenciaNormalizada_Y");

        // Recorre todos los tiempos de permanencia registrados
        for (int i = 0; i < tiempos.Count; i++)
        {
            csv.AppendLine($"{tiempos[i]:F3},{tiempoPermanenciaX[i]:F3},{tiempoPermanenciaY[i]:F3}," +
                           $"{tiempoPermanenciaNormalizadoX[i]:F3},{tiempoPermanenciaNormalizadoY[i]:F3}");
        }

        // Define la ruta de la carpeta donde se guardará el archivo
        string carpeta = @"C:\Users\Manuel Delado\Documents";
        string prefijo = "tiempo_permanencia";
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
