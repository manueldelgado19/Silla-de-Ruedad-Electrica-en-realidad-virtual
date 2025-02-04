using UnityEngine;
using UnityEditor;
using System;
using System.IO;

#if UNITY_EDITOR
public class ParticipantSelector : EditorWindow
{
    private static int participanteSeleccionado = 1;
    private static int intentoSeleccionado = 1;
    private static string rutaBase = @"C:\Users\Manuel Delado\Documents\VR_Study";

    [MenuItem("VR Study/Configurar Participante")]
    public static void ShowWindow()
    {
        var window = GetWindow<ParticipantSelector>("Configurar Participante");
        window.minSize = new Vector2(300, 200);
    }

    private void OnGUI()
    {
        GUILayout.Label("Configuraci�n de Participante", EditorStyles.boldLabel);

        EditorGUILayout.Space(10);

        participanteSeleccionado = Mathf.Max(1, EditorGUILayout.IntField("N�mero de Participante:", participanteSeleccionado));
        intentoSeleccionado = Mathf.Max(1, EditorGUILayout.IntField("N�mero de Intento:", intentoSeleccionado));

        EditorGUILayout.Space(20);

        // Mostrar la ruta donde se guardar�n los datos
        EditorGUILayout.LabelField("Ruta de guardado:");
        EditorGUILayout.SelectableLabel(Path.Combine(rutaBase,
            $"Participante_{participanteSeleccionado}",
            $"Intento_{intentoSeleccionado}"));

        EditorGUILayout.Space(20);

        if (GUILayout.Button("Guardar Configuraci�n"))
        {
            GuardarConfiguracion();
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox("Esta configuraci�n se usar� para organizar los archivos CSV generados.", MessageType.Info);
    }

    private void GuardarConfiguracion()
    {
        // Guardar en PlayerPrefs
        PlayerPrefs.SetInt("ParticipanteActual", participanteSeleccionado);
        PlayerPrefs.SetInt("IntentoActual", intentoSeleccionado);
        PlayerPrefs.Save();

        // Crear estructura de carpetas
        CrearEstructuraCarpetas();

        EditorUtility.DisplayDialog("Configuraci�n Guardada",
            $"Participante: {participanteSeleccionado}\n" +
            $"Intento: {intentoSeleccionado}\n\n" +
            $"Los datos se guardar�n en:\n" +
            $"{Path.Combine(rutaBase, $"Participante_{participanteSeleccionado}", $"Intento_{intentoSeleccionado}")}",
            "OK");
    }

    private void CrearEstructuraCarpetas()
    {
        try
        {
            // Crear carpeta base si no existe
            if (!Directory.Exists(rutaBase))
            {
                Directory.CreateDirectory(rutaBase);
            }

            // Crear carpeta del participante
            string rutaParticipante = Path.Combine(rutaBase, $"Participante_{participanteSeleccionado}");
            if (!Directory.Exists(rutaParticipante))
            {
                Directory.CreateDirectory(rutaParticipante);
            }

            // Crear carpeta del intento
            string rutaIntento = Path.Combine(rutaParticipante, $"Intento_{intentoSeleccionado}");
            if (!Directory.Exists(rutaIntento))
            {
                Directory.CreateDirectory(rutaIntento);
            }

            Debug.Log($"Estructura de carpetas creada en: {rutaIntento}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al crear carpetas: {e.Message}");
            EditorUtility.DisplayDialog("Error",
                $"Error al crear las carpetas: {e.Message}", "OK");
        }
    }
}
#endif