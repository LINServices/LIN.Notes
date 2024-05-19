namespace LIN.Notes.Components.Elements;


public partial class DeletePopup
{



    [Parameter]
    public string Content { get; set; }


    /// <summary>
    /// Acción al presionar sobre el botón de editar.
    /// </summary>
    [Parameter]
    public Action OnAccept { get; set; } = () => { };


    [Parameter]
    public Action OnCancel { get; set; } = () => { };



    /// <summary>
    /// Key.
    /// </summary>
    private string Key { get; init; } = Guid.NewGuid().ToString();




    /// <summary>
    /// Abrir el modal.
    /// </summary>
    public void Show()
    {
        _ = InvokeAsync(() =>
        {
            StateHasChanged();
            Js.InvokeVoidAsync("ShowModal", $"popup-modal-{Key}", $"btn-accept-{Key}", $"btn-cancel-{Key}", $"btn-close-{Key}");

        });

    }


}