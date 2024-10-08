using System;
using AvaloniaApplication1.Models.Interfaces;

namespace AvaloniaApplication1.ViewModels;

public record LinkGridItem(int ID, string Title, string Category, DateTime? LastDate) : IGridItem;
