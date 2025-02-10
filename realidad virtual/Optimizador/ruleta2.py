import tkinter as tk
from tkinter import messagebox, simpledialog, ttk
import random
import time
import json
from datetime import datetime
from pathlib import Path
import os
import re
import pandas as pd

class RuletaEstudiantes:
    def __init__(self):
        self.ventana = tk.Tk()
        self.ventana.title("Ruleta de Estudiantes")
        self.ventana.geometry("1200x800")
        
        # Directorio y archivo para guardar los datos
        self.directorio_datos = r"C:\Users\Manuel Delado\Documents\participaciones"
        os.makedirs(self.directorio_datos, exist_ok=True)
        self.registro_file = os.path.join(self.directorio_datos, "registro_participacion.json")
        
        self.historial = []  # Lista de registros (eventos)
        self.tiempo_inicio = None
        
        self.estudiantes = [
            "ALVAREZ MARTINEZ JIMENA JAZMIN",
            "AREVALO REYES ANAIRAM RUBI", 
            "ARMENTA RANGEL OSCAR MANUEL",
            "ARREDONDO MARTINEZ SANJUANA GUADALUPE",
            "CALDERON RIVERA KIMBERLY MIA JAZMIN", 
            "CARRILLO RAMIREZ DADWIN ALI",
            "CHAVEZ ALVAREZ MELISSA GUADALUPE",
            "CISNEROS PALOMO JONATHAN",
            "DAVILA CARRILLO LUIS ANTONIO",
            "DE LARA MIRELES OSCAR YOSEFT",
            "DUEÑAS DE LIRA MARLENE",
            "ESCAMILLA LOPEZ LEONARDO DE JESUS", 
            "FLORES RUIZ MARIA BELEN",
            "GALLARDO CASTRO JOSELINE",
            "GARCIA MARTINEZ NATALY KORAIDE",
            "GONZALEZ ARREDONDO DIANA JANETH",
            "GONZALEZ TRONCOSO DIANA YOSELIN",
            "GUERRERO MONTALVO ALONDRA GUADALUPE",
            "GUTIERREZ AYALA DIEGO",
            "HERNANDEZ MAR CAROLINA",
            "LOERA RODRIGUEZ ALONDRA ZUSET",
            "MARIN NAJERA RIGOBERTO MARTIN",
            "MARTINEZ CARRILLO RUBI MARINA",
            "MONTAÑON HERRERA VALERIA ESTEFANI",
            "MUÑOZ IBARRA VICTORIA JAQUELINE",
            "MUÑOZ LARA SERGIO GUADALUPE",
            "MUÑOZ OLVERA VANESSA YOSELIN",
            "OLVERA GUTIERREZ JAVIER EDUARDO",
            "RAMIREZ CARRILLO LUZ YARETZI",
            "RAMIREZ VALDIVIA REYNA GUADALUPE",
            "RODRIGUEZ GARCIA EMMANUEL HARAHEL",
            "RODRIGUEZ VARGAS FABIOLA JAZMIN",
            "ROSALES CAMPOS CAMILA MARIEL",
            "SALAZAR LOPEZ FATIMA YAZMIN",
            "SALDAÑA DORADO MARIA ITZEL",
            "SAUCEDO ALVARADO MANUELA",
            "SOLANO LOERA FATIMA",
            "TREJO TORRES ASHLEY ADILENE",
            "CASTILLO DAVILA SOFIA ABIGAIL",
            "MUÑOZ AVILA JUAN PABLO"
        ]
        
        # Inicialización de status para cada estudiante
        self.status = {}
        for estudiante in self.estudiantes:
            self.status[estudiante] = {
                'participaciones': 0,
                'no_quisieron': 0,
                'no_estan': 0,
                'observaciones': ''
            }
            
        # Se intenta cargar historial y status desde archivo (si existe)
        self.cargar_historial()
        
        self.setup_ui()
        self.actualizar_vista_historial()
        
    def setup_ui(self):
        self.notebook = ttk.Notebook(self.ventana)
        self.notebook.pack(expand=True, fill='both')
        
        # Pestaña Ruleta
        self.tab_ruleta = ttk.Frame(self.notebook)
        self.notebook.add(self.tab_ruleta, text='Ruleta')
        
        # Pestaña Lista de Estudiantes
        self.tab_lista = ttk.Frame(self.notebook)
        self.notebook.add(self.tab_lista, text='Lista de Estudiantes')
        
        self.setup_tab_ruleta()
        self.setup_tab_lista()
        
    def setup_tab_ruleta(self):
        style = ttk.Style()
        style.configure("TButton", font=("Arial", 12), padding=10)
        style.configure("TLabel", font=("Arial", 12))
        
        self.frame_principal = ttk.PanedWindow(self.tab_ruleta, orient=tk.HORIZONTAL)
        self.frame_principal.pack(expand=True, fill='both', padx=5, pady=5)
        
        self.frame_izquierdo = ttk.Frame(self.frame_principal)
        self.frame_principal.add(self.frame_izquierdo, weight=1)
        
        self.frame_derecho = ttk.Frame(self.frame_principal)
        self.frame_principal.add(self.frame_derecho, weight=1)
        
        self.setup_izquierdo()
        self.setup_derecho()
        
    def setup_tab_lista(self):
        # Treeview con columnas actualizadas para mostrar los contadores
        self.tree_estudiantes = ttk.Treeview(self.tab_lista, 
                                             columns=("Estudiante", "Participaciones", "No quiso", "No está", "Observaciones"),
                                             show="headings")
        self.tree_estudiantes.heading("Estudiante", text="Estudiante")
        self.tree_estudiantes.heading("Participaciones", text="Participaciones")
        self.tree_estudiantes.heading("No quiso", text="No quiso")
        self.tree_estudiantes.heading("No está", text="No está")
        self.tree_estudiantes.heading("Observaciones", text="Observaciones")
        
        self.tree_estudiantes.column("Estudiante", width=300)
        self.tree_estudiantes.column("Participaciones", width=100, anchor=tk.CENTER)
        self.tree_estudiantes.column("No quiso", width=100, anchor=tk.CENTER)
        self.tree_estudiantes.column("No está", width=100, anchor=tk.CENTER)
        self.tree_estudiantes.column("Observaciones", width=300)
        
        self.tree_estudiantes.pack(expand=True, fill='both', padx=5, pady=5)
        
        # Botones para actualizar los contadores manualmente
        frame_botones = ttk.Frame(self.tab_lista)
        frame_botones.pack(pady=10)
        
        ttk.Button(frame_botones, text="Participó", 
                   command=lambda: self.cambiar_estado_manual("Participó")).pack(side=tk.LEFT, padx=5)
        ttk.Button(frame_botones, text="No quiso", 
                   command=lambda: self.cambiar_estado_manual("No quiso")).pack(side=tk.LEFT, padx=5)
        ttk.Button(frame_botones, text="No está", 
                   command=lambda: self.cambiar_estado_manual("No está")).pack(side=tk.LEFT, padx=5)
        ttk.Button(frame_botones, text="Editar Participaciones", 
                   command=self.editar_participaciones).pack(side=tk.LEFT, padx=5)
        
        # Entrada y botón para actualizar observaciones
        self.entry_obs = ttk.Entry(self.tab_lista, width=50)
        self.entry_obs.pack(pady=10)
        ttk.Button(self.tab_lista, text="Actualizar Observación", 
                   command=self.actualizar_observacion).pack(pady=5)
        
        self.actualizar_lista_estudiantes()
        
    def cambiar_estado_manual(self, nuevo_estado):
        """Incrementa el contador correspondiente y crea un registro en el historial."""
        seleccion = self.tree_estudiantes.selection()
        if not seleccion:
            messagebox.showwarning("Aviso", "Seleccione un estudiante")
            return
        estudiante = self.tree_estudiantes.item(seleccion[0])['values'][0]
        self.registrar_estado(nuevo_estado, estudiante)
        
    def actualizar_observacion(self):
        seleccion = self.tree_estudiantes.selection()
        if not seleccion:
            messagebox.showwarning("Aviso", "Seleccione un estudiante")
            return
        estudiante = self.tree_estudiantes.item(seleccion[0])['values'][0]
        obs = self.entry_obs.get()
        self.status[estudiante]['observaciones'] = obs
        self.actualizar_lista_estudiantes()
        self.entry_obs.delete(0, tk.END)
        
    def editar_participaciones(self):
        """Permite editar manualmente los contadores de un estudiante seleccionado."""
        seleccion = self.tree_estudiantes.selection()
        if not seleccion:
            messagebox.showwarning("Aviso", "Seleccione un estudiante para editar")
            return
        estudiante = self.tree_estudiantes.item(seleccion[0])['values'][0]
        data = self.status[estudiante]
        new_participaciones = simpledialog.askinteger("Editar Participaciones", 
                                                      f"Participaciones actuales: {data['participaciones']}\nIngrese nuevo valor:",
                                                      initialvalue=data['participaciones'], minvalue=0)
        if new_participaciones is None:
            return
        new_no_quisieron = simpledialog.askinteger("Editar No quiso", 
                                                   f"No quiso actuales: {data['no_quisieron']}\nIngrese nuevo valor:",
                                                   initialvalue=data['no_quisieron'], minvalue=0)
        if new_no_quisieron is None:
            return
        new_no_estan = simpledialog.askinteger("Editar No está", 
                                               f"No está actuales: {data['no_estan']}\nIngrese nuevo valor:",
                                               initialvalue=data['no_estan'], minvalue=0)
        if new_no_estan is None:
            return
        self.status[estudiante]['participaciones'] = new_participaciones
        self.status[estudiante]['no_quisieron'] = new_no_quisieron
        self.status[estudiante]['no_estan'] = new_no_estan
        self.actualizar_estadisticas()
        self.actualizar_lista_estudiantes()
        self.guardar_historial()
        
    def actualizar_lista_estudiantes(self):
        """Actualiza el treeview de la pestaña de Lista de Estudiantes."""
        for item in self.tree_estudiantes.get_children():
            self.tree_estudiantes.delete(item)
        for estudiante in self.estudiantes:
            data = self.status[estudiante]
            self.tree_estudiantes.insert('', 'end', values=(
                estudiante,
                data.get('participaciones', 0),
                data.get('no_quisieron', 0),
                data.get('no_estan', 0),
                data.get('observaciones', '')
            ))
            
    def setup_izquierdo(self):
        self.titulo = ttk.Label(self.frame_izquierdo, text="Ruleta de Estudiantes", font=("Arial", 24))
        self.titulo.pack(pady=20)
        
        self.frame_stats = ttk.LabelFrame(self.frame_izquierdo, text="Estadísticas")
        self.frame_stats.pack(fill='x', padx=5, pady=5)
        
        self.label_stats = ttk.Label(self.frame_stats, text="")
        self.label_stats.pack(pady=10)
        
        self.frame_botones = ttk.Frame(self.frame_izquierdo)
        self.frame_botones.pack(pady=20)
        
        # Botón de selección aleatoria (se deshabilitará mientras gira)
        self.boton_girar = ttk.Button(self.frame_botones, text="Seleccionar Estudiante", command=self.girar_ruleta)
        self.boton_girar.pack(pady=5)
        
        ttk.Button(self.frame_botones, text="Reiniciar", command=self.confirmar_reinicio).pack(pady=5)
        ttk.Button(self.frame_botones, text="Exportar Historial", command=self.exportar_historial).pack(pady=5)
        ttk.Button(self.frame_botones, text="Agregar Estudiante", command=self.agregar_estudiante).pack(pady=5)
        
        self.frame_resultado = ttk.LabelFrame(self.frame_izquierdo, text="Selección Actual")
        self.frame_resultado.pack(fill='x', padx=5, pady=5)
        
        # Label con fuente grande para mostrar el estudiante seleccionado
        self.label_resultado = ttk.Label(self.frame_resultado, text="", font=("Arial", 32, "bold"))
        self.label_resultado.pack(pady=10)
        
        self.actualizar_estadisticas()
        
    def setup_derecho(self):
        ttk.Label(self.frame_derecho, text="Historial de Participaciones", font=("Arial", 16)).pack(pady=10)
                 
        self.frame_busqueda = ttk.Frame(self.frame_derecho)
        self.frame_busqueda.pack(fill='x', padx=5, pady=5)

        self.entry_busqueda = ttk.Entry(self.frame_busqueda)
        self.entry_busqueda.pack(side=tk.LEFT, expand=True, fill='x', padx=5, pady=5)

        ttk.Button(self.frame_busqueda, text="Buscar", command=self.buscar_estudiante).pack(side=tk.RIGHT, padx=5)
        
        # Treeview para el historial de registros
        self.tree_historial = ttk.Treeview(self.frame_derecho, 
                                           columns=("Fecha", "Estudiante", "Estado", "Cantidad", "Tiempo", "Observaciones"),
                                           show="headings")
        self.tree_historial.heading("Fecha", text="Fecha")
        self.tree_historial.heading("Estudiante", text="Estudiante")
        self.tree_historial.heading("Estado", text="Estado")
        self.tree_historial.heading("Cantidad", text="Cantidad")
        self.tree_historial.heading("Tiempo", text="Tiempo (s)")
        self.tree_historial.heading("Observaciones", text="Observaciones")
        
        self.tree_historial.column("Fecha", width=150)
        self.tree_historial.column("Estudiante", width=300)
        self.tree_historial.column("Estado", width=100, anchor=tk.CENTER)
        self.tree_historial.column("Cantidad", width=80, anchor=tk.CENTER)
        self.tree_historial.column("Tiempo", width=80, anchor=tk.CENTER)
        self.tree_historial.column("Observaciones", width=200)
        
        self.tree_historial.pack(fill='both', expand=True, padx=5, pady=5)
        
        ttk.Button(self.frame_derecho, text="Editar Registro", command=self.editar_registro).pack(pady=5)
        
    def actualizar_estadisticas(self):
        total = len(self.estudiantes)
        estudiantes_participaron = sum(1 for e in self.status if self.status[e].get('participaciones', 0) > 0)
        total_participaciones = sum(self.status[e].get('participaciones', 0) for e in self.status)
        total_no_quisieron = sum(self.status[e].get('no_quisieron', 0) for e in self.status)
        total_no_estan = sum(self.status[e].get('no_estan', 0) for e in self.status)
        pendientes = sum(1 for e in self.status if (
            self.status[e].get('participaciones', 0) == 0 and 
            self.status[e].get('no_quisieron', 0) == 0 and 
            self.status[e].get('no_estan', 0) == 0
        ))
        
        stats = f"Total de estudiantes: {total}\n"
        stats += f"Estudiantes que han participado: {estudiantes_participaron}\n"
        stats += f"Total de participaciones: {total_participaciones}\n"
        stats += f"No quisieron: {total_no_quisieron}\n"
        stats += f"No están: {total_no_estan}\n"
        stats += f"Pendientes: {pendientes}"
        
        self.label_stats.config(text=stats)
        
    def exportar_historial(self):
        if not self.historial:
            messagebox.showwarning("Aviso", "No hay historial para exportar.")
            return
        try:
            filename = os.path.join(self.directorio_datos, f"historial_participaciones_{datetime.now().strftime('%Y%m%d_%H%M%S')}.xlsx")
            # Preparar la información duplicando filas según el campo "cantidad"
            export_data = []
            for registro in self.historial:
                for _ in range(registro.get('cantidad', 1)):
                    export_data.append({
                        'Fecha': registro['fecha'],
                        'Estudiante': registro['estudiante'],
                        'Estado': registro['estado'],
                        'Observaciones': registro.get('observaciones', ''),
                        'Tiempo (s)': registro.get('tiempo', '')
                    })
            df = pd.DataFrame(export_data)
            with pd.ExcelWriter(filename, engine='xlsxwriter') as writer:
                df.to_excel(writer, sheet_name='Historial', index=False)
                estudiantes_df = pd.DataFrame([
                    {
                        'Estudiante': estudiante,
                        'Participaciones': self.status[estudiante].get('participaciones', 0),
                        'No quiso': self.status[estudiante].get('no_quisieron', 0),
                        'No está': self.status[estudiante].get('no_estan', 0),
                        'Observaciones': self.status[estudiante].get('observaciones', '')
                    }
                    for estudiante in self.estudiantes
                ])
                estudiantes_df.to_excel(writer, sheet_name='Lista Estudiantes', index=False)
            messagebox.showinfo("Éxito", f"Historial exportado a {filename}")
        except Exception as e:
            messagebox.showerror("Error", f"Error al exportar: {str(e)}")
            
    def agregar_estudiante(self):
        nuevo = simpledialog.askstring("Agregar Estudiante", "Ingrese el nombre:")
        if not nuevo:
            return
        nuevo = nuevo.upper().strip()
        if not self.validar_nombre(nuevo):
            messagebox.showerror("Error", "Nombre inválido. Use solo letras y espacios.")
            return
        if nuevo in self.estudiantes:
            messagebox.showwarning("Aviso", f"{nuevo} ya está en la lista.")
            return
        self.estudiantes.append(nuevo)
        self.status[nuevo] = {
            'participaciones': 0,
            'no_quisieron': 0,
            'no_estan': 0,
            'observaciones': ''
        }
        messagebox.showinfo("Éxito", f"{nuevo} agregado a la lista")
        self.actualizar_estadisticas()
        self.actualizar_lista_estudiantes()
        self.guardar_historial()
            
    def confirmar_reinicio(self):
        if messagebox.askyesno("Confirmar", "¿Desea reiniciar la ruleta? Se perderá el progreso actual"):
            self.historial = []
            for estudiante in self.estudiantes:
                self.status[estudiante] = {
                    'participaciones': 0,
                    'no_quisieron': 0,
                    'no_estan': 0,
                    'observaciones': ''
                }
            self.guardar_historial()
            self.actualizar_vista_historial()
            self.actualizar_lista_estudiantes()
            self.label_resultado.config(text="")
            self.actualizar_estadisticas()
            
    def validar_nombre(self, nombre):
        return bool(re.match(r'^[A-ZÁÉÍÓÚÑ\s]+$', nombre))
        
    def cargar_historial(self):
        """Carga el historial y status desde el archivo JSON, asegurándose de que cada estudiante tenga la estructura completa."""
        if Path(self.registro_file).exists():
            try:
                with open(self.registro_file, 'r', encoding='utf-8') as f:
                    datos = json.load(f)
                    if isinstance(datos, dict):
                        if 'historial' in datos and 'status' in datos:
                            self.historial = datos['historial']
                            self.status = datos['status']
                            for estudiante in self.estudiantes:
                                if estudiante not in self.status:
                                    self.status[estudiante] = {
                                        'participaciones': 0,
                                        'no_quisieron': 0,
                                        'no_estan': 0,
                                        'observaciones': ''
                                    }
                                else:
                                    self.status[estudiante].setdefault('participaciones', 0)
                                    self.status[estudiante].setdefault('no_quisieron', 0)
                                    self.status[estudiante].setdefault('no_estan', 0)
                                    self.status[estudiante].setdefault('observaciones', '')
                        else:
                            self.historial = []
                    else:
                        self.historial = []
            except Exception as e:
                messagebox.showerror("Error", f"No se pudo cargar el historial: {str(e)}")
                
    def guardar_historial(self):
        try:
            Path(self.registro_file).touch(exist_ok=True)
            datos = {
                'historial': self.historial,
                'status': self.status
            }
            with open(self.registro_file, 'w', encoding='utf-8') as f:
                json.dump(datos, f, ensure_ascii=False, indent=2)
        except Exception as e:
            messagebox.showerror("Error", f"Error al guardar historial: {str(e)}")
            
    def buscar_estudiante(self):
        termino = self.entry_busqueda.get().strip().upper()
        for item in self.tree_historial.get_children():
            self.tree_historial.delete(item)
        for registro in self.historial:
            if termino in registro['estudiante']:
                self.tree_historial.insert('', 'end', values=(
                    registro['fecha'],
                    registro['estudiante'],
                    registro.get('estado', ''),
                    registro.get('cantidad', 1),
                    registro.get('tiempo', ''),
                    registro.get('observaciones', '')
                ))
                
    def registrar_estado(self, estado, estudiante=None):
        """
        Actualiza el contador correspondiente en status y añade un registro en el historial.
        Si 'estudiante' no se pasa, se usa self.estudiante_actual (de la ruleta).
        """
        if estudiante is None:
            if not hasattr(self, 'estudiante_actual'):
                messagebox.showwarning("Aviso", "Primero seleccione un estudiante")
                return
            estudiante = self.estudiante_actual
        
        if estado == "Participó":
            self.status[estudiante]['participaciones'] += 1
        elif estado == "No quiso":
            self.status[estudiante]['no_quisieron'] += 1
        elif estado == "No está":
            self.status[estudiante]['no_estan'] += 1
        
        tiempo = round(time.time() - self.tiempo_inicio, 2) if self.tiempo_inicio else 0
        registro = {
            'estudiante': estudiante,
            'fecha': datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
            'estado': estado,
            'observaciones': '',
            'tiempo': tiempo,
            'cantidad': 1
        }
        self.historial.append(registro)
        self.guardar_historial()
        self.actualizar_vista_historial()
        self.actualizar_lista_estudiantes()
        self.actualizar_estadisticas()
        
    # Método girar_ruleta modificado para usar la "animación" y registrar en el historial
    def girar_ruleta(self):
        try:
            self.tiempo_inicio = time.time()
            # Deshabilitar botón mientras se simula el giro
            self.boton_girar.config(state="disabled")
            self.label_resultado.config(text="Girando...")
            self.ventana.update()
            time.sleep(1)  # Simula el giro
            
            # Se seleccionan sólo los estudiantes sin ningún registro (todos los contadores en 0)
            estudiantes_elegibles = [e for e in self.estudiantes if (
                self.status[e].get('participaciones', 0) +
                self.status[e].get('no_quisieron', 0) +
                self.status[e].get('no_estan', 0)
            ) == 0]
            
            if not estudiantes_elegibles:
                messagebox.showinfo("Aviso", "No hay estudiantes pendientes")
                self.boton_girar.config(state="normal")
                return
            
            self.estudiante_actual = random.choice(estudiantes_elegibles)
            # Actualiza la etiqueta con el alumno seleccionado
            self.label_resultado.config(text=f"¡Seleccionado!\n{self.estudiante_actual}")
            self.boton_girar.config(state="normal")
            
            # Crear registro en el historial
            registro = {
                'estudiante': self.estudiante_actual,
                'fecha': datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
                'estado': "Seleccionado",
                'observaciones': '',
                'tiempo': round(time.time() - self.tiempo_inicio, 2),
                'cantidad': 1
            }
            self.historial.append(registro)
            self.guardar_historial()
            self.actualizar_vista_historial()
            
        except Exception as e:
            messagebox.showerror("Error", f"Error al seleccionar: {str(e)}")
            
    def actualizar_vista_historial(self):
        for item in self.tree_historial.get_children():
            self.tree_historial.delete(item)
        for registro in self.historial:
            self.tree_historial.insert('', 'end', values=(
                registro['fecha'],
                registro['estudiante'],
                registro.get('estado', ''),
                registro.get('cantidad', 1),
                registro.get('tiempo', ''),
                registro.get('observaciones', '')
            ))
            
    def editar_registro(self):
        """
        Permite editar la fecha y el número (cantidad) de una participación en el historial.
        Nota: se utiliza el índice de la lista (suponiendo que el orden del treeview coincide con el de self.historial).
        """
        seleccion = self.tree_historial.selection()
        if not seleccion:
            messagebox.showwarning("Aviso", "Seleccione un registro para editar")
            return
        index = self.tree_historial.index(seleccion[0])
        registro = self.historial[index]
        nueva_fecha = simpledialog.askstring("Editar Registro", "Editar Fecha (YYYY-MM-DD HH:MM:SS):", initialvalue=registro['fecha'])
        if not nueva_fecha:
            return
        try:
            datetime.strptime(nueva_fecha, "%Y-%m-%d %H:%M:%S")
        except:
            messagebox.showerror("Error", "Formato de fecha inválido.")
            return
        nueva_cantidad = simpledialog.askinteger("Editar Registro", "Editar Cantidad:", initialvalue=registro.get('cantidad', 1), minvalue=1)
        if nueva_cantidad is None:
            return
        registro['fecha'] = nueva_fecha
        registro['cantidad'] = nueva_cantidad
        self.historial[index] = registro
        self.guardar_historial()
        self.actualizar_vista_historial()
        
    def iniciar(self):
        self.ventana.mainloop()

if __name__ == "__main__":
    app = RuletaEstudiantes()
    app.iniciar()
