cat << 'INNER_EOF' > /tmp/ApplyDiscount.cs
    [RelayCommand]
    public void ApplyDiscount()
    {
        if (!IsDiscountApplied)
        {
            DiscountAmount = SubTotal * 0.10m;
            IsDiscountApplied = true;
        }
        else
        {
            DiscountAmount = 0;
            IsDiscountApplied = false;
        }
        UpdateTotal();
    }
INNER_EOF
sed -i '/public void OpenSettings()/e cat /tmp/ApplyDiscount.cs' PosCore/ViewModels/MainViewModel.cs
