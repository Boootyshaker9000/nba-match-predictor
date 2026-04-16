# Project Documentation: NBA Match Predictor AI

**Author:** Zdeněk Relich 
**E-mail:** relich@post.cz
**Date of Creation:** April 2026  
**School:** SPŠE Ječná  
**Course:** Software

*Disclaimer: This documentation and the accompanying software were created exclusively as a school project and serve purely educational purposes.*

---

## 1. Requirements Specification
The application is designed to predict the win probability of North American basketball league (NBA) matches based on historical data and current team status (fatigue, injuries).

**Functional Requirements (FR):**
* **FR1:** The user can select any home and away team from the current 30 NBA teams using two dropdown menus (ComboBoxes).
* **FR2:** The application must prevent a prediction if the user selects the same team as both home and away.
* **FR3:** Upon running the analysis, the application locally calculates the percentage chance of winning for both the home and away teams and displays the result with 1-decimal-place precision.
* **FR4:** The application automatically loads current statistical team forms (averages, rebounds, star player absences) from the `team-stats.json` configuration file, so the user does not have to enter them manually.
* **FR5:** The calculation (inference) runs strictly offline using a pre-trained AI model in ONNX format.

**User Use Cases:**
1. The user launches the application.
2. Selects Team A from the `Home Team` menu.
3. Selects Team B from the `Away Team` menu.
4. Clicks the `Calculate Win Probability` button.
5. The user reads the calculated result on the screen.

---

## 2. Architecture Description
The application is designed as a monolithic local desktop application for Windows OS. It relies on separating responsibilities into three main pillars (architectural design inspired by the Code-Behind model in WPF):

1. **Presentation Layer (UI):** Consists of the `MainWindow.xaml` file. It utilizes the WPF framework to display the form and UI controls.
2. **Application and Integration Layer:** Logic contained in `MainWindow.xaml.cs`. This layer acts as a "translator". It takes inputs from the UI, performs *One-Hot Encoding*, and constructs a mathematical array (Tensor) of 80 elements.
3. **Data and AI Layer:** Comprises a serialized data structure (`TeamStats.cs`), a local JSON database, and an Artificial Intelligence module operated by the OnnxRuntime engine.

**Component Diagram (Big Image):**
`[ GUI (XAML) ] <---> [ C# Logic (Mapping) ] <---> [ ONNX AI Model (nba-model.onnx) ]`
`                                |`
`                      [ team-stats.json ]`

---

## 3. Application Flow
A typical application run (from the perspective of an Activity Diagram) proceeds as follows:
1. **Initialization (App Start):** The system attempts to deserialize the `team-stats.json` file into memory (`Dictionary<string, TeamStats>`). It then instantiates an `InferenceSession` to load `nba-model.onnx`. If files are missing, it triggers an Error state.
2. **Waiting for User:** The application renders the window and populates the ComboBoxes with alphabetically sorted keys from the JSON database (team abbreviations).
3. **Action (Button Click):**
    * The system performs basic validation (checking if two *different* teams are selected).
    * Retrieves the corresponding `TeamStats` objects from memory.
    * Creates a one-dimensional array `float[80]` filled with zeros.
    * Performs *Data Mapping*: Maps form and fatigue statistics to indices 0-19.
    * Performs *One-Hot Encoding*: Calculates the alphabetical offset of the team and writes `1.0f` to the corresponding index in the home and away team sections (indices 20-79).
    * The Tensor is passed to the ONNX model via the `session.Run()` method.
4. **Displaying Output:** Retrieves the Probability Map, multiplies by 100, formats the string, and outputs it to the UI.

---

## 4. Interfaces, Libraries, and Dependencies Used
The application does not require a network connection to run (full offline execution) but strongly depends on the following third-party technologies:
* **.NET 8.0 Desktop Runtime:** The runtime environment providing the WPF framework for graphics.
* **Microsoft.ML.OnnxRuntime (version 1.24.4):** An external NuGet library for executing (inference) predictive ML models based on the ONNX standard.
* **System.Text.Json:** Built-in library for fast JSON database deserialization.
* **Scikit-Learn (Python):** (Indirectly). The library used for the initial training of the Random Forest algorithm, which was exported to C#.

---

## 5. Legal and Licensing Aspects
* **Application Source Code:** Provided under the standard MIT License.
* **API and Data Restrictions:** The module (AI brain) was trained on data provided by the official NBA API. NBA team data and logos are the intellectual property of the National Basketball Association. The project is strictly for analytical and educational purposes and cannot be sold commercially.
* **Disclaimer:** The application provides mathematical probabilities that do not guarantee actual match outcomes. It is not intended as a tool for real financial betting.

---

## 6. Program Configuration
The application is intentionally designed as *Data-Driven*. There is no need to modify the source code (C#) to alter team parameters.
All configuration is done by editing the `team-stats.json` file in the root directory. The User/Admin can manually change configuration parameters before starting the application.
* Example: To simulate that Boston's star player will not play due to an injury, simply change the `"STAR_MISSING": 0` parameter to `"STAR_MISSING": 1` under the "BOS" key in the JSON file. The application will automatically account for this change during the next prediction run and adjust the odds accordingly.

---

## 7. Installation and Execution
1. **Running from Source:** Open the folder in the command line and execute `dotnet run` (or press F5 in the Visual Studio / JetBrains Rider IDE).
2. **Compilation for Distribution:** The application can be compiled as a standalone *self-contained* package using the command:
   `dotnet publish -c Release -r win-x64 --self-contained true`
3. **Execution:** Double-click the `NBAMatchPredictor.exe` file.
   *Requirement:* The executable file must always be located in the same folder as the `nba-model.onnx` and `team-stats.json` files.

---

## 8. Error States and Troubleshooting

| Error Name / Type | When it Occurs | Application Reaction / Solution |
| :--- | :--- | :--- |
| **Empty Input** | The user clicks predict without selecting both teams. | The app catches the state before calling the AI and displays a MessageBox: *"Please select both teams!"* |
| **Identical Teams** | The user selects the same team for both sides. | The app displays a MessageBox: *"A team cannot play against itself!"* |
| **FileNotFoundException** | The system cannot find the `.json` or `.onnx` file upon startup. | Caught in a global `try-catch` block during initialization. Output: *"Initialization error"*. |
| **OnnxRuntimeException** (Dimension Error) | If an array with a different number of elements (e.g., 82) than it was trained on (80) is sent to the ONNX model. | The `catch` block during prediction is triggered; the window does not crash but informs the user about the vector length mismatch. |

---

## 9. Verification, Testing, and Validation
Functional validation was conducted through system and Black-box tests:
1. **Data Input Validation:** It was successfully verified that the `inputData` variable always generates an array with a defined length of 80 elements.
2. **One-Hot Encoding Validation:** Edge cases were tested (the first team alphabetically `ATL`, the last `WAS`) to ensure that indices do not fall outside the created `float[]` array, preventing an *IndexOutOfRangeException*.
3. **Integration Validation:** The probabilities (output_label) generated by the program were compared with probabilities generated in a pure Python/Scikit-Learn environment for identical sets of input data. The results match up to the third decimal place.
   The application fully meets all prescribed Functional Requirements.

---

## 10. Version List and Known Bugs (Issues)
* **Version 1.0.0:** Base production version.
* **Issue #1 (Known Bug):** If a new team were to join the NBA in the future (league expansion from 30 to 31 teams), the application would crash during *One-Hot Encoding*. Adding a new team currently requires a complete retraining of the ONNX model and updating the `_numberOfColumns` constant in the C# code from 80 to 82.

---

## 11. Database and Import Data Schema
The application does not use a traditional relational SQL database (an ER diagram is not relevant here); instead, it utilizes a *flat-file NoSQL* database in JSON format, which also serves as the schema for statistical data import.

**Imported Object Schema (`team-stats.json`):**
Represents a collection mapped as a dictionary `Dictionary<string, TeamStats>`.
* **Key (PK):** `string` (3-letter team abbreviation, e.g., "BOS")
* **Values (Nested Object):**
    * `PTS_5G` (float, Points average over the last 5 games)
    * `FG_PCT` (float, Field goal percentage - decimal number 0-1)
    * `FG3_PCT` (float, 3-point field goal percentage - decimal number 0-1)
    * `REB` (float, Average number of rebounds)
    * `AST` (float, Average number of assists)
    * `TOV` (float, Average number of turnovers)
    * `PLUS_MINUS` (float, Average score differential)
    * `DAYS_REST` (float, Number of rest days before the game. 1 = Back-to-Back)
    * `STAR_MISSING` (int/boolean, 0 = Star plays, 1 = Star is missing)

All items in the JSON object are mandatory. In the event of a missing item, the application assigns a default value (0) during serialization via the `System.Text.Json` library.****