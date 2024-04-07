using LIN.Types.Notes.Models;

namespace LIN.Notes.Components.Shared;

public partial class NoteControl
{

    [Parameter]
    public NoteDataModel? Note { get; set; }

}