using LIN.Types.Notes.Models;

namespace LIN.Notes.Components.Pages;


public partial class Home
{


    public static ReadAllResponse<Types.Notes.Models.NoteDataModel>? Notas { get; set; }



    /// <summary>
    /// Constructor.
    /// </summary>
    public Home()
    {
        Load();
        MainPage.OnColor = MauiProgram.Aa;
        MauiProgram.Aa();
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
        catch
        {
        }



    }








    /// <summary>
    /// Refrescar los datos desde el servicio.
    /// </summary>
    private async Task RefreshData()
    {

        if (Notas != null)
            return;

        // Items
        NetworkAccess accessType = Connectivity.Current.NetworkAccess;

        ReadAllResponse<NoteDataModel>? items = null;
        if (accessType == NetworkAccess.Internet)
        {


            items = await Access.Notes.Controllers.Notes.ReadAll(LIN.Access.Notes.Session.Instance.Token);

            if (items.Response == Responses.Success)
            {

                CreateAndUpdate(items.Models);


            }

        }

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
        Notas = items;
        _ = InvokeAsync(StateHasChanged);
        return;

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
        Go(new());
    }




    async void CreateAndUpdate(List<NoteDataModel> notas)
    {
        // Guardar.
        LIN.LocalDataBase.Data.NoteDB noteDB = new();



        var untracks = await noteDB.GetUntrack();


        foreach (var note in untracks)
        {
            _ = LIN.Access.Notes.Controllers.Notes.Update(note.Id, note.Tittle, note.Content, note.Color, LIN.Access.Notes.Session.Instance.Token);
            var nt = notas.FirstOrDefault(t => t.Id == note.Id);

            if (nt != null)
            {
                nt.Tittle = note.Tittle;
                nt.Content = note.Content;
                nt.Color = note.Color;
            }
        }


        // Save.
        await noteDB.Save(notas.Select(t => new LocalDataBase.Models.Note
        {
            Color = t.Color,
            Content = t.Content,
            Id = t.Id,
            Tittle = t.Tittle,
            IsConfirmed = true
        }).ToList());

        _ = this.InvokeAsync(StateHasChanged);
    }

}