using LIN.Access.Notes.Hubs;
using LIN.Notes.Components.Pages;
using SILF.Script.Interfaces;

namespace LIN.Notes.Services;

internal class Realtime
{

    /// <summary>
    /// Id del dispositivo.
    /// </summary>
    public static string DeviceName { get; set; } = string.Empty;


    /// <summary>
    /// Id del dispositivo.
    /// </summary>
    public static string DeviceKey { get; private set; } = string.Empty;


    /// <summary>
    /// Funciones
    /// </summary>
    public static List<IFunction> Actions { get; set; } = [];


    /// <summary>
    /// Hub de tiempo real.
    /// </summary>
    public static NotesHub? NotesHub { get; set; } = null;


    /// <summary>
    /// Iniciar el servicio.
    /// </summary>
    public static void Start()
    {

        // Validar si ya existe el hub.
        if (NotesHub != null)
            return;

        // Llave.
        if (string.IsNullOrWhiteSpace(DeviceKey))
            DeviceKey = Guid.NewGuid().ToString();

        // Generar nuevo hub.
        NotesHub = new(SessionManager.Instance.Default.Token);

        _ = NotesHub.Suscribe();

        // Evento.
        NotesHub.OnReceiveCommand += OnReceiveCommand;

    }


    /// <summary>
    /// Construye las funciones.
    /// </summary>
    public static void Build()
    {
        // Función de actualizar contactos.
        SILFFunction updateColor = new((values) =>
        {
            // Obtener el parámetro.
            var value = values.FirstOrDefault(t => t.Name == "id")?.Objeto.GetValue();

            // Validar el tipo.
            if (value is not decimal)
                return;

            // Id.
            int id = (int)((value as decimal?) ?? 0);


            value = values.FirstOrDefault(t => t.Name == "color")?.Objeto.GetValue();

            int color = (int)((value as decimal?) ?? 0);

            Home.Instance.UpdateColor(id, color);

        })
        // Propiedades
        {
            Name = "updateColor",
            Parameters =
            [
                new("id", new("number")),
                new("color", new("number"))
            ]
        };

        // Función de actualizar contactos.
        SILFFunction update = new(async (values) =>
        {
            // Obtener el parámetro.
            var value = values.FirstOrDefault(t => t.Name == "id")?.Objeto.GetValue();

            // Validar el tipo.
            if (value is not decimal)
                return;

            // Id.
            int id = (int)((value as decimal?) ?? 0);


            var x = await LIN.Access.Notes.Controllers.Notes.Read(id, SessionManager.Instance.Default.Token);

            if (x.Response != Responses.Success)
                return;
            Home.Instance.Update(id, x.Model.Content);

        })
        // Propiedades
        {
            Name = "update",
            Parameters =
            [
                new("id", new("number"))
            ]
        };


        // Función de actualizar contactos.
        SILFFunction add = new(async (values) =>
        {
            // Obtener el parámetro.
            var value = values.FirstOrDefault(t => t.Name == "id")?.Objeto.GetValue();

            // Validar el tipo.
            if (value is not decimal)
                return;

            // Id.
            int id = (int)((value as decimal?) ?? 0);


            var x = await LIN.Access.Notes.Controllers.Notes.Read(id, SessionManager.Instance.Default.Token);

            if (x.Response != Responses.Success)
                return;

            Home.Instance.Add(x.Model);

        })
        // Propiedades
        {
            Name = "add",
            Parameters =
            [
                new("id", new("number"))
            ]
        };

        // Eliminar
        SILFFunction remove = new(async (values) =>
        {

            // Obtener el parámetro.
            var value = values.FirstOrDefault(t => t.Name == "id")?.Objeto.GetValue();

            // Validar el tipo.
            if (value is not decimal)
                return;

            // Id.
            int id = (int)((value as decimal?) ?? 0);


            Home.Instance.RemoveOne(id);

        })
        // Propiedades
        {
            Name = "delete",
            Parameters =
            [
                new("id", new("number"))
            ]
        };

        Actions = [updateColor, remove, update, add];

    }


    /// <summary>
    /// Evento al recibir un comando.
    /// </summary>
    /// <param name="e">Comando</param>
    private static void OnReceiveCommand(object? sender, Types.Notes.Transient.CommandModel e)
    {

        // Generar la app.
        var app = new SILF.Script.App(e.Command);

        // Agregar funciones del framework de Inventory.
        app.AddDefaultFunctions(Actions);

        // Ejecutar app.
        app.Run();

    }


    /// <summary>
    /// Cerrar conexión.
    /// </summary>
    public static void Close()
    {
        DeviceKey = string.Empty;
        NotesHub = null;
    }

}