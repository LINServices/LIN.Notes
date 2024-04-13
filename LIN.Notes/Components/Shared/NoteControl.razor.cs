namespace LIN.Notes.Components.Shared;

public partial class NoteControl
{

    /// <summary>
    /// Modelo de la nota.
    /// </summary>
    [Parameter]
    public NoteDataModel? Note { get; set; }



    /// <summary>
    /// Evento click sobre la nota.
    /// </summary>
    [Parameter]
    public Action<NoteDataModel?> OnClick { get; set; } = (e) => { };



    /// <summary>
    /// Obtener las clases de los colores.
    /// </summary>
    string GetClass()
    {
        return (Note?.Color) switch
        {
            1 => "bg-salmon/50 dark:bg-salmon/20",
            2 => "bg-glass/50 dark:bg-glass/20",
            3 => "bg-cream-green/50 dark:bg-cream-green/20",
            4 => "bg-cream-purple/50 dark:bg-cream-purple/20",
            _ => "bg-yell/50 dark:bg-yell/20",
        };
    }



}