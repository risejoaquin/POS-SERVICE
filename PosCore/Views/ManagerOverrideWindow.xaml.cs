using System.Windows;
using System.Linq;
using PosCore.Data;

namespace PosCore.Views
{
    public partial class ManagerOverrideWindow : Window
    {
        private readonly PosDbContext _dbContext;
        public bool IsAuthorized { get; private set; } = false;
        public string AuthorizedBy { get; private set; } = string.Empty;

        public ManagerOverrideWindow(string actionDescription, PosDbContext dbContext)
        {
            InitializeComponent();
            ActionDescText.Text = $"Acción: {actionDescription}";
            _dbContext = dbContext;
            PinBox.Focus();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnAuthorize_Click(object sender, RoutedEventArgs e)
        {
            var pin = PinBox.Password;
            if (string.IsNullOrWhiteSpace(pin))
            {
                MessageBox.Show("Ingrese un PIN válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Simple check: In a real app we'd hash this or call API. 
            // For now we check if there's an admin user with this pin, or hardcode a fallback "1234" for the demo
            if (pin == "1234" || pin == "admin")
            {
                IsAuthorized = true;
                AuthorizedBy = "Gerente (Admin)";
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("PIN incorrecto. Autorización denegada.", "Denegado", MessageBoxButton.OK, MessageBoxImage.Error);
                PinBox.Clear();
                PinBox.Focus();
            }
        }
    }
}
