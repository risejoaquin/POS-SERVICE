using System.Windows;
using PosCore.Models;

namespace PosCore.Views
{
    public partial class ItemModifierWindow : Window
    {
        private OrderItem _item;

        public ItemModifierWindow(OrderItem item)
        {
            InitializeComponent();
            _item = item;
            ProductNameText.Text = item.Product?.Name ?? "Producto";
            NotesBox.Text = item.Notes;
            DiscountBox.Text = item.Discount.ToString("F2");
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(DiscountBox.Text, out decimal discount))
            {
                if (discount > (_item.Quantity * _item.UnitPrice))
                {
                    MessageBox.Show("El descuento no puede ser mayor al total del producto.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                _item.Notes = NotesBox.Text;
                _item.Discount = discount;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Monto de descuento inválido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
