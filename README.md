# ♟️ Ajedrez WPF con Stockfish

Aplicación de ajedrez desarrollada en WPF (.NET) que permite jugar:

- 👤 vs 👤 (dos jugadores en local)
- 👤 vs 🤖 (contra un motor de ajedrez)

El motor utilizado es Stockfish, uno de los engines de ajedrez más potentes disponibles.

---

## 🚀 Características

- Interfaz gráfica basada en WPF  
- Movimiento de piezas con interacción visual  
- Validación de jugadas  
- Modo local para dos jugadores  
- Modo contra motor (Stockfish)  
- Integración con motor externo mediante ejecución de binario  

---

## ⚙️ Requisitos

- .NET (versión compatible con WPF)  
- Sistema operativo Windows  
- El ejecutable de Stockfish incluido en la carpeta `Engines/`  

---

## 📦 Estructura del proyecto
```bash
/Ajedrez
├── Engines/
│ └── stockfish.exe
├── Images/
├── Sounds/
├── Src/
│ └── Logic/
│ └── Windows/
├── App.xaml
```

---

## ▶️ Ejecución
1. Clonar o descargar el proyecto  
2. Asegurarse de que `stockfish.exe` está en la carpeta `Engines`  
3. Ejecutar la aplicación desde Visual Studio o mediante el `.exe` generado  

---

## 🤖 Motor de ajedrez
This project uses Stockfish, a free and open-source chess engine licensed under the GNU GPL v3.
https://stockfishchess.org/
