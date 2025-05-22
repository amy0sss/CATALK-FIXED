using CaTALK.MVVM.ViewModels.Helpers;
using CaTALK.MVVM.Models;
using CaTALK.MVVM.ViewModels.Helpers;
using CaTALK.MVVM.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CaTALK.MVVM.ViewModels
{
    public class UserManagement : INotifyPropertyChanged
    {
        #region RESTApi Components
        HttpClient client;
        JsonSerializerOptions _serializerOptions;
        string baseUrl = "https://6809e3e61f1a52874cde3be6.mockapi.io/";
        #endregion
        public UserManagement()
        {
            client = new HttpClient();
            _serializerOptions = new JsonSerializerOptions { WriteIndented = true };
            PickPhotoCommand = new Command(async () => await PickPhotoAsync());
        }
        #region Getters and Setters
        private bool _isPassword = true;
        private bool _isConfirmPassword = true;
        private string _passwordImage = "hide.png";
        private string _confirmPasswordImage = "hide.png";
        private string _username;
        private string _password;
        private string _confirmPassword;
        private string _avatar;
        private ObservableCollection<User> _users;
        private User _currentUser;
        private ImageSource _avatarImage;
        private bool _isHidden = false;
        private string _errorMessage;

        private DateTime _birthday;
        public DateTime Birthday
        {
            get => _birthday;
            set => SetProperty(ref _birthday, value);
        }

        private DateTime _createdAt;
        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        private DateTime _modifiedAt;
        public DateTime ModifiedAt
        {
            get => _modifiedAt;
            set => SetProperty(ref _modifiedAt, value);
        }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        private string _id;
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private object[] _friends;
        public object[] Friends
        {
            get => _friends;
            set => SetProperty(ref _friends, value);
        }

        private object[] _friendRequests;
        public object[] FriendRequests
        {
            get => _friendRequests;
            set => SetProperty(ref _friendRequests, value);
        }

        public ImageSource AvatarImage
        {
            get => _avatarImage;
            set => SetProperty(ref _avatarImage, value);
        }
        public string Avatar
        {
            get => _avatar;
            set { _avatar = value; OnPropertyChanged(); }
        }
        public bool IsPassword
        {
            get => _isPassword;
            set { _isPassword = value; OnPropertyChanged(); PasswordImage = _isPassword ? "hide.png" : "view.png"; }
        }
        public bool IsConfirmPassword
        {
            get => _isConfirmPassword;
            set { _isConfirmPassword = value; OnPropertyChanged(); ConfirmPasswordImage = _isConfirmPassword ? "hide.png" : "view.png"; }
        }
        public bool IsHidden
        {
            get => _isHidden;
            set { _isHidden = value; OnPropertyChanged(); }
        }
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }
        public string PasswordImage
        {
            get => _passwordImage;
            set { _passwordImage = value; OnPropertyChanged(); }
        }
        public string ConfirmPasswordImage
        {
            get => _confirmPasswordImage;
            set { _confirmPasswordImage = value; OnPropertyChanged(); }
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
            set { _currentUser = value; OnPropertyChanged();
                FriendRequests = _currentUser?.friendRequests;
                Friends = _currentUser?.friends;
            }
        }
        #endregion

        #region API Controller Commands
        public ICommand TogglePasswordCommand => new Command(() => IsPassword = !IsPassword);
        public ICommand ToggleConfirmPasswordCommand => new Command(() => IsConfirmPassword = !IsConfirmPassword);
        public ICommand PickPhotoCommand { get; }


        #region LOGIN
        public ICommand Login => new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please fill all fields.";
                return;
            }

            try
            {            
                var encodedUsername = Uri.EscapeDataString(Username);
                var encodedPassword = Uri.EscapeDataString(Password);

                // Similar Only
                var response = await client.GetStringAsync($"{baseUrl}User/?username={encodedUsername}&password={encodedPassword}");
                var users = JsonSerializer.Deserialize<ObservableCollection<User>>(response, _serializerOptions);

                // Strict Validation
                var validUser = users?.FirstOrDefault(u => u.username == encodedUsername && u.password == encodedPassword);

                if (validUser != null)
                {
                    if (!validUser.isActive)
                    {
                        await App.Current.MainPage.DisplayAlert("Account Deactivated",
                            "Your account is deactivated. Please contact support or register a new account.", "OK");
                        return;
                    }

                    CurrentUser = validUser;
                    var userManagement = this; // or the current UserManagement instance
                    var postManagement = new PostManagement(userManagement);
                    App.Current.MainPage = new NavigationPage(new Home(postManagement));
                    return;
                }

                ErrorMessage = "Invalid username or password.";
                IsHidden = true;
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
                    string.IsNullOrWhiteSpace(Avatar) ||
                    Birthday == default)
                {
                    ErrorMessage = "Please fill all required fields.";
                    return;
                }

                if (!ValidateRegistration()) return;

                try
                {
                    var response = await client.GetStringAsync($"{baseUrl}User/?username={Uri.EscapeDataString(Username)}");
                    var users = JsonSerializer.Deserialize<ObservableCollection<User>>(response, _serializerOptions);

                    if (users?.Any(u => u.username.Equals(Username, StringComparison.OrdinalIgnoreCase)) == true)
                    {
                        ErrorMessage = "Username already exists!";
                        return;
                    }
                }
                catch (HttpRequestException httpEx) when (httpEx.StatusCode == HttpStatusCode.NotFound) { }

                var newUser = new User
                {
                    username = Username,
                    password = Password,
                    avatar = Avatar,
                    birthday = Birthday,
                    isActive = true,
                    createdAt = DateTime.UtcNow,
                    modifiedAt = DateTime.UtcNow,
                    friends = null,
                    friendRequests = null
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

        #region PHOTO PICKER
        private async Task PickPhotoAsync()
        {
            var photoPath = await PhotoPickerHelper.PickPhotoAsync();
            if (!string.IsNullOrWhiteSpace(photoPath))
            {
                Avatar = photoPath;
                UpdateAvatarImage();
            }
        }
        #endregion

        #region GetAllUser
        public async Task FetchAllUsersAsync()
        {
            try
            {
                var response = await client.GetAsync($"{baseUrl}users"); // Your API endpoint

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var users = JsonSerializer.Deserialize<ObservableCollection<User>>(json, _serializerOptions);

                    Users = users;
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Failed to load users.", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Exception: {ex.Message}", "Close");
            }
        }
        #endregion




        #region Deactivate Profile
        public ICommand DeactivateAccount => new Command(async () =>
        {
            try
            {
                if (CurrentUser == null || string.IsNullOrWhiteSpace(CurrentUser.id))
                {
                    await App.Current.MainPage.DisplayAlert("Error", "No user selected to deactivate.", "OK");
                    return;
                }

                bool confirm = await App.Current.MainPage.DisplayAlert(
                    "Deactivate Account",
                    "Are you sure you want to deactivate your account?",
                    "Yes",
                    "Cancel");

                if (!confirm)
                    return;

                var updatedUser = new User
                {
                    id = CurrentUser.id,
                    username = CurrentUser.username,
                    password = CurrentUser.password,
                    avatar = CurrentUser.avatar,
                    birthday = CurrentUser.birthday,
                    createdAt = CurrentUser.createdAt,
                    modifiedAt = DateTime.UtcNow,
                    friends = CurrentUser.friends,
                    friendRequests = CurrentUser.friendRequests,
                    isActive = false
                };

                var json = JsonSerializer.Serialize(updatedUser, _serializerOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"{baseUrl}User/{CurrentUser.id}", content);

                if (response.IsSuccessStatusCode)
                {
                    await App.Current.MainPage.DisplayAlert("Success", "Your account has been deactivated.", "OK");
                    App.Current.MainPage = new NavigationPage(new Login()); // Navigate to login or landing page
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Failed to deactivate account.", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Internal Server Error", ex.Message, "Close");
            }
        });
        #endregion

        #region EditProfile
        public ICommand EditProfile => new Command(async () =>
        {
            try
            {
                if (CurrentUser == null || string.IsNullOrWhiteSpace(CurrentUser.id))
                {
                    await App.Current.MainPage.DisplayAlert("Error", "No user selected to update.", "OK");
                    return;
                }

                if (!CurrentUser.isActive)
                {
                    await App.Current.MainPage.DisplayAlert("Account Deactivated", "You cannot edit a deactivated account.", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Please fill in all required fields.";
                    return;
                }

                // Update CurrentUser fields from ViewModel inputs, ensuring isActive stays true
                var updatedUser = new User
                {
                    id = CurrentUser.id,
                    username = Username,
                    password = Password,
                    avatar = Avatar,
                    birthday = Birthday,
                    friends = CurrentUser.friends, friendRequests = CurrentUser.friendRequests, // preserve existing friends and friend requests
                    isActive = true,
                    createdAt = CurrentUser.createdAt,
                    modifiedAt = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(updatedUser, _serializerOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"{baseUrl}User/{CurrentUser.id}", content);

                if (response.IsSuccessStatusCode)
                {
                    CurrentUser = updatedUser;
                    await App.Current.MainPage.DisplayAlert("Success", "Profile updated successfully!", "OK");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Failed to update profile.", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Internal Server Error", ex.Message, "Close");
            }
        });
        #endregion

        // Send friend request (adds current user's id to the target user's friendRequests list)
        public ICommand AddFriendCommand => new Command<string>(async (friendId) =>
        {
            try
            {
                if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.id) || string.IsNullOrEmpty(friendId))
                    return;

                // Get the target user from API
                var response = await client.GetStringAsync($"{baseUrl}User/{friendId}");
                var targetUser = JsonSerializer.Deserialize<User>(response, _serializerOptions);

                if (targetUser == null)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "User not found.", "OK");
                    return;
                }

                // Check if already friends or request pending
                var targetFriends = targetUser.friends?.ToList() ?? new List<object>();
                var targetRequests = targetUser.friendRequests.ToList() ?? new List<object>();

                if (targetFriends.Contains(CurrentUser.id))
                {
                    await App.Current.MainPage.DisplayAlert("Info", "You are already friends.", "OK");
                    return;
                }

                if (targetRequests.Contains(CurrentUser.id))
                {
                    await App.Current.MainPage.DisplayAlert("Info", "Friend request already sent.", "OK");
                    return;
                }

                // Add current user's id to target user's friendRequests
                targetRequests.Add(CurrentUser.id);
                targetUser.friendRequests = targetRequests.ToArray();

                bool success = await UpdateUserFriendsAsync(targetUser);

                if (success)
                    await App.Current.MainPage.DisplayAlert("Success", "Friend request sent!", "OK");
                else
                    await App.Current.MainPage.DisplayAlert("Error", "Failed to send friend request.", "OK");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "Close");
            }
        });

        // Accept friend request
        public ICommand AcceptFriendRequestCommand => new Command<string>(async (requesterId) =>
        {
            try
            {
                if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.id) || string.IsNullOrEmpty(requesterId))
                    return;

                // Remove requesterId from friendRequests
                var currentRequests = CurrentUser.friendRequests?.ToList() ?? new List<object>();
                if (!currentRequests.Contains(requesterId))
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Friend request not found.", "OK");
                    return;
                }
                currentRequests.Remove(requesterId);
                CurrentUser.friendRequests = currentRequests.ToArray();

                // Add requesterId to friends list of CurrentUser
                var currentFriends = CurrentUser.friends?.ToList() ?? new List<object>();
                if (!currentFriends.Contains(requesterId))
                    currentFriends.Add(requesterId);
                CurrentUser.friends = currentFriends.ToArray();

                // Update CurrentUser on server
                bool updatedCurrent = await UpdateUserFriendsAsync(CurrentUser);
                if (!updatedCurrent)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Failed to update your friend list.", "OK");
                    return;
                }

                // Also add CurrentUser.id to the requester user’s friends list
                var response = await client.GetStringAsync($"{baseUrl}User/{requesterId}");
                var requesterUser = JsonSerializer.Deserialize<User>(response, _serializerOptions);

                if (requesterUser == null)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Requester user not found.", "OK");
                    return;
                }

                var requesterFriends = requesterUser.friends?.ToList() ?? new List<object>();
                if (!requesterFriends.Contains(CurrentUser.id))
                    requesterFriends.Add(CurrentUser.id);
                requesterUser.friends = requesterFriends.ToArray();

                bool updatedRequester = await UpdateUserFriendsAsync(requesterUser);

                if (updatedRequester)
                    await App.Current.MainPage.DisplayAlert("Success", "Friend request accepted!", "OK");
                else
                    await App.Current.MainPage.DisplayAlert("Error", "Failed to update requester friend list.", "OK");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "Close");
            }
        });

        // Delete friend
        public ICommand DeleteFriendCommand => new Command<string>(async (friendId) =>
        {
            try
            {
                if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.id) || string.IsNullOrEmpty(friendId))
                    return;

                var currentFriends = CurrentUser.friends?.ToList() ?? new List<object>();
                if (!currentFriends.Contains(friendId))
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Friend not found in your friend list.", "OK");
                    return;
                }
                currentFriends.Remove(friendId);
                CurrentUser.friends = currentFriends.ToArray();

                bool updatedCurrent = await UpdateUserFriendsAsync(CurrentUser);
                if (!updatedCurrent)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Failed to update your friend list.", "OK");
                    return;
                }

                // Also remove CurrentUser from friend's friend list
                var response = await client.GetStringAsync($"{baseUrl}User/{friendId}");
                var friendUser = JsonSerializer.Deserialize<User>(response, _serializerOptions);

                if (friendUser != null)
                {
                    var friendUserFriends = friendUser.friends?.ToList() ?? new List<object>();
                    if (friendUserFriends.Contains(CurrentUser.id))
                    {
                        friendUserFriends.Remove(CurrentUser.id);
                        friendUser.friends = friendUserFriends.ToArray();
                        await UpdateUserFriendsAsync(friendUser);
                    }
                }

                await App.Current.MainPage.DisplayAlert("Success", "Friend removed successfully.", "OK");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "Close");
            }
        });

        #endregion

        #region Helper Methods
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
        private void UpdateAvatarImage()
        {
            if (!string.IsNullOrWhiteSpace(Avatar))
            {
                AvatarImage = ImageSource.FromFile(Avatar);
            }
        }
        private bool ValidateRegistration()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Username and password are required.";
                return false;
            }

            if (Birthday > DateTime.UtcNow)
            {
                ErrorMessage = "Birthday cannot be in the future.";
                return false;
            }

            return true;
        }
        private async Task<bool> UpdateUserFriendsAsync(User updatedUser)
        {
            if (updatedUser == null || string.IsNullOrEmpty(updatedUser.id))
                return false;

            var json = JsonSerializer.Serialize(updatedUser, _serializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"{baseUrl}User/{updatedUser.id}", content);
            return response.IsSuccessStatusCode;
        }
            #endregion
        }
}
