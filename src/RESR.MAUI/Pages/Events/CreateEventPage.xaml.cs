using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using RESR.MAUI.Services;
using RESR.Models.Categories;
using RESR.Models.Resources;

namespace RESR.MAUI.Pages.Events;

public partial class CreateEventPage : ContentPage
{
    private const int TitleMaxLength = 50;
    private const int DescriptionMaxLength = 5000;
    private const int SubtitleMaxLength = 255;
    private const int AddressMaxLength = 255;
    private const int MaxImages = 6;
    private const long MaxImageSizeBytes = 5 * 1024 * 1024;

    private readonly IEventsApiClient _eventsApiClient;
    private readonly ICategoriesApiClient _categoriesApiClient;
    private readonly IDepartmentsApiClient _departmentsApiClient;

    public ObservableCollection<CategoryResponse> Categories { get; } = new();
    public ObservableCollection<DepartmentOption> Departments { get; } = new();
    public ObservableCollection<ImageItem> SelectedImages { get; } = new();
    public ObservableCollection<ImageOption> DefaultImageOptions { get; } = new();
    private int? _defaultImageIndex;

    public CreateEventPage(
        IEventsApiClient eventsApiClient,
        ICategoriesApiClient categoriesApiClient,
        IDepartmentsApiClient departmentsApiClient)
    {
        _eventsApiClient = eventsApiClient;
        _categoriesApiClient = categoriesApiClient;
        _departmentsApiClient = departmentsApiClient;

        InitializeComponent();
        BindingContext = this;
        SelectedImagesView.ItemsSource = SelectedImages;

        VisibilityPicker.ItemsSource = new[] { "PUBLIC", "PRIVATE" };
        VisibilityPicker.SelectedIndex = 0;
        StartDatePicker.Date = DateTime.Today;
        EndDatePicker.Date = DateTime.Today.AddDays(1);
        EndDatePicker.IsEnabled = false;
        UpdateTitleCounter();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (Categories.Count == 0 || Departments.Count == 0)
        {
            await LoadOptionsAsync();
        }
    }

    private void OnTitleChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateTitleCounter();
    }

    private void OnHasEndDateChanged(object? sender, CheckedChangedEventArgs e)
    {
        EndDatePicker.IsEnabled = e.Value;
        var startDate = StartDatePicker.Date ?? DateTime.Today;
        var endDate = EndDatePicker.Date ?? startDate;

        if (e.Value && endDate <= startDate)
        {
            EndDatePicker.Date = startDate.AddDays(1);
        }
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
            StatusLabel.TextColor = Colors.Black;
            StatusLabel.Text = "Validation en cours...";

            var title = TitleEntry.Text?.Trim() ?? string.Empty;
            var subtitle = SubtitleEntry.Text?.Trim() ?? string.Empty;
            var address = AddressEntry.Text?.Trim() ?? string.Empty;
            var description = DescriptionEditor.Text?.Trim() ?? string.Empty;

            if (title.Length < 3)
            {
                StatusLabel.Text = "Le titre doit contenir au moins 3 caracteres.";
                return;
            }

            if (title.Length > TitleMaxLength)
            {
                StatusLabel.Text = $"Le titre ne doit pas depasser {TitleMaxLength} caracteres.";
                return;
            }

            if (subtitle.Length > SubtitleMaxLength)
            {
                StatusLabel.Text = $"Le sous-titre ne doit pas depasser {SubtitleMaxLength} caracteres.";
                return;
            }

            if (address.Length > AddressMaxLength)
            {
                StatusLabel.Text = $"L'adresse ne doit pas depasser {AddressMaxLength} caracteres.";
                return;
            }

            if (description.Length > DescriptionMaxLength)
            {
                StatusLabel.Text = $"La description ne doit pas depasser {DescriptionMaxLength} caracteres.";
                return;
            }

            if (CategoryPicker.SelectedItem is not CategoryResponse selectedCategory)
            {
                StatusLabel.Text = "Selectionne une categorie.";
                return;
            }

            var startDate = StartDatePicker.Date ?? DateTime.Today;
            DateTime? endDate = HasEndDateCheckBox.IsChecked
                ? EndDatePicker.Date ?? startDate.AddDays(1)
                : null;

            if (endDate is not null && endDate <= startDate)
            {
                StatusLabel.Text = "La date de fin doit etre strictement apres la date de debut.";
                return;
            }

            var visibility = VisibilityPicker.SelectedItem?.ToString() ?? "PUBLIC";
            var selectedDepartment = DepartmentPicker.SelectedItem as DepartmentOption;

            await _eventsApiClient.CreateAsync(
                new CreateEventRequest(
                    title,
                    string.IsNullOrWhiteSpace(description) ? null : description,
                    visibility,
                    selectedCategory.IdCategory,
                    string.IsNullOrWhiteSpace(subtitle) ? null : subtitle,
                    startDate,
                    endDate,
                    string.IsNullOrWhiteSpace(address) ? null : address,
                    selectedDepartment?.IdDepartment),
                SelectedImages.Select(image => image.Upload).ToList(),
                _defaultImageIndex,
                CancellationToken.None);

            StatusLabel.TextColor = Colors.Green;
            StatusLabel.Text = "Evenement cree avec succes.";

            TitleEntry.Text = string.Empty;
            SubtitleEntry.Text = string.Empty;
            AddressEntry.Text = string.Empty;
            DescriptionEditor.Text = string.Empty;
            CategoryPicker.SelectedItem = null;
            DepartmentPicker.SelectedItem = null;
            VisibilityPicker.SelectedIndex = 0;
            StartDatePicker.Date = DateTime.Today;
            HasEndDateCheckBox.IsChecked = false;
            EndDatePicker.Date = DateTime.Today.AddDays(1);
            SelectedImages.Clear();
            DefaultImageOptions.Clear();
            DefaultImageContainer.IsVisible = false;
            SelectedImagesView.IsVisible = false;
            _defaultImageIndex = null;
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
            System.Diagnostics.Debug.WriteLine($"Create event failed: {ex}");
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
            StatusLabel.Text = $"Selection des images impossible: {ex.Message}";
        }
    }

    private void OnDefaultImageChanged(object? sender, EventArgs e)
    {
        _defaultImageIndex = DefaultImagePicker.SelectedIndex >= 0 ? DefaultImagePicker.SelectedIndex : null;
    }

    private async Task LoadOptionsAsync()
    {
        try
        {
            StatusLabel.TextColor = Colors.Black;
            StatusLabel.Text = "Chargement des options...";

            var categoriesTask = LoadCategoriesAsync();
            var departmentsTask = LoadDepartmentsAsync();

            await Task.WhenAll(categoriesTask, departmentsTask);

            Categories.Clear();
            foreach (var category in categoriesTask.Result.OrderBy(c => c.Name))
            {
                Categories.Add(category);
            }

            Departments.Clear();
            foreach (var department in departmentsTask.Result.OrderBy(d => d.Code))
            {
                Departments.Add(new DepartmentOption(department.IdDepartment, department.Name, department.Code));
            }

            StatusLabel.TextColor = Colors.Black;
            StatusLabel.Text = "Renseigne les champs obligatoires.";
        }
        catch (ApiException ex)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = $"Erreur options ({(int)ex.StatusCode}): {ex.Message}";
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

    private Task<IReadOnlyList<CategoryResponse>> LoadCategoriesAsync()
    {
        return _categoriesApiClient.GetCategoriesAsync(CancellationToken.None);
    }

    private async Task<IReadOnlyList<DepartmentOption>> LoadDepartmentsAsync()
    {
        var departments = await _departmentsApiClient.GetDepartmentsAsync(CancellationToken.None);
        return departments
            .Select(department => new DepartmentOption(department.IdDepartment, department.Name, department.Code))
            .ToList();
    }

    public sealed record DepartmentOption(int IdDepartment, string Name, int Code)
    {
        public string DisplayLabel => $"{Code} - {Name}";
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
