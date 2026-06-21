namespace Fin.Domain.Models;

public class Icon : Entity
{
    public string Name { get; set; }
    public string Url { get; set; }
    public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
}
