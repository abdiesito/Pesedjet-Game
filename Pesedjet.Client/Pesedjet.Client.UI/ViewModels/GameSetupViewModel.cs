using System;
using System.Collections.Generic;
using System.Globalization;
using System.ServiceModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Rooms;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.UI.ViewModels;

public partial class GameSetupViewModel : ViewModelBase, INavigable
{
    private const int MinimumCapacity = 2;
    private const int MaximumCapacity = 6;
    private const int DefaultCapacity = 4;
    private const string GoalInfoKey = "GameSetup.GoalInfo";

    // Amulets needed to win for each capacity from 2 to 6 players (RJ-03).
    private static readonly int[] AmuletGoals = [6, 5, 4, 3, 3];

    private readonly INavigator _navigator;
    private readonly IPlayerSession _playerSession;
    private readonly IRoomClient _roomClient;
    private readonly IRoomSession _roomSession;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AmuletGoal), nameof(AmuletSlots), nameof(GoalInfo))]
    private int _capacity = DefaultCapacity;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPublicSelected), nameof(IsPrivateSelected))]
    private RoomVisibility _visibility = RoomVisibility.Public;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand), nameof(CreateRoomCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasConnectionError;

    public IReadOnlyList<CapacityOption> Capacities { get; }

    public bool ShowsTopBar => true;

    public bool IsGuest => _playerSession.IsGuest;

    public bool IsPrivateAvailable => !_playerSession.IsGuest;

    public bool IsPublicSelected => Visibility == RoomVisibility.Public;

    public bool IsPrivateSelected => Visibility == RoomVisibility.Private;

    public int AmuletGoal => AmuletGoals[Capacity - MinimumCapacity];

    public IReadOnlyList<bool> AmuletSlots => BuildAmuletSlots();

    public string GoalInfo => string.Format(
        CultureInfo.CurrentCulture,
        LocalizationManager.Instance[GoalInfoKey],
        Capacity,
        AmuletGoal);

    public GameSetupViewModel(
        INavigator navigator,
        IPlayerSession playerSession,
        IRoomClient roomClient,
        IRoomSession roomSession)
    {
        _roomClient = roomClient;
        _roomSession = roomSession;
        _navigator = navigator;
        _playerSession = playerSession;

        List<CapacityOption> options = new();

        for (int value = MinimumCapacity; value <= MaximumCapacity; value++)
        {
            options.Add(new CapacityOption
            {
                Value = value,
                IsSelected = value == DefaultCapacity,
                SelectCommand = SelectCapacityCommand
            });
        }

        Capacities = options;
    }

    [RelayCommand]
    private void SelectCapacity(CapacityOption option)
    {
        foreach (CapacityOption capacityOption in Capacities)
        {
            capacityOption.IsSelected = capacityOption == option;
        }

        Capacity = option.Value;
    }

    [RelayCommand]
    private void SelectPublic()
    {
        Visibility = RoomVisibility.Public;
    }

    [RelayCommand]
    private void SelectPrivate()
    {
        // The guest has no friends to invite, so the private room is not offered to him (RN04).
        if (IsPrivateAvailable)
        {
            Visibility = RoomVisibility.Private;
        }
    }

    [RelayCommand(CanExecute = nameof(CanRespond))]
    private void Cancel()
    {
        _navigator.GoBack();
    }

    [RelayCommand(CanExecute = nameof(CanRespond))]
    private async Task CreateRoomAsync()
    {
        IsBusy = true;
        HasConnectionError = false;

        try
        {
            string roomCode = await _roomClient.CreateRoomAsync(
                RoomModelFactory.CreateLocalMember(_playerSession),
                Visibility == RoomVisibility.Public,
                Capacity);

            _roomSession.Current = RoomModelFactory.CreateCreatedRoom(roomCode, Capacity, Visibility, _playerSession);
            _navigator.NavigateTo<WaitingRoomViewModel>();
        }
        catch (FaultException<ServiceFault>)
        {
            // The capacity is limited by the screen, so a refusal from the server is shown as a failed request.
            HasConnectionError = true;
        }
        catch (CommunicationException)
        {
            HasConnectionError = true;
        }
        catch (TimeoutException)
        {
            HasConnectionError = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanRespond()
    {
        return !IsBusy;
    }

    private List<bool> BuildAmuletSlots()
    {
        List<bool> slots = new();

        for (int slot = 0; slot < MaximumCapacity; slot++)
        {
            slots.Add(slot < AmuletGoal);
        }

        return slots;
    }
}
