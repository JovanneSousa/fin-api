namespace Fin.Domain.Models
{
    public class UserHiddenCategory
    {
        public string UserId { get; set; }
        public Usuario Usuario { get; set; }
        public string CategoryId { get; set; }
        public Categoria Category { get; set; }
    }
}
