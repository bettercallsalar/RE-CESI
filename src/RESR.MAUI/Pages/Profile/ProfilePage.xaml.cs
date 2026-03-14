using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using RESR.MAUI.Pages.Articles;
using RESR.MAUI.Pages.Auth;
using RESR.MAUI.Pages.Home;
using RESR.MAUI.Services;
using RESR.Models.Marks;
using RESR.Models.Resources;
using RESR.Models.Users;

namespace RESR.MAUI.Pages.Profile;

public partial class ProfilePage : ContentPage
{
    private const int MarksPageSize = 50;

    private readonly IUsersApiClient _usersApiClient;
    private readonly IMarksApiClient _marksApiClient;
    private readonly IResourcesApiClient _resourcesApiClient;
    private readonly IApiSession _session;

    private CancellationTokenSource? _loadCts;

    public ProfilePage(
        IUsersApiClient usersApiClient,
        IMarksApiClient marksApiClient,
        IResourcesApiClient resourcesApiClient,
        IApiSession session)
    {
        _usersApiClient = usersApiClient;
        _marksApiClient = marksApiClient;
        _resourcesApiClient = resourcesApiClient;
        _session = session;

        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_session.IsAuthenticated)
        {
            StatusLabel.Text = "Connecte-toi pour acceder a ton profil.";
            await Shell.Current.GoToAsync(nameof(LoginPage));
            return;
        }

        await LoadProfileAsync();
    }

    protected override void OnDisappearing()
    {
        _loadCts?.Cancel();
        base.OnDisappearing();
    }

    private async Task LoadProfileAsync()
    {
        if (_loadCts is not null)
        {
            StatusLabel.Text = "Chargement deja en cours...";
            return;
        }

        _loadCts = new CancellationTokenSource();
        StatusLabel.Text = "Chargement du profil et des marques...";
        FavoritesContainer.Children.Clear();
        ReadLaterContainer.Children.Clear();

        try
        {
            var profileTask = _usersApiClient.GetMeAsync(_loadCts.Token);
            var favoritesTask = LoadAllMarksAsync((page, pageSize, ct) => _marksApiClient.GetFavoritesAsync(page, pageSize, ct), _loadCts.Token);
            var readLaterTask = LoadAllMarksAsync((page, pageSize, ct) => _marksApiClient.GetReadLaterAsync(page, pageSize, ct), _loadCts.Token);

            await Task.WhenAll(profileTask, favoritesTask, readLaterTask);

            var me = await profileTask;
            if (me is null)
            {
                StatusLabel.Text = "Profil introuvable.";
                return;
            }

            BindProfile(me);

            var favorites = await BuildProfileMarkItemsAsync(await favoritesTask, _loadCts.Token);
            var readLater = await BuildProfileMarkItemsAsync(await readLaterTask, _loadCts.Token);

            RenderMarks(FavoritesContainer, FavoritesEmptyLabel, favorites);
            RenderMarks(ReadLaterContainer, ReadLaterEmptyLabel, readLater);

            StatusLabel.Text = "Profil charge.";
        }
        catch (ApiException ex)
        {
            StatusLabel.Text = $"Erreur profil ({(int)ex.StatusCode}) : {ex.Message}";
        }
        catch (TimeoutException ex)
        {
            StatusLabel.Text = ex.Message;
        }
        catch (OperationCanceledException)
        {
            StatusLabel.Text = "Requete annulee.";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Erreur inattendue: {ex.Message}";
        }
        finally
        {
            _loadCts.Dispose();
            _loadCts = null;
        }
    }

    private void BindProfile(UserResponse me)
    {
        UsernameLabel.Text = me.Username;
        EmailLabel.Text = me.Email;
        FirstNameLabel.Text = me.FirstName;
        BirthDateLabel.Text = me.BirthDate?.ToString("yyyy-MM-dd") ?? "Non renseignee";
        BioLabel.Text = string.IsNullOrWhiteSpace(me.Bio) ? "Non renseignee" : me.Bio;
        DepartmentLabel.Text = $"{me.Department.Name} ({me.Department.Code})";
        VerifiedLabel.Text = me.IsVerified ? "Oui" : "Non";
    }

    private async Task<IReadOnlyList<MarkResponse>> LoadAllMarksAsync(
        Func<int, int, CancellationToken, Task<PaginatedMarksResponse>> fetchPageAsync,
        CancellationToken ct)
    {
        var items = new List<MarkResponse>();
        var page = 1;

        while (true)
        {
            var response = await fetchPageAsync(page, MarksPageSize, ct);
            items.AddRange(response.Items);

            if (response.TotalPages <= 0 || page >= response.TotalPages)
                break;

            page++;
        }

        return items;
    }

    private async Task<IReadOnlyList<ProfileMarkItem>> BuildProfileMarkItemsAsync(
        IReadOnlyList<MarkResponse> marks,
        CancellationToken ct)
    {
        if (marks.Count == 0)
            return [];

        var items = await Task.WhenAll(marks.Select(mark => BuildProfileMarkItemAsync(mark, ct)));
        return items
            .Where(item => item is not null)
            .Select(item => item!)
            .ToList();
    }

    private async Task<ProfileMarkItem?> BuildProfileMarkItemAsync(MarkResponse mark, CancellationToken ct)
    {
        var article = await _resourcesApiClient.GetArticleByIdAsync(mark.IdRessource, ct);
        if (article is not null)
        {
            var summary = FirstNonEmpty(article.Description, article.Content, "Aucune description disponible.");

            return new ProfileMarkItem(
                mark.IdRessource,
                "Article",
                article.Title,
                $"Publie le {article.CreatedAt:dd/MM/yyyy}",
                ToExcerpt(summary, 160),
                Route: $"{nameof(ArticleDetailPage)}?idResource={mark.IdRessource}");
        }

        var @event = await _resourcesApiClient.GetEventByIdAsync(mark.IdRessource, ct);
        if (@event is not null)
        {
            var summary = FirstNonEmpty(@event.Description, @event.Subtitle, "Aucune description disponible.");
            var location = FirstNonEmpty(@event.Address, @event.Department?.Name, "Lieu a confirmer");

            return new ProfileMarkItem(
                mark.IdRessource,
                "Evenement",
                @event.Title,
                @event.EndDate.HasValue
                    ? $"Du {@event.StartDate:dd/MM/yyyy} au {@event.EndDate:dd/MM/yyyy}"
                    : $"Le {@event.StartDate:dd/MM/yyyy}",
                ToExcerpt($"{summary}  |  {location}", 160),
                Route: null);
        }

        return new ProfileMarkItem(
            mark.IdRessource,
            "Ressource",
            $"Ressource #{mark.IdRessource}",
            "Ressource indisponible",
            "Cette ressource n'est plus accessible depuis l'application.",
            Route: null);
    }

    private void RenderMarks(
        VerticalStackLayout container,
        Label emptyLabel,
        IReadOnlyList<ProfileMarkItem> items)
    {
        container.Children.Clear();
        emptyLabel.IsVisible = items.Count == 0;

        foreach (var item in items)
            container.Children.Add(BuildMarkCard(item));
    }

    private View BuildMarkCard(ProfileMarkItem item)
    {
        var badge = new Border
        {
            BackgroundColor = Color.FromArgb("#342B9A"),
            Padding = new Thickness(10, 4),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            HorizontalOptions = LayoutOptions.Start,
            Content = new Label
            {
                Text = item.Type,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White
            }
        };

        var openButton = new Button
        {
            Text = "Ouvrir",
            HorizontalOptions = LayoutOptions.End,
            IsVisible = !string.IsNullOrWhiteSpace(item.Route)
        };

        openButton.Clicked += async (_, _) => await NavigateToMarkedResourceAsync(item.Route);

        return new Border
        {
            BackgroundColor = Color.FromArgb("#F8F8FB"),
            Stroke = Color.FromArgb("#D7D7D7"),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20) },
            Padding = new Thickness(18),
            Content = new VerticalStackLayout
            {
                Spacing = 10,
                Children =
                {
                    badge,
                    new Label
                    {
                        Text = item.Title,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 18,
                        TextColor = Color.FromArgb("#1E1C44")
                    },
                    new Label
                    {
                        Text = item.Subtitle,
                        FontSize = 12,
                        TextColor = Color.FromArgb("#6A678A")
                    },
                    new Label
                    {
                        Text = item.Summary,
                        FontSize = 14,
                        TextColor = Color.FromArgb("#2C2C2C"),
                        LineBreakMode = LineBreakMode.WordWrap
                    },
                    openButton
                }
            }
        };
    }

    private async Task NavigateToMarkedResourceAsync(string? route)
    {
        if (string.IsNullOrWhiteSpace(route) || Shell.Current is null)
            return;

        try
        {
            await Shell.Current.GoToAsync(route);
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Navigation impossible : {TrimMessage(ex.Message)}";
        }
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        _session.Clear();
        await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        return string.Empty;
    }

    private static string ToExcerpt(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value.Replace("\r", " ").Replace("\n", " ").Trim();
        if (normalized.Length <= maxLength)
            return normalized;

        return normalized[..Math.Max(0, maxLength - 3)].TrimEnd() + "...";
    }

    private static string TrimMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return "Erreur inconnue.";

        return ToExcerpt(message, 180);
    }

    private sealed record ProfileMarkItem(
        int IdResource,
        string Type,
        string Title,
        string Subtitle,
        string Summary,
        string? Route);
}
