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
using CaTALK.MVVM.Models;
using Microsoft.Maui;

namespace CaTALK.MVVM.ViewModels
{
    public class PostManagement : INotifyPropertyChanged
    {
        #region RESTApi Components
        HttpClient client;
        JsonSerializerOptions _serializerOptions;
        string baseUrl = "https://6809e3e61f1a52874cde3be6.mockapi.io/";
        #endregion

        private ObservableCollection<Post> _posts { get; set; } = new ObservableCollection<Post>();
        private UserManagement _userManagement;

        public PostManagement(UserManagement userManagement)
        {
            client = new HttpClient();
            _serializerOptions = new JsonSerializerOptions { WriteIndented = true };
            _userManagement = userManagement;

            // Subscribe to property changes in UserManagement if needed
            if (_userManagement != null)
            {
                _userManagement.PropertyChanged += OnUserManagementPropertyChanged;
            }
        }
        public async Task InitializeAsync()
        {
            await _userManagement.FetchAllUsersAsync();
            await FetchPostsAsync();
            StartTimeAgoUpdater();
        }

        

        private string _timeAgo;
        public string TimeAgo
        {
            get => _timeAgo;
            set => SetProperty(ref _timeAgo, value);
        }

        private string _id;
        public string id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        public ObservableCollection<Post> Posts
        {
            get => _posts;
            set { _posts = value; OnPropertyChanged(); }
        }
        private string _userId;
        public string userId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        private string _content;
        public string content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        private string _caption;
        public string caption
        {
            get => _caption;
            set => SetProperty(ref _caption, value);
        }

        private DateTime _postedAt;
        public DateTime postedAt
        {
            get => _postedAt;
            set => SetProperty(ref _postedAt, value);
        }

        private DateTime _modifiedAt;
        public DateTime modifiedAt
        {
            get => _modifiedAt;
            set => SetProperty(ref _modifiedAt, value);
        }

        private int _likes;
        public int likes
        {
            get => _likes;
            set => SetProperty(ref _likes, value);
        }

        private object[] _comments;
        public object[] comments
        {
            get => _comments;
            set => SetProperty(ref _comments, value);
        }

        private int _shares;
        public int shares
        {
            get => _shares;
            set => SetProperty(ref _shares, value);
        }

        private bool _isArchived;
        public bool isArchived
        {
            get => _isArchived;
            set => SetProperty(ref _isArchived, value);
        }

        private string _postAvatar;
        public string PostAvatar
        {
            get => _postAvatar;
            set {
                if (_postAvatar != value)
                {
                    _postAvatar = value;
                    OnPropertyChanged(nameof(PostAvatar));
                }
            }
        }
        #region Get Posts
        public ICommand GetPosts => new Command(async () => await FetchPostsAsync());
        public async Task FetchPostsAsync()
        {
            try
            {
                var response = await client.GetAsync($"{baseUrl}Post");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var posts = JsonSerializer.Deserialize<List<Post>>(json, _serializerOptions);

                    Posts.Clear();
                    foreach (var post in posts.OrderByDescending(p => p.postedAt))
                    {
                        post.TimeAgo = GetTimeAgo(post.postedAt);

                        var user = AllUsers.FirstOrDefault(u => u.id == post.userId);
                        post.PostAvatar = user?.avatar ?? "user.png";

                        Posts.Add(post);
                    }


                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Failed to load posts.", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Exception: {ex.Message}", "Close");
            }
        }



        #endregion
        #region UserManagement Data Access Properties

        // Direct access to UserManagement instance
        public UserManagement UserManager => _userManagement;

        // Current user information
        public User CurrentUser => _userManagement?.CurrentUser;
        public string CurrentUserId => _userManagement?.CurrentUser?.id;
        public string CurrentUsername => _userManagement?.CurrentUser?.username;
        public string CurrentUserAvatar => _userManagement?.CurrentUser?.avatar;
        public DateTime? CurrentUserBirthday => _userManagement?.CurrentUser?.birthday;
        public bool IsCurrentUserActive => _userManagement?.CurrentUser?.isActive ?? false;

        // Friends and friend requests
        public object[] CurrentUserFriends => _userManagement?.CurrentUser?.friends;
        public object[] CurrentUserFriendRequests => _userManagement?.CurrentUser?.friendRequests;

        // All users collection
        public List<User> AllUsers => _userManagement?.Users.ToList();


        private Post _selectedPost;
        public Post SelectedPost
        {
            get => _selectedPost;
            set
            {
                if (SetProperty(ref _selectedPost, value))
                {
                    OnPropertyChanged(nameof(PostAvatar)); 
                }
            }
        }


        #region Create Post
        public ICommand CreatePost => new Command(async () =>
        {
            try
            {
                if (!IsUserLoggedIn)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "You must be logged in to post.", "OK");
                    return;
                }
               
                var newPost = new Post
                {
                    userId = _userManagement.CurrentUser.id,
                    content = "Write your post content here...", // Replace this with actual user input
                    caption = "Caption here", // Replace this with actual user input
                    postedAt = DateTime.UtcNow,
                    modifiedAt = DateTime.UtcNow,
                    likes = 0,
                    comments = null,
                    shares = 0,
                    isArchived = false
                };

                var json = JsonSerializer.Serialize(newPost, _serializerOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{baseUrl}posts", content);

                if (response.IsSuccessStatusCode)
                {
                    await App.Current.MainPage.DisplayAlert("Success", "Post created successfully!", "OK");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Failed to create post.", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Internal Server Error", ex.Message, "Close");
            }
        });
        #endregion


        // Authentication state
        public bool IsUserLoggedIn => _userManagement?.CurrentUser != null;

        #endregion

        #region Helper Methods for UserManagement Operations
        private void StartTimeAgoUpdater()
        {
            Device.StartTimer(TimeSpan.FromMinutes(1), () =>
            {
                foreach (var post in Posts)
                {
                    post.TimeAgo = GetTimeAgo(post.postedAt);
                }

                return true; // True = repeat timer
            });
        }

        // Method to check if a user is a friend
        public bool IsFriend(string userId)
        {
            if (CurrentUserFriends == null || string.IsNullOrEmpty(userId))
                return false;

            return CurrentUserFriends.Contains(userId);
        }

        // Method to check if there's a pending friend request
        public bool HasPendingFriendRequest(string userId)
        {
            if (CurrentUserFriendRequests == null || string.IsNullOrEmpty(userId))
                return false;

            return CurrentUserFriendRequests.Contains(userId);
        }
        public static string GetTimeAgo(DateTime postedAt)
        {
            var timeSpan = DateTime.UtcNow - postedAt;

            if (timeSpan.TotalSeconds < 60)
                return $"{(int)timeSpan.TotalSeconds}s ago";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}d ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)}w ago";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)}mo ago";

            return $"{(int)(timeSpan.TotalDays / 365)}y ago";
        }

        // Method to get friend count
        public int GetFriendCount()
        {
            return CurrentUserFriends?.Length ?? 0;
        }

        // Method to get pending friend request count
        public int GetPendingRequestCount()
        {
            return CurrentUserFriendRequests?.Length ?? 0;
        }

        // Method to check if user can perform actions (is active and logged in)
        public bool CanPerformUserActions()
        {
            return IsUserLoggedIn && IsCurrentUserActive;
        }

        // Method to get user by ID from the users collection
        public User GetUserById(string userId)
        {
            if (AllUsers == null || string.IsNullOrEmpty(userId))
                return null;

            return AllUsers.FirstOrDefault(u => u.id == userId);
        }

        // Method to refresh current user data
        public async Task RefreshCurrentUserData()
        {
            if (_userManagement != null && !string.IsNullOrEmpty(CurrentUserId))
            {
                try
                {
                    var response = await client.GetStringAsync($"{baseUrl}User/{CurrentUserId}");
                    var updatedUser = JsonSerializer.Deserialize<User>(response, _serializerOptions);

                    if (updatedUser != null)
                    {
                        _userManagement.CurrentUser = updatedUser;
                        OnPropertyChanged(nameof(CurrentUser));
                        OnPropertyChanged(nameof(CurrentUserFriends));
                        OnPropertyChanged(nameof(CurrentUserFriendRequests));
                    }
                }
                catch (Exception ex)
                {
                    // Handle error appropriately
                    System.Diagnostics.Debug.WriteLine($"Error refreshing user data: {ex.Message}");
                }
            }
        }

        #endregion

        #region Event Handlers
        private void OnUserManagementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Propagate relevant property changes
            switch (e.PropertyName)
            {
                case nameof(UserManagement.CurrentUser):
                    OnPropertyChanged(nameof(CurrentUser));
                    OnPropertyChanged(nameof(CurrentUserId));
                    OnPropertyChanged(nameof(CurrentUsername));
                    OnPropertyChanged(nameof(CurrentUserAvatar));
                    OnPropertyChanged(nameof(CurrentUserBirthday));
                    OnPropertyChanged(nameof(IsCurrentUserActive));
                    OnPropertyChanged(nameof(CurrentUserFriends));
                    OnPropertyChanged(nameof(CurrentUserFriendRequests));
                    OnPropertyChanged(nameof(IsUserLoggedIn));
                    break;
                case nameof(UserManagement.Users):
                    OnPropertyChanged(nameof(AllUsers));
                    break;
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

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

        #endregion
    }
}