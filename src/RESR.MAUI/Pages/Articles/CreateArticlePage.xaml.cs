using RESR.MAUI.Services;
using RESR.Models.Categories;
using RESR.Models.Resources;
using System.Collections.ObjectModel;
using System.Linq;

namespace RESR.MAUI.Pages.Articles;

public partial class CreateArticlePage : ContentPage
{
    private const int TitleMaxLength = 50;
    private const int DescriptionMaxLength = 5000;

    private readonly IArticlesApiClient _articlesApiClient;
    private readonly ICategoriesApiClient _categoriesApiClient;

    public ObservableCollection<CategoryResponse> Categories { get; } = new();

    public CreateArticlePage(IArticlesApiClient articlesApiClient, ICategoriesApiClient categoriesApiClient)
    {
        _articlesApiClient = articlesApiClient;
        _categoriesApiClient = categoriesApiClient;
        InitializeComponent();
        BindingContext = this;

        VisibilityPicker.ItemsSource = new[] { "PUBLIC", "PRIVATE" };
        VisibilityPicker.SelectedIndex = 0;
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

        try
        {
            StatusLabel.TextColor = Colors.Black;
            StatusLabel.Text = "Validation en cours...";

            var title = TitleEntry.Text?.Trim() ?? string.Empty;
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

            var descriptionHtml = DescriptionEditor.Text?.Trim() ?? string.Empty;
            var descriptionLength = descriptionHtml.Length;
            if (descriptionLength > DescriptionMaxLength)
            {
                StatusLabel.Text = $"La description ne doit pas depasser {DescriptionMaxLength} caracteres.";
                return;
            }

            var contentHtml = ContentEditor.Text?.Trim() ?? string.Empty;
            var contentLength = contentHtml.Length;
            if (contentLength == 0)
            {
                StatusLabel.Text = "Le contenu est obligatoire.";
                return;
            }

            if (CategoryPicker.SelectedItem is not CategoryResponse selectedCategory)
            {
                StatusLabel.Text = "Selectionne une categorie.";
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
                CancellationToken.None);

            StatusLabel.TextColor = Colors.Green;
            StatusLabel.Text = "Article cree avec succes.";

            TitleEntry.Text = string.Empty;
            CategoryPicker.SelectedItem = null;
            DescriptionEditor.Text = string.Empty;
            ContentEditor.Text = string.Empty;
            UpdateTitleCounter();
        }
        catch (ApiException ex)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = ex.Message;
        }
        catch (TimeoutException)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = "Le serveur ne repond pas. Reessaie plus tard.";
        }
        catch (Exception ex)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = "Une erreur est survenue lors de la creation.";
            System.Diagnostics.Debug.WriteLine($"Create article failed: {ex}");
        }
        finally
        {
            CreateButton.IsEnabled = true;
        }
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
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = $"Erreur categories ({(int)ex.StatusCode}): {ex.Message}";
        }
        catch (TimeoutException ex)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = ex.Message;
        }
        catch (Exception ex)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = $"Erreur inattendue: {ex.Message}";
        }
    }
}
