using LIN.Types.Notes.Models;

namespace LIN.Notes.Components.Shared;

public partial class NoteControl
{

    [Parameter]
    public NoteDataModel? Note { get; set; }


    [Parameter]
    public Action<NoteDataModel?> OnClick { get; set; } = (e) => { };


    string GetClass()
    {


        switch (Note?.Color)
        {
            case 1:
                return "bg-salmon/50 dark:bg-salmon/20";
            case 2:
                return "bg-glass/50 dark:bg-glass/20";
            case 3:
                return "bg-cream-green/50 dark:bg-cream-green/20";
        }


        return "bg-yell/50 dark:bg-yell/20";

    }



}