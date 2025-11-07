using System;
using System.Collections.Generic;

namespace quiztime.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public string Naam { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }

        // Elke quiz heeft meerdere vragen
        public List<Vraag> Vragen { get; set; } = new List<Vraag>();

        public string AantalVragenText => $"Vragen: {Vragen.Count}";
    }
}
