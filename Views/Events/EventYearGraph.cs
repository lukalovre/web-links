using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using WebLinks.ViewModels;

namespace WebLinks.Views;

public class EventYearGraph : Control
{
    public static readonly StyledProperty<IReadOnlyList<YearlyEventCount>?> ItemsProperty =
        AvaloniaProperty.Register<EventYearGraph, IReadOnlyList<YearlyEventCount>?>(nameof(Items));

    static EventYearGraph()
    {
        ItemsProperty.Changed.AddClassHandler<EventYearGraph>((control, _) => control.InvalidateVisual());
    }

    public IReadOnlyList<YearlyEventCount>? Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var items = Items;
        if (items is null || items.Count == 0 || Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        const double leftMargin = 34;
        const double rightMargin = 8;
        const double topMargin = 8;
        const double bottomMargin = 28;
        var chartWidth = Bounds.Width - leftMargin - rightMargin;
        var chartHeight = Bounds.Height - topMargin - bottomMargin;
        var maxCount = Math.Max(1, MaxCount(items));
        var slotWidth = chartWidth / items.Count;
        var barWidth = Math.Max(4, slotWidth * 0.62);
        var axisBrush = new SolidColorBrush(Color.Parse("#090a0e"));
        var barBrush = new SolidColorBrush(Color.Parse("#d8513f"));
        var textBrush = new SolidColorBrush(Color.Parse("#1F2937"));
        var axisPen = new Pen(axisBrush, 1);

        context.DrawLine(axisPen, new Point(leftMargin, topMargin), new Point(leftMargin, topMargin + chartHeight));
        context.DrawLine(axisPen, new Point(leftMargin, topMargin + chartHeight), new Point(Bounds.Width - rightMargin, topMargin + chartHeight));

        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var barHeight = chartHeight * item.Count / (double)maxCount;
            var x = leftMargin + index * slotWidth + (slotWidth - barWidth) / 2;
            var y = topMargin + chartHeight - barHeight;

            context.FillRectangle(barBrush, new Rect(x, y, barWidth, barHeight));
            DrawText(context, textBrush, item.Count.ToString(CultureInfo.InvariantCulture), x + barWidth / 2, Math.Max(topMargin, y - 16), 12, true);
            DrawText(context, textBrush, item.Year.ToString(CultureInfo.InvariantCulture), x + barWidth / 2, topMargin + chartHeight + 6, 11, false);
        }
    }

    private static int MaxCount(IReadOnlyList<YearlyEventCount> items)
    {
        var maxCount = 0;
        foreach (var item in items)
        {
            maxCount = Math.Max(maxCount, item.Count);
        }

        return maxCount;
    }

    private static void DrawText(DrawingContext context, IBrush brush, string text, double centerX, double y, double fontSize, bool bold)
    {
        var typeface = new Typeface("Inter", FontStyle.Normal, bold ? FontWeight.Bold : FontWeight.Normal);
        var formattedText = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, fontSize, brush);
        context.DrawText(formattedText, new Point(centerX - formattedText.Width / 2, y));
    }
}
