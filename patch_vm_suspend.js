const fs = require('fs');
let vm = fs.readFileSync('PosCore/ViewModels/MainViewModel.cs', 'utf8');

vm = vm.replace(
    'public partial class MainViewModel : ObservableObject\n{\n    private readonly PosDbContext _dbContext;',
    `public partial class MainViewModel : ObservableObject
{
    private readonly PosDbContext _dbContext;

    public static ObservableCollection<ObservableCollection<OrderItem>> SuspendedOrders { get; set; } = new();`
);

vm = vm.replace(
    '[RelayCommand]\n    private void OpenLogs()',
    `[RelayCommand]
    private void SuspendOrder()
    {
        if (!Cart.Any()) return;
        
        // Add current cart to suspended
        var suspendedCart = new ObservableCollection<OrderItem>(Cart);
        SuspendedOrders.Add(suspendedCart);
        
        Cart.Clear();
        UpdateTotal();
        _ = ShowNotification("Orden suspendida exitosamente.", false);
    }

    [RelayCommand]
    private void ResumeOrder()
    {
        if (Cart.Any())
        {
            _ = ShowNotification("Hay una orden en curso. Ciérrela o suspéndala antes de retomar otra.", true);
            return;
        }

        var resumeWindow = new PosCore.Views.SuspendedOrdersWindow(SuspendedOrders);
        if (resumeWindow.ShowDialog() == true && resumeWindow.SelectedOrder != null)
        {
            foreach (var item in resumeWindow.SelectedOrder)
            {
                Cart.Add(item);
            }
            SuspendedOrders.Remove(resumeWindow.SelectedOrder);
            UpdateTotal();
            _ = ShowNotification("Orden retomada.", false);
        }
    }

    [RelayCommand]
    private void OpenLogs()`
);

fs.writeFileSync('PosCore/ViewModels/MainViewModel.cs', vm);
