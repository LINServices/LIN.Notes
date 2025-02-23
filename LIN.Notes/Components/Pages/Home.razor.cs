using LIN.Access.Notes.Sessions;
using LIN.Types.Enumerations;

namespace LIN.Notes.Components.Pages;


public partial class Home : IDisposable
{

    public static Home Instance { get; private set; }

    private int Color = -1;


    /// <summary>
    /// Notas actuales.
    /// </summary>
    public static (List<NoteDataModel> Models, Responses Response)? Notes { get; set; }


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

        Instance = this;
    }


    /// <summary>
    /// Al actualizar la sesión.
    /// </summary>
    /// <param name="e">Session.</param>
    private void OnUpdateSession(object? sender, Session e)
    {
        if (!e.IsLocalOpen)
        {
            e.CloseSession();
            NavigationManager.NavigateTo("/login");
            return;
        }

        if (e.Type == SessionType.Connected)
            InvokeAsync(() => RefreshData(true));

        InvokeAsync(StateHasChanged);
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
            SessionManager.Instance.Default.Type = SessionType.Local;

            // Actualizar la pantalla.
            _ = InvokeAsync(StateHasChanged);
            return;
        }

        // La sesión es local.
        if (SessionManager.Instance.Default.Type == SessionType.Local)
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

        var x = SessionManager.Instance.Default.Account;

        // Cargar notas locales.
        Notes = new()
        {
            Response = Responses.Success,
            Models = (await new LocalDataBase.Data.NoteDB().Get()).Where(t => !t.IsDeleted).Select(t => new NoteDataModel()
            {
                Color = t.Color,
                Content = t.Content,
                Id = t.Id,
                Tittle = t.Tittle,
                Language = (Languages)t.Language
            }).ToList()
        };

        _ = InvokeAsync(StateHasChanged);

        // Si tiene acceso a internet.
        if (SessionManager.Instance.Default.Type == SessionType.Connected && accessType == NetworkAccess.Internet)
        {

            // Obtener desde la API.
            items = await Access.Notes.Controllers.Notes.ReadAll(SessionManager.Instance.Default.Token);

            // Notas.
            var notes = items.Models.ToList();

            // Fue correcto.
            if (items.Response == Responses.Success)
                CreateAndUpdate(notes);

            // Rellena los items
            Notes = (notes, items.Response);

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

    private bool IsClean = false;


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
                var response = await Access.Notes.Controllers.Notes.Delete(note.Id, SessionManager.Instance.Default.Token);

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
                var response = await Access.Notes.Controllers.Notes.Create(newModel, SessionManager.Instance.Default.Token);

                // Validar.
                if (response.Response == Responses.Success)
                {
                    // Establecer el nuevo Id.
                    newModel.Id = response.LastId;

                    // Obtener idioma.
                    var lang = await Access.Notes.Controllers.Notes.ReadLang(response.LastId, SessionManager.Instance.Default.Token);

                    newModel.Language = lang.Model;

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
            }, SessionManager.Instance.Default.Token);


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
            IsConfirmed = true,
            Language = (int)t.Language
        }).ToList());

        // Establecer nuevo estado.
        IsClean = false;

        // Invocar el cambio.
        _ = this.InvokeAsync(StateHasChanged);
    }





    /// <summary>
    /// Limpiar.
    /// </summary>
    /// <param name="notas">Lista de notas.</param>
    public async void RemoveOne(int id)
    {
        await this.InvokeAsync(async () =>
         {

             Notes.Value.Models.RemoveAll(t => t.Id == id);

             StateHasChanged();

             // Base de datos local.
             var noteDB = new LocalDataBase.Data.NoteDB();

             await noteDB.DeleteOne(id, true);


         });

    }



    /// <summary>
    /// Limpiar.
    /// </summary>
    public async void UpdateColor(int id, int color)
    {
        await this.InvokeAsync(async () =>
        {

            foreach (var note in Notes?.Models.Where(t => t.Id == id) ?? [])
            {
                note.Color = color;
            }

            StateHasChanged();

            // Base de datos local.
            var noteDB = new LocalDataBase.Data.NoteDB();

            var md = Notes?.Models.Where(t => t.Id == id).FirstOrDefault();

            if (md is null)
                return;

            await noteDB.Update(new()
            {

                IsDeleted = false,
                Color = color,
                Content = md.Content,
                Id = id,
                IsConfirmed = true,
                Tittle = md.Tittle
            });


        });

    }


    public async void Add(NoteDataModel model)
    {
        await this.InvokeAsync(async () =>
        {

            var exist = Notes.Value.Models.Exists(t => t.Id == model.Id);

            if (exist)
                return;

            Notes.Value.Models.Add(model);
            StateHasChanged();

            // Base de datos local.
            var noteDB = new LocalDataBase.Data.NoteDB();


            await noteDB.Save(new LocalDataBase.Models.Note()
            {
                Color = model.Color,
                Content = model.Content,
                Id = model.Id,
                IsConfirmed = true,
                Tittle = model.Tittle
            });

        });

    }



    public async void Update(int id, string content)
    {
        await this.InvokeAsync(async () =>
        {

            foreach (var note in Notes?.Models.Where(t => t.Id == id) ?? [])
            {
                note.Content = content;
            }

            StateHasChanged();

            // Base de datos local.
            var noteDB = new LocalDataBase.Data.NoteDB();

            var md = Notes?.Models.Where(t => t.Id == id).FirstOrDefault();

            if (md is null)
                return;

            await noteDB.Update(new()
            {
                IsDeleted = false,
                Color = md.Color,
                Content = content,
                Id = id,
                IsConfirmed = true,
                Tittle = md.Tittle
            });


        });

    }


    public static bool HaveAuthError { get; set; } = false;


    /// <summary>
    /// Iniciar sesión de servidor.
    /// </summary>
    /// <param name="user">Usuario.</param>
    /// <param name="password">Contraseña.</param>
    private async Task Start(string user, string password)
    {

        // Iniciar sesión.
        var response = await LIN.Access.Notes.SessionManager.Instance.Default.LoginWith(user, password, true);

        switch (response)
        {
            case Responses.Success:
                break;
            case Responses.Unauthorized:
                HaveAuthError = true;
                break;
        }

        StateHasChanged();


    }

    private void SetColor(int color)
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