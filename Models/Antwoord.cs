using System;

namespace quiztime.Models
{
    public class Antwoord
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string Tekst { get; set; }
        public bool IsCorrect { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
