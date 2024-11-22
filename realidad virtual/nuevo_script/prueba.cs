using UnityEngine;
using ViveSR.anipal.Eye;

public class CheckSRanipal : MonoBehaviour
{
    private EyeData_v2 eyeData = new EyeData_v2();

    void Start()
    {
        // Comprobar si SRanipal Eye est� funcionando correctamente
        bool eyeTrackingAvailable = SRanipal_Eye_API.IsViveProEye();
        if (eyeTrackingAvailable)
        {
            Debug.Log("SRanipal est� funcionando y el dispositivo de seguimiento ocular est� disponible.");
        }
        else
        {
            Debug.LogError("SRanipal no detecta un dispositivo de seguimiento ocular. Verifica la conexi�n y configuraci�n.");
        }
    }

    void Update()
    {
        // Alternativa: Comprobar si el framework est� activo
        var isFrameworkActive = SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.WORKING;
        if (isFrameworkActive)
        {
            Debug.Log("El Framework de SRanipal est� funcionando.");
        }
        else
        {
            Debug.LogWarning("El Framework de SRanipal no est� activo. Verifica la configuraci�n.");
        }

        // Alternativa: Usar datos b�sicos de seguimiento con Ray
        Ray gazeRay;
        if (SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out gazeRay))
        {
            Debug.Log($"Rayo de la mirada detectado: Origen = {gazeRay.origin}, Direcci�n = {gazeRay.direction}");
        }
        else
        {
            Debug.LogWarning("No se detectaron datos de la mirada.");
        }
    }
}