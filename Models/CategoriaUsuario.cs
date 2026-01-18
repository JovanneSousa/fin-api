namespace fin_api.Models;

public class CategoriaUsuario
{
    public string UserId { get; set; }
    public Usuario Usuario { get; set; }
    public string CategoriaId { get; set; }
    public Categoria Categoria { get; set; }

    public string? IconId { get; set; }
    public Icon? Icon { get; set; }

}
