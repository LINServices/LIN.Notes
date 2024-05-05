using SQLite;

namespace LIN.LocalDataBase.Data
{
    public class NoteDB
    {

        /// <summary>
        /// Base de datos
        /// </summary>
        SQLiteAsyncConnection? Database;



        /// <summary>
        /// Constructor
        /// </summary>
        public NoteDB()
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

                var result = await Database.CreateTableAsync<Models.Note>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception en UserLocalDB: " + ex);
            }

        }




        /// <summary>
        /// Guarda un usuario
        /// </summary>
        public async Task Save(Models.Note modelo)
        {
            await Init();
            await Delete();
            await Database!.InsertAsync(modelo);
        }


        /// <summary>
        /// Guarda un usuario
        /// </summary>
        public async Task Append(Models.Note modelo)
        {
            await Init();
            await Database!.InsertAsync(modelo);
        }





        /// <summary>
        /// Guarda un usuario
        /// </summary>
        public async Task Save(List<Models.Note> modelos)
        {
            await Init();
            await Delete();
            await Database!.InsertAllAsync(modelos);
        }



        /// <summary>
        /// Obtiene todos los usuarios
        /// </summary>
        public async Task<List<Models.Note>> Get()
        {
            await Init();
            return await Database!.Table<Models.Note>().OrderBy(t => t.Id).ToListAsync() ?? new();
        }



        /// <summary>
        /// Obtiene todos los usuarios
        /// </summary>
        public async Task<List<Models.Note>> GetUntrack()
        {
            await Init();
            return await Database!.Table<Models.Note>().Where(t => !t.IsConfirmed).OrderBy(t => t.Id).ToListAsync() ?? new();
        }






        /// <summary>
        /// Elimina todos los usuarios
        /// </summary>
        public async Task Delete()
        {
            await Init();

            // Elimina todos los usuarios
            await Database!.Table<Models.Note>().DeleteAsync(T => 1 == 1);

        }


        /// <summary>
        /// Elimina todos los usuarios
        /// </summary>
        public async Task Remove(int id)
        {
            await Init();

            // Elimina todos los usuarios
            await Database!.Table<Models.Note>().DeleteAsync(T => T.Id == id);

        }

        // <summary>
        /// Elimina todos los usuarios
        /// </summary>
        public async Task DeleteOne(int id, bool isConfirmed)
        {
            await Init();

            // Elimina todos los usuarios

            await Database.QueryAsync<Models.Note>("UPDATE [Note] SET [IsDeleted] = ?, [IsConfirmed] = ? WHERE [Id] = ?", true, isConfirmed, id);

        }




        /// <summary>
        /// Elimina todos los usuarios
        /// </summary>
        public async Task Update(Models.Note note)
        {
            await Init();
            await Database.QueryAsync<Models.Note>("UPDATE [Note] SET [Tittle] = ?, [Content] = ?, [Color] = ?, [IsConfirmed] = ? WHERE [Id] = ?", note.Tittle, note.Content, note.Color, note.IsConfirmed, note.Id);
        }






    }
}
