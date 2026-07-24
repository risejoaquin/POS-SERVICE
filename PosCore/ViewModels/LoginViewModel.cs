using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PosCore.Models;
using PosCore.Services;
using System;
using System.Threading.Tasks;

namespace PosCore.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly SessionManager _sessionManager;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public Action? RequestClose { get; set; }

    public LoginViewModel(IApiService apiService, SessionManager sessionManager)
    {
        _apiService = apiService;
        _sessionManager = sessionManager;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Ingrese usuario y contraseña";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        var result = await _apiService.LoginAsync(Username, Password);
        if (result != null && !string.IsNullOrEmpty(result.Token))
        {
            _sessionManager.Token = result.Token;
            _sessionManager.CurrentTenantId = result.TenantId;
            _sessionManager.Username = Username;

            RequestClose?.Invoke();
        }
        else
        {
            ErrorMessage = "Usuario o contraseña incorrectos, o sin conexión.";
        }
        IsLoading = false;
    }
}
