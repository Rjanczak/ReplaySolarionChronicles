using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ReplaySolarionChronicles
{
    public class BoardGameScenario
    {
        private string folderPath;
        public string Name { get; set; }
        
        public List<BoardGameScene> Scenes { get; set; }

        public static BoardGameScenario LoadFromJSON(string fileName)
        {
            string fileContent = File.ReadAllText(fileName);
            BoardGameScenario createdScenario = JsonConvert.DeserializeObject<BoardGameScenario>(fileContent);
            createdScenario.folderPath = Path.GetDirectoryName(fileName);
            return createdScenario;
        }

        public string GetImageFolder()
        {
            return folderPath;
        }
    }

    public class BoardGameScene
    {
        int currentDialogue;
        public string Image { get; set; }
        public string Dialogue { get; set; }
    }
}
