using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class GazeDirectionwitharea : MonoBehaviour
{
    public float UltimoAnguloGazeX
    {
        get { return records.Count > 0 ? records[records.Count - 1].angleX : 0f; }
    }

    public float UltimoAnguloGazeY
    {
        get { return records.Count > 0 ? records[records.Count - 1].angleY : 0f; }
    }

    public string UltimaDireccionGaze
    {
        get { return records.Count > 0 ? records[records.Count - 1].direction : "OutOfArea"; }
    }

    [SerializeField]
    private LineRenderer gazeRayLine;

    // Area colliders originales
    [SerializeField]
    public BoxCollider areaFront;
    [SerializeField]
    public BoxCollider areaDown;
    [SerializeField]
    public BoxCollider areaUp;
    [SerializeField]
    public BoxCollider areaLeft;
    [SerializeField]
    public BoxCollider areaRight;
    [SerializeField]
    public BoxCollider areaUpLeft;
    [SerializeField]
    public BoxCollider areaUpRight;
    [SerializeField]
    public BoxCollider areaDownLeft;
    [SerializeField]
    public BoxCollider areaDownRight;

    // Extensiones
    [SerializeField]
    public BoxCollider areaRightExtension;
    [SerializeField]
    public BoxCollider areaDownRightExtension;
    [SerializeField]
    public BoxCollider areaDownLeftExtension;
    [SerializeField]
    public BoxCollider areaDownExtension;
    [SerializeField]
    public BoxCollider areaLeftExtension;

    [SerializeField]
    public float sampleInterval = 0.2f;
    [SerializeField]
    public LayerMask hitLayers;

    private float lastSampleTime;
    private float startTime;
    private Dictionary<BoxCollider, string> areaNames;
    private List<(float time, float angleX, float angleY, string direction)> records;
    private RaycastHit[] hitBuffer = new RaycastHit[10];

    private readonly Dictionary<string, (float minH, float maxH, float minV, float maxV)> angleRanges =
        new Dictionary<string, (float minH, float maxH, float minV, float maxV)>
    {
       { "Front", (-8f, 8f, -3f, 3f) },
       { "Right", (15f, 30f, -15f, 15f) },
       { "RightExtension", (31f, 45f, -15f, 15f)},
       { "Left", (-30f, -15f, -15f, 15f) },
       { "LeftExtension", (-45f, -31f, -15f, 15f) },
       { "Up", (-8f, 8f, 15f, 40f) },
       { "Down", (-8f, 8f, -27.5f, -15f) },
       { "DownExtension", (-8f, 8f, -40f, -28.5f) },
       { "UpRight", (15f, 45f, 15f, 40f) },
       { "UpLeft", (-45f, -15f, 15f, 40f) },
       { "DownRight", (15f, 30f, -27.5f, -15f) },
       { "DownRightExtension", (31f, 45f, -40f, -28.5f) },
       { "DownLeft", (-30f, -15f, -27.5f, -15f) },
       { "DownLeftExtension", (-45f, -31f, -40f, -28.5f) }
    };

    private void Awake()
    {
        records = new List<(float, float, float, string)>();
    }

    private void Start()
    {
        startTime = Time.time;

        if (gazeRayLine == null)
        {
            Debug.LogError("LineRenderer not assigned!");
            enabled = false;
            return;
        }

        InitializeAreas();

        if (hitLayers.value == 0)
        {
            hitLayers = LayerMask.GetMask("Default");
        }
    }

    private void InitializeAreas()
    {
        areaNames = new Dictionary<BoxCollider, string>();

        // Areas originales
        InitializeArea(areaFront, "Front");
        InitializeArea(areaDown, "Down");
        InitializeArea(areaUp, "Up");
        InitializeArea(areaLeft, "Left");
        InitializeArea(areaRight, "Right");
        InitializeArea(areaUpLeft, "UpLeft");
        InitializeArea(areaUpRight, "UpRight");
        InitializeArea(areaDownLeft, "DownLeft");
        InitializeArea(areaDownRight, "DownRight");

        // Extensiones
        InitializeArea(areaRightExtension, "Right");
        InitializeArea(areaDownRightExtension, "DownRight");
        InitializeArea(areaDownLeftExtension, "DownLeft");
        InitializeArea(areaDownExtension, "Down");
        InitializeArea(areaLeftExtension, "Left");

        if (areaNames.Count == 0)
        {
            Debug.LogError("No areas assigned! Please assign box colliders in the inspector.");
            enabled = false;
        }
    }

    private void InitializeArea(BoxCollider collider, string name)
    {
        if (collider != null)
        {
            areaNames.Add(collider, name);
            collider.isTrigger = true;
        }
    }

    private void Update()
    {
        if (Time.time - lastSampleTime >= sampleInterval)
        {
            lastSampleTime = Time.time;
            RecordGazeDirection();
        }
    }

    private void RecordGazeDirection()
    {
        Vector3[] positions = new Vector3[2];
        gazeRayLine.GetPositions(positions);
        Vector3 direction = (positions[1] - positions[0]).normalized;

        float horizontalAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        float verticalAngle = Mathf.Atan2(direction.y, direction.z) * Mathf.Rad2Deg;

        string detectedDirection = DetectDirectionFromColliders(positions[0], direction);
        string angleBasedDirection = GetDirectionFromAngles(horizontalAngle, verticalAngle);

        float currentTime = Time.time - startTime;
        records.Add((currentTime, horizontalAngle, verticalAngle, detectedDirection));

        Debug.Log($"Time: {currentTime:F2}s, H: {horizontalAngle:F2}°, V: {verticalAngle:F2}°, Dir: {detectedDirection}");
    }

    private string DetectDirectionFromColliders(Vector3 origin, Vector3 direction)
    {
        int hitCount = Physics.SphereCastNonAlloc(origin, 0.1f, direction, hitBuffer, 100f, hitLayers);

        for (int i = 0; i < hitCount; i++)
        {
            BoxCollider hitCollider = hitBuffer[i].collider as BoxCollider;
            if (hitCollider != null && areaNames.ContainsKey(hitCollider))
            {
                return areaNames[hitCollider];
            }
        }

        return "OutOfArea";
    }

    private string GetDirectionFromAngles(float horizontalAngle, float verticalAngle)
    {
        string baseDirection = "";
        foreach (var range in angleRanges)
        {
            if (horizontalAngle >= range.Value.minH &&
                horizontalAngle <= range.Value.maxH &&
                verticalAngle >= range.Value.minV &&
                verticalAngle <= range.Value.maxV)
            {
                baseDirection = range.Key.Replace("Extension", "");
                return baseDirection;
            }
        }
        return "OutOfArea";
    }

    public void SaveDataToCSV()
    {
        if (records.Count == 0)
        {
            Debug.LogWarning("No data to save!");
            return;
        }

        StringBuilder csv = new StringBuilder();
        csv.AppendLine("Time,AngleX,AngleY,Direction");

        foreach (var record in records)
        {
            csv.AppendLine($"{record.time:F3},{record.angleX:F6},{record.angleY:F6},{record.direction}");
        }

        string folder = @"C:\Users\Manuel Delado\Documents";
        string prefix = "gaze_tracking";
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filePath = Path.Combine(folder, $"{prefix}_{timestamp}.csv");

        try
        {
            File.WriteAllText(filePath, csv.ToString());
            Debug.Log($"Data saved to: {filePath}");
        }
        catch (IOException ex)
        {
            Debug.LogError($"Error saving data: {ex.Message}");
        }
    }

    private void OnDisable()
    {
        SaveDataToCSV();
    }
}