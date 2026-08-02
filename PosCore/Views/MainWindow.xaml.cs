using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using PosCore.ViewModels;
using PosCore.Models;
using System.Linq;

namespace PosCore.Views
{
    public partial class MainWindow : Window
    {
        private StringBuilder _inputBuffer = new StringBuilder();

        public void ShowLoading(string message = "Cargando...")
        {
            MainOverlay.Show(message);
        }

        public void HideLoading()
        {
            MainOverlay.Hide();
        }

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        
        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                int index = -1;
                if (e.SystemKey >= Key.D1 && e.SystemKey <= Key.D9)
                    index = e.SystemKey - Key.D1;
                else if (e.SystemKey >= Key.NumPad1 && e.SystemKey <= Key.NumPad9)
                    index = e.SystemKey - Key.NumPad1;
                
                if (index >= 0 && DataContext is MainViewModel vm)
                {
                    if (index < vm.Shortcuts.Count)
                    {
                        var shortcut = vm.Shortcuts[index];
                        vm.ExecuteShortcutCommand.Execute(shortcut.Action);
                    }
                    e.Handled = true;
                    return;
                }
            }


            // Ignore modifiers and tab/etc if needed, but we'll focus on letters/digits
            if (e.Key == Key.Enter)
            {
                var input = _inputBuffer.ToString();
                _inputBuffer.Clear();

                if (string.IsNullOrWhiteSpace(input)) return;

                if (DataContext is MainViewModel vm)
                {
                    var barcodeProcessor = new BarcodeProcessor(vm.DbContext);
                    
                    bool isBarcode = await barcodeProcessor.DetectBarcodeTiming(input);
                    BarcodeProcessor.ClearKeystrokes();

                    if (isBarcode)
                    {
                        var product = barcodeProcessor.LookupProduct(input);
                        if (product == null)
                        {
                            var result = MessageBox.Show($"Producto no encontrado. ¿Crear nuevo?", "No encontrado", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes)
                            {
                                // Placeholder para abrir crear nuevo producto
                                MessageBox.Show("Abriendo ventana de creación de producto...", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        else
                        {
                            var existingItem = vm.Cart.FirstOrDefault(c => c.ProductId == product.Id);
                            if (existingItem != null)
                            {
                                existingItem.Quantity++;
                                
                            }
                            else
                            {
                                vm.Cart.Add(new OrderItem
                                {
                                    ProductId = product.Id,
                                    Product = product,
                                    UnitPrice = product.Price,
                                    Quantity = 1,
                                });
                            }
                            vm.UpdateTotal();
                        }
                    }
                    else
                    {
                        // Check shortcuts
                        var shortcut = barcodeProcessor.ProcessShortcuts(input);
                        switch (shortcut)
                        {
                            case ShortcutAction.Mostrador:
                                MessageBox.Show("Atajo detectado: Mostrador");
                                break;
                            case ShortcutAction.Descuento:
                                MessageBox.Show("Atajo detectado: Descuento");
                                if (!vm.IsDiscountApplied)
                                {
                                    vm.ApplyDiscountCommand?.Execute(null); // Assuming there's a command, if not, toggle it
                                }
                                break;
                            case ShortcutAction.AdminPanel:
                                MessageBox.Show("Atajo detectado: Panel Admin");
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            else
            {
                BarcodeProcessor.RegisterKeystroke();
                
                if (e.Key >= Key.D0 && e.Key <= Key.D9)
                    _inputBuffer.Append((char)('0' + (e.Key - Key.D0)));
                else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                    _inputBuffer.Append((char)('0' + (e.Key - Key.NumPad0)));
                else if (e.Key >= Key.A && e.Key <= Key.Z)
                    _inputBuffer.Append(e.Key.ToString());
            }
        }
    }
}
