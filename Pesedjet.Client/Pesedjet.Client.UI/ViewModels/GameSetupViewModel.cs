using System;
using System.Collections.Generic;
using System.Globalization;
using System.ServiceModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Lobbies;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Server.Contracts.DTOs;
using Pesedjet.Client.UI.Utilities.Threading;
using Pesedjet.Client.UI.ViewModels.Dependencies;
using Pesedjet.Client.UI.Models.Lobbies;

namespace Pesedjet.Client.UI.ViewModels;

public partial class GameSetupViewModel : ViewModelBase, INavigable
{
    private const int MinimumCapacity = 2;
    private const int MaximumCapacity = 6;
    private const int DefaultCapacity = 4;
    private const string GoalInfoKey = "GameSetup.GoalInfo";
    
    private static readonly int[] AmuletGoals = [6, 5, 4, 3, 3];

    private readonly INavigator _navigator;
    private readonly IPlayerSession _playerSession;
    private readonly ILobbyClient _lobbyClient;
    private readonly ILobbySession _lobbySession;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AmuletGoal), nameof(AmuletSlots), nameof(GoalInfo))]
    private int _capacity = DefaultCapacity;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPublicSelected), nameof(IsPrivateSelected))]
    private LobbyVisibility _visibility = LobbyVisibility.Public;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand), nameof(CreateLobbyCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasConnectionError;

    public IReadOnlyList<CapacityOption> Capacities { get; }

    public bool ShowsTopBar => true;

    public bool IsGuest => _playerSession.IsGuest;

    public bool IsPrivateAvailable => !_playerSession.IsGuest;

    public bool IsPublicSelected => Visibility == LobbyVisibility.Public;

    public bool IsPrivateSelected => Visibility == LobbyVisibility.Private;

    public int AmuletGoal => AmuletGoals[Capacity - MinimumCapacity];

    public IReadOnlyList<bool> AmuletSlots => BuildAmuletSlots();

    public string GoalInfo => string.Format(
        CultureInfo.CurrentCulture,
        LocalizationManager.Instance[GoalInfoKey],
        Capacity,
        AmuletGoal);

    public GameSetupViewModel(GameSetupDependencies dependencies)
    {
        _lobbyClient = dependencies.LobbyClient;
        _lobbySession = dependencies.LobbySession;
        _navigator = dependencies.Navigator;
        _playerSession = dependencies.PlayerSession;

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
        Visibility = LobbyVisibility.Public;
    }

    [RelayCommand]
    private void SelectPrivate()
    {
        // The guest has no friends to invite, so the private lobby is not offered to him (RN04).
        if (IsPrivateAvailable)
        {
            Visibility = LobbyVisibility.Private;
        }
    }

    [RelayCommand(CanExecute = nameof(CanRespond))]
    private void Cancel()
    {
        _navigator.GoBack();
    }

    [RelayCommand(CanExecute = nameof(CanRespond))]
    private void CreateLobby()
    {
        IsBusy = true;
        HasConnectionError = false;

        LobbyCreationRequest request = new(
            LobbyModelFactory.CreateLocalMember(_playerSession),
            Visibility == LobbyVisibility.Public,
            Capacity);

        BackgroundJob<LobbyCreationRequest, string> job = new()
        {
            Work = CreateLobbyOnWorker,
            State = request,
            OnCompleted = OnCreateLobbyCompleted,
            OnFailed = OnCreateLobbyFailed
        };

        BackgroundWork.Start(job);
    }

    private string CreateLobbyOnWorker(LobbyCreationRequest request)
    {
        return _lobbyClient.CreateLobby(request.Host, request.IsPublic, request.Capacity);
    }

    private void OnCreateLobbyCompleted(string lobbyCode, LobbyCreationRequest request)
    {
        IsBusy = false;
        _lobbySession.Current = LobbyModelFactory.CreateCreatedLobby(lobbyCode, request, _playerSession);
        _navigator.NavigateTo<LobbyViewModel>();
    }

    // The capacity is limited by the screen, so any failure is shown as a failed request.
    private void OnCreateLobbyFailed(Exception error, LobbyCreationRequest request)
    {
        IsBusy = false;
        HasConnectionError = true;
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
