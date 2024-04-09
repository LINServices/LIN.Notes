using LIN.Access.Notes;

namespace LIN.Notes.Components.Pages;


public partial class Note
{


    /// <summary>
    /// Id de la nota.
    /// </summary>
    [Parameter]
    [SupplyParameterFromQuery]
    public int Id { get; set; }



    /// <summary>
    /// Modelo de la nota.
    /// </summary>
    Types.Notes.Models.NoteDataModel? NoteDataModel { get; set; }



    /// <summary>
    /// Titulo.
    /// </summary>
    string Tittle
    {
        get => (string.IsNullOrWhiteSpace(NoteDataModel?.Tittle)) ? "Sin Titulo" : NoteDataModel?.Tittle ?? "";
        set
        {
            if (NoteDataModel == null)
                return;

            NoteDataModel.Tittle = (string.IsNullOrWhiteSpace(value)) ? "Sin titulo" : value;

            if (!IsCreated)
                Create();
        }
    }



    /// <summary>
    /// Contenido.
    /// </summary>
    string Content
    {
        get => NoteDataModel?.Content ?? ""; set
        {
            if (NoteDataModel == null)
                return;

            NoteDataModel.Content = value;
            if (!IsCreated)
                Create();
        }
    }



    bool IsCreated { get; set; }

    protected override void OnParametersSet()
    {

        // Obtener la nota.
        var note = Home.Notas?.Models?.Where(t => t.Id == Id).FirstOrDefault();

        // Si la nota no existe.
        NoteDataModel = note ?? new();

        // Color.
        MainPage.OnColor = SetColor;

        base.OnParametersSet();
    }



    void Back()
    {
        JS.InvokeVoidAsync("BackLast");
    }



    void SetColor()
    {
        GetClass();
    }





    string GetClass()
    {



        switch (NoteDataModel?.Color)
        {
            case 1:
                MauiProgram.Set([245, 188, 169], [47, 24, 17]);
                return "bg-salmon/50 dark:bg-salmon/20";
            case 2:
                MauiProgram.Set([203, 219, 237], [30, 37, 44]);
                return "bg-glass/50 dark:bg-glass/20";
            case 3:
                MauiProgram.Set([211, 234, 184], [34, 43, 23]);
                return "bg-cream-green/50 dark:bg-cream-green/20";
        }

        MauiProgram.Set([251, 233, 165], [49, 42, 15]);
        return "bg-yell/50 dark:bg-yell/20";

    }



    async void Input(Microsoft.AspNetCore.Components.ChangeEventArgs e)
    {

        string value = e.Value?.ToString() ?? "";

        if (NoteDataModel == null)
            return;


        if (NoteDataModel.Id <= 0)
            return;

        var response = await LIN.Access.Notes.Controllers.Notes.Update(NoteDataModel.Id, NoteDataModel.Tittle, value, NoteDataModel.Color, LIN.Access.Notes.Session.Instance.Token);

        var db = new LocalDataBase.Data.NoteDB();

        _ = db.Update(new LocalDataBase.Models.Note()
        {
            Color = NoteDataModel.Color,
            Content = value,
            Id = NoteDataModel.Id,
            Tittle = NoteDataModel.Tittle,
            IsConfirmed = response.Response == Responses.Success,
        });

    }



    async void InputTittle(Microsoft.AspNetCore.Components.ChangeEventArgs e)
    {

        string value = e.Value?.ToString() ?? "";

        if (NoteDataModel == null)
            return;

        if (NoteDataModel.Id <= 0)
            return;

        var response = await LIN.Access.Notes.Controllers.Notes.Update(NoteDataModel.Id, value, NoteDataModel.Content, NoteDataModel.Color, LIN.Access.Notes.Session.Instance.Token);

        var db = new LocalDataBase.Data.NoteDB();

        _ = db.Update(new LocalDataBase.Models.Note()
        {
            Color = NoteDataModel.Color,
            Content = NoteDataModel.Content,
            Id = NoteDataModel.Id,
            Tittle = value,
            IsConfirmed = response.Response == Responses.Success,
        });

    }




    async void Create()
    {

        if (IsCreated || NoteDataModel?.Id != 0)
            return;

        IsCreated = true;

        var access = Microsoft.Maui.Networking.Connectivity.Current.NetworkAccess;

        CreateResponse response;
        if (access == NetworkAccess.Internet)
        {
            response = await LIN.Access.Notes.Controllers.Notes.Create(new Types.Notes.Models.NoteDataModel()
            {
                Color = NoteDataModel.Color,
                Content = NoteDataModel.Content,
                Tittle = NoteDataModel.Tittle
            }, LIN.Access.Notes.Session.Instance.Token);
        }
        else
        {
            response = new()
            {
                LastID = 0
            };
        }


        LocalDataBase.Data.NoteDB db = new();

        _ = db.Append(new()
        {
            Id = response.LastID,
            Color = NoteDataModel.Color,
            Content = NoteDataModel.Content,
            Tittle = NoteDataModel.Tittle,
            IsConfirmed = response.LastID > 0
        });



        NoteDataModel.Id = response.LastID;

        Home.Notas?.Models.Add(NoteDataModel);



    }



    async void SetNewColor(int color)
    {

        if (NoteDataModel == null)
            return;

        NoteDataModel.Color = color;

        if (NoteDataModel.Id > 0)
            await LIN.Access.Notes.Controllers.Notes.Update(NoteDataModel.Id, color, Session.Instance.Token);

        StateHasChanged();


    }


    async void Delete()
    {
        if (NoteDataModel == null || NoteDataModel.Id <= 0)
            return;

        var response = await LIN.Access.Notes.Controllers.Notes.Delete(NoteDataModel.Id, Session.Instance.Token);


        if (response.Response == Responses.Success)
        {
            Home.Notas.Models.RemoveAll(t => t.Id == NoteDataModel.Id);
            NavigationManager.NavigateTo("Home");
        }


        var db = new LocalDataBase.Data.NoteDB();

        _ = db.DeleteOne(NoteDataModel.Id);

        StateHasChanged();

    }

}