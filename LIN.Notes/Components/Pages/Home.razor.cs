namespace LIN.Notes.Components.Pages;


public partial class Home
{


    /// <summary>
    /// Drawer.
    /// </summary>
    private Elements.Drawer? Drawer { get; set; }



    /// <summary>
    /// Hub de Passkey.
    /// </summary>
    public static PassKeyHub PassKeyHub { get; set; } = BuildHub();



    /// <summary>
    /// Lista de intentos.
    /// </summary>
    private List<PassKeyModel> Intentos { get; set; } = [];



    /// <summary>
    /// Constructor.
    /// </summary>
    public Home()
    {
        Load();
        SuscribeEvents();
    }




    /// <summary>
    /// Suscribir el evento.
    /// </summary>
    private async void SuscribeEvents()
    {
        await PassKeyHub.Suscribe();
        PassKeyHub.OnReceiveIntent += OnReceiveIntentIntentAdmin;
    }



    /// <summary>
    /// Evento al recibir un intento (Administradores).
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">Passkey</param>
    private void OnReceiveIntentIntentAdmin(object? sender, PassKeyModel e)
    {

        // Si no existe el Drawer.
        if (Drawer == null)
            return;

        // Propiedades.
        Drawer.Passkey = e;
        Drawer.Show();
    }



    /// <summary>
    /// Obtener datos desde el servicio.
    /// </summary>
    private async void Load()
    {

        // Rellena los datos
        _ = RefreshCount();
        var dataRes = await RefreshData();

        // Validación.
        if (!dataRes)
        {
            // Mostrar error.
            return;
        }

        // Renderizar.
        Render();
    }



    /// <summary>
    /// Renderiza la lista de intentos.
    /// </summary>
    private void Render()
    {

        // Si no hay intentos.
        if (Intentos.Count == 0)
            return;

        // Mostrar el primero.
        ShowOnDrawer(Intentos[0]);

        // Renderizar en la pantalla.
        if (Intentos.Count > 1)
        {
            foreach (var i in Intentos.Skip(1))
            {
                // Renderizar en la pantalla.
            }
            return;
        }

    }



    /// <summary>
    /// Muestra un intento en el Drawer.
    /// </summary>
    /// <param name="e">Passkey model.</param>
    private void ShowOnDrawer(PassKeyModel e)
    {
        if (Drawer == null)
            return;

        Drawer.Passkey = e;
        Drawer.Show();
    }



    /// <summary>
    /// Refrescar los datos desde el servicio.
    /// </summary>
    private async Task<bool> RefreshData()
    {

        // Items
        var items = await Access.Auth.Controllers.Intents.ReadAll(LIN.Access.Auth.SessionAuth.Instance.AccountToken);

        // Análisis de respuesta
        if (items.Response != Responses.Success)
            return false;

        // Rellena los items
        Intentos = [.. items.Models];
        return true;

    }



    int count = 0;

    /// <summary>
    /// Refrescar los datos desde el servicio.
    /// </summary>
    private async Task<bool> RefreshCount()
    {

        // Items
        var items = await Access.Auth.Controllers.Intents.Count(LIN.Access.Auth.SessionAuth.Instance.AccountToken);

        // Rellena los items
        count = items.Model;
        StateHasChanged();
        return true;

    }




    /// <summary>
    /// Cerrar la sesión.
    /// </summary>
    async void CloseSession()
    {

        await this.InvokeAsync(async () =>
        {
            LIN.Access.Auth.SessionAuth.CloseSession();
            LIN.LocalDataBase.Data.UserDB db = new();
            await db.DeleteUsers();
            PassKeyHub.Disconnect();
            PassKeyHub = null!;
            NavigationManager?.NavigateTo("/");
        });
    }



    /// <summary>
    /// Construir el hub.
    /// </summary>
    public static PassKeyHub BuildHub()
    {
        // Sesión.
        var session = Access.Auth.SessionAuth.Instance;

        // Crear el Hub.
        return new(session.Account.Identity.Unique, string.Empty, session.AccountToken, true);
    }



    /// <summary>
    /// Aceptar.
    /// </summary>
    void OnSuccess()
    {
        count++;
        StateHasChanged();
    }


    async void Close()
    {
        PassKeyHub = null;
        LIN.Access.Auth.SessionAuth.CloseSession();
        LocalDataBase.Data.UserDB database = new();
        await database.DeleteUsers();
    }

}