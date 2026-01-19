namespace fin_api.Models;

public class Icon : Entity
{
    public string Name { get; set; }
    public string Url { get; set; }
    public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
}
