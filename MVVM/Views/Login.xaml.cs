using CaTALK.MVVM.ViewModels;

namespace CaTALK.MVVM.Views;

public partial class Login : ContentPage
{
	public Login()
	{
		InitializeComponent();
		BindingContext = new UserManagement();
	}

    private void Tapped(object sender, TappedEventArgs e)
    {
		Navigation.PushAsync(new Register());
    }

    private void PasswordTextChanged(object sender, TextChangedEventArgs e)
    {
        var viewModel = BindingContext as UserManagement;
        if (viewModel == null) return;

        if (e.NewTextValue.Length < 8 && e.NewTextValue.Length > 0)
        {
            viewModel.IsHidden = true;
            viewModel.ErrorMessage = "Password must be at least 8 characters long.";
        }
        else
        {
            viewModel.IsHidden = false;
            viewModel.ErrorMessage = string.Empty; 
        }
    }
}