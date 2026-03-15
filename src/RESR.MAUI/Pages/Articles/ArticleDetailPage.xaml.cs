using RESR.MAUI.Pages.Profile;
using RESR.MAUI.Services;
using RESR.Models.Resources;

namespace RESR.MAUI.Pages.Articles;

public partial class ArticleDetailPage : ContentPage, IQueryAttributable
{
    private readonly IResourcesApiClient _resourcesApiClient;
    private CancellationTokenSource? _loadCts;
    private int? _idResource;
    private bool _useOwnAccess;
    private bool _shouldLoad;
    private ArticleResponse? _article;

    public ArticleDetailPage(IResourcesApiClient resourcesApiClient)
    {
        _resourcesApiClient = resourcesApiClient;
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
        await LoadArticleAsync(_idResource.Value);
    }

    protected override void OnDisappearing()
    {
        _loadCts?.Cancel();
        base.OnDisappearing();
    }

    private async Task LoadArticleAsync(int idResource)
    {
        if (_loadCts is not null)
            return;

        _loadCts = new CancellationTokenSource();
        SetLoadingState(true);

        try
        {
            var article = _useOwnAccess
                ? await _resourcesApiClient.GetOwnArticleByIdAsync(idResource, _loadCts.Token)
                : await _resourcesApiClient.GetArticleByIdAsync(idResource, _loadCts.Token);

            if (article is null && _useOwnAccess)
                article = await _resourcesApiClient.GetArticleByIdAsync(idResource, _loadCts.Token);

            if (article is null)
            {
                HeaderCaptionLabel.Text = "Aucun contenu a afficher.";
                StatusLabel.Text = "Article introuvable.";
                ArticleContentLayout.IsVisible = false;
                return;
            }

            _article = article;
            BindArticle(article);
            ArticleContentLayout.IsVisible = true;
            StatusLabel.Text = string.Empty;
        }
        catch (ApiException ex)
        {
            ArticleContentLayout.IsVisible = false;
            HeaderCaptionLabel.Text = "Erreur de chargement";
            StatusLabel.Text = $"Erreur API ({(int)ex.StatusCode}) : {TrimMessage(ex.Message)}";
        }
        catch (TimeoutException ex)
        {
            ArticleContentLayout.IsVisible = false;
            HeaderCaptionLabel.Text = "Temps depasse";
            StatusLabel.Text = ex.Message;
        }
        catch (OperationCanceledException)
        {
            StatusLabel.Text = "Chargement annule.";
        }
        catch (Exception ex)
        {
            ArticleContentLayout.IsVisible = false;
            HeaderCaptionLabel.Text = "Erreur inattendue";
            StatusLabel.Text = $"Impossible d'afficher l'article : {TrimMessage(ex.Message)}";
        }
        finally
        {
            _loadCts?.Dispose();
            _loadCts = null;
            SetLoadingState(false);
        }
    }

    private void BindArticle(ArticleResponse article)
    {
        Title = article.Title;
        HeaderCaptionLabel.Text = $"Article #{article.IdResource}";
        TitleLabel.Text = article.Title;
        AuthorButton.Text = string.IsNullOrWhiteSpace(article.Author.Username)
            ? $"Utilisateur #{article.IdUser}"
            : article.Author.Username;
        MetaLabel.Text = BuildMetaLabel(article);

        var description = Normalize(article.Description);
        DescriptionLabel.Text = description;
        DescriptionLabel.IsVisible = !string.IsNullOrWhiteSpace(description);

        ContentLabel.Text = Normalize(article.Content);
    }

    private static string BuildMetaLabel(ArticleResponse article)
    {
        var parts = new List<string>
        {
            $"Publie le {article.CreatedAt:dd/MM/yyyy}",
            $"Visibilite {article.Visibility.ToLowerInvariant()}"
        };

        if (!article.IsApproved)
            parts.Add("non approuve");

        if (article.ModifiedAt.HasValue)
            parts.Add("modifie");

        return string.Join("  |  ", parts);
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
    }

    private async void OnAuthorClicked(object? sender, EventArgs e)
    {
        if (_article is null || Shell.Current is null)
            return;

        await Shell.Current.GoToAsync(
            $"{nameof(UserProfilePage)}?idUser={_article.IdUser}&username={Uri.EscapeDataString(_article.Author.Username ?? string.Empty)}&firstName={Uri.EscapeDataString(_article.Author.FirstName ?? string.Empty)}");
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
