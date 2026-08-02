with open("PosCore/ViewModels/MainViewModel.cs", "r") as f:
    content = f.read()

old_code = """    public void UpdateTotal()
    {
        SubTotal = Cart.Sum(i => i.SubTotal);
        // Simulate auto discount evaluation (e.g. 10% off for combo if more than 2 items)
        if (Cart.Count >= 2)
        {
            DiscountAmount = SubTotal * 0.10m;
            IsDiscountApplied = true;
        }
        else
        {
            DiscountAmount = 0;
            IsDiscountApplied = false;
        }
        Total = SubTotal - DiscountAmount;
    }"""

new_code = """    public void UpdateTotal()
    {
        SubTotal = Cart.Sum(i => i.SubTotal);
        if (IsDiscountApplied)
        {
            DiscountAmount = SubTotal * 0.10m;
        }
        else
        {
            DiscountAmount = 0;
        }
        Total = SubTotal - DiscountAmount;
    }"""

if old_code in content:
    content = content.replace(old_code, new_code)
    with open("PosCore/ViewModels/MainViewModel.cs", "w") as f:
        f.write(content)
    print("Replaced!")
else:
    print("Not found.")
