using System.Windows;
using System.Windows.Controls;

namespace PosCore.Views
{
    public partial class PaymentWindow : Window
    {
        public bool IsPaid { get; private set; } = false;
        public decimal Total { get; }
        
        private string _inputBuffer = "";
        private decimal _tendered = 0m;

        public PaymentWindow(decimal total)
        {
            InitializeComponent();
            Total = total;
            TotalText.Text = total.ToString("C");
            UpdateTenderedText();
            
            CustomerPhoneBox.GotFocus += (s, e) => {
                if (CustomerPhoneBox.Text == "Teléfono del cliente...")
                    CustomerPhoneBox.Text = "";
            };
            CustomerPhoneBox.LostFocus += (s, e) => {
                if (string.IsNullOrWhiteSpace(CustomerPhoneBox.Text))
                    CustomerPhoneBox.Text = "Teléfono del cliente...";
            };
        }

        private void UpdateTenderedText()
        {
            if (string.IsNullOrEmpty(_inputBuffer))
            {
                _tendered = 0;
            }
            else
            {
                // The input buffer is in cents (e.g. "1500" = 15.00)
                if (decimal.TryParse(_inputBuffer, out decimal cents))
                {
                    _tendered = cents / 100m;
                }
            }

            TenderedText.Text = _tendered.ToString("C");

            if (_tendered >= Total && Total > 0)
            {
                ChangeText.Text = $"Cambio: {(_tendered - Total).ToString("C")}";
                ChangeText.Visibility = Visibility.Visible;
            }
            else
            {
                ChangeText.Visibility = Visibility.Hidden;
            }
        }

        private void BtnNum_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is string num)
            {
                if (_inputBuffer.Length < 8) // max digits
                {
                    _inputBuffer += num;
                    UpdateTenderedText();
                }
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            _inputBuffer = "";
            UpdateTenderedText();
        }
        
        private void BtnExact_Click(object sender, RoutedEventArgs e)
        {
            _tendered = Total;
            _inputBuffer = ((long)(Total * 100)).ToString();
            UpdateTenderedText();
        }

        private void BtnSearchCustomer_Click(object sender, RoutedEventArgs e)
        {
            var phone = CustomerPhoneBox.Text.Trim();
            if (phone.Length >= 10 && phone != "Teléfono del cliente...")
            {
                CustomerInfoPanel.Visibility = Visibility.Visible;
                CustomerNameText.Text = "Cliente: Cliente Leal Frecuente";
                CustomerPointsText.Text = "Puntos disponibles: 250 pts ($25.00)";
            }
            else
            {
                MessageBox.Show("Ingrese un número de teléfono válido.", "No encontrado", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnPay_Click(object sender, RoutedEventArgs e)
        {
            if (_tendered < Total)
            {
                MessageBox.Show("El monto recibido es menor al total.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            IsPaid = true;
            DialogResult = true;
            Close();
        }
    }
}
