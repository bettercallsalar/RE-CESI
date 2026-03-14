using System.Collections.ObjectModel;
using RESR.MAUI.Services;
using RESR.Models.Comments;
using RESR.Models.Resources;

namespace RESR.MAUI.Pages.Articles;

public partial class ArticleDetailPage : ContentPage, IQueryAttributable
{
    private readonly IResourcesApiClient _resourcesApiClient;
    private readonly ICommentsApiClient _commentsApiClient;
    private readonly IApiSession _session;
    private CancellationTokenSource? _loadCts;
    private CancellationTokenSource? _commentActionCts;
    private int? _idResource;
    private bool _shouldLoad;
    private ArticleResponse? _article;
    private IReadOnlyList<CommentResponse> _comments = Array.Empty<CommentResponse>();
    private readonly HashSet<int> _expandedCommentIds = [];
    private readonly ObservableCollection<CommentThreadItem> _visibleCommentItems = [];
    private int? _replyToCommentId;

    public ArticleDetailPage(IResourcesApiClient resourcesApiClient, ICommentsApiClient commentsApiClient, IApiSession session)
    {
        _resourcesApiClient = resourcesApiClient;
        _commentsApiClient = commentsApiClient;
        _session = session;
        InitializeComponent();
        BindableLayout.SetItemsSource(CommentsListLayout, _visibleCommentItems);
        UpdateCommentComposerState();
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
        UpdateCommentComposerState();

        if (!_shouldLoad || !_idResource.HasValue)
            return;

        _shouldLoad = false;
        await LoadArticleAsync(_idResource.Value);
    }

    protected override void OnDisappearing()
    {
        _loadCts?.Cancel();
        _commentActionCts?.Cancel();
        base.OnDisappearing();
    }

    private async Task LoadArticleAsync(int idResource)
    {
        if (_loadCts is not null)
            return;

        _loadCts = new CancellationTokenSource();
        SetLoadingState(true);
        CommentsCard.IsVisible = false;

        try
        {
            ArticleResponse? article = await _resourcesApiClient.GetArticleByIdAsync(idResource, _loadCts.Token);
            if (article is null)
            {
                StatusLabel.Text = "Article introuvable.";
                HeaderCaptionLabel.Text = "Aucun contenu a afficher.";
                ArticleContentLayout.IsVisible = false;
                CommentsCard.IsVisible = false;
                return;
            }

            _article = article;
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
            CommentsCard.IsVisible = true;
            await LoadCommentsAsync(article.IdResource, preserveExpansion: false, _loadCts.Token);
        }
        catch (ApiException ex)
        {
            HeaderCaptionLabel.Text = "Erreur de chargement";
            StatusLabel.Text = $"Erreur API ({(int)ex.StatusCode}) : {TrimMessage(ex.Message)}";
            ArticleContentLayout.IsVisible = false;
            CommentsCard.IsVisible = false;
        }
        catch (TimeoutException ex)
        {
            HeaderCaptionLabel.Text = "Temps depasse";
            StatusLabel.Text = ex.Message;
            ArticleContentLayout.IsVisible = false;
            CommentsCard.IsVisible = false;
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
            CommentsCard.IsVisible = false;
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

    private async Task LoadCommentsAsync(int idResource, bool preserveExpansion, CancellationToken ct)
    {
        SetCommentsLoadingState(true);

        try
        {
            _comments = await _commentsApiClient.GetByResourceIdAsync(idResource, ct);
            ApplyExpandedState(preserveExpansion);
            RebuildCommentThread();
            CommentsSummaryLabel.Text = BuildCommentsSummary();

            if (string.IsNullOrWhiteSpace(CommentsStatusLabel.Text) || !CommentsStatusLabel.Text.StartsWith("Erreur", StringComparison.OrdinalIgnoreCase))
                CommentsStatusLabel.Text = string.Empty;
        }
        catch (ApiException ex)
        {
            _comments = Array.Empty<CommentResponse>();
            _visibleCommentItems.Clear();
            NoCommentsLabel.IsVisible = false;
            CommentsSummaryLabel.Text = "Impossible de charger les commentaires.";
            CommentsStatusLabel.Text = $"Erreur API ({(int)ex.StatusCode}) : {TrimMessage(ex.Message)}";
        }
        catch (TimeoutException ex)
        {
            _comments = Array.Empty<CommentResponse>();
            _visibleCommentItems.Clear();
            NoCommentsLabel.IsVisible = false;
            CommentsSummaryLabel.Text = "Chargement interrompu.";
            CommentsStatusLabel.Text = ex.Message;
        }
        catch (OperationCanceledException)
        {
            CommentsStatusLabel.Text = "Chargement des commentaires annule.";
        }
        catch (Exception ex)
        {
            _comments = Array.Empty<CommentResponse>();
            _visibleCommentItems.Clear();
            NoCommentsLabel.IsVisible = false;
            CommentsSummaryLabel.Text = "Erreur inattendue.";
            CommentsStatusLabel.Text = $"Impossible d'afficher les commentaires : {TrimMessage(ex.Message)}";
        }
        finally
        {
            SetCommentsLoadingState(false);
        }
    }

    private void ApplyExpandedState(bool preserveExpansion)
    {
        var expandableIds = _comments
            .Where(comment => comment.IdParentComment.HasValue)
            .Select(comment => comment.IdParentComment!.Value)
            .ToHashSet();

        if (!preserveExpansion || _expandedCommentIds.Count == 0)
        {
            _expandedCommentIds.Clear();
            foreach (var id in expandableIds)
                _expandedCommentIds.Add(id);

            return;
        }

        _expandedCommentIds.IntersectWith(expandableIds);
    }

    private void RebuildCommentThread()
    {
        _visibleCommentItems.Clear();

        var childrenByParent = _comments
            .Where(comment => comment.IdParentComment.HasValue)
            .GroupBy(comment => comment.IdParentComment!.Value)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(comment => comment.CreatedAt).ToList());

        var commentsById = _comments.ToDictionary(comment => comment.IdComment);
        var roots = _comments
            .Where(comment => !comment.IdParentComment.HasValue || !commentsById.ContainsKey(comment.IdParentComment.Value))
            .OrderBy(comment => comment.CreatedAt)
            .ToList();

        var descendantCounts = BuildDescendantCountLookup(childrenByParent);

        foreach (var root in roots)
            AddVisibleComment(root, depth: 0, childrenByParent, descendantCounts);

        NoCommentsLabel.IsVisible = _comments.Count == 0;
    }

    private Dictionary<int, int> BuildDescendantCountLookup(Dictionary<int, List<CommentResponse>> childrenByParent)
    {
        var counts = new Dictionary<int, int>();

        int CountDescendants(int idComment)
        {
            if (counts.TryGetValue(idComment, out var cachedCount))
                return cachedCount;

            if (!childrenByParent.TryGetValue(idComment, out var children))
            {
                counts[idComment] = 0;
                return 0;
            }

            var count = children.Count;
            foreach (var child in children)
                count += CountDescendants(child.IdComment);

            counts[idComment] = count;
            return count;
        }

        foreach (var comment in _comments)
            CountDescendants(comment.IdComment);

        return counts;
    }

    private void AddVisibleComment(
        CommentResponse comment,
        int depth,
        Dictionary<int, List<CommentResponse>> childrenByParent,
        IReadOnlyDictionary<int, int> descendantCounts)
    {
        descendantCounts.TryGetValue(comment.IdComment, out var descendantCount);
        var hasChildren = descendantCount > 0;
        var isExpanded = hasChildren && _expandedCommentIds.Contains(comment.IdComment);

        _visibleCommentItems.Add(new CommentThreadItem(
            comment.IdComment,
            depth,
            BuildCommentAuthorLabel(comment),
            BuildCommentMetaLabel(comment),
            comment.DeletedAt.HasValue ? "Commentaire supprime." : Normalize(comment.Content),
            hasChildren,
            isExpanded,
            descendantCount,
            _session.IsAuthenticated && !comment.DeletedAt.HasValue,
            new Thickness(Math.Min(depth * 24, 120), 0, 0, 0),
            comment.DeletedAt.HasValue ? Color.FromArgb("#7B768E") : Color.FromArgb("#2F2B4C"),
            comment.DeletedAt.HasValue ? Color.FromArgb("#F6F3F0") : Colors.White,
            comment.DeletedAt.HasValue ? Color.FromArgb("#DDD5CD") : Color.FromArgb("#E5E0DA")));

        if (!hasChildren || !isExpanded || !childrenByParent.TryGetValue(comment.IdComment, out var children))
            return;

        foreach (var child in children)
            AddVisibleComment(child, depth + 1, childrenByParent, descendantCounts);
    }

    private string BuildCommentsSummary()
    {
        var totalCount = _comments.Count;
        var replyCount = _comments.Count(comment => comment.IdParentComment.HasValue);

        if (totalCount == 0)
            return "Aucun commentaire pour le moment.";

        return $"{totalCount} message(s) dont {replyCount} reponse(s).";
    }

    private string BuildCommentAuthorLabel(CommentResponse comment)
    {
        var label = $"Utilisateur #{comment.IdUser}";

        if (_article is not null && comment.IdUser == _article.IdUser)
            return $"{label} · auteur";

        return label;
    }

    private static string BuildCommentMetaLabel(CommentResponse comment)
    {
        var parts = new List<string>
        {
            comment.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
        };

        if (comment.ModifiedAt.HasValue)
            parts.Add("modifie");

        if (comment.DeletedAt.HasValue)
            parts.Add("supprime");

        return string.Join("  ·  ", parts);
    }

    private void UpdateCommentComposerState()
    {
        var isAuthenticated = _session.IsAuthenticated;
        CommentComposerLayout.IsVisible = isAuthenticated;
        CommentLoginHintBorder.IsVisible = !isAuthenticated;
        ReplyTargetBorder.IsVisible = isAuthenticated && _replyToCommentId.HasValue;
        ComposerTitleLabel.Text = _replyToCommentId.HasValue ? "Publier une reponse" : "Ajouter un commentaire";
        PostCommentButton.Text = _replyToCommentId.HasValue ? "Publier la reponse" : "Publier";
        ReplyTargetLabel.Text = _replyToCommentId.HasValue
            ? $"Reponse au commentaire #{_replyToCommentId.Value}"
            : string.Empty;
    }

    private void SetCommentsLoadingState(bool isLoading)
    {
        CommentsLoadingIndicator.IsVisible = isLoading;
        CommentsLoadingIndicator.IsRunning = isLoading;
        RefreshCommentsButton.IsEnabled = !isLoading && _article is not null;
    }

    private void SetCommentActionState(bool isBusy)
    {
        CommentEditor.IsEnabled = !isBusy;
        PostCommentButton.IsEnabled = !isBusy;
        CancelReplyButton.IsEnabled = !isBusy;
        RefreshCommentsButton.IsEnabled = !isBusy && _article is not null;
    }

    private async void OnRefreshCommentsClicked(object? sender, EventArgs e)
    {
        if (!_idResource.HasValue || _loadCts is not null || _commentActionCts is not null)
            return;

        using var cts = new CancellationTokenSource();
        await LoadCommentsAsync(_idResource.Value, preserveExpansion: true, cts.Token);
    }

    private void OnReplyClicked(object? sender, EventArgs e)
    {
        if (sender is not BindableObject bindable || bindable.BindingContext is not CommentThreadItem item)
            return;

        if (!_session.IsAuthenticated)
        {
            CommentsStatusLabel.Text = "Connectez-vous pour repondre a un commentaire.";
            return;
        }

        _replyToCommentId = item.IdComment;
        CommentsStatusLabel.Text = $"Reponse preparee pour le commentaire #{item.IdComment}.";
        UpdateCommentComposerState();
        MainThread.BeginInvokeOnMainThread(() => CommentEditor.Focus());
    }

    private void OnCancelReplyClicked(object? sender, EventArgs e)
    {
        _replyToCommentId = null;
        CommentsStatusLabel.Text = "Mode reponse annule.";
        UpdateCommentComposerState();
    }

    private void OnToggleRepliesClicked(object? sender, EventArgs e)
    {
        if (sender is not BindableObject bindable || bindable.BindingContext is not CommentThreadItem item || !item.HasChildren)
            return;

        if (!_expandedCommentIds.Add(item.IdComment))
            _expandedCommentIds.Remove(item.IdComment);

        RebuildCommentThread();
    }

    private async void OnPostCommentClicked(object? sender, EventArgs e)
    {
        if (!_idResource.HasValue || _commentActionCts is not null)
            return;

        if (!_session.IsAuthenticated)
        {
            CommentsStatusLabel.Text = "Connectez-vous pour publier un commentaire.";
            return;
        }

        var content = Normalize(CommentEditor.Text);
        if (string.IsNullOrWhiteSpace(content))
        {
            CommentsStatusLabel.Text = "Le commentaire ne peut pas etre vide.";
            return;
        }

        _commentActionCts = new CancellationTokenSource();
        SetCommentActionState(true);
        CommentsStatusLabel.Text = _replyToCommentId.HasValue
            ? "Publication de la reponse en cours..."
            : "Publication du commentaire en cours...";

        try
        {
            var replyTargetId = _replyToCommentId;
            var createdComment = await _commentsApiClient.CreateAsync(
                _idResource.Value,
                new CreateCommentRequest(content, replyTargetId),
                _commentActionCts.Token);

            if (replyTargetId.HasValue)
                _expandedCommentIds.Add(replyTargetId.Value);

            CommentEditor.Text = string.Empty;
            _replyToCommentId = null;
            UpdateCommentComposerState();
            await LoadCommentsAsync(_idResource.Value, preserveExpansion: true, _commentActionCts.Token);
            CommentsStatusLabel.Text = createdComment.IdParentComment.HasValue
                ? "Reponse publiee."
                : "Commentaire publie.";
        }
        catch (ApiException ex)
        {
            CommentsStatusLabel.Text = $"Erreur API ({(int)ex.StatusCode}) : {TrimMessage(ex.Message)}";
        }
        catch (TimeoutException ex)
        {
            CommentsStatusLabel.Text = ex.Message;
        }
        catch (OperationCanceledException)
        {
            CommentsStatusLabel.Text = "Publication annulee.";
        }
        catch (Exception ex)
        {
            CommentsStatusLabel.Text = $"Impossible de publier le commentaire : {TrimMessage(ex.Message)}";
        }
        finally
        {
            _commentActionCts.Dispose();
            _commentActionCts = null;
            SetCommentActionState(false);
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

    private sealed record CommentThreadItem(
        int IdComment,
        int Depth,
        string AuthorLabel,
        string MetaLabel,
        string DisplayContent,
        bool HasChildren,
        bool IsExpanded,
        int DescendantCount,
        bool CanReply,
        Thickness IndentMargin,
        Color ContentTextColor,
        Color CardBackgroundColor,
        Color CardStrokeColor)
    {
        public string ToggleRepliesLabel => IsExpanded
            ? $"Masquer {DescendantCount} reponse(s)"
            : $"Afficher {DescendantCount} reponse(s)";
    }
}
