using Microsoft.AspNetCore.Mvc;

namespace GestionBibliotheque.Services;

public interface ILayoutService
{
    Task PopulateAsync(Controller controller, string activePage);
}
