namespace NutriCam.Models;

/// <summary>Une séance proposée : jour, activité, consigne et durée.</summary>
public record ActivitySession(string Day, string Title, string Detail, string Duration);

/// <summary>Proposition d'activité physique pour la semaine, déterminée par le système selon l'objectif et le niveau d'activité.</summary>
public record ActivityPlan(
    int Sessions,
    int MinutesPerSession,
    int TotalMinutes,
    IReadOnlyList<ActivitySession> Days,
    string Note);
