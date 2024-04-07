namespace LIN.Notes
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            Application.Current.RequestedThemeChanged += (s, a) =>
            {
                MauiProgram.Aa();
            };

        }
    }
}
