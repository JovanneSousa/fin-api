using Microsoft.AspNetCore.Identity;

namespace fin_api.Models
{
    public class UserHiddenCategory
    {
        public string UserId { get; set; }
        public string CategoryId { get; set; }
        public Categoria Category { get; set; }
    }
}
