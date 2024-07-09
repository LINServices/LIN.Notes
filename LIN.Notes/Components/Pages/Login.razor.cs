namespace LIN.Notes.Components.Pages;


public partial class Login
{

    /// <summary>
    /// Navegación.
    /// </summary>
    [Inject]
    private NavigationManager? NavigationManager { get; set; }


    /// <summary>
    /// Obtiene si se esta log con una llave de acceso
    /// </summary>
    private bool IsWithKey { get; set; } = false;


    /// <summary>
    /// Usuario
    /// </summary>
    private string User { get; set; } = "";


    /// <summary>
    /// Contraseña
    /// </summary>
    private string Password { get; set; } = "";


    /// <summary>
    /// Mensaje que se muestra al cargar
    /// </summary>
    private string LogMessage { get; set; } = "Iniciando Sesión";


    /// <summary>
    /// Mensaje de error
    /// </summary>
    private string ErrorMessage { get; set; } = "";


    /// <summary>
    /// Visibilidad del error.
    /// </summary>
    private bool ErrorVisible { get; set; } = false;



    /// <summary>
    /// Mostrar el botón de cancelar.
    /// </summary>
    private bool ShowButtonCancel { get; set; } = false;



    /// <summary>
    /// Sección actual.
    /// </summary>
    private int Section { get; set; } = 3;



    /// <summary>
    /// La respuesta de hub ya fue recibida.
    /// </summary>
    private bool isResponseReceive = false;



    /// <summary>
    /// Id único de inicio passkey.
    /// </summary>
    private string Unique = "";


    /// <summary>
    /// Hub passkey.
    /// </summary>
    private PassKeyHub? hub = null;




    /// <summary>
    /// Evento.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {

        // Validar sesión activa.
        if (Access.Auth.SessionAuth.IsOpen)
        {
            NavigationManager?.NavigateTo("/home");
            return;
        }

        // Base de datos local.
        LocalDataBase.Data.UserDB database = new();

        // Obtener conexión actual.
        NetworkAccess accessType = Connectivity.Current.NetworkAccess;

        // Validar sesión local.
        var localSession = await database.GetDefault();

        // Si hay sesión local.
        if (localSession != null)
        {

            // Generar.
            Session.GenerateLocal(new()
            {
                Name = localSession.Name,
                Identity = new()
                {
                    Unique = localSession.Unique
                },
                Id = localSession.Id,
                Profile = localSession.Profile
            }, (accessType == NetworkAccess.Internet ? SessionType.Sync : SessionType.Local));

            // Navegar.
            NavigationManager?.NavigateTo("/home");
        }


        if (accessType != NetworkAccess.Internet)
            return;


      



        // Si no existe
        if (localSession == null)
        {
            UpdateSection(0);
            return;
        }

        UpdateSection(3);

        IsWithKey = false;
        User = localSession.Unique;
        Password = localSession.Password;

        Start();

        _ = base.OnInitializedAsync();

    }



    /// <summary>
    /// Actualizar la sección.
    /// </summary>
    /// <param name="section">Id de la sección</param>
    private void UpdateSection(int section)
    {
        InvokeAsync(() =>
        {
            Section = section;
            StateHasChanged();
        });
    }



    /// <summary>
    /// Oculta los errores
    /// </summary>
    void HideError()
    {
        ErrorVisible = false;
        StateHasChanged();
    }



    /// <summary>
    /// Oculta los errores
    /// </summary>
    void GoToForget()
    {
        NavigationManager?.NavigateTo("/login/forgetPassword");
    }



    /// <summary>
    /// Muestra un mensaje
    /// </summary>
    void ShowError(string message)
    {
        InvokeAsync(() =>
        {
            UpdateSection(0);
            ErrorVisible = true;
            ErrorMessage = message;
            StateHasChanged();
        });
    }



    /// <summary>
    /// Muestra un mensaje
    /// </summary>
    void GotoLoginKey()
    {
        IsWithKey = !IsWithKey;
        StateHasChanged();
    }



    /// <summary>
    /// Inicia sesión.
    /// </summary>
    private async void Start()
    {

        // Validar si es con llave.
        if (IsWithKey)
        {
            StartKey();
            return;
        }

        // Estado cargando.
        UpdateSection(3);

        // Ocultar el error.
        HideError();

        // Validar parámetros.
        if (User.Length <= 0 || Password.Length <= 0)
        {
            ShowError("Completa todos los campos");
            return;
        }

        // Iniciar sesión.
        var (Session, Response) = await LIN.Access.Notes.Session.LoginWith(User, Password, true);

        // Validar respuesta.
        switch (Response)
        {

            // Correcto.
            case Responses.Success:

                // Iniciar servicios de tiempo real.
                Services.Realtime.Start();

                // Obtener local db.
                LocalDataBase.Data.UserDB database = new();

                // Guardar información.
                await database.SaveUser(new()
                {
                    Id = Session!.Account.Id,
                    Unique = Session!.Account.Identity.Unique,
                    Name = Session.Account.Name,
                    Profile = Session!.Account.Profile,
                    Password = Password
                });

                // Navegar.
                NavigationManager?.NavigateTo("/home");
                return;

            // Contraseña incorrecta.
            case Responses.InvalidPassword:
                ShowError("La contraseña es incorrecta");
                break;

            // No existe la cuenta.
            case Responses.NotExistAccount:
                ShowError($"No se encontró el usuario '{User}'");
                break;

            // Desautorizado por la organización.
            case Responses.UnauthorizedByOrg:
                ShowError($"Tu organización no permite que accedas a esta app");
                break;

            default:
                ShowError("Inténtalo mas tarde");
                break;


        }

    }



    /// <summary>
    /// Inicia sesión
    /// </summary>
    async void StartKey()
    {
        // Id único.
        string localUnique = Guid.NewGuid().ToString();
        Unique = localUnique;

        // Mostrar mensaje.
        ShowButtonCancel = false;
        isResponseReceive = false;
        LogMessage = "Revisa tu dispositivo";
        UpdateSection(3);
        HideError();

        // Validar parámetros.
        if (User.Length <= 0)
        {
            ShowError("Completa el campo de usuario");
            return;
        }

        // Crear el hub.
        hub = new(User, string.Empty, string.Empty);

        // Esperar la creación.
        await hub.Suscribe();

        // Crear evento.
        hub.OnReceiveResponse += OnReceiveResponse;

        // Intento.
        PassKeyModel intent = new()
        {
            User = User
        };

        // Enviar el evento.
        hub.SendIntent(intent);

        // Mostrar el botón de cancelar.
        await Task.Delay(3000);
        ShowButtonCancel = true;
        StateHasChanged();

        // Tiempo de expiración.
        await Task.Delay(30000);

        if (isResponseReceive || localUnique != Unique)
            return;

        // Desconectar el hub.
        hub?.Disconnect();
        hub = null;
        Show("La sesión de passkey ha expirado");
        StateHasChanged();

    }



    /// <summary>
    /// Evento al recibir la respuesta de passkey.
    /// </summary>
    private async void OnReceiveResponse(object? sender, PassKeyModel e)
    {

        // Nuevo estado.
        isResponseReceive = true;

        // Segun el estado.
        switch (e.Status)
        {
            case PassKeyStatus.Success:
                break;

            case PassKeyStatus.Rejected:
                Show("El intento Passkey fue rechazada");
                return;

            case PassKeyStatus.Expired:
                Show("La sesión expiro");
                return;

            case PassKeyStatus.BlockedByOrg:
                Show("Tu organización no permite que inicies en esta aplicación.");
                return;

            default:
                Show("Hubo un error al iniciar sesión con passkey");
                return;
        }


        // Estado de animación.
        UpdateSection(1);

        // Generar login.
        var logIn = Access.Notes.Session.LoginWith(e.Token);

        // Esperar 4 segundos.
        await Task.Delay(4000);

        // Esperar la respuesta de login.
        var (_, response) = await logIn;

        // Segun la respuesta.
        switch (response)
        {

            // Correcto.
            case Responses.Success:
                NavigationManager?.NavigateTo("/home");
                return;

            // Otros.
            default:
                Show("Hubo un error al iniciar sesión");
                break;
        }

    }



    /// <summary>
    /// Cancelar passkey.
    /// </summary>
    void CancelPasskey()
    {
        hub?.Disconnect();
        hub = null;
        UpdateSection(0);
        return;
    }



    /// <summary>
    /// Mostrar error.
    /// </summary>
    /// <param name="message">Mensaje de error.</param>
    void Show(string message)
    {
        InvokeAsync(async () =>
        {
            // Estado fallido.
            ErrorMessage = message;
            UpdateSection(2);

            // Esperar 2 segundos.
            await Task.Delay(2000);

            ShowError(message);
        });
    }



}