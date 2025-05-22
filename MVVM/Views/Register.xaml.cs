using CaTALK.MVVM.ViewModels;

namespace CaTALK.MVVM.Views;

public partial class Register : ContentPage
{
	public Register()
	{
		InitializeComponent();
		BindingContext = new UserManagement();
        MyDatePicker.MaximumDate = DateTime.Today.AddYears(-18);
    }

    private void PasswordTextChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is not UserManagement viewModel)
            return;

        string password = txtPass.Text;
        string confirmPassword = txtConfirmPass.Text;
        string newPassword = e.NewTextValue;

        if (!string.IsNullOrWhiteSpace(newPassword) && newPassword.Length < 8)
        {
            viewModel.IsHidden = true;
            viewModel.ErrorMessage = "Password must be at least 8 characters long.";
            lblInfo.IsVisible = false;
        }
        else if (!string.IsNullOrWhiteSpace(password) && !string.IsNullOrWhiteSpace(confirmPassword) && password != confirmPassword)
        {
            viewModel.IsHidden = true;
            viewModel.ErrorMessage = "Passwords do not match.";
            lblInfo.IsVisible = true;
        }
        else
        {
            viewModel.IsHidden = false;
            viewModel.ErrorMessage = string.Empty;
            lblInfo.IsVisible = false;
        }
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        Navigation.PopAsync();
    }

    private void ImageButton_Clicked(object sender, EventArgs e)
    {
        frAvatar.IsVisible = txtAvatar.IsVisible = true;
        App.Current.MainPage.DisplayAlert(
            "Notice",
            "Preview is currently unavailable when pasting a URL. We're working on adding this feature soon. Thank you for your patience!",
            "Got it"
        );
    }

    private void MyDatePicker_DateSelected(object sender, DateChangedEventArgs e)
    {
        var viewModel = BindingContext as UserManagement;
        viewModel.Birthday = MyDatePicker.Date;
    }

    private void ImageButton_Clicked_1(object sender, EventArgs e)
    {
        frAvatar.IsVisible = txtAvatar.IsVisible = false;
    }
}