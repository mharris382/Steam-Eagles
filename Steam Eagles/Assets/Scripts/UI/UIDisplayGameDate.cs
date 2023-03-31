using CoreLib.GameTime;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using TMPro;

namespace UI
{
    [System.Serializable]
    public class UIDisplayGameDate
    {
        [Required]
        public TextMeshProUGUI text;


        [InfoBox("@ExampleDate")]
        public string formatSpecifier = "Week {0}, Day {1}";


        [UsedImplicitly]
        string ExampleDate
        {
            get
            {
                var date = new GameDate(3, 12);
                return string.Format(formatSpecifier, date.gameWeeks, date.gameDays);
            }
        }


        public void Display(GameDate gameDate)
        {
            text.text = GetDisplayString(gameDate);
        }
        
        private string GetDisplayString(GameDate gameDate)
        {
            return string.Format(formatSpecifier, gameDate.gameWeeks, gameDate.gameDays);
        }
    }
}