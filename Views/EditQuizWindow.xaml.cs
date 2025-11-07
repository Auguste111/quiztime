using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using quiztime.Models;
using quiztime.Services;

namespace quiztime.Views
{
    public partial class EditQuizWindow : Window
    {
        private Quiz quiz;
        private List<Quiz> quizzen;
        private readonly QuizJsonService _jsonService;
        private Vraag selectedVraag;

        public EditQuizWindow(Quiz q, List<Quiz> allQuizzen)
        {
            InitializeComponent();
            quiz = q;
            quizzen = allQuizzen;
            _jsonService = QuizJsonService.Instance;

            NaamBox.Text = quiz.Naam;
            QuizTitelLabel.Text = $"- {quiz.Naam}";
            VragenLijst.ItemsSource = quiz.Vragen;
        }

        private void VragenLijst_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selectedVraag = VragenLijst.SelectedItem as Vraag;
            
            if (selectedVraag != null)
            {
                VraagTekstBox.Text = selectedVraag.Tekst;
                AntwoordenLijst.ItemsSource = null;
                AntwoordenLijst.ItemsSource = selectedVraag.Antwoorden;
                
                // Laad foto preview
                UpdateFotoPreview();
                
                // Update knop state - check of we al 4 antwoorden hebben
                UpdateAddAntwoordButtonState();
            }
            else
            {
                VraagTekstBox.Text = "";
                AntwoordenLijst.ItemsSource = null;
                VraagAfbeeldingPreview.Source = null;
                FotoPathLabel.Text = "Geen foto";
            }
        }

        private void UpdateFotoPreview()
        {
            if (selectedVraag != null && !string.IsNullOrEmpty(selectedVraag.FotoPath))
            {
                try
                {
                    var bitmap = new BitmapImage(new Uri(selectedVraag.FotoPath, UriKind.RelativeOrAbsolute));
                    VraagAfbeeldingPreview.Source = bitmap;
                    FotoPathLabel.Text = System.IO.Path.GetFileName(selectedVraag.FotoPath);
                }
                catch
                {
                    VraagAfbeeldingPreview.Source = null;
                    FotoPathLabel.Text = "Fout bij laden van foto";
                }
            }
            else
            {
                VraagAfbeeldingPreview.Source = null;
                FotoPathLabel.Text = "Geen foto";
            }
        }

        private void SelectFoto_Click(object sender, RoutedEventArgs e)
        {
            if (selectedVraag == null)
            {
                MessageBox.Show("Selecteer eerst een vraag!");
                return;
            }

            var openFileDialog = new OpenFileDialog
            {
                Title = "Selecteer een afbeelding",
                Filter = "Afbeeldingen (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|Alle bestanden (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                selectedVraag.FotoPath = openFileDialog.FileName;
                UpdateFotoPreview();
            }
        }

        private void RemoveFoto_Click(object sender, RoutedEventArgs e)
        {
            if (selectedVraag != null)
            {
                selectedVraag.FotoPath = null;
                UpdateFotoPreview();
            }
        }

        private void VraagTekstBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Update de vraag tekst in real-time terwijl je typt
            if (selectedVraag != null)
            {
                selectedVraag.Tekst = VraagTekstBox.Text;
                VragenLijst.Items.Refresh();
            }
        }

        private void NaamBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Update de quiz naam en header in real-time
            quiz.Naam = NaamBox.Text;
            QuizTitelLabel.Text = $"- {quiz.Naam}";
        }

        private void AddVraag_Click(object sender, RoutedEventArgs e)
        {
            var vraag = new Vraag
            {
                Tekst = "Nieuwe vraag",
                Antwoorden = new List<Antwoord>
                {
                    new Antwoord { Tekst = "Antwoord A", IsCorrect = false },
                    new Antwoord { Tekst = "Antwoord B", IsCorrect = true }
                },
                CreatedAt = DateTime.Now,
                Id = 0  
            };

            quiz.Vragen.Add(vraag);
            VragenLijst.Items.Refresh();
        }

        private void RemoveVraag_Click(object sender, RoutedEventArgs e)
        {
            if (VragenLijst.SelectedItem is Vraag v)
            {
                quiz.Vragen.Remove(v);
                VragenLijst.Items.Refresh();
                VraagTekstBox.Text = "";
                AntwoordenLijst.ItemsSource = null;
            }
        }

        private void UpdateAddAntwoordButtonState()
        {
            // Vind de "Toevoegen" knop
            var addButton = this.FindName("AddAntwoordBtn") as Button;
            if (addButton == null)
                return;

            if (selectedVraag == null)
            {
                addButton.IsEnabled = false;
                return;
            }

            addButton.IsEnabled = selectedVraag.Antwoorden.Count < 4;
            
            System.Diagnostics.Debug.WriteLine($"AddAntwoordBtn IsEnabled = {addButton.IsEnabled} (Antwoorden: {selectedVraag.Antwoorden.Count}/4)");
        }

        private void AddAntwoord_Click(object sender, RoutedEventArgs e)
        {
            if (selectedVraag == null)
            {
                MessageBox.Show("Selecteer eerst een vraag!");
                return;
            }

            if (string.IsNullOrWhiteSpace(NieuwAntwoordBox.Text))
            {
                MessageBox.Show("Vul een antwoord in!");
                return;
            }

            if (selectedVraag.Antwoorden.Count >= 4)
            {
                MessageBox.Show("Maximum 4 antwoorden per vraag!");
                return;
            }

            var antwoord = new Antwoord
            {
                Tekst = NieuwAntwoordBox.Text,
                IsCorrect = NieuwAntwoordCorrectCheckbox.IsChecked ?? false,
                CreatedAt = DateTime.Now,
                Id = 0  // Markeert als nieuw
            };

            selectedVraag.Antwoorden.Add(antwoord);
            AntwoordenLijst.Items.Refresh();
            NieuwAntwoordBox.Text = "";
            NieuwAntwoordCorrectCheckbox.IsChecked = false;
            NieuwAntwoordBox.Focus();
            
            // Update de knop status
            UpdateAddAntwoordButtonState();
            
            System.Diagnostics.Debug.WriteLine($"Antwoord toegevoegd: '{antwoord.Tekst}' (Correct: {antwoord.IsCorrect})");
        }

        private void NieuwAntwoordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                AddAntwoord_Click(null, null);
                e.Handled = true;
            }
        }

        private void RemoveAntwoord_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as FrameworkElement;
            var antwoordId = (int)button.Tag;

            var antwoord = AntwoordenLijst.SelectedItem as Antwoord;
            if (antwoord == null)
            {
                MessageBox.Show("Selecteer een antwoord om te verwijderen!");
                return;
            }

            selectedVraag.Antwoorden.Remove(antwoord);
            AntwoordenLijst.Items.Refresh();
            
            // Update knop state - mogelijk is de knop nu weer available
            UpdateAddAntwoordButtonState();
        }

        private void Answer_CorrectChanged(object sender, RoutedEventArgs e)
        {

            if (selectedVraag != null)
            {
                AntwoordenLijst.Items.Refresh();
            }
        }

        private void SaveAndClose_Click(object sender, RoutedEventArgs e)
        {
            // Update de vraag tekst als er een geselecteerd is
            if (selectedVraag != null)
            {
                selectedVraag.Tekst = VraagTekstBox.Text;
            }

            quiz.Naam = NaamBox.Text;
            
            try
            {
                // Sla alles op in JSON
                _jsonService.UpdateQuiz(quiz);
                MessageBox.Show("Quiz succesvol opgeslagen!");
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij opslaan: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
