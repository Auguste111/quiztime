using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using quiztime.Models;

namespace quiztime.Services
{
    public static class QuizService
    {
        private static string FilePath = "Data/quizzen.json";

        public static List<Quiz> LoadQuizzen()
        {
            if (!File.Exists(FilePath))
                return new List<Quiz>();

            var json = File.ReadAllText(FilePath);
            return JsonConvert.DeserializeObject<List<Quiz>>(json);
        }

        public static void SaveQuizzen(List<Quiz> quizzen)
        {
            var json = JsonConvert.SerializeObject(quizzen, Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }
    }
}
