namespace NutriCam.Helpers;

/// <summary>Raccourci pour déclarer les BindableProperty des contrôles personnalisés.</summary>
public static class Bind
{
    public static BindableProperty Prop<TOwner, T>(
        string name,
        T defaultValue,
        Action<TOwner, T>? changed = null,
        BindingMode mode = BindingMode.OneWay)
        where TOwner : BindableObject
    {
        return BindableProperty.Create(
            name,
            typeof(T),
            typeof(TOwner),
            defaultValue,
            mode,
            propertyChanged: (bindable, _, newValue) =>
                changed?.Invoke((TOwner)bindable, newValue is T typed ? typed : default!));
    }
}
