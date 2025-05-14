using CaTALK.MVVM.ViewModels;

namespace CaTALK.MVVM.Views;

public partial class Test : ContentPage
{
	public Test()
	{
		InitializeComponent();
		BindingContext = new UserManagement();
	}
}