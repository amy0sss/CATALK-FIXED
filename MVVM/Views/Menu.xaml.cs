using CaTALK.MVVM.ViewModels;

namespace CaTALK.MVVM.Views;

public partial class Menu : ContentPage
{
    public Menu()
    {
        InitializeComponent();
        this.BindingContext = new PostManagement(new UserManagement());
    }
    public Menu(PostManagement postManagement)
    {
        InitializeComponent();
        this.BindingContext = postManagement;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        AnimateLogo();
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