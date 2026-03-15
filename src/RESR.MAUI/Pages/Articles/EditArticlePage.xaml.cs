using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using RESR.MAUI.Services;
using RESR.Models.Categories;
using RESR.Models.Resources;

namespace RESR.MAUI.Pages.Articles;

public partial class EditArticlePage : ContentPage, IQueryAttributable
{
    private const int TitleMaxLength = 50;
    private const int DescriptionMaxLength = 5000;
    private const int MaxImages = 6;
    private const long MaxImageSizeBytes = 5 * 1024 * 1024;

    private readonly IArticlesApiClient _articlesApiClient;
    private readonly ICategoriesApiClient _categoriesApiClient;
    private int _idResource;
    private bool _isLoaded;

    public ObservableCollection<CategoryResponse> Categories { get; } = new();
    public ObservableCollection<ImageItem> SelectedImages { get; } = new();
    public ObservableCollection<ImageOption> DefaultImageOptions { get; } = new();
    private int? _defaultImageIndex;

    public EditArticlePage(IArticlesApiClient articlesApiClient, ICategoriesApiClient categoriesApiClient)
    {
        _articlesApiClient = articlesApiClient;
        _categoriesApiClient = categoriesApiClient;
        InitializeComponent();
        BindingContext = this;
        SelectedImagesView.ItemsSource = SelectedImages;
        VisibilityPicker.ItemsSource = new[] { "PUBLIC", "PRIVATE" };
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("idResource", out var value) && int.TryParse(value?.ToString(), out var id))
        {
            _idResource = id;
            _isLoaded = false;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isLoaded || _idResource <= 0)
            return;

        _isLoaded = true;
        await LoadPageAsync();
    }

    private async Task LoadPageAsync()
    {
        try
        {
            StatusLabel.Text = "Chargement de l'article...";
            var categoriesTask = _categoriesApiClient.GetCategoriesAsync(CancellationToken.None);
            var articleTask = _articlesApiClient.GetByIdAsync(_idResource, CancellationToken.None);
            await Task.WhenAll(categoriesTask, articleTask);

            Categories.Clear();
            foreach (var category in categoriesTask.Result.OrderBy(c => c.Name))
                Categories.Add(category);

            var article = articleTask.Result;
            TitleEntry.Text = article.Title;
            DescriptionEditor.Text = article.Description ?? string.Empty;
            ContentEditor.Text = article.Content;
            VisibilityPicker.SelectedItem = article.Visibility;
            CategoryPicker.SelectedItem = Categories.FirstOrDefault(c => c.IdCategory == article.IdCategory);
            UpdateTitleCounter();
            StatusLabel.Text = "Mets a jour les champs puis enregistre.";
        }
        catch (Exception ex)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = ex is ApiException apiEx ? apiEx.Message : $"Chargement impossible: {ex.Message}";
        }
    }

    private void OnTitleChanged(object? sender, TextChangedEventArgs e) => UpdateTitleCounter();

    private void UpdateTitleCounter() => TitleCounterLabel.Text = $"{TitleEntry.Text?.Length ?? 0}/{TitleMaxLength}";

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        SaveButton.IsEnabled = false;
        PickImagesButton.IsEnabled = false;
        try
        {
            var title = TitleEntry.Text?.Trim() ?? string.Empty;
            var description = DescriptionEditor.Text?.Trim() ?? string.Empty;
            var content = ContentEditor.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(title))
            {
                StatusLabel.Text = "Le titre est obligatoire.";
                return;
            }
            if (title.Length > TitleMaxLength)
            {
                StatusLabel.Text = $"Le titre ne doit pas depasser {TitleMaxLength} caracteres.";
                return;
            }
            if (description.Length > DescriptionMaxLength)
            {
                StatusLabel.Text = $"La description ne doit pas depasser {DescriptionMaxLength} caracteres.";
                return;
            }
            if (string.IsNullOrWhiteSpace(content))
            {
                StatusLabel.Text = "Le contenu est obligatoire.";
                return;
            }
            if (CategoryPicker.SelectedItem is not CategoryResponse category)
            {
                StatusLabel.Text = "Selectionne une categorie.";
                return;
            }

            await _articlesApiClient.UpdateAsync(
                _idResource,
                new UpdateArticleRequest(title, string.IsNullOrWhiteSpace(description) ? null : description, VisibilityPicker.SelectedItem?.ToString(), category.IdCategory, content),
                SelectedImages.Select(x => x.Upload).ToList(),
                _defaultImageIndex,
                CancellationToken.None);

            StatusLabel.TextColor = Colors.Green;
            StatusLabel.Text = "Article mis a jour avec succes.";
        }
        catch (Exception ex)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = ex is ApiException apiEx ? apiEx.Message : $"Mise a jour impossible: {ex.Message}";
        }
        finally
        {
            SaveButton.IsEnabled = true;
            PickImagesButton.IsEnabled = true;
        }
    }

    private async void OnPickImagesClicked(object? sender, EventArgs e)
    {
        try
        {
            var picks = await FilePicker.Default.PickMultipleAsync(new PickOptions { PickerTitle = "Choisir des images", FileTypes = FilePickerFileType.Images });
            if (picks is null) return;
            var images = new List<ImageItem>();
            foreach (var pick in picks.Take(MaxImages))
            {
                if (pick is null)
                {
                    continue;
                }

                images.Add(await LoadImageAsync(pick));
            }
            SelectedImages.Clear();
            foreach (var image in images) SelectedImages.Add(image);
            DefaultImageOptions.Clear();
            for (var index = 0; index < SelectedImages.Count; index++) DefaultImageOptions.Add(new ImageOption(index, $"{index + 1}. {SelectedImages[index].FileName}"));
            _defaultImageIndex = SelectedImages.Count > 0 ? 0 : null;
            DefaultImagePicker.SelectedIndex = _defaultImageIndex ?? -1;
            DefaultImageContainer.IsVisible = SelectedImages.Count > 0;
            SelectedImagesView.IsVisible = SelectedImages.Count > 0;
        }
        catch (Exception ex)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = $"Selection des images impossible: {ex.Message}";
        }
    }

    private void OnDefaultImageChanged(object? sender, EventArgs e) => _defaultImageIndex = DefaultImagePicker.SelectedIndex >= 0 ? DefaultImagePicker.SelectedIndex : null;

    private static async Task<ImageItem> LoadImageAsync(FileResult pick)
    {
        await using var stream = await pick.OpenReadAsync();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);
        if (memory.Length > MaxImageSizeBytes) throw new InvalidOperationException("Chaque image doit faire moins de 5 Mo.");
        var contentType = string.IsNullOrWhiteSpace(pick.ContentType) ? "image/*" : pick.ContentType;
        if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Seules les images sont autorisees.");
        return new ImageItem(pick.FileName, $"({Math.Round(memory.Length / 1024d, 1)} Ko)", new SelectedImageUpload(pick.FileName, contentType, memory.ToArray(), memory.Length));
    }

    public sealed record ImageItem(string FileName, string Description, SelectedImageUpload Upload);
    public sealed record ImageOption(int Index, string Label);
}
