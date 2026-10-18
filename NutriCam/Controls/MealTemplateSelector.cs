using NutriCam.ViewModels;

namespace NutriCam.Controls;

/// <summary>Choisit la carte compacte ou la grande carte selon MealItem.IsFeatured.</summary>
public class MealTemplateSelector : DataTemplateSelector
{
    public DataTemplate? Compact { get; set; }
    public DataTemplate? Featured { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        => (item is MealItem { IsFeatured: true } ? Featured : Compact)!;
}
