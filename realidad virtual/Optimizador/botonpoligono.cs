using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SimplificadorMasivo))]
public class SimplificadorMasivoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SimplificadorMasivo simplificador = (SimplificadorMasivo)target;

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Iniciar Reducci�n de Pol�gonos", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Confirmar Reducci�n",
                "�Est�s seguro de que quieres iniciar el proceso de reducci�n de pol�gonos?\n\nSe mantendr�n sin cambios:\n- Pilares\n- Columnas\n- Paredes\n- Vigas",
                "S�, iniciar", "Cancelar"))
            {
                simplificador.SimplificarTodos();
            }
        }
    }
}
