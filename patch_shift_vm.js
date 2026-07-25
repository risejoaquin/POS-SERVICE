const fs = require('fs');
let code = fs.readFileSync('PosCore/ViewModels/ShiftViewModel.cs', 'utf8');

code = code.replace(
    `        var sales = await _dbContext.Orders
            .Where(o => o.OrderDate >= CurrentShift.OpenedAt && !o.IsReturned)
            .SumAsync(o => o.TotalAmount);`,
    `        var salesList = await _dbContext.Orders
            .Where(o => o.OrderDate >= CurrentShift.OpenedAt && !o.IsReturned)
            .Select(o => o.TotalAmount)
            .ToListAsync();
        var sales = salesList.Sum();`
);

fs.writeFileSync('PosCore/ViewModels/ShiftViewModel.cs', code);
