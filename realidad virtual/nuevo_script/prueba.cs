using UnityEngine;
using ViveSR.anipal.Eye;

public class CheckSRanipal : MonoBehaviour
{
    private EyeData_v2 eyeData = new EyeData_v2();

    void Start()
    {
        // Comprobar si SRanipal Eye está funcionando correctamente
        bool eyeTrackingAvailable = SRanipal_Eye_API.IsViveProEye();
        if (eyeTrackingAvailable)
        {
            Debug.Log("SRanipal está funcionando y el dispositivo de seguimiento ocular está disponible.");
        }
        else
        {
            Debug.LogError("SRanipal no detecta un dispositivo de seguimiento ocular. Verifica la conexión y configuración.");
        }
    }

    void Update()
    {
        // Alternativa: Comprobar si el framework está activo
        var isFrameworkActive = SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.WORKING;
        if (isFrameworkActive)
        {
            Debug.Log("El Framework de SRanipal está funcionando.");
        }
        else
        {
            Debug.LogWarning("El Framework de SRanipal no está activo. Verifica la configuración.");
        }

        // Alternativa: Usar datos básicos de seguimiento con Ray
        Ray gazeRay;
        if (SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out gazeRay))
        {
            Debug.Log($"Rayo de la mirada detectado: Origen = {gazeRay.origin}, Dirección = {gazeRay.direction}");
        }
        else
        {
            Debug.LogWarning("No se detectaron datos de la mirada.");
        }
    }
}