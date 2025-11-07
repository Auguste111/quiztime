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

            // Publieksscherm openen
            display = new DisplayWindow();

            // Probeer tweede scherm te vinden
            var screens = Screen.AllScreens;
            System.Diagnostics.Debug.WriteLine($"🖥️ Aantal schermen gevonden: {screens.Length}");
            
            foreach (var i in Enumerable.Range(0, screens.Length))
            {
                System.Diagnostics.Debug.WriteLine($"   Scherm {i}: {screens[i].DeviceName} (Primair: {screens[i].Primary})");
            }

            if (screens.Length > 1)
            {
                // Pak het NIET-primaire scherm (tweede scherm)
                var secondScreen = screens.FirstOrDefault(s => !s.Primary);
                if (secondScreen == null)
                    secondScreen = screens[1];
                
                var workingArea = secondScreen.WorkingArea;
                display.WindowStartupLocation = WindowStartupLocation.Manual;
                display.Left = workingArea.Left;
                display.Top = workingArea.Top;
                display.Width = workingArea.Width;
                display.Height = workingArea.Height;
                // Niet maximized, zodat je kan slepen
                
                System.Diagnostics.Debug.WriteLine($"✅ DisplayWindow op tweede scherm: {secondScreen.DeviceName}");
            }
            else
            {
                // Als er maar 1 scherm is: toon als apart venster
                display.WindowStartupLocation = WindowStartupLocation.Manual;
                var primaryScreen = screens[0].WorkingArea;
                
                display.Left = primaryScreen.Left + 600;
                display.Top = primaryScreen.Top;
                display.Width = primaryScreen.Width - 600;
                display.Height = primaryScreen.Height;
                
                System.Diagnostics.Debug.WriteLine($"⚠️ Slechts 1 scherm, DisplayWindow rechts gepositioneerd");
            }

            display.Show();
            
            // Na Show(), maximaliseer het venster op het scherm waar het staat
            if (screens.Length > 1)
            {
                display.WindowState = System.Windows.WindowState.Maximized;
                System.Diagnostics.Debug.WriteLine($"📺 DisplayWindow gemaximaliseerd op tweede scherm");
            }
            
            ToonVraag();
        }

        private void ToonVraag()
        {
            if (huidigeVraagIndex >= quiz.Vragen.Count)
            {
                // Quiz is afgelopen - sluit stiekem zonder melding
                display.StopTimer();
                display.Close();  // Sluit het DisplayWindow
                this.Close();     // Sluit QuizControlWindow en terug naar MainWindow
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
            display.Close();  // Sluit het DisplayWindow
            this.Close();     // Sluit QuizControlWindow en terug naar MainWindow
        }
    }
}
