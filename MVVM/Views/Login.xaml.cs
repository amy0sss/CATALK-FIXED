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
}