using UnityEngine;

public class MyMessageListener : MonoBehaviour
{
    public GameObject wheelchair;  // Asigna aqu� el GameObject de la silla de ruedas en el Inspector
    public Camera camera;          // Asigna aqu� la c�mara en el Inspector
    public float movementSpeed = 1.0f;  // Controla la velocidad de movimiento
    public float rotationSpeed = 100.0f; // Controla la velocidad de rotaci�n
    public bool invertX = false;  // Invierte el eje X si es true
    public bool invertY = false;  // Invierte el eje Y si es true

    // Este m�todo se llama cuando llega un mensaje desde el Arduino
    void OnMessageArrived(string msg)
    {
        Debug.Log("Mensaje recibido: " + msg);
        ProcessMessage(msg);
    }

    // Este m�todo se llama cuando hay un evento de conexi�n/desconexi�n
    void OnConnectionEvent(bool success)
    {
        Debug.Log(success ? "Dispositivo conectado" : "Dispositivo desconectado");
    }

    // Procesa el mensaje recibido
    void ProcessMessage(string msg)
    {
        string[] data = msg.Split(',');
        if (data.Length >= 2)
        {
            try
            {
                float angle = float.Parse(data[0]);
                float magnitude = float.Parse(data[1]);

                // Aplica la inversi�n de ejes si es necesario
                if (invertX) angle = 360 - angle;
                if (invertY) magnitude *= -1;

                // Normaliza el �ngulo entre 0 y 360 grados
                angle = angle % 360;
                if (angle < 0) angle += 360;

                // Definimos umbrales para determinar movimiento o rotaci�n
                float forwardThreshold = 45f; // Umbral para movimiento hacia adelante/atr�s
                float rotationThreshold = 45f; // Umbral para rotaci�n

                // Calculamos la diferencia m�nima entre el �ngulo y las direcciones clave
                float angleToForward = Mathf.Min(Mathf.Abs(angle - 0), Mathf.Abs(angle - 360));
                float angleToBackward = Mathf.Abs(angle - 180);
                float angleToRight = Mathf.Abs(angle - 90);
                float angleToLeft = Mathf.Abs(angle - 270);

                // Movimiento hacia adelante o atr�s
                if (angleToForward < forwardThreshold || angleToBackward < forwardThreshold)
                {
                    // Direcci�n de movimiento basada en la c�mara
                    Vector3 cameraForward = camera.transform.forward;
                    cameraForward.y = 0; // Ignora la componente vertical
                    cameraForward.Normalize();

                    // Si el �ngulo est� cerca de 180 grados, invertimos la direcci�n
                    if (angleToBackward < forwardThreshold)
                    {
                        cameraForward *= -1;
                    }

                    // Mueve la silla de ruedas
                    Vector3 movement = cameraForward * (magnitude / 100.0f) * movementSpeed;
                    wheelchair.transform.Translate(movement * Time.deltaTime, Space.World);
                }
                // Rotaci�n a la derecha o izquierda
                else if (angleToRight < rotationThreshold || angleToLeft < rotationThreshold)
                {
                    // Invertimos la direcci�n de rotaci�n
                    float rotationDirection = (angleToLeft < rotationThreshold) ? -1 : 1;
                    float rotationAmount = rotationDirection * (magnitude / 100.0f) * rotationSpeed * Time.deltaTime;
                    wheelchair.transform.Rotate(0, rotationAmount, 0);
                }
                else
                {
                    // Movimiento en direcci�n diagonal
                    Vector3 movementDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                    movementDirection.y = 0;
                    movementDirection.Normalize();

                    // Mueve la silla de ruedas
                    Vector3 movement = movementDirection * (magnitude / 100.0f) * movementSpeed;
                    wheelchair.transform.Translate(movement * Time.deltaTime, Space.World);
                }
            }
            catch (System.FormatException)
            {
                Debug.LogWarning("El formato del mensaje no es v�lido: " + msg);
            }
        }
        else
        {
            Debug.LogWarning("Mensaje incompleto o malformado: " + msg);
        }
    }
}
