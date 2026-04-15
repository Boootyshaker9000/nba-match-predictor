# NBA Match Predictor AI

A high-performance C# desktop application built with **WPF** and **.NET 8** that predicts the outcome of NBA matchups using a pre-trained **Machine Learning** model.

## Overview
This project bridges the gap between Data Science (Python/Scikit-Learn) and Software Engineering (C#/.NET). It utilizes a **Random Forest** classification model, exported via **ONNX**, to provide real-time win probabilities for any NBA matchup based on historical performance, fatigue factors, and roster changes.

## Key Features
* **AI-Powered Predictions:** Uses a serialized Random Forest model for highly accurate sports forecasting.
* **Dynamic Data Loading:** Automatically loads team statistics, rest days, and star player availability from a local `team-stats.json` database.
* **Fatigue Analysis:** Account for "Back-to-Back" games and rest days, a crucial factor in NBA performance.
* **One-Hot Encoding Integration:** Seamlessly maps categorical team data to the numerical format required by the ONNX runtime.
* **Self-Contained Deployment:** Optimized for Windows with a clean, responsive WPF user interface.

## Data Format (JSON)
The application expects a `team-stats.json` file in the root directory. Each team is identified by its 3-letter abbreviation. Below is an example of the required structure:

```json
{
  "BOS": {
    "PTS_5G": 118.2,
    "FG_PCT": 0.485,
    "FG3_PCT": 0.392,
    "REB": 46.1,
    "AST": 26.5,
    "TOV": 11.8,
    "PLUS_MINUS": 10.2,
    "DAYS_REST": 2,
    "STAR_MISSING": 0
  },
  "LAL": {
    "PTS_5G": 115.5,
    "FG_PCT": 0.491,
    "FG3_PCT": 0.375,
    "REB": 42.8,
    "AST": 28.1,
    "TOV": 13.5,
    "PLUS_MINUS": 1.5,
    "DAYS_REST": 1,
    "STAR_MISSING": 1
  }
}
```

## Tech Stack
* **Frontend:** C# / WPF (.NET 8.0)
* **AI Runtime:** Microsoft.ML.OnnxRuntime
* **Model Format:** ONNX (Open Neural Network Exchange)
* **Data Format:** JSON (System.Text.Json)
* **Original Model:** Python / Scikit-Learn (Random Forest)

## Project Structure
* `MainWindow.xaml`: Defines the graphical user interface.
* `MainWindow.xaml.cs`: Contains the prediction logic and ONNX inference engine.
* `TeamStats.cs`: Data model for deserializing team statistics.
* `team-stats.json`: Local database containing the latest 5-game averages for all 30 NBA teams.
* `nba-model.onnx`: The serialized brain of the application.

## Installation & Build

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* Windows OS (Required for WPF)

### Building from Command Line
1.  Navigate to the project root directory.
2.  Restore dependencies and build the project:
    ```bash
    dotnet build -c Release
    ```
3.  To create a standalone executable:
    ```bash
    dotnet publish -c Release -r win-x64 --self-contained true
    ```

## How to Use
1.  Launch `NBAMatchPredictor.exe`.
2.  Select the **Home Team** from the dropdown menu.
3.  Select the **Away Team** from the dropdown menu.
4.  Click **Calculate Win Probability**.
5.  The AI will analyze the stats and display the win percentage for both teams.

> **Note:** Ensure that `nba-model.onnx` and `team-stats.json` are located in the same directory as the executable.

## How the Model Works
The underlying model was trained on a dataset of over 1,500 NBA games. It evaluates **80 unique features** per matchup, including:
* **Team Form:** Points, Rebounds, Assists, and Shooting percentages over the last 5 games.
* **Fatigue:** Days of rest and back-to-back game status.
* **Roster Impact:** Automated detection of missing "Franchise Players."

---
*Disclaimer: This tool is intended for educational and analytical purposes only. It does not guarantee betting success.*
