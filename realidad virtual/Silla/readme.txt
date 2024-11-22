
Primero, se debe de tener eel proyecto Unity  abierto

Para importar los archivos:
En la seccion Project la cual se encuentra en la parte inferior. 
click derecho, create,folder a esta carpeta se la va a llamar "Models" o "Assets" en tu proyecto de Unity 
Arrastra toda la carpeta que contiene los archivos (.fbx, texturas y archivos .meta) directamente a esa carpeta en Unity
O también puedes copiar los archivos y pegarlos directamente en la carpeta del proyecto de Unity

Para agregar el objeto a tu escena:

Una vez importado, localiza el archivo "Silla de ruedas.fbx" en el Project Window de Unity
Arrastra el modelo directamente a la escena oa la jerarquía (Hierarchy Window)
También puedes hacer clic derecho en la Ventana de Jerarquía → Objeto 3D → importar el modelo desde allí


Asegúrese de que las texturas se hayan importado correctamente y estén aplicadas al modelo.
Verifica que:
Las texturas se ven correctamente.
El modelo tenga la escala apropiada para tu escena.
La orientación sea la correcta.
sino es asi cambialas

Una vez en la escena, puedes usar las herramientas de transformación para:

Posicionar el objeto (herramienta Move)
Rotar el objeto (herramienta Rotate)
Escalar el objeto (herramienta Scale)
La opcion espacalar es importante ya que varia segun el ambiente en el cual se esta adjuntando
En el caso de la silla es importante tener en cuenta el tamaño del box colider ya que el diseño por defecto viene con el colider demasidado grande y entra en el piso lo cual es un problema, asi como tambien choca con los colliders de los objetos.
si la silla aparede de otro color, dentro de la silla esta _grouppasted_polysurface, en materiales sleccionar los colores de tu interes.
