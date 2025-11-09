using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace quiztime.Models
{
    public class Antwoord : INotifyPropertyChanged
    {
        private int _id;
        private int _questionId;
        private string _tekst;
        private bool _isCorrect;
        private DateTime _createdAt;

        public int Id { get => _id; set => SetField(ref _id, value); }
        public int QuestionId { get => _questionId; set => SetField(ref _questionId, value); }
        public string Tekst { get => _tekst; set => SetField(ref _tekst, value); }
        public bool IsCorrect { get => _isCorrect; set => SetField(ref _isCorrect, value); }
        public DateTime CreatedAt { get => _createdAt; set => SetField(ref _createdAt, value); }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
