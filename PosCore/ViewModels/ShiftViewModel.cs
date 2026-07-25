using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PosCore.Data;
using PosCore.Models;

namespace PosCore.ViewModels;

public partial class ShiftViewModel : ObservableObject
{
    private readonly PosDbContext _dbContext;

    [ObservableProperty]
    private CashRegisterShift? _currentShift;

    [ObservableProperty]
    private bool _hasActiveShift;

    [ObservableProperty]
    private decimal _startingCashInput;

    [ObservableProperty]
    private decimal _actualEndingCashInput;

    [ObservableProperty]
    private decimal _calculatedExpectedCash;

    [ObservableProperty]
    private decimal _totalSalesInShift;

    public ShiftViewModel(PosDbContext dbContext)
    {
        _dbContext = dbContext;
        LoadCurrentShiftCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadCurrentShiftAsync()
    {
        CurrentShift = await _dbContext.CashRegisterShifts
            .Where(s => !s.IsClosed)
            .OrderByDescending(s => s.OpenedAt)
            .FirstOrDefaultAsync();

        HasActiveShift = CurrentShift != null;

        if (HasActiveShift)
        {
            await CalculateExpectedCashAsync();
        }
    }

    private async Task CalculateExpectedCashAsync()
    {
        if (CurrentShift == null) return;

        var sales = await _dbContext.Orders
            .Where(o => o.OrderDate >= CurrentShift.OpenedAt && !o.IsReturned)
            .SumAsync(o => o.TotalAmount);

        TotalSalesInShift = sales;
        CalculatedExpectedCash = CurrentShift.StartingCash + sales;
        ActualEndingCashInput = CalculatedExpectedCash; // Default to expected
    }

    [RelayCommand]
    private async Task OpenShiftAsync()
    {
        if (HasActiveShift)
        {
            MessageBox.Show("Ya hay un turno abierto.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var shift = new CashRegisterShift
        {
            OpenedAt = DateTime.Now,
            OpenedBy = "Cajero", // En un sistema completo usarías el usuario logueado
            StartingCash = StartingCashInput,
            IsClosed = false
        };

        _dbContext.CashRegisterShifts.Add(shift);
        
        // Outbox event
        var jsonOptions = new System.Text.Json.JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles };
        _dbContext.OutboxMessages.Add(new OutboxMessage
        {
            EventType = "ShiftOpened",
            Payload = System.Text.Json.JsonSerializer.Serialize(shift, jsonOptions),
            CreatedAt = DateTime.Now
        });

        await _dbContext.SaveChangesAsync();

        MessageBox.Show("Turno abierto exitosamente.", "Turno", MessageBoxButton.OK, MessageBoxImage.Information);
        
        StartingCashInput = 0;
        await LoadCurrentShiftAsync();
    }

    [RelayCommand]
    private async Task CloseShiftAsync()
    {
        if (!HasActiveShift || CurrentShift == null)
        {
            MessageBox.Show("No hay turno abierto.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show($"¿Seguro que desea cerrar el turno?\nEfectivo esperado: {CalculatedExpectedCash:C}", "Cerrar Turno", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        CurrentShift.ClosedAt = DateTime.Now;
        CurrentShift.ClosedBy = "Cajero";
        CurrentShift.ExpectedEndingCash = CalculatedExpectedCash;
        CurrentShift.ActualEndingCash = ActualEndingCashInput;
        CurrentShift.Difference = ActualEndingCashInput - CalculatedExpectedCash;
        CurrentShift.IsClosed = true;
        CurrentShift.LastUpdated = DateTime.Now;

        // Outbox event
        var jsonOptions = new System.Text.Json.JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles };
        _dbContext.OutboxMessages.Add(new OutboxMessage
        {
            EventType = "ShiftClosed",
            Payload = System.Text.Json.JsonSerializer.Serialize(CurrentShift, jsonOptions),
            CreatedAt = DateTime.Now
        });

        await _dbContext.SaveChangesAsync();

        string difMsg = CurrentShift.Difference == 0 
            ? "Caja cuadrada perfectamente." 
            : $"Diferencia: {CurrentShift.Difference:C}";

        MessageBox.Show($"Turno cerrado exitosamente.\n{difMsg}", "Turno", MessageBoxButton.OK, MessageBoxImage.Information);
        
        await LoadCurrentShiftAsync();
        
        // Cerrar ventana
        var window = Application.Current.Windows.OfType<Views.ShiftWindow>().FirstOrDefault();
        window?.Close();
    }
}
