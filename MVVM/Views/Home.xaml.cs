using CaTALK.MVVM.ViewModels;

namespace CaTALK.MVVM.Views;
public partial class Home : ContentPage
{
	public Home()
	{
		InitializeComponent();
		BindingContext = new UserManagement();
	}
}