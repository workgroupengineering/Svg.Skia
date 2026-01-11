// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Svg.Skia.TypefaceProviders;

namespace Svg.Skia;

/// <summary>
/// Asset loader implementation using SkiaSharp types.
/// </summary>
public class SkiaSvgAssetLoader : Model.ISvgAssetLoader
{
    private readonly struct MatchCharacterKey : System.IEquatable<MatchCharacterKey>
    {
        public MatchCharacterKey(
            string? familyName,
            SkiaSharp.SKFontStyleWeight weight,
            SkiaSharp.SKFontStyleWidth width,
            SkiaSharp.SKFontStyleSlant slant,
            int codepoint)
        {
            FamilyName = familyName;
            Weight = weight;
            Width = width;
            Slant = slant;
            Codepoint = codepoint;
        }

        public string? FamilyName { get; }
        public SkiaSharp.SKFontStyleWeight Weight { get; }
        public SkiaSharp.SKFontStyleWidth Width { get; }
        public SkiaSharp.SKFontStyleSlant Slant { get; }
        public int Codepoint { get; }

        public bool Equals(MatchCharacterKey other)
        {
            return string.Equals(FamilyName, other.FamilyName, System.StringComparison.Ordinal)
                && Weight == other.Weight
                && Width == other.Width
                && Slant == other.Slant
                && Codepoint == other.Codepoint;
        }

        public override bool Equals(object? obj)
        {
            return obj is MatchCharacterKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = FamilyName?.GetHashCode() ?? 0;
                hash = (hash * 397) ^ (int)Weight;
                hash = (hash * 397) ^ (int)Width;
                hash = (hash * 397) ^ (int)Slant;
                hash = (hash * 397) ^ Codepoint;
                return hash;
            }
        }
    }

    private readonly struct ProviderTypefaceKey : System.IEquatable<ProviderTypefaceKey>
    {
        public ProviderTypefaceKey(
            ITypefaceProvider provider,
            string familyName,
            SkiaSharp.SKFontStyleWeight weight,
            SkiaSharp.SKFontStyleWidth width,
            SkiaSharp.SKFontStyleSlant slant)
        {
            Provider = provider;
            FamilyName = familyName;
            Weight = weight;
            Width = width;
            Slant = slant;
        }

        public ITypefaceProvider Provider { get; }
        public string FamilyName { get; }
        public SkiaSharp.SKFontStyleWeight Weight { get; }
        public SkiaSharp.SKFontStyleWidth Width { get; }
        public SkiaSharp.SKFontStyleSlant Slant { get; }

        public bool Equals(ProviderTypefaceKey other)
        {
            return ReferenceEquals(Provider, other.Provider)
                && string.Equals(FamilyName, other.FamilyName, System.StringComparison.Ordinal)
                && Weight == other.Weight
                && Width == other.Width
                && Slant == other.Slant;
        }

        public override bool Equals(object? obj)
        {
            return obj is ProviderTypefaceKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = RuntimeHelpers.GetHashCode(Provider);
                hash = (hash * 397) ^ (FamilyName?.GetHashCode() ?? 0);
                hash = (hash * 397) ^ (int)Weight;
                hash = (hash * 397) ^ (int)Width;
                hash = (hash * 397) ^ (int)Slant;
                return hash;
            }
        }
    }

    private readonly struct PaintSignature : System.IEquatable<PaintSignature>
    {
        public PaintSignature(ShimSkiaSharp.SKPaint paint)
        {
            Style = paint.Style;
            IsAntialias = paint.IsAntialias;
            StrokeWidth = paint.StrokeWidth;
            StrokeCap = paint.StrokeCap;
            StrokeJoin = paint.StrokeJoin;
            StrokeMiter = paint.StrokeMiter;
            TextSize = paint.TextSize;
            TextAlign = paint.TextAlign;
            LcdRenderText = paint.LcdRenderText;
            SubpixelText = paint.SubpixelText;
            TextEncoding = paint.TextEncoding;
            BlendMode = paint.BlendMode;
            FilterQuality = paint.FilterQuality;
            if (paint.Typeface is { } typeface)
            {
                HasTypeface = true;
                TypefaceFamilyName = typeface.FamilyName;
                TypefaceWeight = typeface.FontWeight;
                TypefaceWidth = typeface.FontWidth;
                TypefaceSlant = typeface.FontSlant;
            }
            else
            {
                HasTypeface = false;
                TypefaceFamilyName = null;
                TypefaceWeight = default;
                TypefaceWidth = default;
                TypefaceSlant = default;
            }
            Shader = paint.Shader;
            ColorFilter = paint.ColorFilter;
            ImageFilter = paint.ImageFilter;
            PathEffect = paint.PathEffect;

            if (paint.Color is { } color)
            {
                HasColor = true;
                Color = color;
            }
            else
            {
                HasColor = false;
                Color = default;
            }
        }

        public ShimSkiaSharp.SKPaintStyle Style { get; }
        public bool IsAntialias { get; }
        public float StrokeWidth { get; }
        public ShimSkiaSharp.SKStrokeCap StrokeCap { get; }
        public ShimSkiaSharp.SKStrokeJoin StrokeJoin { get; }
        public float StrokeMiter { get; }
        public float TextSize { get; }
        public ShimSkiaSharp.SKTextAlign TextAlign { get; }
        public bool LcdRenderText { get; }
        public bool SubpixelText { get; }
        public ShimSkiaSharp.SKTextEncoding TextEncoding { get; }
        public ShimSkiaSharp.SKBlendMode BlendMode { get; }
        public ShimSkiaSharp.SKFilterQuality FilterQuality { get; }
        public bool HasTypeface { get; }
        public string? TypefaceFamilyName { get; }
        public ShimSkiaSharp.SKFontStyleWeight TypefaceWeight { get; }
        public ShimSkiaSharp.SKFontStyleWidth TypefaceWidth { get; }
        public ShimSkiaSharp.SKFontStyleSlant TypefaceSlant { get; }
        public ShimSkiaSharp.SKShader? Shader { get; }
        public ShimSkiaSharp.SKColorFilter? ColorFilter { get; }
        public ShimSkiaSharp.SKImageFilter? ImageFilter { get; }
        public ShimSkiaSharp.SKPathEffect? PathEffect { get; }
        public bool HasColor { get; }
        public ShimSkiaSharp.SKColor Color { get; }

        public bool Equals(PaintSignature other)
        {
            return Style == other.Style
                && IsAntialias == other.IsAntialias
                && StrokeWidth.Equals(other.StrokeWidth)
                && StrokeCap == other.StrokeCap
                && StrokeJoin == other.StrokeJoin
                && StrokeMiter.Equals(other.StrokeMiter)
                && TextSize.Equals(other.TextSize)
                && TextAlign == other.TextAlign
                && LcdRenderText == other.LcdRenderText
                && SubpixelText == other.SubpixelText
                && TextEncoding == other.TextEncoding
                && BlendMode == other.BlendMode
                && FilterQuality == other.FilterQuality
                && HasTypeface == other.HasTypeface
                && string.Equals(TypefaceFamilyName, other.TypefaceFamilyName, StringComparison.Ordinal)
                && TypefaceWeight == other.TypefaceWeight
                && TypefaceWidth == other.TypefaceWidth
                && TypefaceSlant == other.TypefaceSlant
                && ReferenceEquals(Shader, other.Shader)
                && ReferenceEquals(ColorFilter, other.ColorFilter)
                && ReferenceEquals(ImageFilter, other.ImageFilter)
                && ReferenceEquals(PathEffect, other.PathEffect)
                && HasColor == other.HasColor
                && (!HasColor || (Color.Red == other.Color.Red
                    && Color.Green == other.Color.Green
                    && Color.Blue == other.Color.Blue
                    && Color.Alpha == other.Color.Alpha));
        }

        public override bool Equals(object? obj)
        {
            return obj is PaintSignature other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = (int)Style;
                hash = (hash * 397) ^ (IsAntialias ? 1 : 0);
                hash = (hash * 397) ^ StrokeWidth.GetHashCode();
                hash = (hash * 397) ^ (int)StrokeCap;
                hash = (hash * 397) ^ (int)StrokeJoin;
                hash = (hash * 397) ^ StrokeMiter.GetHashCode();
                hash = (hash * 397) ^ TextSize.GetHashCode();
                hash = (hash * 397) ^ (int)TextAlign;
                hash = (hash * 397) ^ (LcdRenderText ? 1 : 0);
                hash = (hash * 397) ^ (SubpixelText ? 1 : 0);
                hash = (hash * 397) ^ (int)TextEncoding;
                hash = (hash * 397) ^ (int)BlendMode;
                hash = (hash * 397) ^ (int)FilterQuality;
                hash = (hash * 397) ^ (HasTypeface ? 1 : 0);
                hash = (hash * 397) ^ (TypefaceFamilyName?.GetHashCode() ?? 0);
                hash = (hash * 397) ^ (int)TypefaceWeight;
                hash = (hash * 397) ^ (int)TypefaceWidth;
                hash = (hash * 397) ^ (int)TypefaceSlant;
                hash = (hash * 397) ^ (Shader is null ? 0 : RuntimeHelpers.GetHashCode(Shader));
                hash = (hash * 397) ^ (ColorFilter is null ? 0 : RuntimeHelpers.GetHashCode(ColorFilter));
                hash = (hash * 397) ^ (ImageFilter is null ? 0 : RuntimeHelpers.GetHashCode(ImageFilter));
                hash = (hash * 397) ^ (PathEffect is null ? 0 : RuntimeHelpers.GetHashCode(PathEffect));
                hash = (hash * 397) ^ (HasColor ? 1 : 0);
                if (HasColor)
                {
                    hash = (hash * 397) ^ Color.Red;
                    hash = (hash * 397) ^ Color.Green;
                    hash = (hash * 397) ^ Color.Blue;
                    hash = (hash * 397) ^ Color.Alpha;
                }

                return hash;
            }
        }
    }

    private sealed class CachedSkPaint
    {
        public CachedSkPaint(PaintSignature signature, SkiaSharp.SKPaint paint)
        {
            Signature = signature;
            Paint = paint;
        }

        public PaintSignature Signature { get; }
        public SkiaSharp.SKPaint Paint { get; }

        public void Dispose()
        {
            if (Paint.Handle != IntPtr.Zero)
            {
                Paint.Dispose();
            }
        }
    }

    private readonly SkiaModel _skiaModel;
    private const int MatchCharacterCacheLimit = 4096;
    private const int ProviderTypefaceCacheLimit = 512;
    private const int PaintCacheRefTrimThreshold = 1024;
    private readonly ConcurrentDictionary<MatchCharacterKey, SkiaSharp.SKTypeface?> _matchCharacterCache = new();
    private readonly ConcurrentDictionary<ProviderTypefaceKey, SkiaSharp.SKTypeface?> _providerTypefaceCache = new();
    private readonly object _paintCacheLock = new();
    private ConditionalWeakTable<ShimSkiaSharp.SKPaint, CachedSkPaint> _paintCache = new();
    private readonly List<WeakReference<SkiaSharp.SKPaint>> _paintCacheRefs = new();
    private IList<ITypefaceProvider>? _providerStateList;
    private int _providerStateHash;

    /// <summary>
    /// Initializes a new instance of <see cref="SkiaSvgAssetLoader"/>.
    /// </summary>
    /// <param name="skiaModel">Model used to convert font data.</param>
    public SkiaSvgAssetLoader(SkiaModel skiaModel)
    {
        _skiaModel = skiaModel;
    }

    /// <inheritdoc />
    public ShimSkiaSharp.SKImage LoadImage(System.IO.Stream stream)
    {
        var data = ShimSkiaSharp.SKImage.FromStream(stream);
        using var image = SkiaSharp.SKImage.FromEncodedData(data);
        return new ShimSkiaSharp.SKImage { Data = data, Width = image.Width, Height = image.Height };
    }

    /// <inheritdoc />
    public List<Model.TypefaceSpan> FindTypefaces(string? text, ShimSkiaSharp.SKPaint paintPreferredTypeface)
    {
        var ret = new List<Model.TypefaceSpan>();

        if (text is null || string.IsNullOrEmpty(text))
        {
            return ret;
        }

        EnsureTypefaceProviderCaches();

        var preferredTypeface = paintPreferredTypeface.Typeface;
        var weight = _skiaModel.ToSKFontStyleWeight(preferredTypeface?.FontWeight ?? ShimSkiaSharp.SKFontStyleWeight.Normal);
        var width = _skiaModel.ToSKFontStyleWidth(preferredTypeface?.FontWidth ?? ShimSkiaSharp.SKFontStyleWidth.Normal);
        var slant = _skiaModel.ToSKFontStyleSlant(preferredTypeface?.FontSlant ?? ShimSkiaSharp.SKFontStyleSlant.Upright);
        var preferredFamily = preferredTypeface?.FamilyName;
        System.Func<int, SkiaSharp.SKTypeface?> matchCharacter = codepoint =>
            MatchCharacter(preferredFamily, weight, width, slant, codepoint);

        using var runningPaint = _skiaModel.ToSKPaint(paintPreferredTypeface);
        if (runningPaint is null)
        {
            return ret;
        }

        var currentTypefaceStartIndex = 0;
        var i = 0;

        void YieldCurrentTypefaceText()
        {
            var currentTypefaceText = text.Substring(currentTypefaceStartIndex, i - currentTypefaceStartIndex);

            ret.Add(new(currentTypefaceText, runningPaint.MeasureText(currentTypefaceText),
                runningPaint.Typeface is null
                    ? null
                    : ShimSkiaSharp.SKTypeface.FromFamilyName(
                        runningPaint.Typeface.FamilyName,
                        // SkiaSharp provides int properties here. Let's just assume our
                        // ShimSkiaSharp defines the same values as SkiaSharp and convert directly
                        (ShimSkiaSharp.SKFontStyleWeight)runningPaint.Typeface.FontWeight,
                        (ShimSkiaSharp.SKFontStyleWidth)runningPaint.Typeface.FontWidth,
                        (ShimSkiaSharp.SKFontStyleSlant)runningPaint.Typeface.FontSlant)
            ));
        }

        for (; i < text.Length; i++)
        {
            var typeface = matchCharacter(char.ConvertToUtf32(text, i));

            if (i == 0)
            {
                runningPaint.Typeface = typeface;
            }
            else if (runningPaint.Typeface is null
                     && typeface is { } || runningPaint.Typeface is { }
                     && typeface is null || runningPaint.Typeface is { } l
                     && typeface is { } r
                     && (l.FamilyName, l.FontWeight, l.FontWidth, l.FontSlant) != (r.FamilyName, r.FontWeight, r.FontWidth, r.FontSlant))
            {
                YieldCurrentTypefaceText();

                currentTypefaceStartIndex = i;
                runningPaint.Typeface = typeface;
            }

            if (char.IsHighSurrogate(text[i]))
            {
                i++;
            }
        }

        YieldCurrentTypefaceText();

        return ret;
    }

    /// <inheritdoc />
    public ShimSkiaSharp.SKFontMetrics GetFontMetrics(ShimSkiaSharp.SKPaint paint)
    {
        using var skPaint = _skiaModel.ToSKPaint(paint);
        if (skPaint is null)
        {
            return default;
        }

        skPaint.GetFontMetrics(out var skMetrics);
        return new ShimSkiaSharp.SKFontMetrics
        {
            Top = skMetrics.Top,
            Ascent = skMetrics.Ascent,
            Descent = skMetrics.Descent,
            Bottom = skMetrics.Bottom,
            Leading = skMetrics.Leading
        };
    }

    /// <inheritdoc />
    public float MeasureText(string? text, ShimSkiaSharp.SKPaint paint, ref ShimSkiaSharp.SKRect bounds)
    {
        var skPaint = GetCachedPaint(paint);
        if (skPaint is null || text is null)
        {
            bounds = default;
            return 0f;
        }

        var skBounds = new SkiaSharp.SKRect();
        var width = skPaint.MeasureText(text, ref skBounds);
        bounds = new ShimSkiaSharp.SKRect(skBounds.Left, skBounds.Top, skBounds.Right, skBounds.Bottom);
        return width;
    }

    /// <inheritdoc />
    public ShimSkiaSharp.SKPath? GetTextPath(string? text, ShimSkiaSharp.SKPaint paint, float x, float y)
    {
        var skPaint = GetCachedPaint(paint);
        if (skPaint is null || text is null)
        {
            return null;
        }

        using var skPath = skPaint.GetTextPath(text, x, y);
        return _skiaModel.FromSKPath(skPath);
    }

    private void EnsureTypefaceProviderCaches()
    {
        var providers = _skiaModel.Settings.TypefaceProviders;
        var hash = ComputeTypefaceProviderHash(providers);
        if (!ReferenceEquals(providers, _providerStateList) || hash != _providerStateHash)
        {
            _providerStateList = providers;
            _providerStateHash = hash;
            _matchCharacterCache.Clear();
            _providerTypefaceCache.Clear();
            ClearPaintCache();
        }
    }

    private static int ComputeTypefaceProviderHash(IList<ITypefaceProvider>? providers)
    {
        unchecked
        {
            var hash = 17;
            if (providers is null)
            {
                return hash;
            }

            hash = (hash * 397) ^ providers.Count;
            for (var i = 0; i < providers.Count; i++)
            {
                var provider = providers[i];
                if (provider is null)
                {
                    continue;
                }

                hash = (hash * 397) ^ RuntimeHelpers.GetHashCode(provider);
                hash = (hash * 397) ^ provider.GetHashCode();
                if (provider is CustomTypefaceProvider custom)
                {
                    hash = (hash * 397) ^ (custom.Typeface?.Handle.GetHashCode() ?? 0);
                }
                else if (provider is FontManagerTypefaceProvider fontManagerProvider)
                {
                    hash = (hash * 397) ^ (fontManagerProvider.FontManager?.Handle.GetHashCode() ?? 0);
                }
            }

            return hash;
        }
    }

    private void ClearPaintCache()
    {
        lock (_paintCacheLock)
        {
            foreach (var weak in _paintCacheRefs)
            {
                if (weak.TryGetTarget(out var paint) && paint.Handle != IntPtr.Zero)
                {
                    paint.Dispose();
                }
            }

            _paintCacheRefs.Clear();
            _paintCache = new ConditionalWeakTable<ShimSkiaSharp.SKPaint, CachedSkPaint>();
        }
    }

    private void TrimPaintCacheRefsIfNeeded()
    {
        if (_paintCacheRefs.Count <= PaintCacheRefTrimThreshold)
        {
            return;
        }

        for (var i = _paintCacheRefs.Count - 1; i >= 0; i--)
        {
            var weak = _paintCacheRefs[i];
            if (!weak.TryGetTarget(out var paint) || paint.Handle == IntPtr.Zero)
            {
                _paintCacheRefs.RemoveAt(i);
            }
        }
    }

    private SkiaSharp.SKPaint? GetCachedPaint(ShimSkiaSharp.SKPaint paint)
    {
        EnsureTypefaceProviderCaches();

        var signature = new PaintSignature(paint);
        lock (_paintCacheLock)
        {
            if (_paintCache.TryGetValue(paint, out var cached))
            {
                if (cached.Paint.Handle != IntPtr.Zero && cached.Signature.Equals(signature))
                {
                    return cached.Paint;
                }

                cached.Dispose();
                _paintCache.Remove(paint);
            }

            var skPaint = _skiaModel.ToSKPaint(paint);
            if (skPaint is null)
            {
                return null;
            }

            _paintCache.Add(paint, new CachedSkPaint(signature, skPaint));
            _paintCacheRefs.Add(new WeakReference<SkiaSharp.SKPaint>(skPaint));
            TrimPaintCacheRefsIfNeeded();
            return skPaint;
        }
    }

    private void TrimCachesIfNeeded()
    {
        if (_matchCharacterCache.Count > MatchCharacterCacheLimit)
        {
            _matchCharacterCache.Clear();
        }

        if (_providerTypefaceCache.Count > ProviderTypefaceCacheLimit)
        {
            _providerTypefaceCache.Clear();
        }
    }

    private SkiaSharp.SKTypeface? MatchCharacter(
        string? familyName,
        SkiaSharp.SKFontStyleWeight weight,
        SkiaSharp.SKFontStyleWidth width,
        SkiaSharp.SKFontStyleSlant slant,
        int codepoint)
    {
        var normalizedFamily = familyName;
        var key = new MatchCharacterKey(normalizedFamily, weight, width, slant, codepoint);
        if (_matchCharacterCache.TryGetValue(key, out var cached))
        {
            if (cached is not null && cached.Handle != IntPtr.Zero)
            {
                return cached;
            }

            _matchCharacterCache.TryRemove(key, out _);
        }

        var typeface = TryMatchCharacterFromCustomProviders(normalizedFamily, weight, width, slant, codepoint);
        if (typeface is null)
        {
            typeface = normalizedFamily is null
                ? SkiaSharp.SKFontManager.Default.MatchCharacter(codepoint)
                : SkiaSharp.SKFontManager.Default.MatchCharacter(
                    normalizedFamily,
                    weight,
                    width,
                    slant,
                    null,
                    codepoint);
        }

        if (typeface is { } && typeface.Handle == IntPtr.Zero)
        {
            typeface = null;
        }

        _matchCharacterCache.TryAdd(key, typeface);
        TrimCachesIfNeeded();
        return typeface;
    }

    private SkiaSharp.SKTypeface? GetProviderTypeface(
        ITypefaceProvider provider,
        string familyName,
        SkiaSharp.SKFontStyleWeight weight,
        SkiaSharp.SKFontStyleWidth width,
        SkiaSharp.SKFontStyleSlant slant)
    {
        var key = new ProviderTypefaceKey(provider, familyName, weight, width, slant);
        if (_providerTypefaceCache.TryGetValue(key, out var cached))
        {
            if (cached is not null && cached.Handle != IntPtr.Zero)
            {
                return cached;
            }

            _providerTypefaceCache.TryRemove(key, out _);
        }

        var typeface = provider.FromFamilyName(familyName, weight, width, slant);
        if (typeface is { } && typeface.Handle == IntPtr.Zero)
        {
            typeface = null;
        }
        _providerTypefaceCache.TryAdd(key, typeface);
        TrimCachesIfNeeded();
        return typeface;
    }

    /// <summary>
    /// Attempts to find a typeface from custom providers that can render the specified character.
    /// </summary>
    /// <param name="familyName">The preferred font family name.</param>
    /// <param name="weight">The font weight.</param>
    /// <param name="width">The font width.</param>
    /// <param name="slant">The font slant.</param>
    /// <param name="codepoint">The character codepoint to match.</param>
    /// <returns>A matching typeface from custom providers, or null if none found.</returns>
    private SkiaSharp.SKTypeface? TryMatchCharacterFromCustomProviders(string? familyName, SkiaSharp.SKFontStyleWeight weight, SkiaSharp.SKFontStyleWidth width, SkiaSharp.SKFontStyleSlant slant, int codepoint)
    {
        if (_skiaModel.Settings.TypefaceProviders is null || _skiaModel.Settings.TypefaceProviders.Count == 0)
        {
            return null;
        }

        var familyKey = familyName ?? "Default";
        foreach (var provider in _skiaModel.Settings.TypefaceProviders)
        {
            var typeface = GetProviderTypeface(provider, familyKey, weight, width, slant);
            if (typeface is { } && typeface.ContainsGlyph(codepoint))
            {
                return typeface;
            }
        }

        return null;
    }
}
