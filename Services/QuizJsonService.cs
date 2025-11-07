using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using quiztime.Models;

namespace quiztime.Services
{
    public class QuizJsonService
    {
        private readonly string _jsonPath;
        private List<Quiz> _quizzes;

        public QuizJsonService()
        {
            // Sla quizzes op in Data folder
            string dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            _jsonPath = Path.Combine(dataFolder, "quizzes.json");
            System.Diagnostics.Debug.WriteLine($"📁 JSON pad: {_jsonPath}");

            // Laad quizzes uit JSON
            LoadFromJson();
        }

        /// <summary>
        /// Laad alle quizzes uit JSON bestand
        /// </summary>
        private void LoadFromJson()
        {
            if (File.Exists(_jsonPath))
            {
                try
                {
                    string json = File.ReadAllText(_jsonPath);
                    _quizzes = JsonConvert.DeserializeObject<List<Quiz>>(json) ?? new List<Quiz>();
                    System.Diagnostics.Debug.WriteLine($"✅ {_quizzes.Count} quizzes geladen uit JSON");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Fout bij laden JSON: {ex.Message}");
                    _quizzes = new List<Quiz>();
                }
            }
            else
            {
                _quizzes = new List<Quiz>();
                System.Diagnostics.Debug.WriteLine($"⚠️ JSON bestand niet gevonden, nieuwe lijst aangemaakt");
            }
        }

        /// <summary>
        /// Sla alle quizzes op naar JSON bestand
        /// </summary>
        private void SaveToJson()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_quizzes, Formatting.Indented);
                File.WriteAllText(_jsonPath, json);
                System.Diagnostics.Debug.WriteLine($"✅ {_quizzes.Count} quizzes opgeslagen naar JSON");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Fout bij opslaan JSON: {ex.Message}");
            }
        }

        /// <summary>
        /// Haal alle quizzes op
        /// </summary>
        public List<Quiz> GetAllQuizzes()
        {
            return _quizzes.ToList();
        }

        /// <summary>
        /// Haal een quiz op via ID
        /// </summary>
        public Quiz GetQuizById(int id)
        {
            return _quizzes.FirstOrDefault(q => q.Id == id);
        }

        /// <summary>
        /// Voeg een quiz toe of update deze
        /// </summary>
        public void UpdateQuiz(Quiz quiz)
        {
            if (quiz.Id == 0)
            {
                // Nieuwe quiz: genereer ID
                quiz.Id = _quizzes.Any() ? _quizzes.Max(q => q.Id) + 1 : 1;
                _quizzes.Add(quiz);
                System.Diagnostics.Debug.WriteLine($"✅ Nieuwe quiz '{quiz.Naam}' toegevoegd met ID {quiz.Id}");
            }
            else
            {
                // Update bestaande quiz
                var existing = _quizzes.FirstOrDefault(q => q.Id == quiz.Id);
                if (existing != null)
                {
                    existing.Naam = quiz.Naam;
                    existing.Description = quiz.Description;
                    existing.Vragen = quiz.Vragen;
                    System.Diagnostics.Debug.WriteLine($"✅ Quiz '{quiz.Naam}' (ID {quiz.Id}) bijgewerkt");
                }
            }

            SaveToJson();
        }

        /// <summary>
        /// Verwijder een quiz
        /// </summary>
        public void DeleteQuiz(int id)
        {
            var quiz = _quizzes.FirstOrDefault(q => q.Id == id);
            if (quiz != null)
            {
                _quizzes.Remove(quiz);
                SaveToJson();
                System.Diagnostics.Debug.WriteLine($"✅ Quiz '{quiz.Naam}' (ID {id}) verwijderd");
            }
        }
    }
}
