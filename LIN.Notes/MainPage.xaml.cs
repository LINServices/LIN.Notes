namespace LIN.Notes
{
    public partial class MainPage : ContentPage
    {



        public static Action OnColor = MauiProgram.Aa;


        public MainPage()
        {
            InitializeComponent();

            Application.Current.RequestedThemeChanged += (s, a) =>
            {
                OnColor();
            };

        }


        protected override void OnAppearing()
        {
            OnColor();
            base.OnAppearing();
        }
    }
}
