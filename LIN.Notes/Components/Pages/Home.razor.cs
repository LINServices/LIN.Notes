using LIN.Access.Notes;
using LIN.Types.Notes.Models;

namespace LIN.Notes.Components.Pages;


public partial class Home
{


    /// <summary>
    /// Drawer.
    /// </summary>
    private Elements.Drawer? Drawer { get; set; }




    public static ReadAllResponse<Types.Notes.Models.NoteDataModel>? Notas { get; set; }



    /// <summary>
    /// Constructor.
    /// </summary>
    public Home()
    {
        Load();
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
        var dataRes = await RefreshData();

        StateHasChanged();
        // Validación.
        if (!dataRes)
        {
            // Mostrar error.
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
        var items = await Access.Notes.Controllers.Notes.ReadAll(LIN.Access.Notes.Session.Instance.Token);

        // Rellena los items
        Notas = items;
        StateHasChanged();
        return true;

    }



    int count = 0;





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
            NavigationManager?.NavigateTo("/");
        });
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
        Notas = null;
        LIN.Access.Auth.SessionAuth.CloseSession();
        LocalDataBase.Data.UserDB database = new();
        await database.DeleteUsers();
    }



    void Go(NoteDataModel note)
    {
        var url = NavigationManager.BaseUri + "note";

        url = Global.Utilities.Network.Web.AddParameters(url, new Dictionary<string, string>()
        { 
            {"Id", note.Id.ToString() }
        });

        NavigationManager.NavigateTo(url);

    }


    async void Open()
    {

        var x = await LIN.Access.Notes.Controllers.Notes.Create(new NoteDataModel()
        {
            Color = 0
        }, Session.Instance.Token);


        if (x.Response != Responses.Success)
            return;

        var n = new NoteDataModel() { Id = x.LastID };

        Notas.Models.Add(n);


        Go(n);
    }


}