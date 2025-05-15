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
using System.Net;

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
        private string _avatar;
        private ObservableCollection<User> _users;
        private User _currentUser;

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
        public string Avatar
        {
            get => _avatar;
            set
            {
                _avatar = value;
                OnPropertyChanged();
            }
        }
        public bool IsPassword
        {
            get => _isPassword;
            set
            {
                _isPassword = value;
                OnPropertyChanged();
                PasswordImage = _isPassword ? "hide.png" : "view.png";
            }
        }

        public bool IsConfirmPassword
        {
            get => _isConfirmPassword;
            set
            {
                _isConfirmPassword = value;
                OnPropertyChanged();
                ConfirmPasswordImage = _isConfirmPassword ? "hide.png" : "view.png";
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


        public ObservableCollection<User> Users
        {
            get => _users;
            set { _users = value; OnPropertyChanged(); }
        }

        public User CurrentUser
        {
            get => _currentUser;
            set { _currentUser = value; OnPropertyChanged(); }
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

            var encodedUsername = Uri.EscapeDataString(Username);
            var response = await client.GetStringAsync($"{baseUrl}User/?username={encodedUsername}");

            var users = JsonSerializer.Deserialize<ObservableCollection<User>>(response, _serializerOptions);

            // Add the user to the ObservableCollection Instance
            Users = users;
            var user = users?.FirstOrDefault(u => u.password == Password);

            if (user != null)
            {
                CurrentUser = user; // Store the whole object
                App.Current.MainPage = new NavigationPage(new Home { BindingContext = this });
            }

            else
            {
                await App.Current.MainPage.DisplayAlert("Error", "Invalid username or password.", "OK");
            }
        });
        #endregion

        #region REGISTER
        public ICommand Register => new Command(async () =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Username) ||
                    string.IsNullOrWhiteSpace(Password) ||
                    string.IsNullOrWhiteSpace(Avatar))
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Please fill all fields.", "OK");
                    return;
                }

                ObservableCollection<User> existingUsers = new();

                try
                {
                    var response = await client.GetStringAsync($"{baseUrl}User/?username={Uri.EscapeDataString(Username)}");
                    existingUsers = JsonSerializer.Deserialize<ObservableCollection<User>>(response, _serializerOptions);
                }
                catch (HttpRequestException httpEx) when (httpEx.StatusCode == HttpStatusCode.NotFound)
                {
                    // No matching user found, proceed to register
                }

                if (existingUsers.Any(u => u.username == Username))
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Username already exists.", "OK");
                    return;
                }

                var newUser = new User
                {
                    username = Username,
                    password = Password,
                    avatar = Avatar
                };

                var json = JsonSerializer.Serialize(newUser, _serializerOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await client.PostAsync($"{baseUrl}User", content);

                if (result.IsSuccessStatusCode)
                {
                    await App.Current.MainPage.DisplayAlert("Success", "Account created successfully!", "OK");
                    App.Current.MainPage = new NavigationPage(new Home { BindingContext = this });
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Registration failed. Try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Internal Server Error", ex.Message, "Close");
            }
        });

        #endregion

        #region HelperMethods
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
