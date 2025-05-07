using CaTALK.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using CaTALK.MVVM.Views;

namespace CaTALK.MVVM.ViewModels
{
    public class UserManagement : INotifyPropertyChanged
    {
        #region RESTApi Components
        HttpClient client;
        JsonSerializerOptions _serializerOptions;
        string baseUrl = "https://6809e3e61f1a52874cde3be6.mockapi.io/";
        #endregion

        #region Fields
        private bool _isPassword = true;
        private bool _isConfirmPassword = true;
        private string _passwordImage = "eye_closed.png";
        private string _confirmPasswordImage = "eye_closed.png";
        private string _username;
        private string _password;
        private string _confirmPassword;
        #endregion

        public UserManagement()
        {
            client = new HttpClient();
            _serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true, //JSON payload configuration
            };

        }

        #region OnPropertyChanged
        public bool IsPassword
        {
            get => _isPassword;
            set
            {
                _isPassword = value;
                OnPropertyChanged();
                PasswordImage = _isPassword ? "eye_closed.png" : "eye.png";
            }
        }

        public bool IsConfirmPassword
        {
            get => _isConfirmPassword;
            set
            {
                _isConfirmPassword = value;
                OnPropertyChanged();
                ConfirmPasswordImage = _isConfirmPassword ? "eye_closed.png" : "eye.png";
            }
        }

        public string PasswordImage
        {
            get => _passwordImage;
            set
            {
                _passwordImage = value;
                OnPropertyChanged();
            }
        }

        public string ConfirmPasswordImage
        {
            get => _confirmPasswordImage;
            set
            {
                _confirmPasswordImage = value;
                OnPropertyChanged();
            }
        }
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }


        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }


        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); }
        }

        #endregion

        #region API Controller Commands
        public ICommand TogglePasswordCommand => new Command(() => IsPassword = !IsPassword);
        public ICommand ToggleConfirmPasswordCommand => new Command(() => IsConfirmPassword = !IsConfirmPassword);

        #endregion
        #region LOGIN
        public ICommand Login => new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                await App.Current.MainPage.DisplayAlert("Error", "Please fill all fields.", "OK");
                return;
            }

            var response = await client.GetStringAsync($"{baseUrl}/User");
            var users = JsonSerializer.Deserialize<List<User>>(response, _serializerOptions);
            var user = users.FirstOrDefault(u => u.username == Username && u.password == Password);

            if (user != null)
            {
                await App.Current.MainPage.DisplayAlert("Success", "Login successful!", "OK");

                await App.Current.MainPage.Navigation.PushAsync(new Home());
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error", "Invalid username or password.", "OK");
            }

        });
        #endregion

        #region REGISTER

        #endregion

        #region HelperMethods
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
