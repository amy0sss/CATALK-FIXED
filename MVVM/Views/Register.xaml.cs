using CaTALK.MVVM.ViewModels;

namespace CaTALK.MVVM.Views;

public partial class Register : ContentPage
{
	public Register()
	{
		InitializeComponent();
		BindingContext = new UserManagement();
	}
}