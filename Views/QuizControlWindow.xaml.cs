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
            
            ToonVraag();
        }

        private void ToonVraag()
        {
            if (huidigeVraagIndex >= quiz.Vragen.Count)
            {
                // Quiz is afgelopen - toon eindscherm op display
                display.StopTimer();
                display.IsQuizEnded = true;
                display.IsWaiting = false;  // Zet wachtscherm uit, eindscherm aan
                this.Close();     // Sluit alleen QuizControlWindow, terug naar MainWindow
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
