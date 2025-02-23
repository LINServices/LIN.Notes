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
    /// Titulo de la nota.
    /// </summary>
    private string Tittle { get; set; } = string.Empty;


    /// <summary>
    /// Contenido.
    /// </summary>
    private string Content { get; set; } = string.Empty;


    /// <summary>
    /// Contador.
    /// </summary>
    private int _counter = 0;


    /// <summary>
    /// Modal de eliminar.
    /// </summary>
    private DeleteModal? DeletePop { get; set; }


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
    private async void Back() => await JSRuntime.InvokeVoidAsync("backLast");


    /// <summary>
    /// Establecer el nuevo color.
    /// </summary>
    private void SetColor()
    {
        GetClass();
    }


    /// <summary>
    /// Obtener las clases.
    /// </summary>
    private string GetClass()
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
            case 4:
                MauiProgram.Set([198, 187, 217], [28, 25, 36]);
                return "bg-cream-purple/50 dark:bg-cream-purple/20";
        }

        MauiProgram.Set([251, 233, 165], [49, 42, 15]);
        return "bg-yell/50 dark:bg-yell/20";

    }


    /// <summary>
    /// Input.
    /// </summary>
    private async void Input(ChangeEventArgs e)
    {

        // Incrementar contador.
        _counter += 1;

        // Contador local.
        int localCounter = _counter;

        // Esperar.
        await Task.Delay(400);

        // Si en la espera el contador cambio, no hace nada.
        if (_counter != localCounter || NoteDataModel is null)
            return;

        // Obtener nuevo valor de para la nota.
        string value = e.Value?.ToString() ?? "";

        // Establecer nota.
        NoteDataModel.Content = value;

        await Save();

    }


    /// <summary>
    /// Maneja el evento de cambio de texto en el input del título.
    /// </summary>
    private async void InputTittle(Microsoft.AspNetCore.Components.ChangeEventArgs e)
    {

        // Incrementa el contador global y almacena el valor del contador localmente.
        int localCounter = ++_counter;

        // Espera 500 milisegundos.
        await Task.Delay(500);

        // Si el contador global ha cambiado durante la espera, sale del método.
        if (_counter != localCounter)
            return;

        // Obtiene el nuevo valor del input.
        string value = e.Value?.ToString() ?? "";

        // Si el modelo de datos de la nota es nulo, sale del método.
        if (NoteDataModel == null)
            return;

        // Actualiza el título de la nota con el nuevo valor.
        NoteDataModel.Tittle = value;

        // Guarda los cambios.
        await Save();
    }


    /// <summary>
    /// Crear nota en la API.
    /// </summary>
    /// <param name="model">Modelo.</param>
    private static async Task<int> Create(NoteDataModel model)
    {

        // Acceso.
        var access = Connectivity.Current.NetworkAccess;

        // Respuesta.
        CreateResponse response;

        // Validar el acceso a internet.
        if (access == NetworkAccess.Internet)
        {
            // Solicitud a la API.
            response = await LIN.Access.Notes.Controllers.Notes.Create(new NoteDataModel()
            {
                Color = model.Color,
                Content = model.Content,
                Tittle = model.Tittle
            },
            SessionManager.Instance.Default.Token);

            // Respuesta.
            return (response.Response == Responses.Success) ? response.LastId : 0;
        }

        return 0;

    }









    private async void SetNewColor(int color)
    {

        if (NoteDataModel == null)
            return;

        NoteDataModel.Color = color;


        ResponseBase response = new();



        if (NoteDataModel.Id > 0)
            response = await LIN.Access.Notes.Controllers.Notes.Update(NoteDataModel.Id, color, SessionManager.Instance.Default.Token);


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

    private async void Delete()
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
            response = await LIN.Access.Notes.Controllers.Notes.Delete(NoteDataModel.Id, SessionManager.Instance.Default.Token);

        // ELiminar en local.
        await localDataBase.DeleteOne(NoteDataModel.Id, response.Response == Responses.Success);

        if (response.Response == Responses.Success)
        {
            Home.Notes?.Models.RemoveAll(t => t.Id == NoteDataModel.Id);
            NavigationManager.NavigateTo("Home");
            StateHasChanged();
        }

        // Mostrar error


    }

    private bool IsSaving = false;

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

        // Si la nota ya existe, la actualiza.
        if (NoteDataModel.Id > 0)
        {
            var responseUpdate = await Access.Notes.Controllers.Notes.Update(NoteDataModel, SessionManager.Instance.Default.Token);
            NoteDataModel.Language = responseUpdate.Model;
            response = responseUpdate;
        }

        // Crear local.
        else if (NoteDataModel.Id == 0)
        {

            // Id.
            NoteDataModel.Id = new Random().Next(-100000, -2);

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

            // Si esta confirmado.
            if (isConfirmed)
            {
                NoteDataModel.Id = isCreated;
                var lang = await LIN.Access.Notes.Controllers.Notes.ReadLang(isCreated, SessionManager.Instance.Default.Token);
                NoteDataModel.Language = lang.Model;
            }

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
            Language = (int)NoteDataModel.Language,
            IsConfirmed = response.Response == Responses.Success
        });

        // Nuevo estado.
        IsSaving = false;

    }


}