using CaTALK.MVVM.ViewModels;
using Microsoft.Maui;

namespace CaTALK.MVVM.Views;
public partial class Home : ContentPage
{
    PostManagement postManager;
    public Home()
	{
		InitializeComponent();
		this.BindingContext = new PostManagement(new UserManagement());
	}
    public Home(PostManagement postManagement)
    {
        InitializeComponent();
        this.BindingContext = postManagement;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await postManager.InitializeAsync(); // ✅ safe to await here

        AnimateLogo(); // or await if needed
    }
    private async void AnimateLogo()
    {
        while (true)
        {
            await LogoImage.ScaleTo(1.2, 500, Easing.CubicInOut);
            await LogoImage.ScaleTo(1.0, 500, Easing.CubicInOut);
            await Task.Delay(1000); // pause between cycles
        }
    }
}