using System.Text.Json.Serialization;

namespace NBAMatchPredictor
{
    /// <summary>
    /// Represents the statistical data of an NBA team over a 5-game span, 
    /// including performance metrics, fatigue factors, and roster availability.
    /// </summary>
    public class TeamStats
    {
        /// <summary>Average points scored per game over the last 5 games.</summary>
        public float Pts5G { get; set; }
        
        /// <summary>Field goal percentage over the last 5 games.</summary>
        public float FgPct { get; set; }
        
        /// <summary>Three-point field goal percentage over the last 5 games.</summary>
        public float Fg3Pct { get; set; }
        
        /// <summary>Average rebounds per game over the last 5 games.</summary>
        public float Reb { get; set; }
        
        /// <summary>Average assists per game over the last 5 games.</summary>
        public float Ast { get; set; }
        
        /// <summary>Average turnovers per game over the last 5 games.</summary>
        public float Tov { get; set; }
        
        /// <summary>Average plus-minus rating over the last 5 games.</summary>
        public float PlusMinus { get; set; }
        
        /// <summary>Number of rest days prior to the match.</summary>
        public float DaysRest { get; set; }
        
        /// <summary>Indicates if a star player is missing (1) or playing (0).</summary>
        public int StarMissing { get; set; }
    }
}