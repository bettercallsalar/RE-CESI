using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using RESR.MAUI.Services;
using RESR.Models.Categories;
using RESR.Models.Resources;

namespace RESR.MAUI.Pages.Events;

public partial class EditEventPage : ContentPage, IQueryAttributable
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
    private int _idResource;
    private bool _isLoaded;
    private bool _useOwnAccess;

    public ObservableCollection<CategoryResponse> Categories { get; } = new();
    public ObservableCollection<DepartmentOption> Departments { get; } = new();
    public ObservableCollection<ImageItem> SelectedImages { get; } = new();
    public ObservableCollection<ImageOption> DefaultImageOptions { get; } = new();
    private int? _defaultImageIndex;

    public EditEventPage(IEventsApiClient eventsApiClient, ICategoriesApiClient categoriesApiClient, IDepartmentsApiClient departmentsApiClient)
    {
        _eventsApiClient = eventsApiClient;
        _categoriesApiClient = categoriesApiClient;
        _departmentsApiClient = departmentsApiClient;
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
        if (_isLoaded || _idResource <= 0) return;
        _isLoaded = true;
        await LoadPageAsync();
    }

    private async Task LoadPageAsync()
    {
        try
        {
            StatusLabel.Text = "Chargement de l'evenement...";
            var categoriesTask = _categoriesApiClient.GetCategoriesAsync(CancellationToken.None);
            var departmentsTask = _departmentsApiClient.GetDepartmentsAsync(CancellationToken.None);
            var eventTask = _useOwnAccess
                ? _eventsApiClient.GetOwnByIdAsync(_idResource, CancellationToken.None)
                : _eventsApiClient.GetByIdAsync(_idResource, CancellationToken.None);
            await Task.WhenAll(categoriesTask, departmentsTask, eventTask);

            Categories.Clear();
            foreach (var category in categoriesTask.Result.OrderBy(c => c.Name)) Categories.Add(category);
            Departments.Clear();
            foreach (var department in departmentsTask.Result.OrderBy(d => d.Code)) Departments.Add(new DepartmentOption(department.IdDepartment, department.Name, department.Code));

            var @event = eventTask.Result;
            TitleEntry.Text = @event.Title;
            SubtitleEntry.Text = @event.Subtitle ?? string.Empty;
            DescriptionEditor.Text = @event.Description ?? string.Empty;
            AddressEntry.Text = @event.Address ?? string.Empty;
            VisibilityPicker.SelectedItem = @event.Visibility;
            CategoryPicker.SelectedItem = Categories.FirstOrDefault(c => c.IdCategory == @event.IdCategory);
            DepartmentPicker.SelectedItem = Departments.FirstOrDefault(d => d.IdDepartment == @event.Department?.IdDepartment);
            StartDatePicker.Date = @event.StartDate;
            HasEndDateCheckBox.IsChecked = @event.EndDate.HasValue;
            EndDatePicker.IsEnabled = @event.EndDate.HasValue;
            EndDatePicker.Date = @event.EndDate ?? @event.StartDate.AddDays(1);
            UpdateTitleCounter();
            StatusLabel.Text = "Mettez a jour les champs puis enregistrez.";
        }
        catch (Exception ex)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = ex is ApiException apiEx
                ? UserFeedback.FromApiException(apiEx, "Impossible de charger l'evenement pour le moment.")
                : UserFeedback.FromUnexpected("Impossible de charger l'evenement pour le moment.");
        }
    }

    private void OnTitleChanged(object? sender, TextChangedEventArgs e) => UpdateTitleCounter();
    private void UpdateTitleCounter() => TitleCounterLabel.Text = $"{TitleEntry.Text?.Length ?? 0}/{TitleMaxLength}";

    private void OnHasEndDateChanged(object? sender, CheckedChangedEventArgs e)
    {
        EndDatePicker.IsEnabled = e.Value;
        var startDate = StartDatePicker.Date ?? DateTime.Today;
        var endDate = EndDatePicker.Date ?? startDate;
        if (e.Value && endDate <= startDate) EndDatePicker.Date = startDate.AddDays(1);
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        SaveButton.IsEnabled = false;
        PickImagesButton.IsEnabled = false;
        try
        {
            var title = TitleEntry.Text?.Trim() ?? string.Empty;
            var subtitle = SubtitleEntry.Text?.Trim() ?? string.Empty;
            var description = DescriptionEditor.Text?.Trim() ?? string.Empty;
            var address = AddressEntry.Text?.Trim() ?? string.Empty;
            if (title.Length < 3) { StatusLabel.Text = "Le titre doit contenir au moins 3 caracteres."; return; }
            if (title.Length > TitleMaxLength) { StatusLabel.Text = $"Le titre ne doit pas depasser {TitleMaxLength} caracteres."; return; }
            if (subtitle.Length > SubtitleMaxLength) { StatusLabel.Text = $"Le sous-titre ne doit pas depasser {SubtitleMaxLength} caracteres."; return; }
            if (description.Length > DescriptionMaxLength) { StatusLabel.Text = $"La description ne doit pas depasser {DescriptionMaxLength} caracteres."; return; }
            if (address.Length > AddressMaxLength) { StatusLabel.Text = $"L'adresse ne doit pas depasser {AddressMaxLength} caracteres."; return; }
            if (CategoryPicker.SelectedItem is not CategoryResponse category) { StatusLabel.Text = "Selectionnez une categorie."; return; }
            var startDate = StartDatePicker.Date ?? DateTime.Today;
            DateTime? endDate = HasEndDateCheckBox.IsChecked ? EndDatePicker.Date ?? startDate.AddDays(1) : null;
            if (endDate is not null && endDate <= startDate) { StatusLabel.Text = "La date de fin doit etre strictement apres la date de debut."; return; }
            var department = DepartmentPicker.SelectedItem as DepartmentOption;

            await _eventsApiClient.UpdateAsync(
                _idResource,
                new UpdateEventRequest(title, string.IsNullOrWhiteSpace(description) ? null : description, VisibilityPicker.SelectedItem?.ToString(), category.IdCategory, string.IsNullOrWhiteSpace(subtitle) ? null : subtitle, startDate, endDate, string.IsNullOrWhiteSpace(address) ? null : address, department?.IdDepartment),
                SelectedImages.Select(x => x.Upload).ToList(),
                _defaultImageIndex,
                CancellationToken.None);

            StatusLabel.TextColor = Colors.Green;
            StatusLabel.Text = "Evenement mis a jour avec succes.";
        }
        catch (Exception ex)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = ex is ApiException apiEx
                ? UserFeedback.FromApiException(apiEx, "La mise a jour de l'evenement a echoue.")
                : UserFeedback.FromUnexpected("La mise a jour de l'evenement a echoue.");
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
            StatusLabel.Text = DisplayText.FirstNonEmpty(DisplayText.ToExcerpt(ex.Message, 180), "Impossible de selectionner les images.");
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

    public sealed record DepartmentOption(int IdDepartment, string Name, int Code)
    {
        public string DisplayLabel => $"{Code} - {Name}";
    }
    public sealed record ImageItem(string FileName, string Description, SelectedImageUpload Upload);
    public sealed record ImageOption(int Index, string Label);
}
