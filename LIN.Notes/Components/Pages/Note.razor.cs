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


        MainPage.OnColor = SetColor;

        NoteDataModel = note;



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

        await LIN.Access.Notes.Controllers.Notes.Update(NoteDataModel.Id, NoteDataModel.Tittle, value, NoteDataModel.Color, LIN.Access.Notes.Session.Instance.Token);

    }


}