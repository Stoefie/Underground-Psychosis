using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Underground_Psychosis.Properties;

namespace Underground_Psychosis.Managers
{
    public static class HighScoreManager
    {
        // Get current high score
        public static int GetHighScore()
        {
            return Settings.Default.HighScore;
        }

        // Try to set a new high score, returns true if it was higher
        public static bool TrySetHighScore(int newScore)
        {
            if (newScore > Settings.Default.HighScore)
            {
                Settings.Default.HighScore = newScore;
                Settings.Default.Save();
                return true;
            }
            return false;
        }
    }
}
