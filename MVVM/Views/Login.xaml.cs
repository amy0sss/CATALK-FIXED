using CaTALK.MVVM.ViewModels;

namespace CaTALK.MVVM.Views;

public partial class Login : ContentPage
{
	public Login()
	{
		InitializeComponent();
		BindingContext = new UserManagement();
	}
}