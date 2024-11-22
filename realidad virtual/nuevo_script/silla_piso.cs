using UnityEngine;

public class LockPositionY : MonoBehaviour
{
    public float fixedYPosition = 0.0f; // La posición Y fija que quieres mantener (ajústala según tu escenario)

    void LateUpdate()
    {
        // Obtiene la posición actual del objeto
        Vector3 currentPosition = transform.position;

        // Mantén la posición en X y Z, pero fija el eje Y
        currentPosition.y = fixedYPosition;

        // Aplica la posición corregida al objeto
        transform.position = currentPosition;
    }
}
