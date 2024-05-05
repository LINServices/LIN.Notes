
namespace LIN.Notes.Components.Pages;


public partial class Home : IDisposable
{


    int Color = -1;


    /// <summary>
    /// Notas actuales.
    /// </summary>
    public static ReadAllResponse<NoteDataModel>? Notes { get; set; }



    /// <summary>
    /// Constructor.
    /// </summary>
    public Home()
    {
        // Cargar datos.
        Load();

        // Acción al cambiar el tema.
        MainPage.OnColor = MauiProgram.Aa;

        // Ejecutar cambio de tema.
        MauiProgram.Aa();

        // Evento al cambiar la conexión.
        Connectivity.ConnectivityChanged += OnConnectivityChanged;

        // Evento al actualizar la sesión.
        LIN.Access.Notes.Observers.SessionObserver.OnUpdate += OnUpdateSession;
    }



    /// <summary>
    /// Al actualizar la sesión.
    /// </summary>
    /// <param name="e">Session.</param>
    private void OnUpdateSession(object? sender, Session e)
    {
        if (e.Type == SessionType.Connected)
            this.InvokeAsync(() => RefreshData(true));
        
        this.InvokeAsync(StateHasChanged);
    }




    /// <summary>
    /// Obtener datos desde el servicio.
    /// </summary>
    private async void Load()
    {

        try
        {
            // Rellena los datos
            await RefreshData();
            _ = InvokeAsync(StateHasChanged);
        }
        catch (Exception)
        {
        }

    }



    /// <summary>
    /// Evento al cambiar el estado de conexión.
    /// </summary>
    /// <param name="sender">Objeto.</param>
    /// <param name="e">Evento.</param>
    private async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {

        // Si no hay acceso a internet.
        if (e.NetworkAccess != NetworkAccess.Internet)
        {
            // Establecer la sesión como local.
            Session.Instance.Type = SessionType.Local;

            // Actualizar la pantalla.
            _ = InvokeAsync(StateHasChanged);
            return;
        }


        // La sesión es local.
        if (Session.Instance.Type == SessionType.Local)
        {

            // Base de datos local.
            var database = new LocalDataBase.Data.UserDB();

            // Usuario.
            var user = await database.GetDefault();

            // Iniciar sesión.
            await Start(user?.Unique ?? "", user?.Password ?? "");

        }


        // Refrescar datos.
        _ = InvokeAsync(() => RefreshData(true));

        // Refrescar pantalla.
        _ = InvokeAsync(StateHasChanged);

    }



    /// <summary>
    /// Refrescar los datos desde el servicio.
    /// </summary>
    private async Task RefreshData(bool force = false)
    {

        // Si no es forzado y los datos están cargados.
        if (!force && Notes != null)
            return;

        // Respuestas.
        ReadAllResponse<NoteDataModel>? items = null;

        // Obtiene la conectividad actual.
        NetworkAccess accessType = Connectivity.Current.NetworkAccess;

        var x = Session.Instance.Account;

        // Cargar notas locales.
        Notes = new()
        {
            Response = Responses.Success,
            Models = (await new LocalDataBase.Data.NoteDB().Get()).Where(t => !t.IsDeleted).Select(t => new NoteDataModel()
            {
                Color = t.Color,
                Content = t.Content,
                Id = t.Id,
                Tittle = t.Tittle
            }).ToList()
        };

        _ = InvokeAsync(StateHasChanged);

        // Si tiene acceso a internet.
        if (Session.Instance.Type == SessionType.Connected && accessType == NetworkAccess.Internet)
        {

            // Obtener desde la API.
            items = await Access.Notes.Controllers.Notes.ReadAll(LIN.Access.Notes.Session.Instance.Token);

            // Fue correcto.
            if (items.Response == Responses.Success)
                CreateAndUpdate(items.Models);

            // Rellena los items
            Notes = items;

        }



        // Actualizar pantalla.
        _ = InvokeAsync(StateHasChanged);
        return;

    }



    /// <summary>
    /// Cerrar la sesión.
    /// </summary>
    private async void CloseSession()
    {
        await InvokeAsync(async () =>
        {
            LIN.Access.Notes.Observers.SessionObserver.Dispose();
            LIN.Access.Auth.SessionAuth.CloseSession();
            LIN.LocalDataBase.Data.UserDB db = new();
            await db.DeleteUsers();

            LIN.LocalDataBase.Data.NoteDB ntdb = new();
            await ntdb.Delete();

            NavigationManager?.NavigateTo("/");
        });
    }



    /// <summary>
    /// Navegar a una nota.
    /// </summary>
    /// <param name="note">Modelo.</param>
    private void Go(NoteDataModel note)
    {

        // Url.
        var url = NavigationManager.BaseUri + "note";

        // Agregar parámetros.
        url = Global.Utilities.Network.Web.AddParameters(url, new Dictionary<string, string>()
        {
            {"Id", note.Id.ToString() }
        });

        // Navegar.
        NavigationManager.NavigateTo(url);

    }



    /// <summary>
    /// Ir a crear nueva nota.
    /// </summary>
    private void Create() => Go(new());



    bool IsClean = false;


    /// <summary>
    /// Limpiar.
    /// </summary>
    /// <param name="notas">Lista de notas.</param>
    private async void CreateAndUpdate(List<NoteDataModel> notas)
    {

        // Cleaning.
        if (IsClean)
            return;

        // Establecer nuevo estado.
        IsClean = true;

        // Base de datos local.
        var noteDB = new LocalDataBase.Data.NoteDB();

        // Obtener notas no seguidas.
        var notTracked = await noteDB.GetUntrack();

        // Recorrer las notas no seguidas.
        foreach (var note in notTracked)
        {

            // Eliminar del servidor.
            if (note.IsDeleted && note.Id > 0)
            {
                // Solicitud a la API.
                var response = await Access.Notes.Controllers.Notes.Delete(note.Id, Session.Instance.Token);

                if (response.Response == Responses.Success)
                {
                    // Eliminar de la lista local.
                    notas.RemoveAll(localNote => localNote.Id == note.Id);

                    // Eliminar del track local.
                    await noteDB.Remove(note.Id);

                }

                continue;
            }

            // Crear nota.
            if (note.Id < 0)
            {

                // Nuevo modelo.
                var newModel = new NoteDataModel()
                {
                    Id = 0,
                    Tittle = note.Tittle,
                    Content = note.Content,
                    Color = note.Color
                };

                // Enviar a la API.
                var response = await Access.Notes.Controllers.Notes.Create(newModel, Session.Instance.Token);

                // Validar.
                if (response.Response == Responses.Success)
                {
                    // Establecer el nuevo Id.
                    newModel.Id = response.LastID;

                    // Agregar a lista local.
                    Notes?.Models.Add(newModel);

                    // Eliminar del track local.
                    await noteDB.Remove(note.Id);
                    continue;
                }

                newModel.Id = note.Id;
                notas.Add(newModel);

                continue;

            }


            // Actualizar.
            await Access.Notes.Controllers.Notes.Update(new()
            {
                Id = note.Id,
                Tittle = note.Tittle,
                Content = note.Content,
                Color = note.Color
            }, Session.Instance.Token);


            // Obtener la nota local.
            var localNote = notas.FirstOrDefault(t => t.Id == note.Id);

            // Si la nota si existe.
            if (localNote != null)
            {
                localNote.Tittle = note.Tittle;
                localNote.Content = note.Content;
                localNote.Color = note.Color;
            }

        }


        // Guardar en la base de datos local.
        await noteDB.Save(notas.Select(t => new LocalDataBase.Models.Note
        {
            Color = t.Color,
            Content = t.Content,
            Id = t.Id,
            Tittle = t.Tittle,
            IsConfirmed = true
        }).ToList());

        // Establecer nuevo estado.
        IsClean = false;

        // Invocar el cambio.
        _ = this.InvokeAsync(StateHasChanged);
    }



    /// <summary>
    /// Iniciar sesión de servidor.
    /// </summary>
    /// <param name="user">Usuario.</param>
    /// <param name="password">Contraseña.</param>
    private static async Task Start(string user, string password)
    {

        // Iniciar sesión.
        var (_, _) = await LIN.Access.Notes.Session.LoginWith(user, password, true);

    }




    void SetColor(int color)
    {
        Color = color;
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {

        // Eliminar evento al cambiar la conexión.
        Connectivity.ConnectivityChanged -= OnConnectivityChanged;

        // Eliminar evento al actualizar la sesión.
        LIN.Access.Notes.Observers.SessionObserver.OnUpdate -= OnUpdateSession;
    }
}