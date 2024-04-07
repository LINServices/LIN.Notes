namespace LIN.Notes.Components.Pages;


public partial class Note
{


    [Parameter]
    [SupplyParameterFromQuery]
    public int Id { get; set; }


    Types.Notes.Models.NoteDataModel? NoteDataModel { get; set; }



    string Content
    {
        get => NoteDataModel?.Content ?? ""; set
        {
            if (NoteDataModel == null)
                return;

            NoteDataModel.Content = value;

        }
    }


    protected override void OnParametersSet()
    {
        var note = Home.Notas?.Models?.Where(t => t.Id == Id).FirstOrDefault();

        if (note == null)
        {
            NavigationManager.NavigateTo("/home");
            return;
        }

        NoteDataModel = note;



        base.OnParametersSet();
    }

    void Back()
    {
        JS.InvokeVoidAsync("BackLast");
    }

    string GetClass()
    {


        switch (NoteDataModel?.Color)
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

    async void Input(Microsoft.AspNetCore.Components.ChangeEventArgs e)
    {

        string value = e.Value?.ToString() ?? "";

        if (NoteDataModel == null)
            return;

        await LIN.Access.Notes.Controllers.Notes.Update(NoteDataModel.Id, NoteDataModel.Tittle, value, NoteDataModel.Color, LIN.Access.Notes.Session.Instance.Token);

    }

}