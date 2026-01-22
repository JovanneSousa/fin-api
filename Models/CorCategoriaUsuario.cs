namespace fin_api.Models
{
    public class CorCategoriaUsuario
    {
        public string UserId { get; set; }
        public Usuario Usuario { get; set; }
        public string CategoriaId { get; set; }
        public Categoria Categoria { get; set; }

        public string CorId { get; set; }
        public Cor Cor { get; set; }
    }
}
