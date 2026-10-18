namespace NutriCam.ViewModels;

/// <summary>Une ligne du bilan : un jour, le nombre de repas mangés et la progression correspondante.</summary>
public record DayStat(string Day, string Count, double Progress);
