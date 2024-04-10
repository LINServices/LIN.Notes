namespace LIN.Notes.Components.Pages;


public partial class Home
{


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
            Session.Instance.IsLocal = true;

            // Actualizar la pantalla.
            _ = InvokeAsync(StateHasChanged);
            return;
        }


        // La sesión es local.
        if (Session.Instance.IsLocal)
        {

            // Base de datos local.
            var database = new LocalDataBase.Data.UserDB();

            // Usuario.
            var user = await database.GetDefault();

            // Iniciar sesión.
            await Home.Start(user?.UserU ?? "", user?.Password ?? "");

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

        // Si tiene acceso a internet.
        if (accessType == NetworkAccess.Internet)
        {

            // Obtener desde la API.
            items = await Access.Notes.Controllers.Notes.ReadAll(LIN.Access.Notes.Session.Instance.Token);

            // Fue correcto.
            if (items.Response == Responses.Success)
                CreateAndUpdate(items.Models);

        }

        // Si no hay conexión.
        else
            items = new()
            {
                Response = Responses.Success,
                Models = (await new LocalDataBase.Data.NoteDB().Get()).Select(t => new NoteDataModel()
                {
                    Color = t.Color,
                    Content = t.Content,
                    Id = t.Id,
                    Tittle = t.Tittle
                }).ToList()
            };


        // Rellena los items
        Notes = items;

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
            LIN.Access.Auth.SessionAuth.CloseSession();
            LIN.LocalDataBase.Data.UserDB db = new();
            await db.DeleteUsers();
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



    /// <summary>
    /// Limpiar.
    /// </summary>
    /// <param name="notas">Lista de notas.</param>
    private async void CreateAndUpdate(List<NoteDataModel> notas)
    {

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
                await Access.Notes.Controllers.Notes.Delete(note.Id, Session.Instance.Token);

                // Eliminar de la lista local.
                notas.RemoveAll(localNote => localNote.Id == note.Id);
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
                }

                continue;

            }


            // Actualizar.
            await LIN.Access.Notes.Controllers.Notes.Update(note.Id, note.Tittle, note.Content, note.Color, Session.Instance.Token);

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
        var (_, _) = await LIN.Access.Notes.Session.LoginWith(user, password);

    }



}