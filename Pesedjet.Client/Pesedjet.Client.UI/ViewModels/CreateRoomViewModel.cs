#nullable enable

using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.UI.Navigation;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.UI.ViewModels;

public partial class CreateRoomViewModel : ViewModelBase
{
    private const int DefaultCapacity = 4;
    private const string ConnectionErrorKey = "MSG-GEN-001";

    private readonly INavigator _navigator;
    private readonly IRoomClient _roomClient;

    [ObservableProperty]
    private int _selectedCapacity = DefaultCapacity;

    [ObservableProperty]
    private bool _isPublic = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyCanExecuteChangedFor(nameof(CreateRoomCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private bool _isBusy;

    public CreateRoomViewModel(INavigator navigator, IRoomClient roomClient)
    {
        _navigator = navigator;
        _roomClient = roomClient;
    }

    public IReadOnlyList<int> AvailableCapacities { get; } = new[] { 2, 3, 4, 5, 6 };

    public RoomMember CurrentPlayerMember { get; private set; } = new();

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public bool IsNotBusy => !IsBusy;

    public void Initialize(RoomMember currentPlayerMember)
    {
        CurrentPlayerMember = currentPlayerMember;
        SelectedCapacity = DefaultCapacity;
        IsPublic = true;
        ErrorMessage = string.Empty;
    }

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private async Task CreateRoomAsync()
    {
        ErrorMessage = string.Empty;
        IsBusy = true;

        try
        {
            string roomCode = await _roomClient.CreateRoomAsync(CurrentPlayerMember, IsPublic, SelectedCapacity);
            _navigator.NavigateToCreatedLobby(roomCode, CurrentPlayerMember, SelectedCapacity);
        }
        catch (FaultException<ServiceFault> ex)
        {
            ErrorMessage = Localize(ex.Detail.MessageKey);
        }
        catch (CommunicationException)
        {
            ErrorMessage = Localize(ConnectionErrorKey);
        }
        catch (TimeoutException)
        {
            ErrorMessage = Localize(ConnectionErrorKey);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private void Cancel()
    {
        _navigator.GoBack();
    }

    partial void OnSelectedCapacityChanged(int value)
    {
        ErrorMessage = string.Empty;
    }

    private static string Localize(string key)
    {
        string text = LocalizationManager.Instance[key];

        return text;
    }
}