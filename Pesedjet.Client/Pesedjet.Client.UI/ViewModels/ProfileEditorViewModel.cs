using System.Collections.Generic;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Client.UI.Utilities.Validation;

namespace Pesedjet.Client.UI.ViewModels;

public partial class ProfileEditorViewModel : DialogViewModelBase
{
    private const string ConfirmUsernameChangeKey = "ProfileEditor.ConfirmUsernameChange";
    private const string ConfirmGenericKey = "ProfileEditor.ConfirmGeneric";

    private readonly IPlayerSession _playerSession;
    private readonly PlayerProfile _originalProfile;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasUsernameError),
        nameof(IsUsernameRequired),
        nameof(IsUsernameFormatInvalid),
        nameof(IsUsernameAlreadyRegistered))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private FieldValidationError _usernameValidationError;

    [ObservableProperty]
    private string _username;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DescriptionCounter), nameof(IsDescriptionAtLimit))]
    private string _description;

    [ObservableProperty]
    private AvatarOption? _selectedAvatar;

    [ObservableProperty]
    private bool _isConfirming;

    [ObservableProperty]
    private string _confirmMessage = string.Empty;

    public IReadOnlyList<AvatarOption> Avatars => AvatarCatalog.All;

    public bool HasUsernameError => UsernameValidationError != FieldValidationError.None;

    public bool IsUsernameRequired => UsernameValidationError == FieldValidationError.Required;

    public bool IsUsernameFormatInvalid => UsernameValidationError == FieldValidationError.InvalidFormat;

    public bool IsUsernameAlreadyRegistered => UsernameValidationError == FieldValidationError.AlreadyRegistered;

    public string DescriptionCounter => $"{Description.Length}/{ProfileRules.DescriptionMaximumLength}";

    public bool IsDescriptionAtLimit => Description.Length >= ProfileRules.DescriptionMaximumLength;

    public ProfileEditorViewModel(IPlayerSession playerSession)
    {
        _playerSession = playerSession;
        _originalProfile = playerSession.CurrentPlayer ?? new PlayerProfile(string.Empty, 0, 0);
        _username = _originalProfile.Username;
        _description = _originalProfile.Description;
        _selectedAvatar = AvatarCatalog.Find(_originalProfile.AvatarId);
    }

    // Esc leaves the confirmation first and only then closes the editor.
    public override void Dismiss()
    {
        if (IsConfirming)
        {
            IsConfirming = false;
        }
        else
        {
            RequestClose();
        }
    }

    partial void OnUsernameChanged(string value)
    {
        UsernameValidationError = UsernameRules.Validate(value);
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        Username = Username.Trim();

        if (!HasUsernameError)
        {
            ConfirmMessage = BuildConfirmMessage();
            IsConfirming = true;
        }
    }

    protected override void OnLanguageChanged()
    {
        if (IsConfirming)
        {
            ConfirmMessage = BuildConfirmMessage();
        }

        base.OnLanguageChanged();
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestClose();
    }

    [RelayCommand]
    private void KeepEditing()
    {
        IsConfirming = false;
    }

    [RelayCommand]
    private void ConfirmSave()
    {
        // TODO: Send the profile update to the server (CU-006). If the username is already taken (RN08), mark
        // FieldValidationError.AlreadyRegistered and go back to editing instead of closing.
        _playerSession.CurrentPlayer = _originalProfile with
        {
            Username = Username,
            Description = Description,
            AvatarId = SelectedAvatar?.Id
        };
        RequestClose();
    }

    private bool CanSave()
    {
        return !HasUsernameError;
    }

    private string BuildConfirmMessage()
    {
        string message = LocalizationManager.Instance[ConfirmGenericKey];

        if (Username != _originalProfile.Username)
        {
            message = string.Format(
                CultureInfo.CurrentCulture,
                LocalizationManager.Instance[ConfirmUsernameChangeKey],
                _originalProfile.Username,
                Username);
        }

        return message;
    }
}
