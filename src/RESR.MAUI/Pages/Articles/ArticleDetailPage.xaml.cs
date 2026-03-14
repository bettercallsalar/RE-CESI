using Microsoft.Maui.Graphics;
using RESR.MAUI.Pages.Auth;
using RESR.MAUI.Services;
using RESR.Models.Resources;

namespace RESR.MAUI.Pages.Articles;

public partial class ArticleDetailPage : ContentPage, IQueryAttributable
{
    private readonly IResourcesApiClient _resourcesApiClient;
    private readonly IMarksApiClient _marksApiClient;
    private readonly IApiSession _session;

    private CancellationTokenSource? _loadCts;
    private ArticleResponse? _article;
    private int _idResource;
    private int? _loadedResourceId;
    private bool _isFavorite;
    private bool _isReadLater;
    private bool _isMarkActionInProgress;

    public ArticleDetailPage(
        IResourcesApiClient resourcesApiClient,
        IMarksApiClient marksApiClient,
        IApiSession session)
    {
        _resourcesApiClient = resourcesApiClient;
        _marksApiClient = marksApiClient;
        _session = session;

        InitializeComponent();
        ApplyAuthState();
        ApplyMarkButtonStyles();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("idResource", out var rawValue))
            return;

        var candidate = rawValue?.ToString();
        if (!int.TryParse(candidate, out var idResource) || idResource <= 0)
            return;

        _idResource = idResource;
        _loadedResourceId = null;
        _article = null;
        _isFavorite = false;
        _isReadLater = false;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        ApplyAuthState();

        if (_idResource <= 0)
        {
            StatusLabel.Text = "Article invalide.";
            return;
        }

        if (_loadCts is not null)
            return;

        if (_loadedResourceId == _idResource && _session.IsAuthenticated)
        {
            await RefreshMarksAsync();
            return;
        }

        if (_loadedResourceId == _idResource)
            return;

        await LoadArticleAsync();
    }

    protected override void OnDisappearing()
    {
        _loadCts?.Cancel();
        base.OnDisappearing();
    }

    private async void OnFavoriteClicked(object? sender, EventArgs e)
    {
        if (!await EnsureAuthenticatedAsync())
            return;

        await ToggleMarkAsync(
            isActive: _isFavorite,
            activateAsync: async ct => _ = await _marksApiClient.MarkAsFavoriteAsync(_idResource, ct),
            deactivateAsync: ct => _marksApiClient.UnmarkAsFavoriteAsync(_idResource, ct),
            activatingLabel: "Ajout aux favoris...",
            deactivatingLabel: "Retrait des favoris...");
    }

    private async void OnReadLaterClicked(object? sender, EventArgs e)
    {
        if (!await EnsureAuthenticatedAsync())
            return;

        await ToggleMarkAsync(
            isActive: _isReadLater,
            activateAsync: async ct => _ = await _marksApiClient.MarkAsReadLaterAsync(_idResource, ct),
            deactivateAsync: ct => _marksApiClient.UnmarkAsReadLaterAsync(_idResource, ct),
            activatingLabel: "Ajout a lire plus tard...",
            deactivatingLabel: "Retrait de la liste lire plus tard...");
    }

    private async Task LoadArticleAsync()
    {
        if (_loadCts is not null)
            return;

        _loadCts = new CancellationTokenSource();
        SetActionState(isLoading: true);
        StatusLabel.Text = "Chargement de l'article...";

        try
        {
            _article = await _resourcesApiClient.GetArticleByIdAsync(_idResource, _loadCts.Token);
            if (_article is null)
            {
                StatusLabel.Text = "Article introuvable.";
                return;
            }

            BindArticle(_article);
            _loadedResourceId = _idResource;
            await LoadMarksCoreAsync(_loadCts.Token);
            StatusLabel.Text = "Article charge.";
        }
        catch (ApiException ex)
        {
            StatusLabel.Text = $"Erreur article ({(int)ex.StatusCode}) : {TrimMessage(ex.Message)}";
        }
        catch (TimeoutException ex)
        {
            StatusLabel.Text = ex.Message;
        }
        catch (OperationCanceledException)
        {
            StatusLabel.Text = "Chargement annule.";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Erreur inattendue : {TrimMessage(ex.Message)}";
        }
        finally
        {
            _loadCts?.Dispose();
            _loadCts = null;
            SetActionState(isLoading: false);
        }
    }

    private async Task RefreshMarksAsync()
    {
        if (_loadCts is not null)
            return;

        _loadCts = new CancellationTokenSource();
        SetActionState(isLoading: true);

        try
        {
            await LoadMarksCoreAsync(_loadCts.Token);
        }
        catch (ApiException ex)
        {
            MarkHintLabel.Text = $"Erreur marks ({(int)ex.StatusCode}) : {TrimMessage(ex.Message)}";
        }
        catch (TimeoutException ex)
        {
            MarkHintLabel.Text = ex.Message;
        }
        catch (OperationCanceledException)
        {
            MarkHintLabel.Text = "Requete annulee.";
        }
        finally
        {
            _loadCts?.Dispose();
            _loadCts = null;
            SetActionState(isLoading: false);
        }
    }

    private async Task LoadMarksCoreAsync(CancellationToken ct)
    {
        if (!_session.IsAuthenticated)
        {
            _isFavorite = false;
            _isReadLater = false;
            ApplyAuthState();
            ApplyMarkButtonStyles();
            MarkHintLabel.Text = BuildMarkHint();
            return;
        }

        var favoriteTask = _marksApiClient.GetFavoriteAsync(_idResource, ct);
        var readLaterTask = _marksApiClient.GetReadLaterAsync(_idResource, ct);

        await Task.WhenAll(favoriteTask, readLaterTask);

        _isFavorite = await favoriteTask is not null;
        _isReadLater = await readLaterTask is not null;

        ApplyAuthState();
        ApplyMarkButtonStyles();
        MarkHintLabel.Text = BuildMarkHint();
    }

    private void BindArticle(ArticleResponse article)
    {
        Title = article.Title;
        TitleLabel.Text = article.Title;

        var author = string.IsNullOrWhiteSpace(article.Author.Username)
            ? $"Auteur #{article.IdUser}"
            : article.Author.Username;

        MetaLabel.Text = $"Publie le {article.CreatedAt:dd/MM/yyyy}  |  {author}  |  {article.Visibility.ToLowerInvariant()}";

        DescriptionCard.IsVisible = !string.IsNullOrWhiteSpace(article.Description);
        DescriptionLabel.Text = article.Description ?? string.Empty;
        ContentLabel.Text = string.IsNullOrWhiteSpace(article.Content)
            ? "Aucun contenu disponible."
            : article.Content.Trim();
    }

    private void ApplyAuthState()
    {
        FavoriteButton.IsEnabled = _session.IsAuthenticated && !_isMarkActionInProgress && _article is not null;
        ReadLaterButton.IsEnabled = _session.IsAuthenticated && !_isMarkActionInProgress && _article is not null;
    }

    private void ApplyMarkButtonStyles()
    {
        ApplyMarkButtonStyle(FavoriteButton, _isFavorite);
        ApplyMarkButtonStyle(ReadLaterButton, _isReadLater);
    }

    private async Task<bool> EnsureAuthenticatedAsync()
    {
        if (_session.IsAuthenticated)
            return true;

        MarkHintLabel.Text = "Connecte-toi pour enregistrer cet article.";

        if (Shell.Current is not null)
            await Shell.Current.GoToAsync(nameof(LoginPage));

        return false;
    }

    private async Task ToggleMarkAsync(
        bool isActive,
        Func<CancellationToken, Task> activateAsync,
        Func<CancellationToken, Task> deactivateAsync,
        string activatingLabel,
        string deactivatingLabel)
    {
        if (_article is null || _isMarkActionInProgress)
            return;

        _isMarkActionInProgress = true;
        SetActionState(isLoading: true);
        MarkHintLabel.Text = isActive ? deactivatingLabel : activatingLabel;

        using var cts = new CancellationTokenSource();

        try
        {
            if (isActive)
                await deactivateAsync(cts.Token);
            else
                await activateAsync(cts.Token);

            await LoadMarksCoreAsync(cts.Token);
        }
        catch (ApiException ex)
        {
            MarkHintLabel.Text = $"Erreur marks ({(int)ex.StatusCode}) : {TrimMessage(ex.Message)}";
        }
        catch (TimeoutException ex)
        {
            MarkHintLabel.Text = ex.Message;
        }
        catch (OperationCanceledException)
        {
            MarkHintLabel.Text = "Requete annulee.";
        }
        finally
        {
            _isMarkActionInProgress = false;
            SetActionState(isLoading: false);
            ApplyAuthState();
            ApplyMarkButtonStyles();
        }
    }

    private void SetActionState(bool isLoading)
    {
        FavoriteButton.IsEnabled = !isLoading && _session.IsAuthenticated && _article is not null;
        ReadLaterButton.IsEnabled = !isLoading && _session.IsAuthenticated && _article is not null;
    }

    private string BuildMarkHint()
    {
        if (_article is null)
            return "Le contenu de l'article apparait ici.";

        if (!_session.IsAuthenticated)
            return "Connecte-toi pour ajouter cet article a tes favoris ou a ta liste lire plus tard.";

        var states = new List<string>();
        if (_isFavorite)
            states.Add("dans tes favoris");
        if (_isReadLater)
            states.Add("dans ta liste lire plus tard");

        return states.Count == 0
            ? "Tu peux enregistrer cet article pour plus tard ou en favori."
            : $"Cet article est deja {string.Join(" et ", states)}.";
    }

    private static void ApplyMarkButtonStyle(Button button, bool isActive)
    {
        button.BackgroundColor = isActive
            ? Color.FromArgb("#342B9A")
            : Colors.White;

        button.TextColor = isActive
            ? Colors.White
            : Color.FromArgb("#342B9A");
    }

    private static string TrimMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return "Erreur inconnue.";

        var normalized = message.Replace("\r", " ").Replace("\n", " ").Trim();
        return normalized.Length <= 180
            ? normalized
            : normalized[..177] + "...";
    }
}
