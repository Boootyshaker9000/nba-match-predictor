using System;
using System.Windows;

namespace NBAMatchPredictor
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            Application app = new Application();
            
            MainWindow mainWindow = new MainWindow();
            
            app.Run(mainWindow);
        }
    }
}