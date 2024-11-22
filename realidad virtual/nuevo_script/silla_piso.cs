using UnityEngine;

public class LockPositionY : MonoBehaviour
{
    public float fixedYPosition = 0.0f; // La posici�n Y fija que quieres mantener (aj�stala seg�n tu escenario)

    void LateUpdate()
    {
        // Obtiene la posici�n actual del objeto
        Vector3 currentPosition = transform.position;

        // Mant�n la posici�n en X y Z, pero fija el eje Y
        currentPosition.y = fixedYPosition;

        // Aplica la posici�n corregida al objeto
        transform.position = currentPosition;
    }
}
