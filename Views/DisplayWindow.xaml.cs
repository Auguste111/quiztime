using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using quiztime.Models;

namespace quiztime.Views
{
    /// <summary>
    /// Converter om juiste antwoorden groen te kleuren (alleen als ShowCorrectAnswer = true)
    /// </summary>
    public class CorrectAnswerColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length < 2)
                return new SolidColorBrush(Color.FromRgb(22, 33, 62)); // Donkerblauw
            
            bool isCorrect = (bool)values[0];
            bool showAnswer = (bool)values[1];
            
            if (isCorrect && showAnswer)
            {
                return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Groen (#4CAF50)
            }
            return new SolidColorBrush(Color.FromRgb(22, 33, 62)); // Originele donkerblauwe kleur (#16213E)
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class DisplayWindow : Window
    {
        private DispatcherTimer timer;
        private int tijdOver = 30;
        private bool showCorrectAnswer = false;
        private bool isWaiting = true;

        public bool IsShowingCorrectAnswer
        {
            get { return showCorrectAnswer; }
            set { showCorrectAnswer = value; }
        }

        public bool IsWaiting
        {
            get { return isWaiting; }
            set { isWaiting = value; }
        }

        public DisplayWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            this.IsWaiting = true;
        }

        /// <summary>
        /// Positioneer het DisplayWindow op het juiste scherm
        /// </summary>
        public void InitializeDisplayOnAllScreens()
        {
            var screens = System.Windows.Forms.Screen.AllScreens;
            System.Diagnostics.Debug.WriteLine($"🖥️ Aantal schermen: {screens.Length}");

            if (screens.Length > 1)
            {
                // Pak het NIET-primaire scherm (tweede scherm)
                var secondScreen = screens.FirstOrDefault(s => !s.Primary);
                if (secondScreen == null)
                    secondScreen = screens[1];

                var workingArea = secondScreen.WorkingArea;
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = workingArea.Left;
                this.Top = workingArea.Top;
                this.Width = workingArea.Width;
                this.Height = workingArea.Height;

                System.Diagnostics.Debug.WriteLine($"✅ DisplayWindow op tweede scherm: {secondScreen.DeviceName} ({workingArea.Left}, {workingArea.Top}, {workingArea.Width}x{workingArea.Height})");
            }
            else
            {
                // Als er maar 1 scherm is: toon als apart venster
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                var primaryScreen = screens[0].WorkingArea;

                this.Left = primaryScreen.Left + 600;
                this.Top = primaryScreen.Top;
                this.Width = primaryScreen.Width - 600;
                this.Height = primaryScreen.Height;

                System.Diagnostics.Debug.WriteLine($"⚠️ Slechts 1 scherm, DisplayWindow rechts gepositioneerd");
            }
        }

        /// <summary>
        /// Zet een vraag op het display (met foto als beschikbaar)
        /// </summary>
        public void SetQuestion(Vraag vraag)
        {
            // Reset de correct answer flag voor een nieuwe vraag
            this.IsShowingCorrectAnswer = false;
            this.IsWaiting = false;
            
            VraagText.Text = vraag.Tekst;
            AntwoordLijst.ItemsSource = vraag.Antwoorden;

            // Laad foto als die beschikbaar is
            if (!string.IsNullOrEmpty(vraag.FotoPath))
            {
                try
                {
                    var bitmap = new BitmapImage(new Uri(vraag.FotoPath, UriKind.RelativeOrAbsolute));
                    VraagAfbeelding.Source = bitmap;
                    VraagAfbeelding.Visibility = Visibility.Visible;
                }
                catch
                {
                    VraagAfbeelding.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                VraagAfbeelding.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Start een timer voor de quizvraag
        /// </summary>
        public void StartTimer(int seconden = 30)
        {
            tijdOver = seconden;
            TimerLabel.Text = $"⏳ {tijdOver}s";

            // Stop een eventuele oude timer
            if (timer != null)
                timer.Stop();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                tijdOver--;
                TimerLabel.Text = $"⏳ {tijdOver}s";

                if (tijdOver <= 0)
                {
                    timer.Stop();
                    TimerLabel.Text = "⏰ Tijd is op!";
                }
            };
            timer.Start();
        }

        /// <summary>
        /// Stop handmatig de timer
        /// </summary>
        public void StopTimer()
        {
            if (timer != null)
                timer.Stop();
            TimerLabel.Text = "";
        }

        /// <summary>
        /// Toon het juiste antwoord (nu groen gekleurd in de grid)
        /// </summary>
        public void ShowCorrectAnswer(string correctText)
        {
            StopTimer();
            this.IsShowingCorrectAnswer = true;
            // Force UI update door ItemsSource opnieuw in te stellen
            var currentItems = AntwoordLijst.ItemsSource;
            AntwoordLijst.ItemsSource = null;
            AntwoordLijst.ItemsSource = currentItems;
        }

        /// <summary>
        /// Verberg het juiste antwoord display
        /// </summary>
        public void HideCorrectAnswer()
        {
            this.IsShowingCorrectAnswer = false;
            // Force UI update door ItemsSource opnieuw in te stellen
            var currentItems = AntwoordLijst.ItemsSource;
            AntwoordLijst.ItemsSource = null;
            AntwoordLijst.ItemsSource = currentItems;
        }

        /// <summary>
        /// Internal: deze methode is nu niet meer nodig
        /// </summary>
        private void HideCorrectAnswer_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Niet meer nodig
        }
    }
}

