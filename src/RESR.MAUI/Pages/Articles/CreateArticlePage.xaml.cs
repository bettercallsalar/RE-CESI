using RESR.MAUI.Pages.Home;
using RESR.MAUI.Services;
using RESR.Models.Categories;
using RESR.Models.Resources;
using System.Collections.ObjectModel;
using System.Linq;

namespace RESR.MAUI.Pages.Articles;

public partial class CreateArticlePage : ContentPage
{
    private static readonly Color MutedStatusColor = Color.FromArgb("#5F5F66");
    private static readonly Color ErrorStatusColor = Color.FromArgb("#AB231E");
    private static readonly Color SuccessStatusColor = Color.FromArgb("#1D6B43");

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
                CancellationToken.None);

            StatusLabel.TextColor = SuccessStatusColor;
            StatusLabel.Text = "Article cree avec succes.";

            TitleEntry.Text = string.Empty;
            CategoryPicker.SelectedItem = null;
            VisibilityPicker.SelectedIndex = 0;
            DescriptionEditor.Text = string.Empty;
            ContentEditor.Text = string.Empty;
            UpdateTitleCounter();
        }
        catch (ApiException ex)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = DisplayText.ToExcerpt(ex.Message, 180);
        }
        catch (TimeoutException)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = "Le serveur ne repond pas. Reessayez plus tard.";
        }
        catch (Exception ex)
        {
            StatusLabel.TextColor = ErrorStatusColor;
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
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = $"Erreur categories ({(int)ex.StatusCode}) : {DisplayText.ToExcerpt(ex.Message, 180)}";
        }
        catch (TimeoutException ex)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = ex.Message;
        }
        catch (Exception ex)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = $"Erreur inattendue : {DisplayText.ToExcerpt(ex.Message, 180)}";
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
        catch (Exception ex)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = $"Retour impossible : {DisplayText.ToExcerpt(ex.Message, 160)}";
        }
    }
}
