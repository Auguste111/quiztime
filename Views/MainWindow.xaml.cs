using System;
using System.Collections.Generic;
using System.Windows;
using quiztime.Models;
using quiztime.Services;

namespace quiztime.Views
{
    public partial class MainWindow : Window
    {
        private List<Quiz> quizzen;
        private readonly QuizDbService _dbService;
        public static DisplayWindow DisplayWindow { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                _dbService = new QuizDbService();
                
                // Start het DisplayWindow bij app-start
                if (DisplayWindow == null)
                {
                    DisplayWindow = new DisplayWindow();
                    DisplayWindow.InitializeDisplayOnAllScreens();
                    DisplayWindow.Show();
                }
                
                LoadQuizzes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij het laden van de database: {ex.Message}", "Fout");
            }
        }

        private void LoadQuizzes()
        {
            try
            {
                quizzen = _dbService.GetAllQuizzes();
                QuizList.ItemsSource = quizzen;
                System.Diagnostics.Debug.WriteLine($"✅ {quizzen.Count} quizzen geladen");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij het laden van quizzen: {ex.Message}\n\n{ex.StackTrace}", "Database Fout");
                System.Diagnostics.Debug.WriteLine($"❌ Fout: {ex}");
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
                MessageBox.Show("Selecteer eerst een quiz uit de lijst.");
                return;
            }

            var qc = new QuizControlWindow(SelectedQuiz);
            qc.Show();
        }


        private void EditQuiz_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedQuiz == null)
            {
                MessageBox.Show("Selecteer eerst een quiz om te bewerken.");
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
                MessageBox.Show("Selecteer eerst een quiz om te verwijderen.");
                return;
            }

            var confirm = MessageBox.Show(
                $"Weet je zeker dat je '{SelectedQuiz.Naam}' wilt verwijderen?",
                "Bevestig verwijderen",
                MessageBoxButton.YesNo);

            if (confirm == MessageBoxResult.Yes)
            {
                _dbService.DeleteQuiz(SelectedQuiz.Id);
                quizzen.Remove(SelectedQuiz);
                QuizList.Items.Refresh();
            }
        }
    }
}
