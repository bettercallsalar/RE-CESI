using RESR.MAUI.Pages.Profile;
using RESR.MAUI.Services;
using RESR.Models.Resources;

namespace RESR.MAUI.Pages.Events;

public partial class EventDetailPage : ContentPage, IQueryAttributable
{
    private readonly IResourcesApiClient _resourcesApiClient;
    private readonly IUsersApiClient _usersApiClient;
    private readonly IApiSession _session;
    private CancellationTokenSource? _loadCts;
    private CancellationTokenSource? _deleteActionCts;
    private int? _idResource;
    private bool _useOwnAccess;
    private bool _shouldLoad;
    private int? _currentUserId;
    private EventResponse? _event;

    public EventDetailPage(
        IResourcesApiClient resourcesApiClient,
        IUsersApiClient usersApiClient,
        IApiSession session)
    {
        _resourcesApiClient = resourcesApiClient;
        _usersApiClient = usersApiClient;
        _session = session;

        InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("idResource", out var rawId) &&
            int.TryParse(rawId?.ToString(), out var idResource) &&
            idResource > 0)
        {
            _idResource = idResource;
            _shouldLoad = true;
            _event = null;
            _currentUserId = null;
        }

        if (query.TryGetValue("useOwnAccess", out var rawOwnAccess) &&
            bool.TryParse(rawOwnAccess?.ToString(), out var useOwnAccess))
        {
            _useOwnAccess = useOwnAccess;
        }
        else
        {
            _useOwnAccess = false;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_shouldLoad || !_idResource.HasValue)
            return;

        _shouldLoad = false;
        await LoadEventAsync(_idResource.Value);
    }

    protected override void OnDisappearing()
    {
        _loadCts?.Cancel();
        _deleteActionCts?.Cancel();
        base.OnDisappearing();
    }

    private async Task LoadEventAsync(int idResource)
    {
        if (_loadCts is not null)
            return;

        _loadCts = new CancellationTokenSource();
        SetLoadingState(true);
        StatusLabel.Text = "Chargement de l'evenement...";
        HeaderCaptionLabel.Text = "Chargement du contenu...";
        EventContentLayout.IsVisible = false;
        EditEventButton.IsVisible = false;
        DeleteEventButton.IsVisible = false;
        _currentUserId = null;

        try
        {
            var @event = await ResolveEventAsync(idResource, _loadCts.Token);
            if (@event is null)
            {
                HeaderCaptionLabel.Text = "Evenement introuvable";
                StatusLabel.Text = "Aucun contenu a afficher.";
                return;
            }

            _event = @event;
            _currentUserId = await TryResolveCurrentUserIdAsync(_loadCts.Token);
            BindEvent(@event);
            EventContentLayout.IsVisible = true;
            StatusLabel.Text = string.Empty;
        }
        catch (ApiException ex)
        {
            HeaderCaptionLabel.Text = "Erreur de chargement";
            StatusLabel.Text = UserFeedback.FromApiException(ex, "Impossible d'afficher l'evenement pour le moment.");
        }
        catch (TimeoutException ex)
        {
            HeaderCaptionLabel.Text = "Service indisponible";
            StatusLabel.Text = UserFeedback.FromTimeout(ex);
        }
        catch (OperationCanceledException)
        {
            StatusLabel.Text = string.Empty;
        }
        catch (Exception)
        {
            HeaderCaptionLabel.Text = "Erreur inattendue";
            StatusLabel.Text = UserFeedback.FromUnexpected("Impossible d'afficher l'evenement pour le moment.");
        }
        finally
        {
            SetLoadingState(false);
            _loadCts?.Dispose();
            _loadCts = null;
        }
    }

    private async Task<EventResponse?> ResolveEventAsync(int idResource, CancellationToken ct)
    {
        if (_useOwnAccess && _session.IsAuthenticated)
        {
            try
            {
                var ownEvent = await _resourcesApiClient.GetOwnEventByIdAsync(idResource, ct);
                if (ownEvent is not null)
                    return ownEvent;
            }
            catch (ApiException)
            {
            }
        }

        return await _resourcesApiClient.GetEventByIdAsync(idResource, ct);
    }

    private void BindEvent(EventResponse @event)
    {
        Title = @event.Title;
        HeaderCaptionLabel.Text = "Presentation complete";
        TitleLabel.Text = @event.Title;
        SubtitleLabel.Text = Normalize(@event.Subtitle);
        SubtitleLabel.IsVisible = !string.IsNullOrWhiteSpace(SubtitleLabel.Text);
        AuthorButton.Text = BuildAuthorLabel(@event);
        MetaLabel.Text = BuildMetaLabel(@event);
        DateRangeLabel.Text = BuildDateRangeLabel(@event);
        AddressLabel.Text = BuildAddressLabel(@event);
        DescriptionLabel.Text = Normalize(@event.Description);
        DescriptionLabel.IsVisible = !string.IsNullOrWhiteSpace(DescriptionLabel.Text);
        var canManageEvent =
            _session.IsAuthenticated &&
            !@event.DeletedAt.HasValue &&
            _currentUserId.HasValue &&
            _currentUserId.Value == @event.IdUser;
        EditEventButton.IsVisible = canManageEvent;
        DeleteEventButton.IsVisible = canManageEvent;
    }

    private static string BuildAuthorLabel(EventResponse @event)
    {
        var username = Normalize(@event.Author.Username);
        var firstName = Normalize(@event.Author.FirstName);

        if (!string.IsNullOrWhiteSpace(username))
            return $"@{username}";

        if (!string.IsNullOrWhiteSpace(firstName))
            return firstName;

        return "Utilisateur";
    }

    private static string BuildMetaLabel(EventResponse @event)
    {
        var parts = new List<string>
        {
            $"Publie le {@event.CreatedAt:dd/MM/yyyy}",
            $"Visibilite {@event.Visibility.ToLowerInvariant()}"
        };

        if (@event.ModifiedAt.HasValue)
            parts.Add("modifie");

        if (!@event.IsApproved)
            parts.Add("non approuve");

        return string.Join("  |  ", parts);
    }

    private static string BuildDateRangeLabel(EventResponse @event)
    {
        if (@event.EndDate.HasValue)
            return $"Du {@event.StartDate:dd/MM/yyyy} au {@event.EndDate.Value:dd/MM/yyyy}";

        return $"Le {@event.StartDate:dd/MM/yyyy}";
    }

    private static string BuildAddressLabel(EventResponse @event)
    {
        var parts = new List<string>();

        var address = Normalize(@event.Address);
        if (!string.IsNullOrWhiteSpace(address))
            parts.Add(address);

        if (@event.Department is not null)
            parts.Add($"{@event.Department.Code} - {@event.Department.Name}");

        return parts.Count > 0
            ? string.Join("  |  ", parts)
            : "Lieu a confirmer";
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        EditEventButton.IsEnabled = !isLoading;
        DeleteEventButton.IsEnabled = !isLoading && _deleteActionCts is null;
    }

    private async Task<int?> TryResolveCurrentUserIdAsync(CancellationToken ct)
    {
        if (!_session.IsAuthenticated)
            return null;

        try
        {
            var me = await _usersApiClient.GetMeAsync(ct);
            return me?.IdUser;
        }
        catch
        {
            return null;
        }
    }

    private async void OnAuthorClicked(object? sender, EventArgs e)
    {
        if (_event is null || Shell.Current is null)
            return;

        var route =
            $"{nameof(UserProfilePage)}?idUser={_event.IdUser}" +
            $"&username={Uri.EscapeDataString(_event.Author.Username ?? string.Empty)}" +
            $"&firstName={Uri.EscapeDataString(_event.Author.FirstName ?? string.Empty)}";

        try
        {
            await Shell.Current.GoToAsync(route);
        }
        catch (Exception)
        {
            StatusLabel.Text = UserFeedback.NavigationError;
        }
    }

    private async void OnDeleteEventClicked(object? sender, EventArgs e)
    {
        if (_event is null || _deleteActionCts is not null)
            return;

        if (!_session.IsAuthenticated || !_currentUserId.HasValue || _currentUserId.Value != _event.IdUser)
        {
            StatusLabel.Text = "Seul l'auteur peut supprimer cet evenement.";
            return;
        }

        var shouldDelete = await DisplayAlertAsync(
            "Supprimer l'evenement",
            "Voulez-vous vraiment supprimer cet evenement ? Cette action est irreversible.",
            "Supprimer",
            "Annuler");

        if (!shouldDelete)
            return;

        _deleteActionCts = new CancellationTokenSource();
        DeleteEventButton.IsEnabled = false;
        StatusLabel.Text = "Suppression de l'evenement en cours...";

        try
        {
            await _resourcesApiClient.DeleteEventAsync(_event.IdResource, _deleteActionCts.Token);
            StatusLabel.Text = "Evenement supprime.";

            if (Shell.Current is not null)
                await Shell.Current.GoToAsync("..");
        }
        catch (ApiException ex)
        {
            StatusLabel.Text = UserFeedback.FromApiException(ex, "Impossible de supprimer l'evenement pour le moment.");
        }
        catch (TimeoutException ex)
        {
            StatusLabel.Text = UserFeedback.FromTimeout(ex);
        }
        catch (OperationCanceledException)
        {
            StatusLabel.Text = string.Empty;
        }
        catch (Exception)
        {
            StatusLabel.Text = UserFeedback.FromUnexpected("Impossible de supprimer l'evenement pour le moment.");
        }
        finally
        {
            _deleteActionCts?.Dispose();
            _deleteActionCts = null;
            DeleteEventButton.IsEnabled = _event is not null;
        }
    }

    private async void OnEditEventClicked(object? sender, EventArgs e)
    {
        if (_event is null || Shell.Current is null)
            return;

        if (!_session.IsAuthenticated || !_currentUserId.HasValue || _currentUserId.Value != _event.IdUser)
        {
            StatusLabel.Text = "Seul l'auteur peut modifier cet evenement.";
            return;
        }

        try
        {
            await Shell.Current.GoToAsync(
                $"{nameof(EditEventPage)}?idResource={_event.IdResource}&useOwnAccess={_useOwnAccess.ToString().ToLowerInvariant()}");
        }
        catch (Exception)
        {
            StatusLabel.Text = UserFeedback.NavigationError;
        }
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value.Replace("\r\n", "\n").Trim();
    }

    private static string TrimMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return "Erreur inconnue.";

        var normalized = message.Replace("\r", " ").Replace("\n", " ").Trim();
        return normalized.Length <= 180
            ? normalized
            : normalized[..177].TrimEnd() + "...";
    }
}
