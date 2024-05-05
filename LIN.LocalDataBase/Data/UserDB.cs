using SQLite;

namespace LIN.LocalDataBase.Data
{
    public class UserDB
    {

        /// <summary>
        /// Base de datos
        /// </summary>
        SQLiteAsyncConnection? Database;



        /// <summary>
        /// Constructor
        /// </summary>
        public UserDB()
        {
        }




        /// <summary>
        /// Inicia la base de datos
        /// </summary>
        private async Task Init()
        {
            try
            {
                if (Database is not null)
                    return;

                Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);

                var result = await Database.CreateTableAsync<Models.User>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception en UserLocalDB: " + ex);
            }

        }




        /// <summary>
        /// Guarda un usuario
        /// </summary>
        public async Task SaveUser(Models.User modelo)
        {
            await Init();
            await DeleteUsers();
            await Database!.InsertAsync(modelo);
        }



        /// <summary>
        /// Obtiene todos los usuarios
        /// </summary>
        public async Task<List<Models.User>> GetUsers()
        {
            await Init();
            return await Database!.Table<Models.User>().ToListAsync() ?? new();
        }



        /// <summary>
        /// Obtiene un usuario especifico
        /// </summary>
        public async Task<Models.User?> GetUser(int id)
        {
            await Init();
            return await Database!.Table<Models.User>().Where(M => M.Id == id).FirstOrDefaultAsync();
        }



        /// <summary>
        /// Obtiene un usuario especifico
        /// </summary>
        public async Task<Models.User?> GetUser(string user)
        {
            await Init();
            return await Database!.Table<Models.User>().Where(M => M.Unique == user).FirstOrDefaultAsync();
        }



        /// <summary>
        /// Obtiene el usuario default
        /// </summary>
        public async Task<Models.User?> GetDefault()
        {
            await Init();

            // Lista de usuarios
            var lista = await Database!.Table<Models.User>().ToListAsync();

            // Si no hay elementos
            if (lista.Count == 0) return null;

            return lista[0];
        }



        /// <summary>
        /// Elimina todos los usuarios
        /// </summary>
        public async Task DeleteUsers()
        {
            await Init();

            // Elimina todos los usuarios
            await Database!.Table<Models.User>().DeleteAsync(T => 1 == 1);

        }


    }
}
