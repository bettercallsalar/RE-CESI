using System.Collections.ObjectModel;
using Microsoft.Maui.Graphics;
using RESR.MAUI.Pages.Auth;
using RESR.MAUI.Services;
using RESR.Models.Comments;
using RESR.Models.Reactions;
using RESR.Models.Resources;

namespace RESR.MAUI.Pages.Articles;

public partial class ArticleDetailPage : ContentPage, IQueryAttributable
{
    private readonly IResourcesApiClient _resourcesApiClient;
    private readonly ICommentsApiClient _commentsApiClient;
    private readonly IReactionsApiClient _reactionsApiClient;
    private readonly IMarksApiClient _marksApiClient;
    private readonly IApiSession _session;
    private readonly IUsersApiClient _usersApiClient;
    private CancellationTokenSource? _loadCts;
    private CancellationTokenSource? _commentActionCts;
    private CancellationTokenSource? _reactionActionCts;
    private CancellationTokenSource? _markActionCts;
    private int? _idResource;
    private bool _shouldLoad;
    private ArticleResponse? _article;
    private IReadOnlyList<CommentResponse> _comments = Array.Empty<CommentResponse>();
    private IReadOnlyList<ReactionResponse> _reactions = Array.Empty<ReactionResponse>();
    private readonly HashSet<int> _expandedCommentIds = [];
    private readonly ObservableCollection<CommentThreadItem> _visibleCommentItems = [];
    private int? _replyToCommentId;
    private int? _currentUserId;
    private ReactionResponse? _currentUserReaction;
    private bool _isFavorite;
    private bool _isReadLater;

    public ArticleDetailPage(
        IResourcesApiClient resourcesApiClient,
        ICommentsApiClient commentsApiClient,
        IReactionsApiClient reactionsApiClient,
        IMarksApiClient marksApiClient,
        IUsersApiClient usersApiClient,
        IApiSession session)
    {
        _resourcesApiClient = resourcesApiClient;
        _commentsApiClient = commentsApiClient;
        _reactionsApiClient = reactionsApiClient;
        _marksApiClient = marksApiClient;
        _usersApiClient = usersApiClient;
        _session = session;
        InitializeComponent();
        BindableLayout.SetItemsSource(CommentsListLayout, _visibleCommentItems);
        UpdateCommentComposerState();
        UpdateReactionControlsState();
        UpdateMarkControlsState();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("idResource", out var rawValue) &&
            int.TryParse(rawValue?.ToString(), out var idResource) &&
            idResource > 0)
        {
            _idResource = idResource;
            _shouldLoad = true;
            _article = null;
            _isFavorite = false;
            _isReadLater = false;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        UpdateCommentComposerState();
        UpdateMarkControlsState();

        if (!_shouldLoad || !_idResource.HasValue)
            return;

        _shouldLoad = false;
        await LoadArticleAsync(_idResource.Value);
    }

    protected override void OnDisappearing()
    {
        _loadCts?.Cancel();
        _commentActionCts?.Cancel();
        _reactionActionCts?.Cancel();
        _markActionCts?.Cancel();
        base.OnDisappearing();
    }

    private async Task LoadArticleAsync(int idResource)
    {
        if (_loadCts is not null)
            return;

        _loadCts = new CancellationTokenSource();
        SetLoadingState(true);
        _currentUserId = null;
        _currentUserReaction = null;
        MarksCard.IsVisible = false;
        CommentsCard.IsVisible = false;
        ReactionsCard.IsVisible = false;

        try
        {
            ArticleResponse? article = await _resourcesApiClient.GetArticleByIdAsync(idResource, _loadCts.Token);
            if (article is null)
            {
                StatusLabel.Text = "Article introuvable.";
                HeaderCaptionLabel.Text = "Aucun contenu a afficher.";
                ArticleContentLayout.IsVisible = false;
                MarksCard.IsVisible = false;
                CommentsCard.IsVisible = false;
                ReactionsCard.IsVisible = false;
                return;
            }

            _article = article;
            _currentUserId = await TryResolveCurrentUserIdAsync(_loadCts.Token);
            Title = article.Title;
            HeaderCaptionLabel.Text = $"Article #{article.IdResource}";
            TitleLabel.Text = article.Title;
            var author = string.IsNullOrWhiteSpace(article.Author.Username)
                ? $"Auteur #{article.IdUser}"
                : article.Author.Username;

            MetaLabel.Text = $"{author}  |  Publie le {article.CreatedAt:dd/MM/yyyy}";

            var description = Normalize(article.Description);
            DescriptionLabel.Text = description;
            DescriptionLabel.IsVisible = !string.IsNullOrWhiteSpace(description);

            ContentLabel.Text = Normalize(article.Content);
            StatusLabel.Text = string.Empty;
            ArticleContentLayout.IsVisible = true;
            MarksCard.IsVisible = true;
            ReactionsCard.IsVisible = true;
            CommentsCard.IsVisible = true;
            await LoadMarksAsync(article.IdResource, _loadCts.Token);
            await LoadReactionsAsync(article.IdResource, _loadCts.Token);
            await LoadCommentsAsync(article.IdResource, preserveExpansion: false, _loadCts.Token);
        }
        catch (ApiException ex)
        {
            HeaderCaptionLabel.Text = "Erreur de chargement";
            StatusLabel.Text = $"Erreur API ({(int)ex.StatusCode}) : {TrimMessage(ex.Message)}";
            ArticleContentLayout.IsVisible = false;
            MarksCard.IsVisible = false;
            CommentsCard.IsVisible = false;
            ReactionsCard.IsVisible = false;
        }
        catch (TimeoutException ex)
        {
            HeaderCaptionLabel.Text = "Temps depasse";
            StatusLabel.Text = ex.Message;
            ArticleContentLayout.IsVisible = false;
            MarksCard.IsVisible = false;
            CommentsCard.IsVisible = false;
            ReactionsCard.IsVisible = false;
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
            MarksCard.IsVisible = false;
            CommentsCard.IsVisible = false;
            ReactionsCard.IsVisible = false;
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

    private async Task LoadReactionsAsync(int idResource, CancellationToken ct)
    {
        SetReactionActionState(true);

        try
        {
            _reactions = await _reactionsApiClient.GetByResourceIdAsync(idResource, ct);
            _currentUserReaction = _currentUserId.HasValue
                ? _reactions.FirstOrDefault(reaction => reaction.IdUser == _currentUserId.Value)
                : null;

            ApplyReactionCounters();

            if (string.IsNullOrWhiteSpace(ReactionsStatusLabel.Text) || !ReactionsStatusLabel.Text.StartsWith("Erreur", StringComparison.OrdinalIgnoreCase))
                ReactionsStatusLabel.Text = string.Empty;
        }
        catch (ApiException ex)
        {
            _reactions = Array.Empty<ReactionResponse>();
            _currentUserReaction = null;
            ApplyReactionCounters();
            ReactionsSummaryLabel.Text = "Impossible de charger les reactions.";
            ReactionsStatusLabel.Text = $"Erreur API ({(int)ex.StatusCode}) : {TrimMessage(ex.Message)}";
        }
        catch (TimeoutException ex)
        {
            _reactions = Array.Empty<ReactionResponse>();
            _currentUserReaction = null;
            ApplyReactionCounters();
            ReactionsSummaryLabel.Text = "Chargement interrompu.";
            ReactionsStatusLabel.Text = ex.Message;
        }
        catch (OperationCanceledException)
        {
            ReactionsStatusLabel.Text = "Chargement des reactions annule.";
        }
        catch (Exception ex)
        {
            _reactions = Array.Empty<ReactionResponse>();
            _currentUserReaction = null;
            ApplyReactionCounters();
            ReactionsSummaryLabel.Text = "Erreur inattendue.";
            ReactionsStatusLabel.Text = $"Impossible d'afficher les reactions : {TrimMessage(ex.Message)}";
        }
        finally
        {
            SetReactionActionState(false);
        }
    }

    private async Task LoadMarksAsync(int idResource, CancellationToken ct)
    {
        SetMarkActionState(true);

        try
        {
            if (!_session.IsAuthenticated)
            {
                _isFavorite = false;
                _isReadLater = false;
                MarkStatusLabel.Text = string.Empty;
            }
            else
            {
                var favoriteTask = _marksApiClient.GetFavoriteAsync(idResource, ct);
                var readLaterTask = _marksApiClient.GetReadLaterAsync(idResource, ct);

                await Task.WhenAll(favoriteTask, readLaterTask);

                _isFavorite = await favoriteTask is not null;
                _isReadLater = await readLaterTask is not null;
                MarkStatusLabel.Text = string.Empty;
            }

            ApplyMarkButtonState(FavoriteButton, _isFavorite);
            ApplyMarkButtonState(ReadLaterButton, _isReadLater);
            UpdateMarkControlsState();
        }
        catch (ApiException ex)
        {
            _isFavorite = false;
            _isReadLater = false;
            ApplyMarkButtonState(FavoriteButton, isSelected: false);
            ApplyMarkButtonState(ReadLaterButton, isSelected: false);
            MarkStatusLabel.Text = $"Erreur API ({(int)ex.StatusCode}) : {TrimMessage(ex.Message)}";
            UpdateMarkControlsState();
        }
        catch (TimeoutException ex)
        {
            MarkStatusLabel.Text = ex.Message;
            UpdateMarkControlsState();
        }
        catch (OperationCanceledException)
        {
            MarkStatusLabel.Text = "Chargement des favoris annule.";
            UpdateMarkControlsState();
        }
        catch (Exception ex)
        {
            MarkStatusLabel.Text = $"Impossible d'afficher les marks : {TrimMessage(ex.Message)}";
            UpdateMarkControlsState();
        }
        finally
        {
            SetMarkActionState(false);
        }
    }

    private void ApplyReactionCounters()
    {
        var likeCount = CountReactions(ReactionNames.Like);
        var dislikeCount = CountReactions(ReactionNames.Dislike);
        var loveCount = CountReactions(ReactionNames.Love);
        var totalCount = _reactions.Count;

        LikeButton.Text = $"?? Like ({likeCount})";
        DislikeButton.Text = $"?? Dislike ({dislikeCount})";
        LoveButton.Text = $"?? Love ({loveCount})";

        ReactionsSummaryLabel.Text = totalCount == 0
            ? "Aucune reaction pour le moment."
            : $"{totalCount} reaction(s) au total.";

        var currentName = _currentUserReaction?.Name?.Trim().ToLowerInvariant();
        ApplyReactionButtonState(LikeButton, currentName == ReactionNames.Like);
        ApplyReactionButtonState(DislikeButton, currentName == ReactionNames.Dislike);
        ApplyReactionButtonState(LoveButton, currentName == ReactionNames.Love);
        UpdateReactionControlsState();
    }

    private static void ApplyMarkButtonState(Button button, bool isSelected)
    {
        button.BackgroundColor = isSelected ? Color.FromArgb("#342B9A") : Color.FromArgb("#F7F7F7");
        button.TextColor = isSelected ? Colors.White : Color.FromArgb("#2C2C2C");
        button.BorderColor = isSelected ? Color.FromArgb("#342B9A") : Color.FromArgb("#D7D7D7");
    }

    private void ApplyReactionButtonState(Button button, bool isSelected)
    {
        button.BackgroundColor = isSelected ? Color.FromArgb("#342B9A") : Color.FromArgb("#F7F7F7");
        button.TextColor = isSelected ? Colors.White : Color.FromArgb("#2C2C2C");
        button.BorderColor = isSelected ? Color.FromArgb("#342B9A") : Color.FromArgb("#D7D7D7");
    }

    private void UpdateReactionControlsState()
    {
        var isAuthenticated = _session.IsAuthenticated;
        ReactionLoginHintBorder.IsVisible = !isAuthenticated;

        if (!isAuthenticated)
        {
            ReactionsStatusLabel.Text = "Connectez-vous pour choisir une reaction.";
        }
        else if (_currentUserReaction is not null)
        {
            ReactionsStatusLabel.Text = $"Votre reaction actuelle : {_currentUserReaction.Name}.";
        }
        else if (!string.IsNullOrWhiteSpace(ReactionsStatusLabel.Text) && ReactionsStatusLabel.Text.StartsWith("Erreur", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        else
        {
            ReactionsStatusLabel.Text = "Choisissez une reaction pour cet article.";
        }
    }

    private void UpdateMarkControlsState()
    {
        var isAuthenticated = _session.IsAuthenticated;
        MarkLoginHintBorder.IsVisible = !isAuthenticated;

        if (_article is null)
        {
            MarkHintLabel.Text = string.Empty;
            return;
        }

        if (!isAuthenticated)
        {
            MarkHintLabel.Text = "Connectez-vous pour ajouter cet article a vos favoris ou a votre liste lire plus tard.";
            return;
        }

        if (!_isFavorite && !_isReadLater)
        {
            MarkHintLabel.Text = "Vous pouvez enregistrer cet article de deux facons independantes.";
            return;
        }

        if (_isFavorite && _isReadLater)
        {
            MarkHintLabel.Text = "Cet article est deja dans vos favoris et dans votre liste lire plus tard.";
            return;
        }

        MarkHintLabel.Text = _isFavorite
            ? "Cet article est deja dans vos favoris."
            : "Cet article est deja dans votre liste lire plus tard.";
    }

    private void SetReactionActionState(bool isBusy)
    {
        LikeButton.IsEnabled = !isBusy;
        DislikeButton.IsEnabled = !isBusy;
        LoveButton.IsEnabled = !isBusy;
    }

    private void SetMarkActionState(bool isBusy)
    {
        var canInteract = !isBusy && _article is not null;
        FavoriteButton.IsEnabled = canInteract;
        ReadLaterButton.IsEnabled = canInteract;
    }

    private int CountReactions(string reactionName)
    {
        return _reactions.Count(reaction => string.Equals(reaction.Name, reactionName, StringComparison.OrdinalIgnoreCase));
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
            comment.DeletedAt.HasValue ? Color.FromArgb("#6B6B6B") : Color.FromArgb("#2C2C2C"),
            comment.DeletedAt.HasValue ? Color.FromArgb("#F7F7F7") : Colors.White,
            comment.DeletedAt.HasValue ? Color.FromArgb("#D7D7D7") : Color.FromArgb("#D7D7D7")));

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
            return $"{label} | auteur";

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

        return string.Join("  |  ", parts);
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

    private async void OnReactionClicked(object? sender, EventArgs e)
    {
        if (_article is null || _reactionActionCts is not null)
            return;

        if (!_session.IsAuthenticated)
        {
            UpdateReactionControlsState();
            return;
        }

        if (sender is not Button button || button.CommandParameter is not string reactionName || !ReactionNames.All.Contains(reactionName))
            return;

        if (!_currentUserId.HasValue)
        {
            _currentUserId = await TryResolveCurrentUserIdAsync(CancellationToken.None);
            if (!_currentUserId.HasValue)
            {
                ReactionsStatusLabel.Text = "Impossible d'identifier votre session.";
                return;
            }
        }

        _reactionActionCts = new CancellationTokenSource();
        SetReactionActionState(true);

        try
        {
            var normalized = reactionName.Trim().ToLowerInvariant();
            if (_currentUserReaction is not null && string.Equals(_currentUserReaction.Name, normalized, StringComparison.OrdinalIgnoreCase))
            {
                await _reactionsApiClient.DeleteAsync(_currentUserReaction.IdReaction, _reactionActionCts.Token);
                ReactionsStatusLabel.Text = $"Reaction {normalized} retiree.";
            }
            else if (_currentUserReaction is not null)
            {
                await _reactionsApiClient.UpdateAsync(_currentUserReaction.IdReaction, new UpdateReactionRequest(normalized), _reactionActionCts.Token);
                ReactionsStatusLabel.Text = $"Reaction mise a jour : {normalized}.";
            }
            else
            {
                await _reactionsApiClient.CreateAsync(_article.IdResource, new CreateReactionRequest(normalized), _reactionActionCts.Token);
                ReactionsStatusLabel.Text = $"Reaction ajoutee : {normalized}.";
            }

            await LoadReactionsAsync(_article.IdResource, _reactionActionCts.Token);
        }
        catch (ApiException ex)
        {
            ReactionsStatusLabel.Text = $"Erreur API ({(int)ex.StatusCode}) : {TrimMessage(ex.Message)}";
        }
        catch (TimeoutException ex)
        {
            ReactionsStatusLabel.Text = ex.Message;
        }
        catch (OperationCanceledException)
        {
            ReactionsStatusLabel.Text = "Action sur les reactions annulee.";
        }
        catch (Exception ex)
        {
            ReactionsStatusLabel.Text = $"Impossible de mettre a jour la reaction : {TrimMessage(ex.Message)}";
        }
        finally
        {
            _reactionActionCts.Dispose();
            _reactionActionCts = null;
            SetReactionActionState(false);
        }
    }

    private async void OnFavoriteClicked(object? sender, EventArgs e)
    {
        if (_article is null || _markActionCts is not null)
            return;

        if (!await EnsureAuthenticatedForMarksAsync())
            return;

        _markActionCts = new CancellationTokenSource();
        SetMarkActionState(true);

        try
        {
            if (_isFavorite)
            {
                await _marksApiClient.UnmarkAsFavoriteAsync(_article.IdResource, _markActionCts.Token);
                MarkStatusLabel.Text = "Article retire des favoris.";
            }
            else
            {
                await _marksApiClient.MarkAsFavoriteAsync(_article.IdResource, _markActionCts.Token);
                MarkStatusLabel.Text = "Article ajoute aux favoris.";
            }

            await LoadMarksAsync(_article.IdResource, _markActionCts.Token);
        }
        catch (ApiException ex)
        {
            MarkStatusLabel.Text = $"Erreur API ({(int)ex.StatusCode}) : {TrimMessage(ex.Message)}";
        }
        catch (TimeoutException ex)
        {
            MarkStatusLabel.Text = ex.Message;
        }
        catch (OperationCanceledException)
        {
            MarkStatusLabel.Text = "Action sur les favoris annulee.";
        }
        catch (Exception ex)
        {
            MarkStatusLabel.Text = $"Impossible de mettre a jour les favoris : {TrimMessage(ex.Message)}";
        }
        finally
        {
            _markActionCts.Dispose();
            _markActionCts = null;
            SetMarkActionState(false);
        }
    }

    private async void OnReadLaterClicked(object? sender, EventArgs e)
    {
        if (_article is null || _markActionCts is not null)
            return;

        if (!await EnsureAuthenticatedForMarksAsync())
            return;

        _markActionCts = new CancellationTokenSource();
        SetMarkActionState(true);

        try
        {
            if (_isReadLater)
            {
                await _marksApiClient.UnmarkAsReadLaterAsync(_article.IdResource, _markActionCts.Token);
                MarkStatusLabel.Text = "Article retire de la liste lire plus tard.";
            }
            else
            {
                await _marksApiClient.MarkAsReadLaterAsync(_article.IdResource, _markActionCts.Token);
                MarkStatusLabel.Text = "Article ajoute a la liste lire plus tard.";
            }

            await LoadMarksAsync(_article.IdResource, _markActionCts.Token);
        }
        catch (ApiException ex)
        {
            MarkStatusLabel.Text = $"Erreur API ({(int)ex.StatusCode}) : {TrimMessage(ex.Message)}";
        }
        catch (TimeoutException ex)
        {
            MarkStatusLabel.Text = ex.Message;
        }
        catch (OperationCanceledException)
        {
            MarkStatusLabel.Text = "Action sur les marks annulee.";
        }
        catch (Exception ex)
        {
            MarkStatusLabel.Text = $"Impossible de mettre a jour les marks : {TrimMessage(ex.Message)}";
        }
        finally
        {
            _markActionCts.Dispose();
            _markActionCts = null;
            SetMarkActionState(false);
        }
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

    private async Task<bool> EnsureAuthenticatedForMarksAsync()
    {
        if (_session.IsAuthenticated)
            return true;

        MarkStatusLabel.Text = "Connectez-vous pour enregistrer cet article.";

        if (Shell.Current is not null)
            await Shell.Current.GoToAsync(nameof(LoginPage));

        return false;
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
