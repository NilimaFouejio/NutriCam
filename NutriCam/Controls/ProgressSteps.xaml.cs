using NutriCam.Helpers;

namespace NutriCam.Controls;

/// <summary>« VOTRE PROGRAMME · n / 5 » avec la barre de progression à 5 segments.</summary>
public partial class ProgressSteps : ContentView
{
    public static readonly BindableProperty StepProperty =
        Bind.Prop<ProgressSteps, int>(nameof(Step), 1, (c, _) => c.Update());

    public int Step { get => (int)GetValue(StepProperty); set => SetValue(StepProperty, value); }

    public ProgressSteps()
    {
        InitializeComponent();
        Update();
    }

    private void Update()
    {
        StepLabel.Text = $"VOTRE PROGRAMME · {Step} / 5";
        var bars = new[] { Bar1, Bar2, Bar3, Bar4, Bar5 };
        for (var i = 0; i < bars.Length; i++)
            bars[i].Color = i < Step ? Palette.Terracotta : Palette.Bordure;
    }
}
