using Microsoft.AspNetCore.Components;
using MudBlazor;
using Streamify.TMDB;

namespace Streamify.Utility;

public record Meta(int TmdbId, string Title, bool IsSeries, string? PosterPath, string? BackdropPath, string? Overview);

public static class PosterCardHelper
{
    public static RenderFragment PosterCard(
        ComponentBase receiver,
        string title,
        string? subtitle,
        Meta meta,
        ITmdbClient tmdb,
        Action onClick,
        double? progress = null,
        bool useBackdrop = false)
        => builder =>
        {
            var seq = 0;
            builder.OpenComponent<MudCard>(seq++);
            builder.AddAttribute(seq++, "Class", "d-flex flex-column");
            builder.AddAttribute(seq++, "Style", "height: 100%; cursor: pointer;");
            builder.AddAttribute(seq++, "Elevation", 2);
            builder.AddAttribute(seq++, "onclick", EventCallback.Factory.Create(receiver, onClick));
            builder.AddAttribute(seq++, "ChildContent", (RenderFragment)(b =>
            {
                var s = 0;

                // Image section with loading state and fallback
                b.OpenElement(s++, "div");
                b.AddAttribute(s++, "class", "position-relative");
                b.AddAttribute(s++, "style", "height: 300px; position: relative !important;");

                // Choose image path and type based on preference and availability
                var imagePath = useBackdrop && !string.IsNullOrEmpty(meta.BackdropPath)
                    ? meta.BackdropPath
                    : meta.PosterPath;

                var imageType = useBackdrop && !string.IsNullOrEmpty(meta.BackdropPath)
                    ? TmdbImageType.Backdrop
                    : TmdbImageType.Poster;

                // Get appropriate size based on container (300px height for cards)
                var imageSize = imageType == TmdbImageType.Poster
                    ? TmdbImageSize.W342
                    : TmdbImageSize.W780;

                var imageUrl = tmdb.GetImageUrl(imagePath, imageType, imageSize);

                if (!string.IsNullOrEmpty(imageUrl))
                {
                    b.OpenComponent<MudImage>(s++);
                    b.AddAttribute(s++, "Src", imageUrl);
                    b.AddAttribute(s++, "Alt", title);
                    b.AddAttribute(s++, "ObjectFit", ObjectFit.Cover);
                    b.AddAttribute(s++, "Class", "flex-grow-0 flex-shrink-0");
                    b.AddAttribute(s++, "Height", 300);
                    b.AddAttribute(s++, "Style", "width: 100%; transition: opacity 0.3s ease; border-radius: 12px;");
                    b.CloseComponent();
                }
                else
                {
                    b.OpenElement(s++, "div");
                    b.AddAttribute(s++, "class", "d-flex align-center justify-center");
                    b.AddAttribute(s++, "style", "height: 300px; background: linear-gradient(135deg, #f5f5f5 0%, #e0e0e0 100%); border-radius: 4px;");

                    b.OpenComponent<MudStack>(s++);
                    b.AddAttribute(s++, "AlignItems", AlignItems.Center);
                    b.AddAttribute(s++, "Spacing", 2);
                    b.AddAttribute(s++, "ChildContent", (RenderFragment)(fallback =>
                    {
                        fallback.OpenComponent<MudIcon>(0);
                        fallback.AddAttribute(1, "Icon", meta.IsSeries ? Icons.Material.Filled.Tv : Icons.Material.Filled.Movie);
                        fallback.AddAttribute(2, "Size", Size.Large);
                        fallback.AddAttribute(3, "Color", Color.Default);
                        fallback.CloseComponent();

                        fallback.OpenComponent<MudText>(4);
                        fallback.AddAttribute(5, "Typo", Typo.caption);
                        fallback.AddAttribute(6, "Align", Align.Center);
                        fallback.AddAttribute(7, "Color", Color.Default);
                        fallback.AddAttribute(8, "ChildContent", (RenderFragment)(t => t.AddContent(0, "Immagine non disponibile")));
                        fallback.CloseComponent();
                    }));
                    b.CloseComponent();
                    b.CloseElement();
                }

                // Progress bar overlay - positioned absolutely within the image container
                if (progress is > 0)
                {
                    b.OpenElement(s++, "div");
                    b.AddAttribute(s++, "style", "position: absolute !important; left: 8px; right: 8px; bottom: 8px; z-index: 1000;");
                    b.OpenComponent<MudProgressLinear>(s++);
                    b.AddAttribute(s++, "Value", progress * 100);
                    b.AddAttribute(s++, "Color", Color.Primary);
                    b.AddAttribute(s++, "Rounded", true);
                    b.AddAttribute(s++, "Size", Size.Medium);
                    b.CloseComponent();
                    b.CloseElement();
                }

                b.CloseElement(); // Close the positioned container div

            }));
            builder.CloseComponent();
        };
}