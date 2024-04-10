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
    private NoteDataModel? NoteDataModel { get; set; }



    /// <summary>
    /// Titulo.
    /// </summary>
    private string Tittle { get; set; } = string.Empty;



    /// <summary>
    /// Contenido.
    /// </summary>
    private string Content { get; set; } = string.Empty;




    /// <summary>
    /// Evento al establecer los parámetros.
    /// </summary>
    protected override void OnParametersSet()
    {

        // Obtener la nota.
        var note = Home.Notes?.Models?.Where(t => t.Id == Id).FirstOrDefault();

        // Si la nota no existe.
        NoteDataModel = note ?? new();

        // Valores
        Tittle = note?.Tittle ?? string.Empty;
        Content = note?.Content ?? string.Empty;

        // Color.
        MainPage.OnColor = SetColor;

        base.OnParametersSet();
    }



    /// <summary>
    /// Volver a atrás.
    /// </summary>
    void Back() => JS.InvokeVoidAsync("BackLast");



    /// <summary>
    /// Establecer el nuevo color.
    /// </summary>
    void SetColor()
    {
        GetClass();
    }



    /// <summary>
    /// Obtener las clases.
    /// </summary>
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



    /// <summary>
    /// Input.
    /// </summary>
    async void Input(Microsoft.AspNetCore.Components.ChangeEventArgs e)
    {


        c += 1;
        int my = c;

        await Task.Delay(500);

        if (c != my)
            return;

        string value = e.Value?.ToString() ?? "";

        if (NoteDataModel == null)
            return;

        NoteDataModel.Content = value;

        await Save();

    }

    int c = 0;


    async void InputTittle(Microsoft.AspNetCore.Components.ChangeEventArgs e)
    {


        c += 1;
        int my = c;

        await Task.Delay(500);

        if (c != my)
            return;


        string value = e.Value?.ToString() ?? "";


        if (NoteDataModel == null)
            return;


        NoteDataModel.Tittle = value;

        await Save();
    }




    static async Task<int> Create(NoteDataModel model)
    {

        // Acceso.
        var access = Connectivity.Current.NetworkAccess;

        // Respuesta.
        CreateResponse response;

        // Validar el acceso a internet.
        if (access == NetworkAccess.Internet)
        {
            // Solicitud a la API.
            response = await LIN.Access.Notes.Controllers.Notes.Create(new Types.Notes.Models.NoteDataModel()
            {
                Color = model.Color,
                Content = model.Content,
                Tittle = model.Tittle
            },
            Session.Instance.Token);

            // Respuesta.
            return (response.Response == Responses.Success) ? response.LastID : 0;
        }

        return 0;

    }



    async void SetNewColor(int color)
    {

        if (NoteDataModel == null)
            return;

        NoteDataModel.Color = color;


        ResponseBase response = new();



        if (NoteDataModel.Id > 0)
            response = await LIN.Access.Notes.Controllers.Notes.Update(NoteDataModel.Id, color, Session.Instance.Token);


        var db = new LocalDataBase.Data.NoteDB();

        _ = db.Update(new LocalDataBase.Models.Note()
        {
            Color = NoteDataModel.Color,
            Content = NoteDataModel.Content,
            Id = NoteDataModel.Id,
            Tittle = NoteDataModel.Tittle,
            IsConfirmed = response.Response == Responses.Success,
            IsDeleted = false
        });


        StateHasChanged();


    }


    async void Delete()
    {
        if (NoteDataModel == null)
            return;


        // Respuesta.
        ResponseBase response = new()
        {
            Response = Responses.Undefined
        };

        // Base de datos local.
        var localDataBase = new LocalDataBase.Data.NoteDB();

        // Respuesta de la API.
        if (NoteDataModel.Id > 0)
            response = await LIN.Access.Notes.Controllers.Notes.Delete(NoteDataModel.Id, Session.Instance.Token);

        // ELiminar en local.
        await localDataBase.DeleteOne(NoteDataModel.Id, response.Response == Responses.Success);


        Home.Notes?.Models.RemoveAll(t => t.Id == NoteDataModel.Id);
        NavigationManager.NavigateTo("Home");
        StateHasChanged();

    }



    bool IsSaving = false;

    /// <summary>
    /// Guardar los cambios.
    /// </summary>
    private async Task Save()
    {

        // Validar parámetros.
        if (NoteDataModel == null || IsSaving)
            return;

        // Establecer nuevo estado.
        IsSaving = true;


        // Respuesta.
        ResponseBase response = new()
        {
            Response = Responses.Undefined
        };

        // Base de datos local.
        var localDataBase = new LocalDataBase.Data.NoteDB();

        // Respuesta de la API.
        if (NoteDataModel.Id > 0)
            response = await Access.Notes.Controllers.Notes.Update(NoteDataModel.Id, NoteDataModel.Tittle, NoteDataModel.Content, NoteDataModel.Color, Session.Instance.Token);

        // Crear local.
        else if (NoteDataModel.Id == 0)
        {

            // Id.
            NoteDataModel.Id = new Random().Next(-10000, -2);

            // Es creado.
            int isCreated = await Create(NoteDataModel);

            // Esta confirmado
            bool isConfirmed = !(isCreated <= 0);

            // Guardar local.
            await localDataBase.Append(new()
            {
                Color = NoteDataModel.Color,
                Content = NoteDataModel.Content,
                Id = isConfirmed ? isCreated : NoteDataModel.Id,
                Tittle = NoteDataModel.Tittle,
                IsConfirmed = isConfirmed
            });

            // Agregar al home.
            Home.Notes?.Models.Add(NoteDataModel);

            // Establecer.
            IsSaving = false;
            return;
        }

        // Actualizar en local.
        await localDataBase.Update(new LocalDataBase.Models.Note()
        {
            Color = NoteDataModel.Color,
            Content = NoteDataModel.Content,
            Id = NoteDataModel.Id,
            Tittle = NoteDataModel.Tittle,
            IsConfirmed = response.Response == Responses.Success,
        });

        // Nuevo estado.
        IsSaving = false;

    }


}