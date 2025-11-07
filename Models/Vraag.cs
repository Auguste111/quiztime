using System;
using System.Collections.Generic;

namespace quiztime.Models
{
    public class Vraag
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public int? TypeId { get; set; }
        public string Tekst { get; set; }
        public TimeSpan? Tijd { get; set; }
        public DateTime CreatedAt { get; set; }
        
        //Foto pad (lokale pad of URL)
        public string FotoPath { get; set; }

        //Koppeling naar antwoorden (multiple choice opties)
        public List<Antwoord> Antwoorden { get; set; } = new List<Antwoord>();
    }
}
