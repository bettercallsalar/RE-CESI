using RESR.MAUI.Services;
using RESR.Models.Resources;

namespace RESR.MAUI.Pages.Articles;

public partial class ArticleDetailPage : ContentPage, IQueryAttributable
{
    private readonly IResourcesApiClient _resourcesApiClient;
    private CancellationTokenSource? _loadCts;
    private int? _idResource;
    private bool _shouldLoad;

    public ArticleDetailPage(IResourcesApiClient resourcesApiClient)
    {
        _resourcesApiClient = resourcesApiClient;
        InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("idResource", out var rawValue) &&
            int.TryParse(rawValue?.ToString(), out var idResource) &&
            idResource > 0)
        {
            _idResource = idResource;
            _shouldLoad = true;
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
            ArticleResponse? article = await _resourcesApiClient.GetArticleByIdAsync(idResource, _loadCts.Token);
            if (article is null)
            {
                StatusLabel.Text = "Article introuvable.";
                HeaderCaptionLabel.Text = "Aucun contenu a afficher.";
                ArticleContentLayout.IsVisible = false;
                return;
            }

            Title = article.Title;
            HeaderCaptionLabel.Text = $"Article #{article.IdResource}";
            TitleLabel.Text = article.Title;
            MetaLabel.Text = $"Auteur #{article.IdUser}  |  Publie le {article.CreatedAt:dd/MM/yyyy}";

            var description = Normalize(article.Description);
            DescriptionLabel.Text = description;
            DescriptionLabel.IsVisible = !string.IsNullOrWhiteSpace(description);

            ContentLabel.Text = Normalize(article.Content);
            StatusLabel.Text = string.Empty;
            ArticleContentLayout.IsVisible = true;
        }
        catch (ApiException ex)
        {
            HeaderCaptionLabel.Text = "Erreur de chargement";
            StatusLabel.Text = $"Erreur API ({(int)ex.StatusCode}) : {TrimMessage(ex.Message)}";
            ArticleContentLayout.IsVisible = false;
        }
        catch (TimeoutException ex)
        {
            HeaderCaptionLabel.Text = "Temps depasse";
            StatusLabel.Text = ex.Message;
            ArticleContentLayout.IsVisible = false;
        }
        catch (OperationCanceledException)
        {
            StatusLabel.Text = "Chargement annule.";
        }
        catch (Exception ex)
        {
            HeaderCaptionLabel.Text = "Erreur inattendue";
            StatusLabel.Text = $"Impossible d'afficher l'article : {TrimMessage(ex.Message)}";
            ArticleContentLayout.IsVisible = false;
        }
        finally
        {
            _loadCts.Dispose();
            _loadCts = null;
            SetLoadingState(false);
        }
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
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
