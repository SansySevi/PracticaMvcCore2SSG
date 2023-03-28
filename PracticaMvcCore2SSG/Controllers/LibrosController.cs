using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PracticaMvcCore2SSG.Extensions;
using PracticaMvcCore2SSG.Models;
using PracticaMvcCore2SSG.Repositories;
using System.Security.Claims;
using TiendaPractica.Filter;

namespace PracticaMvcCore2SSG.Controllers
{
    public class LibrosController : Controller
    {

        private LibrosRepository repo;

        public LibrosController(LibrosRepository repo)
        {
            this.repo = repo;
        }

        #region LOGIN/LOGOUT

        public IActionResult Login()
        {
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            Usuario usuario =
                await this.repo.FindUser(email, password);

            HttpContext.Session.SetString("USUARIO", usuario.Nombre);

            if (usuario != null)
            {
                ClaimsIdentity identity =
                    new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                Claim claimUsername =
                    new Claim(ClaimTypes.Name, usuario.Nombre.ToLower());
                Claim ClaimId = 
                    new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString());
                
                Claim claimRole =
                    new Claim(ClaimTypes.Role, "USUARIO");

                Claim claimEmail =
                    new Claim("Email", usuario.Email.ToString());
                Claim claimImg =
                    new Claim("Imagen", usuario.Foto.ToString());
                Claim claimApellido =
                    new Claim("Apellidos", usuario.Apellidos.ToString());

                identity.AddClaim(claimApellido);
                identity.AddClaim(claimImg);
                identity.AddClaim(claimEmail);
                identity.AddClaim(claimUsername);
                identity.AddClaim(claimRole);
                identity.AddClaim(ClaimId);

                ClaimsPrincipal userPrincipal =
                    new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync
                    (CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal);

                string controller = TempData["controller"].ToString();
                string action = TempData["action"].ToString();

                return RedirectToAction(action, controller);
            }
            else
            {
                ViewData["MENSAJE"] = "Credenciales incorrectas";
                return View();
            }

        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync
                (CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Libros");
        }

        #endregion

        public async Task<IActionResult> Index(int? posicion, int? idgenero)
        {
            if (posicion == null)
            {
                posicion = 1;
            }

            if (idgenero != null)
            {
                List<Libro> libros = await this.repo.GetLibrosGenero(posicion.Value, idgenero.Value);
                ViewData["REGISTROS"] = await this.repo.NumeroRegistros(idgenero.Value);

                return View(libros);
            }
            else
            {
                List<Libro> libros = await this.repo.GetLibros(posicion.Value);
                ViewData["REGISTROS"] = await this.repo.NumeroRegistros();

                return View(libros);
            }

        }

        public async Task<IActionResult> Details(int idlibro, int? idaniadir)
        {


            if (idaniadir != null)
            {
                List<int> carrito;
                if (HttpContext.Session.GetObject<List<int>>("IDSLIBROS") == null)
                {
                    carrito = new List<int>(); //Creamos carrito
                }
                else
                {
                    carrito =
                        HttpContext.Session.GetObject<List<int>>("IDSLIBROS");
                }
                carrito.Add(idaniadir.Value);
                HttpContext.Session.SetObject("IDSLIBROS", carrito);
                ViewData["MENSAJE"] = "Artículos añadidos";
                return RedirectToAction("Carrito");
            }

            Libro libro = await this.repo.GetLibro(idlibro);
            return View(libro);

        }


        public async Task<IActionResult> Carrito(int? idEliminar)
        {
            List<int> productos =
                HttpContext.Session.GetObject<List<int>>("IDSLIBROS");
            if (productos == null)
            {
                ViewData["MENSAJE"] = "No tiene Productos en la Cesta";
                return View();
            }
            else
            {
                if (idEliminar != null)
                {
                    productos.Remove(idEliminar.Value);
                    if (productos.Count() == 0)
                    {
                        HttpContext.Session.Remove("IDSLIBROS");
                    }
                    else
                    {
                        HttpContext.Session.SetObject("IDSLIBROS", productos);
                    }
                }
                List<Libro> librosEnCarrito = await
                    this.repo.GetLibrosCarrito(productos);

                return View(librosEnCarrito); ;
            }
        }

        [AuthorizeUsers]
        public async Task<IActionResult> Pedidos()
        {
            List<VistaPedido> pedidos = await this.repo.GetVistaPedidos(iduser);
            return View(pedidos);
        }

        public async Task<IActionResult> Perfil(int iduser)
        {
            Usuario user = await this.repo.FindUserId(iduser);
            return View(user);
        }

    }
}
