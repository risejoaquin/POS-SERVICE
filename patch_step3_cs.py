import re
with open("PosBuilder/Views/Step3Security.xaml.cs", "r", encoding="utf-8") as f:
    content = f.read()

new_methods = """
        private void ToggleJwtVisibility_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ToggleJwtVisibility.IsChecked == true)
            {
                JwtSecretVisibleBox.Text = JwtSecretBox.Password;
                JwtSecretVisibleBox.Visibility = System.Windows.Visibility.Visible;
                JwtSecretBox.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                JwtSecretBox.Password = JwtSecretVisibleBox.Text;
                JwtSecretVisibleBox.Visibility = System.Windows.Visibility.Collapsed;
                JwtSecretBox.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void JwtSecretVisibleBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ToggleJwtVisibility.IsChecked == true && DataContext is WizardViewModel vm)
            {
                vm.JwtSecret = JwtSecretVisibleBox.Text;
                UpdateStrength(JwtSecretVisibleBox.Text);
            }
        }
        
        private void UpdateStrength(string text)
        {
            int len = text.Length;
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
"""

old_changed = """        private void JwtSecretBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
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
        }"""

new_changed = """        private void JwtSecretBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ToggleJwtVisibility.IsChecked == false && DataContext is WizardViewModel vm)
            {
                vm.JwtSecret = JwtSecretBox.Password;
            }
            UpdateStrength(JwtSecretBox.Password);
        }
"""

if old_changed in content:
    content = content.replace(old_changed, new_changed + new_methods)
    with open("PosBuilder/Views/Step3Security.xaml.cs", "w", encoding="utf-8") as f:
        f.write(content)
    print("Replaced Step3Security cs")
else:
    print("Could not find old_changed")
