using NutriCam.Helpers;

namespace NutriCam.Controls;

/// <summary>
/// Champ de saisie de la maquette : libellé, cadre blanc arrondi, icône (ou œil pour un mot de passe),
/// unité optionnelle (« ans », « cm », « kg ») et message d'erreur.
/// </summary>
public partial class FormField : ContentView
{
    private bool _hidden = true;

    public static readonly BindableProperty TitleProperty =
        Bind.Prop<FormField, string>(nameof(Title), "", (c, v) => c.TitleLabel.Text = v);

    public static readonly BindableProperty PlaceholderProperty =
        Bind.Prop<FormField, string>(nameof(Placeholder), "", (c, v) => c.Input.Placeholder = v);

    public static readonly BindableProperty TextProperty =
        Bind.Prop<FormField, string>(nameof(Text), "", (c, v) =>
        {
            if (c.Input.Text != v) c.Input.Text = v;
        }, BindingMode.TwoWay);

    public static readonly BindableProperty ErrorProperty =
        Bind.Prop<FormField, string>(nameof(Error), "", (c, _) => c.UpdateError());

    public static readonly BindableProperty SuffixProperty =
        Bind.Prop<FormField, string>(nameof(Suffix), "", (c, v) =>
        {
            c.SuffixLabel.Text = v;
            c.SuffixLabel.IsVisible = !string.IsNullOrEmpty(v);
        });

    public static readonly BindableProperty KeyboardTypeProperty =
        Bind.Prop<FormField, Keyboard>(nameof(KeyboardType), Keyboard.Default, (c, v) => c.Input.Keyboard = v);

    public static readonly BindableProperty IsPasswordFieldProperty =
        Bind.Prop<FormField, bool>(nameof(IsPasswordField), false, (c, _) => c.UpdateIcon());

    public static readonly BindableProperty LeadingGlyphProperty =
        Bind.Prop<FormField, string>(nameof(LeadingGlyph), "", (c, _) => c.UpdateIcon());

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string Placeholder { get => (string)GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }
    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public string Error { get => (string)GetValue(ErrorProperty); set => SetValue(ErrorProperty, value); }
    public string Suffix { get => (string)GetValue(SuffixProperty); set => SetValue(SuffixProperty, value); }
    public Keyboard KeyboardType { get => (Keyboard)GetValue(KeyboardTypeProperty); set => SetValue(KeyboardTypeProperty, value); }
    public bool IsPasswordField { get => (bool)GetValue(IsPasswordFieldProperty); set => SetValue(IsPasswordFieldProperty, value); }

    /// <summary>Icône affichée à droite du champ (ex. enveloppe pour l'e-mail).</summary>
    public string LeadingGlyph { get => (string)GetValue(LeadingGlyphProperty); set => SetValue(LeadingGlyphProperty, value); }

    public FormField()
    {
        InitializeComponent();
        Input.TextChanged += (_, e) => Text = e.NewTextValue ?? "";
        UpdateIcon();
    }

    /// <summary>Place le curseur de saisie dans le champ (ouvre le clavier).</summary>
    public void FocusInput() => Input.Focus();

    private void UpdateError()
    {
        var hasError = !string.IsNullOrWhiteSpace(Error);
        ErrorLabel.Text = Error;
        ErrorLabel.IsVisible = hasError;
        Box.Stroke = hasError ? Palette.Erreur : Palette.Bordure;
    }

    private void UpdateIcon()
    {
        if (IsPasswordField)
        {
            Input.IsPassword = _hidden;
            IconLabel.Text = _hidden ? Icons.Eye : Icons.EyeOff;
            IconLabel.IsVisible = true;
        }
        else
        {
            Input.IsPassword = false;
            IconLabel.Text = LeadingGlyph;
            IconLabel.IsVisible = !string.IsNullOrEmpty(LeadingGlyph);
        }
    }

    private void OnIconTapped(object? sender, TappedEventArgs e)
    {
        if (!IsPasswordField) return;
        _hidden = !_hidden;
        UpdateIcon();
    }
}
