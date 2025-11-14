using System;
using WebLinks.Models.Interfaces;

namespace WebLinks.ViewModels;

public record LinkGridItem(int ID, string Title, string Category, DateTime? LastDate) : IGridItem;
