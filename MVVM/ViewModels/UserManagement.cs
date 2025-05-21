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
        private ImageSource _photo;
        private ImageSource _avatarImage;

        #endregion

        public UserManagement()
        {
            client = new HttpClient();
            _serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true, //JSON payload configuration
            };
            PickPhotoCommand = new Command(async () => await PickPhotoAsync());
        }

        #region OnPropertyChanged
        public ImageSource AvatarImage
        {
            get => _avatarImage;
            set => SetProperty(ref _avatarImage, value);
        }
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
        public ImageSource Photo
        {
            get => _photo;
            set => SetProperty(ref _photo, value);
        }
        #endregion

        #region LOGIN
        public ICommand Login => new Command(async () =>
        {
            try
            {
                // User Validations
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Please fill all fields.", "OK");
                    return;
                }

                var encodedUsername = Uri.EscapeDataString(Username);
                var encodedPassword = Uri.EscapeDataString(Password);

                var response = await client.GetStringAsync($"{baseUrl}User/?username={encodedUsername}&password={encodedPassword}");
                var users = JsonSerializer.Deserialize<ObservableCollection<User>>(response, _serializerOptions);

                if (users != null)
                {
                    CurrentUser = users.First(); // Gets the user
                    App.Current.MainPage = new NavigationPage(new Home { BindingContext = this });
                }
                await App.Current.MainPage.DisplayAlert("Error", "Invalid username or password.", "OK");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Internal Server Error", ex.Message, "Close");
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

                // Handles the Not Found
                try
                {
                    var response = await client.GetStringAsync($"{baseUrl}User/?username={Uri.EscapeDataString(Username)}");
                    var users = JsonSerializer.Deserialize<List<User>>(response, _serializerOptions);

                    var exactMatchUser = users?.FirstOrDefault(u => u.username.Equals(Username, StringComparison.OrdinalIgnoreCase));

                    if (exactMatchUser != null)
                    {
                        // User exists, proceed...
                    }
                    else
                    {
                        // No exact match found
                    }
                }
                catch (HttpRequestException httpEx) when (httpEx.StatusCode == HttpStatusCode.NotFound)
                {
                    // No existing user found, continue
                }

                var newUser = new User {
                    username = Username,
                    password = Password,
                    avatar = Avatar
                };

                var json = JsonSerializer.Serialize(newUser, _serializerOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var result = await client.PostAsync($"{baseUrl}User", content);

                if (result.IsSuccessStatusCode)
                {
                    CurrentUser = newUser;
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

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // PhotoGetter
        public ICommand PickPhotoCommand { get; }

        private async Task PickPhotoAsync()
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync();

                if (result != null)
                {
                    var stream = await result.OpenReadAsync();
                    var filePath = Path.Combine(FileSystem.CacheDirectory, result.FileName);

                    using var fileStream = File.OpenWrite(filePath);
                    await stream.CopyToAsync(fileStream);

                    Avatar = filePath;
                    UpdateAvatarImage();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error picking photo: {ex.Message}");
            }
        }

        private void UpdateAvatarImage()
        {
            if (!string.IsNullOrWhiteSpace(Avatar))
            {
                AvatarImage = ImageSource.FromFile(Avatar);
            }
        }
    }
    #endregion
}

