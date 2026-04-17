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
    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// Handles user input, model initialization, and ONNX model inference.
    /// </summary>
    public partial class MainWindow : Window
    {
        private InferenceSession _session;
        private Dictionary<string, TeamStats> _nbaDatabase;
        private List<string> _teamNamesSorted;
        private readonly int _numberOfColumns = 80;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// Loads the necessary data and the ONNX model.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            LoadDataAndModel();
        }

        /// <summary>
        /// Loads the JSON database containing team statistics and initializes the ONNX inference session.
        /// Populates the UI combo boxes with alphabetically sorted team abbreviations.
        /// </summary>
        private void LoadDataAndModel()
        {
            try
            {
                string jsonString = File.ReadAllText("team-stats.json");
                _nbaDatabase = JsonSerializer.Deserialize<Dictionary<string, TeamStats>>(jsonString);
                
                _teamNamesSorted = _nbaDatabase.Keys.OrderBy(key => key).ToList();

                comboHome.ItemsSource = _teamNamesSorted;
                comboAway.ItemsSource = _teamNamesSorted;

                _session = new InferenceSession("nba-model.onnx");
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Initialization error: {exception.Message}");
            }
        }

        /// <summary>
        /// Handles the click event of the Predict button.
        /// Processes input data, performs one-hot encoding, runs the ONNX model inference, 
        /// and displays the calculated win probabilities.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="routedEventArgs">The event data.</param>
        private void btnPredict_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (comboHome.SelectedItem == null || comboAway.SelectedItem == null)
            {
                MessageBox.Show("Please select both teams!");
                return;
            }

            string homeKey = comboHome.SelectedItem.ToString();
            string awayKey = comboAway.SelectedItem.ToString();

            if (homeKey == awayKey)
            {
                MessageBox.Show("A team cannot play against itself!");
                return;
            }

            TeamStats homeTeam = _nbaDatabase[homeKey];
            TeamStats awayTeam = _nbaDatabase[awayKey];

            float[] inputData = new float[_numberOfColumns];

            inputData[0] = homeTeam.Pts5G;
            inputData[1] = homeTeam.FgPct;
            inputData[2] = homeTeam.Fg3Pct;
            inputData[3] = homeTeam.Reb;
            inputData[4] = homeTeam.Ast;
            inputData[5] = homeTeam.Tov;
            inputData[6] = homeTeam.PlusMinus;
            
            inputData[7] = homeTeam.DaysRest;
            inputData[8] = homeTeam.DaysRest <= 1 ? 1.0f : 0.0f;

            inputData[9] = awayTeam.Pts5G;
            inputData[10] = awayTeam.FgPct;
            inputData[11] = awayTeam.Fg3Pct;
            inputData[12] = awayTeam.Reb;
            inputData[13] = awayTeam.Ast;
            inputData[14] = awayTeam.Tov;
            inputData[15] = awayTeam.PlusMinus;

            inputData[16] = awayTeam.DaysRest;
            inputData[17] = awayTeam.DaysRest <= 1 ? 1.0f : 0.0f; 

            inputData[18] = homeTeam.StarMissing;
            inputData[19] = awayTeam.StarMissing;

            int homeTeamIndexStart = 20; 
            int homeIndexOffset = _teamNamesSorted.IndexOf(homeKey);
            inputData[homeTeamIndexStart + homeIndexOffset] = 1.0f;

            int awayTeamIndexStart = 20 + _teamNamesSorted.Count;
            int awayIndexOffset = _teamNamesSorted.IndexOf(awayKey);
            inputData[awayTeamIndexStart + awayIndexOffset] = 1.0f;

            try
            {
                var tensor = new DenseTensor<float>(inputData, new int[] { 1, _numberOfColumns });
                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("float_input", tensor)
                };

                using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = _session.Run(inputs);

                var probabilities = results.Last().AsEnumerable<NamedOnnxValue>();
                var probDictionary = probabilities.First().AsDictionary<Int64, float>();

                float awayChance = probDictionary[0] * 100;
                float homeChance = probDictionary[1] * 100;

                txtResult.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 33, 33));
                txtResult.Text = $"{homeKey} (Home) Win Probability: {homeChance:F1} %\n" +
                                 $"{awayKey} (Away) Win Probability: {awayChance:F1} %";
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Prediction error: {exception.Message}");
            }
        }
    }
}