using System.Windows.Controls;
using System.Windows.Data;
using RestaurantOnline.ViewModels;

namespace RestaurantOnline.Views
{
    public partial class AuthView : UserControl
    {
        public AuthView()
        {
            InitializeComponent();
            PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
        }

        private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is AuthVM viewModel)
            {
                viewModel.Parola = PasswordBox.Password;
            }
        }
    }
} 