
# 🧠 Neural Networks

**By Lauriane Gélébart**  
Project for AI Class, IMAC3 (E5 ESIEE Paris) - October 2024

Developed in **Unity** 🎮

---

### 🚀 Project Overview

This project demonstrates the use of a neural network to train on binary input-output patterns. The system will process 4-input patterns with binary outputs and iteratively train the network to match the expected results. The project displays the inputs, expected outputs, and calculated outputs using Unity objects (cubes and spheres).

Key features:
- **Inputs**: 4 binary input values per pattern
- **Outputs**: Binary outputs, with a configurable number of output neurons (default: 2)
- **Training**: The network is trained over a predefined number of iterations (default: 5000)
- **Interactive Visualization**: Input values and expected outputs can be updated by clicking on the display objects during runtime.

![image](https://github.com/user-attachments/assets/46c972ad-1400-4761-82cb-ffa166f92f0f)

For more information, the console displays the error percentage and the exact output values for each pattern.
![image](https://github.com/user-attachments/assets/c8f842b6-ef44-4698-b648-0b4c5e692b66)

If an output value is **red**, it indicates that the value is not close enough to the expected value (0 or 1) with the `Epsilon` tolerance.

![image](https://github.com/user-attachments/assets/3ea56e9e-a41c-4581-99aa-2171549c90a3)


---

### 🚀 Getting Started

1. **Clone the repository**:
   ```bash
   git clone https://github.com/LaurianeGelebart/ArtificialNeuralNetworks.git
   ```

2. **Open the project** in Unity (version 2022.3.4f1 or later recommended).

3. **Run the project** to start the simulation.



### 🔧 Customizing the Simulation

💡 Adjust parameters in the `NeuralNetworkDemo` script to control the network configuration:

![image](https://github.com/user-attachments/assets/1d842371-5dbe-4349-9d9f-5482b5152bcd)

You can also modify the input-output patterns and observe how the network learns to match the expected results by clicking on them during the simulation.

![image](https://github.com/user-attachments/assets/6e721fb0-fcbc-467b-8c16-e5dd6aacda48)


---
