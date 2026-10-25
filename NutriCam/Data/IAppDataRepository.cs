using NutriCam.Models;

namespace NutriCam.Data;

/// <summary>Persistance locale de l'état de l'application (compte, profil, préférences, programme).</summary>
public interface IAppDataRepository
{
    AppData Load();
    void Save(AppData data);
}
