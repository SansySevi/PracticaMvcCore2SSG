using Microsoft.AspNetCore.Mvc;
using PracticaMvcCore2SSG.Models;
using PracticaMvcCore2SSG.Repositories;

namespace PracticaMvcCore2SSG.ViewComponents
{
    public class MenuGenerosViewComponent : ViewComponent
    {

        private LibrosRepository repo;

        public MenuGenerosViewComponent( LibrosRepository repo)
        {
            this.repo = repo;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<Genero> generos = await this.repo.GetGeneros();
            return View(generos);

        }
    }
}
