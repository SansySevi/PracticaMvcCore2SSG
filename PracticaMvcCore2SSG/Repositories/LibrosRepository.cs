using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PracticaMvcCore2SSG.Data;
using PracticaMvcCore2SSG.Models;


#region PROCEDURES

//Create PROCEDURE SP_LIBROS_GENERO_PAGINAR
//    (@POSICION INT, @IDGENERO INT)
//    AS
//        SELECT POSICION, IdLibro, Titulo, Autor, Editorial, Portada, Resumen, Precio, IdGenero
//		FROM
//            (SELECT CAST(
//                ROW_NUMBER() OVER(ORDER BY Autor DESC) AS INT) AS POSICION,
//                IdLibro, Titulo, Autor, Editorial, Portada, Resumen, Precio, IdGenero
//            FROM LIBROS
//            WHERE IdGenero = @IDGENERO) AS QUERY
//        WHERE QUERY.POSICION >= @POSICION AND QUERY.POSICION<(@POSICION + 6)
//    GO

//create PROCEDURE SP_LIBROS_PAGINAR
//    (@POSICION INT)
//    AS
//        SELECT POSICION, IdLibro, Titulo, Autor, Editorial, Portada, Resumen, Precio, IdGenero
//		FROM
//            (SELECT CAST(
//                ROW_NUMBER() OVER(ORDER BY Autor DESC) AS INT) AS POSICION,
//                IdLibro, Titulo, Autor, Editorial, Portada, Resumen, Precio, IdGenero
//            FROM LIBROS) AS QUERY
//        WHERE QUERY.POSICION >= @POSICION AND QUERY.POSICION<(@POSICION + 6)
//    GO

#endregion

namespace PracticaMvcCore2SSG.Repositories
{
    public class LibrosRepository
    {

        private LibrosContext context;

        public LibrosRepository(LibrosContext context)
        {
            this.context = context;
        }

        public async Task<int> NumeroRegistros()
        {
            return await this.context.Libros.CountAsync();

        }

        public async Task<int> NumeroRegistros(int IdGenero)
        {
            return await this.context.Libros.Where(x => x.IdGenero == IdGenero).CountAsync();

        }

        public async Task<List<Libro>> GetLibros(int posicion)
        { 
            string sql = "SP_LIBROS_PAGINAR @POSICION";
            SqlParameter pampos = new SqlParameter("@POSICION", posicion);
            var consulta = this.context.Libros.FromSqlRaw(sql, pampos);
            List<Libro> libros = await consulta.ToListAsync();
            return libros;
        }

        public async Task<List<Libro>> GetLibrosGenero(int posicion, int gender)
        {
            string sql = "SP_LIBROS_GENERO_PAGINAR @POSICION, @IDGENERO";
            SqlParameter pampos = new SqlParameter("@POSICION", posicion);
            SqlParameter pamgen = new SqlParameter("@IDGENERO", gender);
            var consulta = this.context.Libros.FromSqlRaw(sql, pampos, pamgen);
            List<Libro> libros = await consulta.ToListAsync();
            return libros;
        }


        public async Task<Libro> GetLibro(int idLibro)
        {
           Libro libro = await this.context.Libros.
                FirstOrDefaultAsync(z => z.idLibro == idLibro);
            return libro;
        }

        public async Task<List<Genero>> GetGeneros()
        {
            return await this.context.Generos.ToListAsync();
        }

        public async Task<List<Libro>> GetLibrosCarrito(List<int> ids)
        {
            var consulta = from datos in this.context.Libros
                           where ids.Contains(datos.idLibro)
                           select datos;
            if (consulta.Count() == 0)
            {
                return null;
            }
            return await consulta.ToListAsync();
        }

        public async Task<List<VistaPedido>> GetVistaPedidos(int idusuario)
        {
            //List<VistaPedido> pedidos = await this.context.VistaPedidos.ToListAsync();

            List<VistaPedido> pedidos = await this.context.VistaPedidos.Where(z => z.IdUsuario == idusuario).ToListAsync();
            return pedidos;
        }


        public async Task<Usuario> FindUser(string email, string password)
        {
            Usuario user = new Usuario();

            user = await
                this.context.Usuarios.FirstOrDefaultAsync(x => x.Email == email);
            
            if(password == user.Pass)
            {
                return user;
            }
            else
            {
                return null;
            }
        }

        public async Task<Usuario> FindUserId(int id)
        {
            Usuario user = new Usuario();

            user = await
                this.context.Usuarios.FirstOrDefaultAsync(x => x.IdUsuario == id);

            return user;
        }
    }
}
