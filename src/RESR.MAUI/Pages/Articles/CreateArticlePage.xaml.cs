using RESR.MAUI.Pages.Home;
using RESR.MAUI.Services;
using RESR.Models.Categories;
using RESR.Models.Resources;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;

namespace RESR.MAUI.Pages.Articles;

public partial class CreateArticlePage : ContentPage
{
    private static readonly Color MutedStatusColor = Color.FromArgb("#5F5F66");
    private static readonly Color ErrorStatusColor = Color.FromArgb("#AB231E");
    private static readonly Color SuccessStatusColor = Color.FromArgb("#1D6B43");

    private const int TitleMaxLength = 50;
    private const int DescriptionMaxLength = 5000;
    private const int MaxImages = 6;
    private const long MaxImageSizeBytes = 5 * 1024 * 1024;

    private readonly IArticlesApiClient _articlesApiClient;
    private readonly ICategoriesApiClient _categoriesApiClient;

    public ObservableCollection<CategoryResponse> Categories { get; } = new();
    public ObservableCollection<ImageItem> SelectedImages { get; } = new();
    public ObservableCollection<ImageOption> DefaultImageOptions { get; } = new();
    private int? _defaultImageIndex;

    public CreateArticlePage(IArticlesApiClient articlesApiClient, ICategoriesApiClient categoriesApiClient)
    {
        _articlesApiClient = articlesApiClient;
        _categoriesApiClient = categoriesApiClient;
        InitializeComponent();
        BindingContext = this;
        SelectedImagesView.ItemsSource = SelectedImages;

        VisibilityPicker.ItemsSource = new[] { "PUBLIC", "PRIVATE" };
        VisibilityPicker.SelectedIndex = 0;
        StatusLabel.TextColor = MutedStatusColor;
        UpdateTitleCounter();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (Categories.Count == 0)
        {
            await LoadCategoriesAsync();
        }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await NavigateBackAsync();
    }

    private void OnTitleChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateTitleCounter();
    }

    private void UpdateTitleCounter()
    {
        var length = TitleEntry.Text?.Length ?? 0;
        TitleCounterLabel.Text = $"{length}/{TitleMaxLength}";
    }

    private async void OnCreateClicked(object? sender, EventArgs e)
    {
        CreateButton.IsEnabled = false;
        PickImagesButton.IsEnabled = false;

        try
        {
            StatusLabel.TextColor = MutedStatusColor;
            StatusLabel.Text = "Validation en cours...";

            var title = TitleEntry.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(title))
            {
                StatusLabel.TextColor = ErrorStatusColor;
                StatusLabel.Text = "Le titre est obligatoire.";
                return;
            }

            if (title.Length > TitleMaxLength)
            {
                StatusLabel.TextColor = ErrorStatusColor;
                StatusLabel.Text = $"Le titre ne doit pas depasser {TitleMaxLength} caracteres.";
                return;
            }

            var descriptionHtml = DescriptionEditor.Text?.Trim() ?? string.Empty;
            if (descriptionHtml.Length > DescriptionMaxLength)
            {
                StatusLabel.TextColor = ErrorStatusColor;
                StatusLabel.Text = $"La description ne doit pas depasser {DescriptionMaxLength} caracteres.";
                return;
            }

            var contentHtml = ContentEditor.Text?.Trim() ?? string.Empty;
            if (contentHtml.Length == 0)
            {
                StatusLabel.TextColor = ErrorStatusColor;
                StatusLabel.Text = "Le contenu est obligatoire.";
                return;
            }

            if (CategoryPicker.SelectedItem is not CategoryResponse selectedCategory)
            {
                StatusLabel.TextColor = ErrorStatusColor;
                StatusLabel.Text = "Selectionnez une categorie.";
                return;
            }

            var visibility = VisibilityPicker.SelectedItem?.ToString() ?? "PUBLIC";

            await _articlesApiClient.CreateAsync(
                new CreateArticleRequest(
                    title,
                    string.IsNullOrWhiteSpace(descriptionHtml) ? null : descriptionHtml,
                    visibility,
                    selectedCategory.IdCategory,
                    contentHtml),
                SelectedImages.Select(image => image.Upload).ToList(),
                _defaultImageIndex,
                CancellationToken.None);

            StatusLabel.TextColor = SuccessStatusColor;
            StatusLabel.Text = "Article cree avec succes.";

            TitleEntry.Text = string.Empty;
            CategoryPicker.SelectedItem = null;
            VisibilityPicker.SelectedIndex = 0;
            DescriptionEditor.Text = string.Empty;
            ContentEditor.Text = string.Empty;
            SelectedImages.Clear();
            DefaultImageOptions.Clear();
            DefaultImageContainer.IsVisible = false;
            SelectedImagesView.IsVisible = false;
            _defaultImageIndex = null;
            UpdateTitleCounter();
        }
        catch (ApiException ex)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = UserFeedback.FromApiException(ex, "La publication de l'article a echoue.");
        }
        catch (TimeoutException)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = UserFeedback.TimeoutError;
        }
        catch (Exception)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = "La publication de l'article a echoue.";
        }
        finally
        {
            CreateButton.IsEnabled = true;
            PickImagesButton.IsEnabled = true;
        }
    }

    private async void OnPickImagesClicked(object? sender, EventArgs e)
    {
        try
        {
            var picks = await FilePicker.Default.PickMultipleAsync(new PickOptions
            {
                PickerTitle = "Choisir des images",
                FileTypes = FilePickerFileType.Images
            });

            if (picks is null)
            {
                return;
            }

            var images = new List<ImageItem>();

            foreach (var pick in picks.Take(MaxImages))
            {
                if (pick is null)
                {
                    continue;
                }

                var image = await LoadImageAsync(pick);
                images.Add(image);
            }

            if (images.Count == 0)
            {
                return;
            }

            SelectedImages.Clear();
            foreach (var image in images)
            {
                SelectedImages.Add(image);
            }

            DefaultImageOptions.Clear();
            for (var index = 0; index < SelectedImages.Count; index++)
            {
                DefaultImageOptions.Add(new ImageOption(index, $"{index + 1}. {SelectedImages[index].FileName}"));
            }

            _defaultImageIndex = 0;
            DefaultImagePicker.SelectedIndex = 0;
            DefaultImageContainer.IsVisible = true;
            SelectedImagesView.IsVisible = true;

            if (picks.Count() > MaxImages)
            {
                StatusLabel.TextColor = Colors.Red;
                StatusLabel.Text = $"Seules les {MaxImages} premieres images ont ete conservees.";
            }
        }
        catch (Exception ex)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = DisplayText.FirstNonEmpty(DisplayText.ToExcerpt(ex.Message, 180), "Impossible de selectionner les images.");
        }
    }

    private void OnDefaultImageChanged(object? sender, EventArgs e)
    {
        _defaultImageIndex = DefaultImagePicker.SelectedIndex >= 0 ? DefaultImagePicker.SelectedIndex : null;
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = await _categoriesApiClient.GetCategoriesAsync(CancellationToken.None);
            Categories.Clear();

            foreach (var category in categories.OrderBy(c => c.Name))
            {
                Categories.Add(category);
            }
        }
        catch (ApiException ex)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = UserFeedback.FromApiException(ex, "Impossible de charger les categories pour le moment.");
        }
        catch (TimeoutException ex)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = UserFeedback.FromTimeout(ex);
        }
        catch (Exception)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = UserFeedback.FromUnexpected("Impossible de charger les categories pour le moment.");
        }
    }

    private async Task NavigateBackAsync()
    {
        if (Shell.Current is null)
            return;

        try
        {
            if (Shell.Current.Navigation.NavigationStack.Count > 1)
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        }
        catch (Exception)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = UserFeedback.BackNavigationError;
        }
    }

    private static async Task<ImageItem> LoadImageAsync(FileResult pick)
    {
        await using var stream = await pick.OpenReadAsync();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);

        if (memory.Length > MaxImageSizeBytes)
        {
            throw new InvalidOperationException("Chaque image doit faire moins de 5 Mo.");
        }

        var contentType = string.IsNullOrWhiteSpace(pick.ContentType) ? "image/*" : pick.ContentType;
        if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Seules les images sont autorisees.");
        }

        return new ImageItem(
            pick.FileName,
            $"({Math.Round(memory.Length / 1024d, 1)} Ko)",
            new SelectedImageUpload(pick.FileName, contentType, memory.ToArray(), memory.Length));
    }

    public sealed record ImageItem(string FileName, string Description, SelectedImageUpload Upload);
    public sealed record ImageOption(int Index, string Label);
}
