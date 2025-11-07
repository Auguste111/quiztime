using System.Linq;
using System.Windows;
using System.Windows.Forms; 
using quiztime.Models;

namespace quiztime.Views
{
    public partial class QuizControlWindow : Window
    {
        private DisplayWindow display;
        private Quiz quiz;
        private int huidigeVraagIndex = 0;
        private bool isCorrectAnswerShown = false;

        public QuizControlWindow(Quiz q)
        {
            InitializeComponent();
            quiz = q;
            System.Diagnostics.Debug.WriteLine($"QuizControlWindow geopend: {quiz.Naam}");
            System.Diagnostics.Debug.WriteLine($"Aantal vragen in quiz: {quiz.Vragen.Count}");

            // Gebruik het bestaande DisplayWindow van MainWindow
            display = MainWindow.DisplayWindow;
            
            System.Diagnostics.Debug.WriteLine($"✅ DisplayWindow gebruikt van MainWindow");
            
            // Zorg dat alle knoppen ingeschakeld zijn aan het begin
            VolgendeBtn.IsEnabled = true;
            VorigeBtn.IsEnabled = false;  // Vorige is disabled totdat we voorbij vraag 1 zijn
            ToonAntwoordBtn.IsEnabled = true;
            
            ToonVraag();
        }

        private void ToonVraag()
        {
            if (huidigeVraagIndex >= quiz.Vragen.Count)
            {
                // Quiz is afgelopen - toon eindscherm op display
                // Maar sluit de window NIET - admin kan nog Terug klikken
                ToonEindscherm();
                return;
            }

            var vraag = quiz.Vragen[huidigeVraagIndex];
            VraagPreview.Text = $"Vraag {huidigeVraagIndex + 1}: {vraag.Tekst}";
            
            // Laad vraag met foto op display (dit reset ook het antwoord)
            display.SetQuestion(vraag);
            
            // Reset antwoord knop 
            isCorrectAnswerShown = false;
            UpdateToonAntwoordButton();
            
            display.StartTimer(30);
        }

        private void ToonEindscherm()
        {
            // Toon eindscherm op display (vragen gaan weg!)
            display.StopTimer();
            display.IsQuizEnded = true;
            display.IsWaiting = false;
            
            // Zet de admin panel naar eindstatus
            VraagPreview.Text = "Quiz afgelopen!";
            
            // Disable alle knoppen behalve "Terug naar Hoofdscherm"
            VorigeBtn.IsEnabled = true;  // Terug naar vorige vragen mag nog
            VolgendeBtn.IsEnabled = false;  // Volgende mag niet meer
            ToonAntwoordBtn.IsEnabled = false;  // Antwoord mag niet meer
        }

        private void Volgende_Click(object sender, RoutedEventArgs e)
        {
            display.StopTimer();
            huidigeVraagIndex++;
            ToonVraag();
        }

        private void Vorige_Click(object sender, RoutedEventArgs e)
        {
            display.StopTimer();
            if (huidigeVraagIndex > 0)
                huidigeVraagIndex--;
            
            // Als we teruggaan naar vorige vragen vanuit eindscherm
            // Dan moeten alle knoppen weer ingeschakeld
            if (VolgendeBtn.IsEnabled == false)
            {
                VolgendeBtn.IsEnabled = true;
                ToonAntwoordBtn.IsEnabled = true;
            }
            
            ToonVraag();
        }

        private void ToonAntwoord_Click(object sender, RoutedEventArgs e)
        {
            var vraag = quiz.Vragen[huidigeVraagIndex];
            
            if (isCorrectAnswerShown)
            {
                // Verberg het antwoord door terug te gaan naar normale view
                display.HideCorrectAnswer();  // ← Dit was vergeten!
                isCorrectAnswerShown = false;
            }
            else
            {
                // Toon het antwoord (nu groen gekleurd in de antwoorden grid)
                var correct = vraag.Antwoorden.Find(a => a.IsCorrect);
                if (correct != null)
                {
                    display.ShowCorrectAnswer(correct.Tekst);
                    isCorrectAnswerShown = true;
                }
                else
                {
                    isCorrectAnswerShown = true;
                }
            }
            
            UpdateToonAntwoordButton();
        }

        private void UpdateToonAntwoordButton()
        {
            if (isCorrectAnswerShown)
            {
                ToonAntwoordBtn.Content = "✗ Verberg Antwoord";
                ToonAntwoordBtn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(244, 67, 54)); // Rood
            }
            else
            {
                ToonAntwoordBtn.Content = "✓ Toon Antwoord";
                ToonAntwoordBtn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80)); // Groen
            }
        }

        private void TerugNaarHoofdscherm_Click(object sender, RoutedEventArgs e)
        {
            display.StopTimer();
            display.IsQuizEnded = false;  // Reset eindscherm
            display.IsWaiting = true;     // Zet wachtscherm weer aan
            this.Close();     // Sluit QuizControlWindow, DisplayWindow blijft open
        }
    }
}
