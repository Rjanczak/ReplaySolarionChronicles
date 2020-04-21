using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Minigames;
using Newtonsoft.Json;

namespace ReplaySolarionChronicles
{
    public class BoardGameScenario
    {
        private string folderPath;

        private Dictionary<int, string> grades = new Dictionary<int, string>();
        public string Name { get; set; }

        public List<string> GradeNames { get; set; } = new List<string>();

        public List<int> RequiredScores { get; set; } = new List<int>();

        public List<BoardGameScene> Scenes { get; set; }

        public static BoardGameScenario LoadFromJSON(string fileName)
        {
            string fileContent = File.ReadAllText(fileName);
            BoardGameScenario createdScenario = JsonConvert.DeserializeObject<BoardGameScenario>(fileContent);
            createdScenario.folderPath = Path.GetDirectoryName(fileName);
            int GradeNamesLength = createdScenario.GradeNames.Count();
            int RequiredScoresLength = createdScenario.RequiredScores.Count();
            if (GradeNamesLength != RequiredScoresLength)
                throw new GradeListLengthsDoNotMatchException("Couldn't match lengths of GradeNames and RequiredScores lists in file " + createdScenario.folderPath + fileName);
            else
            {
                for(int i = 0; i < GradeNamesLength; i++)
                    createdScenario.grades.Add(createdScenario.RequiredScores[i], createdScenario.GradeNames[i]);
            }
            return createdScenario;
        }


        public string GetGrade(int score)
        {
            string outputGrade;
            grades.TryGetValue(score, out outputGrade);
            return outputGrade;
        }

        public string GetImageFolder()
        {
            return folderPath;
        }
    }

    public class GradeListLengthsDoNotMatchException : Exception
    {
        public GradeListLengthsDoNotMatchException(string message) : base(message) { }
    }
}
