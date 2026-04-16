using System;
using System.Windows;

namespace NBAMatchPredictor
{
    /// <summary>
    /// The application's entry point class.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point for the application. 
        /// Initializes the WPF environment and runs the main window.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application app = new Application();
            MainWindow mainWindow = new MainWindow();
            app.Run(mainWindow);
        }
    }
}