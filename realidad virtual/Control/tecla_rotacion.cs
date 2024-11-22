using System.Collections;
using UnityEngine;

public class rotaciontecla : MonoBehaviour
{
    public float Speed = 5.0f;
    public float RotationSpeed = 100.0f;

    void Update()
    {
        float rotation = 0f;
      //  float moveDirection = 0f;

        // Rotación con A y D
        if (Input.GetKey(KeyCode.A))
        {
            rotation -= 1f; // A gira a la izquierda
        }
        if (Input.GetKey(KeyCode.D))
        {
            rotation += 1f; // D gira a la derecha
        }

        // Movimiento adelante/atrás con W y S
        if (Input.GetKey(KeyCode.S))
        {
            // Mover hacia donde mira la cámara
            transform.position += transform.right * Speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.W))
        {
            // Mover hacia atrás de donde mira la cámara
            transform.position -= transform.right * Speed * Time.deltaTime;
        }

        // Aplicar rotación
        transform.Rotate(new Vector3(0, rotation * Time.deltaTime * RotationSpeed, 0));
    }
}