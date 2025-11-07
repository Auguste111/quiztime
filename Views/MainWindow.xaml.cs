using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using quiztime.Models;
using quiztime.Services;

namespace quiztime.Views
{
    public partial class MainWindow : Window
    {
        private List<Quiz> quizzen;
        private readonly QuizJsonService _jsonService;
        public static DisplayWindow DisplayWindow { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                _jsonService = QuizJsonService.Instance;
                
                // Start het DisplayWindow bij app-start
                if (DisplayWindow == null)
                {
                    DisplayWindow = new DisplayWindow();
                    DisplayWindow.InitializeDisplayOnAllScreens();
                    DisplayWindow.Show();
                    
                    // Na Show(), even wachten en dan maximaliseer het venster
                    var screens = Screen.AllScreens;
                    if (screens.Length > 1)
                    {
                        // Gebruik Dispatcher om dit uit te stellen tot na rendering
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            DisplayWindow.WindowState = System.Windows.WindowState.Maximized;
                            System.Diagnostics.Debug.WriteLine($"DisplayWindow gemaximaliseerd bij startup op scherm: {screens[1].DeviceName}");
                        }), System.Windows.Threading.DispatcherPriority.Background);
                    }
                }
                
                LoadQuizzes();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Fout bij het laden van de database: {ex.Message}", "Fout");
            }
        }

        private void LoadQuizzes()
        {
            try
            {
                quizzen = _jsonService.GetAllQuizzes();
                QuizList.ItemsSource = quizzen;
                System.Diagnostics.Debug.WriteLine($"{quizzen.Count} quizzen geladen");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Fout bij het laden van quizzen: {ex.Message}\n\n{ex.StackTrace}", "Fout");
                System.Diagnostics.Debug.WriteLine($"Fout: {ex}");
            }
        }

        private Quiz SelectedQuiz => QuizList.SelectedItem as Quiz;

        private void NewQuiz_Click(object sender, RoutedEventArgs e)
        {
            var newQuiz = new Quiz
            {
                Id = 0,
                Naam = "Nieuwe Quiz",
                Vragen = new List<Vraag>()
            };

            var editWindow = new EditQuizWindow(newQuiz, quizzen);
            editWindow.ShowDialog();
            
            // Vernieuw de lijst na sluiten van edit window
            LoadQuizzes();
        }

        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedQuiz == null)
            {
                System.Windows.MessageBox.Show("Selecteer eerst een quiz uit de lijst.");
                return;
            }

            var qc = new QuizControlWindow(SelectedQuiz);
            qc.Show();
        }


        private void EditQuiz_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedQuiz == null)
            {
                System.Windows.MessageBox.Show("Selecteer eerst een quiz om te bewerken.");
                return;
            }

            var editWindow = new EditQuizWindow(SelectedQuiz, quizzen);
            editWindow.ShowDialog();
            
            // Vernieuw de lijst na sluiten van edit window
            QuizList.Items.Refresh();
        }

        private void DeleteQuiz_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedQuiz == null)
            {
                System.Windows.MessageBox.Show("Selecteer eerst een quiz om te verwijderen.");
                return;
            }

            var confirm = System.Windows.MessageBox.Show(
                $"Weet je zeker dat je '{SelectedQuiz.Naam}' wilt verwijderen?",
                "Bevestig verwijderen",
                System.Windows.MessageBoxButton.YesNo);

            if (confirm == System.Windows.MessageBoxResult.Yes)
            {
                _jsonService.DeleteQuiz(SelectedQuiz.Id);
                quizzen.Remove(SelectedQuiz);
                QuizList.Items.Refresh();
            }
        }
    }
}
