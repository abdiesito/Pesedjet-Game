#nullable enable

using System;
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

public partial class JoinRoomViewModel : ViewModelBase
{
    private const string EmptyRoomCodeKey = "MSG-ROOM-001";
    private const string ConnectionErrorKey = "MSG-GEN-001";

    private readonly INavigator _navigator;
    private readonly IRoomClient _roomClient;

    [ObservableProperty]
    private string _roomCodeInput = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyCanExecuteChangedFor(nameof(JoinRoomCommand))]
    private bool _isBusy;

    public JoinRoomViewModel(INavigator navigator, IRoomClient roomClient)
    {
        _navigator = navigator;
        _roomClient = roomClient;
    }

    public RoomMember CurrentPlayerMember { get; private set; } = new();

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public bool IsNotBusy => !IsBusy;

    public void Initialize(RoomMember currentPlayerMember)
    {
        CurrentPlayerMember = currentPlayerMember;
        RoomCodeInput = string.Empty;
        ErrorMessage = string.Empty;
    }

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private async Task JoinRoomAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(RoomCodeInput))
        {
            ErrorMessage = Localize(EmptyRoomCodeKey);
        }
        else
        {
            IsBusy = true;
            string roomCode = RoomCodeInput.Trim().ToUpperInvariant();

            try
            {
                RoomJoinResponse response = await _roomClient.JoinRoomAsync(roomCode, CurrentPlayerMember);
                _navigator.NavigateToJoinedLobby(response, CurrentPlayerMember);
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
    }

    partial void OnRoomCodeInputChanged(string value)
    {
        ErrorMessage = string.Empty;
    }

    private static string Localize(string key)
    {
        string text = LocalizationManager.Instance[key];

        return text;
    }
}