namespace LIN.Notes.Components.Elements;


public partial class BottomNavigation
{


    /// <summary>
    /// Sección actual de la barra de navegación.
    /// </summary>
    [Parameter]
    public DockSettings Settings { get; set; } = new();



    /// <summary>
    /// Elemento SVG del botón central.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }




    /// <summary>
    /// Ir a la sección.
    /// </summary>
    /// <param name="section">Numero de la sección.</param>
    void GoSection(int section)
    {

        if (section == 0)
            navigationManager.NavigateTo("/home");

        else if (section == 1)
            navigationManager.NavigateTo("/inventory");

        else if (section == 2)
            navigationManager.NavigateTo("/contacts");

        else if (section == 3)
            navigationManager.NavigateTo("/account");

    }


}

public class DockSettings
{

    public int Section { set; get; }
    public int DockIcon { set; get; }

    public Action OnCenterClick { get; set; } = () => { };

}