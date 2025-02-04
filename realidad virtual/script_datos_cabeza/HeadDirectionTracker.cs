using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

public class HeadDirectionTracker : MonoBehaviour
{
    // Propiedades públicas para el DataCombiner
    public float UltimoAnguloHorizontal
    {
        get { return angulosHorizontales.Count > 0 ? angulosHorizontales[angulosHorizontales.Count - 1] : 0f; }
    }

    public float UltimoAnguloVertical
    {
        get { return angulosVerticales.Count > 0 ? angulosVerticales[angulosVerticales.Count - 1] : 0f; }
    }

    public string UltimaDireccion
    {
        get { return direcciones.Count > 0 ? direcciones[direcciones.Count - 1] : "Front"; }
    }

    [Header("Reference Settings")]
    public GameObject frontReference;

    [Header("Sampling Settings")]
    [Range(0.1f, 1.0f)]
    public float samplingInterval = 0.2f;

    [Header("Angle Thresholds")]
    [Range(10f, 45f)]
    public float horizontalThreshold = 30f;

    [Range(10f, 45f)]
    public float verticalThreshold = 30f;

    private Camera mainCamera;
    private float lastSampleTime;
    private float startTime;
    private List<float> tiempos = new List<float>();
    private List<float> angulosHorizontales = new List<float>();
    private List<float> angulosVerticales = new List<float>();
    private List<string> direcciones = new List<string>();

    void Start()
    {
        if (!ValidateComponents()) return;
        InitializeTracking();
    }

    private bool ValidateComponents()
    {
        if (frontReference == null)
        {
            Debug.LogError("Front reference object not assigned!");
            enabled = false;
            return false;
        }

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
            enabled = false;
            return false;
        }
        return true;
    }

    private void InitializeTracking()
    {
        startTime = Time.time;
        lastSampleTime = startTime;
    }

    void Update()
    {
        if (Time.time - lastSampleTime >= samplingInterval)
        {
            lastSampleTime = Time.time;
            AnalyzeHeadDirection();
        }
    }

    private void AnalyzeHeadDirection()
    {
        Vector3 toReference = (frontReference.transform.position - mainCamera.transform.position).normalized;
        Vector3 lookDirection = mainCamera.transform.forward;

        // Horizontal calculation
        Vector3 horizontalToReference = new Vector3(toReference.x, 0, toReference.z).normalized;
        Vector3 horizontalLook = new Vector3(lookDirection.x, 0, lookDirection.z).normalized;
        float horizontalAngle = Vector3.SignedAngle(horizontalToReference, horizontalLook, Vector3.up);

        // Vertical calculation
        float verticalAngle = Vector3.SignedAngle(lookDirection, horizontalLook, mainCamera.transform.right);

        string direction = DetermineDirection(horizontalAngle, verticalAngle);
        SaveSample(horizontalAngle, verticalAngle, direction);
    }

    private string DetermineDirection(float horizontalAngle, float verticalAngle)
    {
        bool isLookingDown = verticalAngle < -verticalThreshold;
        bool isLookingUp = verticalAngle > verticalThreshold;
        float diagonalThreshold = horizontalThreshold * 0.6f;

        if (isLookingDown)
        {
            if (horizontalAngle > diagonalThreshold) return "Down-Right";
            if (horizontalAngle < -diagonalThreshold) return "Down-Left";
            return "Down";
        }

        if (isLookingUp)
        {
            if (horizontalAngle > diagonalThreshold) return "Up-Right";
            if (horizontalAngle < -diagonalThreshold) return "Up-Left";
            return "Up";
        }

        if (horizontalAngle > horizontalThreshold) return "Right";
        if (horizontalAngle < -horizontalThreshold) return "Left";

        return "Front";
    }

    private void SaveSample(float horizontalAngle, float verticalAngle, string direction)
    {
        tiempos.Add(Time.time - startTime);
        angulosHorizontales.Add(horizontalAngle);
        angulosVerticales.Add(verticalAngle);
        direcciones.Add(direction);
    }

    public void GuardarDatosEnCSV()
    {
        if (tiempos.Count == 0) return;

        StringBuilder csv = new StringBuilder();
        csv.AppendLine("Tiempo,AnguloHorizontal,AnguloVertical,Direccion");

        for (int i = 0; i < tiempos.Count; i++)
        {
            csv.AppendLine($"{tiempos[i]:F3},{angulosHorizontales[i]:F2},{angulosVerticales[i]:F2},{direcciones[i]}");
        }

        string path = @"C:\Users\Manuel Delado\Documents";
        string filename = ObtenerSiguienteNombreArchivo(path, "head_gaze", ".csv");

        try
        {
            File.WriteAllText(filename, csv.ToString());
            Debug.Log($"Datos guardados en: {filename}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al guardar el archivo: {e.Message}");
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupFilename = Path.Combine(path, $"head_gaze_{timestamp}.csv");
            File.WriteAllText(backupFilename, csv.ToString());
            Debug.Log($"Datos guardados en archivo de respaldo: {backupFilename}");
        }
    }

    private string ObtenerSiguienteNombreArchivo(string folder, string prefix, string extension)
    {
        int count = 1;
        string filename;
        do
        {
            filename = Path.Combine(folder, $"{prefix}{count++}{extension}");
        }
        while (File.Exists(filename));
        return filename;
    }

    private void OnDisable()
    {
        GuardarDatosEnCSV();
    }
}