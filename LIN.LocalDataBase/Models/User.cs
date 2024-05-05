namespace LIN.LocalDataBase.Models;


public class User
{

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unique { get; set; } = string.Empty;
    public byte[] Profile { get; set; } = [];
    public string Password { get; set; } = string.Empty;

}
