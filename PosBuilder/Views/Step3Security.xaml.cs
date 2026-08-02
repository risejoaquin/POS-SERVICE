using System.Windows.Controls;
using System.Windows.Media;
using PosBuilder.ViewModels;

namespace PosBuilder.Views
{
    public partial class Step3Security : UserControl
    {
        public Step3Security()
        {
            InitializeComponent();
        }

        private void JwtSecretBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is WizardViewModel vm)
            {
                vm.JwtSecret = JwtSecretBox.Password;
            }
            
            int len = JwtSecretBox.Password.Length;
            if (len == 0)
            {
                StrengthMeter.Value = 0;
                StrengthLabel.Text = "";
            }
            else if (len < 6)
            {
                StrengthMeter.Value = 33;
                StrengthMeter.Foreground = Brushes.Red;
                StrengthLabel.Text = "Débil";
                StrengthLabel.Foreground = Brushes.Red;
            }
            else if (len < 10)
            {
                StrengthMeter.Value = 66;
                StrengthMeter.Foreground = Brushes.Orange;
                StrengthLabel.Text = "Media";
                StrengthLabel.Foreground = Brushes.Orange;
            }
            else
            {
                StrengthMeter.Value = 100;
                StrengthMeter.Foreground = Brushes.Green;
                StrengthLabel.Text = "Fuerte";
                StrengthLabel.Foreground = Brushes.Green;
            }
        }
    }
}