using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using RESR.MAUI.Pages.Home;

namespace RESR.MAUI.Pages.Information;

public partial class InformationPage : ContentPage, IQueryAttributable
{
    public InformationPage()
    {
        InitializeComponent();
        RenderPage(InformationPageCatalog.GetPage("mentions-legales"));
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var key = query.TryGetValue("key", out var rawKey)
            ? Uri.UnescapeDataString(rawKey?.ToString() ?? string.Empty)
            : "mentions-legales";

        RenderPage(InformationPageCatalog.GetPage(key));
    }

    private void RenderPage(InformationPageDefinition page)
    {
        Title = page.Title;
        PageTitleLabel.Text = page.Title;
        PageCaptionLabel.Text = page.Description;
        UpdatedAtLabel.Text = $"Derniere mise a jour : {page.UpdatedAt}";
        SectionsContainer.Children.Clear();

        foreach (var section in page.Sections)
        {
            var sectionLayout = new VerticalStackLayout
            {
                Spacing = 10
            };

            sectionLayout.Children.Add(new Label
            {
                Text = section.Title,
                Style = (Style)Resources["SectionTitleStyle"]
            });

            foreach (var paragraph in section.Paragraphs)
            {
                sectionLayout.Children.Add(new Label
                {
                    Text = paragraph,
                    Style = (Style)Resources["BodyTextStyle"]
                });
            }

            if (section.Bullets is not null)
            {
                foreach (var bullet in section.Bullets)
                {
                    sectionLayout.Children.Add(new Label
                    {
                        Text = $"• {bullet}",
                        Style = (Style)Resources["BodyTextStyle"],
                        Margin = new Thickness(6, 0, 0, 0)
                    });
                }
            }

            SectionsContainer.Children.Add(new Border
            {
                BackgroundColor = Colors.White,
                Padding = new Thickness(18),
                Stroke = Color.FromArgb("#E5E0DA"),
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(24)
                },
                Content = sectionLayout
            });
        }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
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
        catch
        {
        }
    }
}
