using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace NBAMatchPredictor
{
    public partial class MainWindow : Window
    {
        private InferenceSession? _session;
        private Dictionary<string, TeamStats>? _nbaDatabase;
        private List<string>? _teamNamesSorted;
        
        private readonly int _numberOfColumns = 80;

        public MainWindow()
        {
            InitializeComponent();
            LoadDataAndModel();
        }

        private void LoadDataAndModel()
        {
            try
            {
                // 1. Load JSON database
                string jsonString = File.ReadAllText("team-stats.json");
                _nbaDatabase = JsonSerializer.Deserialize<Dictionary<string, TeamStats>>(jsonString);
                
                // 2. Sort teams alphabetically (Must match Pandas get_dummies logic)
                _teamNamesSorted = _nbaDatabase?.Keys.OrderBy(key => key).ToList();

                // 3. Populate ComboBoxes
                comboHome.ItemsSource = _teamNamesSorted;
                comboAway.ItemsSource = _teamNamesSorted;

                // 4. Initialize AI Model
                _session = new InferenceSession("nba_model.onnx");
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Initialization error: {exception.Message}");
            }
        }

        private void btnPredict_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (comboHome.SelectedItem == null || comboAway.SelectedItem == null)
            {
                MessageBox.Show("Please select both teams!");
                return;
            }

            string? homeKey = comboHome.SelectedItem.ToString();
            string? awayKey = comboAway.SelectedItem.ToString();

            if (homeKey == awayKey)
            {
                MessageBox.Show("A team cannot play against itself!");
                return;
            }

            // Extract stats from loaded JSON
            TeamStats homeTeam = _nbaDatabase?[homeKey];
            TeamStats awayTeam = _nbaDatabase?[awayKey];

            // Create an empty array filled with zeros
            float[] inputData = new float[_numberOfColumns];

            // --- 1. MAPPING DATA FROM JSON TO ARRAY ---
            // Home Form (Indices 0 - 6)
            inputData[0] = homeTeam.Pts5G;
            inputData[1] = homeTeam.FgPct;
            inputData[2] = homeTeam.Fg3Pct;
            inputData[3] = homeTeam.Reb;
            inputData[4] = homeTeam.Ast;
            inputData[5] = homeTeam.Tov;
            inputData[6] = homeTeam.PlusMinus;
            
            // Home Rest/Fatigue (Indices 7 - 8)
            inputData[7] = homeTeam.DaysRest;
            inputData[8] = homeTeam.DaysRest <= 1 ? 1.0f : 0.0f;

            // Away Form (Indices 9 - 15)
            inputData[9] = awayTeam.Pts5G;
            inputData[10] = awayTeam.FgPct;
            inputData[11] = awayTeam.Fg3Pct;
            inputData[12] = awayTeam.Reb;
            inputData[13] = awayTeam.Ast;
            inputData[14] = awayTeam.Tov;
            inputData[15] = awayTeam.PlusMinus;

            // Away Rest/Fatigue (Indices 16 - 17)
            inputData[16] = awayTeam.DaysRest;
            inputData[17] = awayTeam.DaysRest <= 1 ? 1.0f : 0.0f; 

            // Stars Missing (Indices 18 - 19)
            inputData[18] = homeTeam.StarMissing;
            inputData[19] = awayTeam.StarMissing;

            // --- 2. TEAM MAPPING (One-Hot Encoding) ---
            // Finds the team's alphabetical index and assigns 1.0 to the correct column
            int homeTeamIndexStart = 20; 
            int homeIndexOffset = _teamNamesSorted.IndexOf(homeKey);
            inputData[homeTeamIndexStart + homeIndexOffset] = 1.0f;

            int awayTeamIndexStart = 20 + _teamNamesSorted.Count; // Usually starts at index 50
            int awayIndexOffset = _teamNamesSorted.IndexOf(awayKey);
            inputData[awayTeamIndexStart + awayIndexOffset] = 1.0f;

            // --- 3. RUN MODEL ---
            try
            {
                var tensor = new DenseTensor<float>(inputData, new int[] { 1, _numberOfColumns });
                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("float_input", tensor)
                };

                using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = _session.Run(inputs);

                // Extract probabilities (Random Forest returns a Dictionary/Map)
                var probabilities = results.Last().AsEnumerable<NamedOnnxValue>();
                var probDictionary = probabilities.First().AsDictionary<Int64, float>();

                float awayChance = probDictionary[0] * 100;
                float homeChance = probDictionary[1] * 100;

                // Display result in UI
                txtResult.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 33, 33));
                txtResult.Text = $"{homeKey} (Home) Win Probability: {homeChance:F1} %\n" +
                                 $"{awayKey} (Away) Win Probability: {awayChance:F1} %";
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Prediction error: {exception.Message}\n\nThe column count likely doesn't match. Python model might expect a different number than {_numberOfColumns}.");
            }
        }
    }
}