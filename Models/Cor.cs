namespace fin_api.Models
{
    public class Cor : Entity
    {
        public string Url { get; set; }
        public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
    }
}
