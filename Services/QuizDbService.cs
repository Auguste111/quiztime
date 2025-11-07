using System;
using System.Collections.Generic;
using System.Configuration;
using MySql.Data.MySqlClient;
using quiztime.Models;

namespace quiztime.Services
{
    public class QuizDbService
    {
        private readonly string _connectionString;

        public QuizDbService()
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings["QuizDb"];
            if (connectionStringSettings == null)
            {
                throw new ConfigurationErrorsException("De connection string 'QuizDb' is niet gevonden in App.config");
            }
            _connectionString = connectionStringSettings.ConnectionString;
        }

        public List<Quiz> GetAllQuizzes()
        {
            var result = new List<Quiz>();

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT id, naam, description, created_at FROM quiz", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var quiz = new Quiz
                        {
                            Id = reader.GetInt32("id"),
                            Naam = reader.GetString("naam"),
                            Description = reader.GetString("description"),
                            CreatedAt = reader.GetDateTime("created_at"),
                            Vragen = new List<Vraag>()
                        };
                        result.Add(quiz);
                    }
                }
            }

            // Vragen (en antwoorden) per quiz invullen
            for (int i = 0; i < result.Count; i++)
            {
                result[i].Vragen = GetQuestionsForQuiz(result[i].Id);
                System.Diagnostics.Debug.WriteLine($"Quiz '{result[i].Naam}' (ID: {result[i].Id}) → {result[i].Vragen.Count} vragen geladen");
            }

            return result;
        }

        public List<Vraag> GetQuestionsForQuiz(int quizId)
        {
            var result = new List<Vraag>();

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT id, quiz_id, type_id, question, tijd, foto_path, created_at FROM questions WHERE quiz_id=@quizId",
                    conn);
                cmd.Parameters.AddWithValue("@quizId", quizId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var vraag = new Vraag
                        {
                            Id = reader.GetInt32("id"),
                            QuizId = reader.GetInt32("quiz_id"),
                            TypeId = reader.IsDBNull(reader.GetOrdinal("type_id"))
                                ? (int?)null
                                : reader.GetInt32("type_id"),
                            Tekst = reader.GetString("question"),
                            Tijd = reader.IsDBNull(reader.GetOrdinal("tijd"))
                                ? (TimeSpan?)null
                                : reader.GetTimeSpan("tijd"),
                            FotoPath = reader.IsDBNull(reader.GetOrdinal("foto_path"))
                                ? null
                                : reader.GetString("foto_path"),
                            CreatedAt = reader.GetDateTime("created_at"),
                            Antwoorden = new List<Antwoord>()
                        };
                        result.Add(vraag);
                    }
                }
            }

            // Antwoorden per vraag invullen
            for (int i = 0; i < result.Count; i++)
            {
                result[i].Antwoorden = GetAnswersForQuestion(result[i].Id);
                System.Diagnostics.Debug.WriteLine($"Vraag {result[i].Id} → {result[i].Antwoorden.Count} antwoorden geladen");

            }

            return result;
        }

        public List<Antwoord> GetAnswersForQuestion(int questionId)
        {
            var result = new List<Antwoord>();

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT id, question_id, tekst, is_correct, created_at FROM answers WHERE question_id=@qid",
                    conn);
                cmd.Parameters.AddWithValue("@qid", questionId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var a = new Antwoord
                        {
                            Id = reader.GetInt32("id"),
                            QuestionId = reader.GetInt32("question_id"),
                            Tekst = reader.GetString("tekst"),
                            IsCorrect = reader.GetBoolean("is_correct"),
                            CreatedAt = reader.GetDateTime("created_at")
                        };
                        result.Add(a);
                    }
                }
            }

            return result;
        }
    public void DeleteQuiz(int quizId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                // Eerst gekoppelde vragen verwijderen (als er geen ON DELETE CASCADE is)
                var cmdVragen = new MySqlCommand(
                    "DELETE FROM questions WHERE quiz_id=@quizId", conn);
                cmdVragen.Parameters.AddWithValue("@quizId", quizId);
                cmdVragen.ExecuteNonQuery();

                // Daarna gekoppelde antwoorden verwijderen
                var cmdAntwoorden = new MySqlCommand(
                    "DELETE FROM answers WHERE question_id IN (SELECT id FROM questions WHERE quiz_id=@quizId)", conn);
                cmdAntwoorden.Parameters.AddWithValue("@quizId", quizId);
                cmdAntwoorden.ExecuteNonQuery();

                // En als laatste: de quiz zelf
                var cmdQuiz = new MySqlCommand(
                    "DELETE FROM quiz WHERE id=@quizId", conn);
                cmdQuiz.Parameters.AddWithValue("@quizId", quizId);
                cmdQuiz.ExecuteNonQuery();
            }
        }

        // ========== VRAGEN BEWERKEN ==========

        public void UpdateQuestion(Vraag vraag)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "UPDATE questions SET question=@tekst, type_id=@typeId, tijd=@tijd, foto_path=@fotoPath WHERE id=@id",
                    conn);
                cmd.Parameters.AddWithValue("@id", vraag.Id);
                cmd.Parameters.AddWithValue("@tekst", vraag.Tekst);
                cmd.Parameters.AddWithValue("@typeId", vraag.TypeId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@tijd", vraag.Tijd ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@fotoPath", vraag.FotoPath ?? (object)DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        public int CreateQuestion(Vraag vraag)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "INSERT INTO questions (quiz_id, question, type_id, tijd, foto_path, created_at) " +
                    "VALUES (@quizId, @tekst, @typeId, @tijd, @fotoPath, NOW()); SELECT LAST_INSERT_ID();",
                    conn);
                cmd.Parameters.AddWithValue("@quizId", vraag.QuizId);
                cmd.Parameters.AddWithValue("@tekst", vraag.Tekst);
                cmd.Parameters.AddWithValue("@typeId", vraag.TypeId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@tijd", vraag.Tijd ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@fotoPath", vraag.FotoPath ?? (object)DBNull.Value);
                
                var newId = Convert.ToInt32(cmd.ExecuteScalar());
                return newId;
            }
        }

        public void DeleteQuestion(int questionId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                
                // Eerst antwoorden verwijderen
                var cmdAnswers = new MySqlCommand("DELETE FROM answers WHERE question_id=@id", conn);
                cmdAnswers.Parameters.AddWithValue("@id", questionId);
                cmdAnswers.ExecuteNonQuery();

                // Dan de vraag zelf
                var cmdQuestion = new MySqlCommand("DELETE FROM questions WHERE id=@id", conn);
                cmdQuestion.Parameters.AddWithValue("@id", questionId);
                cmdQuestion.ExecuteNonQuery();
            }
        }

        // ========== ANTWOORDEN BEWERKEN ==========

        public void UpdateAnswer(Antwoord antwoord)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "UPDATE answers SET tekst=@tekst, is_correct=@isCorrect WHERE id=@id",
                    conn);
                cmd.Parameters.AddWithValue("@id", antwoord.Id);
                cmd.Parameters.AddWithValue("@tekst", antwoord.Tekst);
                cmd.Parameters.AddWithValue("@isCorrect", antwoord.IsCorrect);
                cmd.ExecuteNonQuery();
            }
        }

        public int CreateAnswer(Antwoord antwoord)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "INSERT INTO answers (question_id, tekst, is_correct, created_at) " +
                    "VALUES (@questionId, @tekst, @isCorrect, NOW()); SELECT LAST_INSERT_ID();",
                    conn);
                cmd.Parameters.AddWithValue("@questionId", antwoord.QuestionId);
                cmd.Parameters.AddWithValue("@tekst", antwoord.Tekst);
                cmd.Parameters.AddWithValue("@isCorrect", antwoord.IsCorrect);
                
                var newId = Convert.ToInt32(cmd.ExecuteScalar());
                return newId;
            }
        }

        public void DeleteAnswer(int answerId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("DELETE FROM answers WHERE id=@id", conn);
                cmd.Parameters.AddWithValue("@id", answerId);
                cmd.ExecuteNonQuery();
            }
        }

        // ========== QUIZ BEWERKEN ==========

        public void UpdateQuiz(Quiz quiz)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                
                // Controleer of het een nieuwe quiz is (Id == 0)
                if (quiz.Id == 0)
                {
                    // INSERT nieuwe quiz
                    var cmd = new MySqlCommand(
                        "INSERT INTO quiz (naam, description, created_at) VALUES (@naam, @description, NOW()); SELECT LAST_INSERT_ID();",
                        conn);
                    cmd.Parameters.AddWithValue("@naam", quiz.Naam);
                    cmd.Parameters.AddWithValue("@description", quiz.Description ?? string.Empty);
                    var result = cmd.ExecuteScalar();
                    quiz.Id = Convert.ToInt32(result);
                }
                else
                {
                    // UPDATE bestaande quiz
                    var cmd = new MySqlCommand(
                        "UPDATE quiz SET naam=@naam, description=@description WHERE id=@id",
                        conn);
                    cmd.Parameters.AddWithValue("@id", quiz.Id);
                    cmd.Parameters.AddWithValue("@naam", quiz.Naam);
                    cmd.Parameters.AddWithValue("@description", quiz.Description ?? string.Empty);
                    cmd.ExecuteNonQuery();
                }

                // Vragen en antwoorden apart opslaan
                foreach (var vraag in quiz.Vragen)
                {
                    if (vraag.Id == 0)
                    {
                        // Nieuwe vraag
                        vraag.QuizId = quiz.Id;
                        var newVraagId = CreateQuestion(vraag);
                        vraag.Id = newVraagId;
                    }
                    else
                    {
                        // Bestaande vraag updaten
                        UpdateQuestion(vraag);
                    }

                    // Antwoorden opslaan
                    foreach (var antwoord in vraag.Antwoorden)
                    {
                        if (antwoord.Id == 0)
                        {
                            // Nieuw antwoord
                            antwoord.QuestionId = vraag.Id;
                            var newAntwoordId = CreateAnswer(antwoord);
                            antwoord.Id = newAntwoordId;
                        }
                        else
                        {
                            // Bestaand antwoord updaten
                            UpdateAnswer(antwoord);
                        }
                    }
                }
            }
        }
    }
}
