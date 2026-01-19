namespace fin_api.Models;

public class IconeCategoriaUsuario
{
    public string UserId { get; set; }
    public Usuario Usuario { get; set; }
    public string CategoriaId { get; set; }
    public Categoria Categoria { get; set; }

    public string IconId { get; set; }
    public Icon Icone { get; set; }

}
