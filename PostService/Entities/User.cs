using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace PostService.Entities
{
    public class User
    {
        public int Id { get; set; }

        [StringLength(50), Unicode(false)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100), Unicode(false)]
        public string Email { get; set; } = string.Empty;

        ////[StringLength(250), Unicode(false)]
        ////public string Password { get; set; } = string.Empty;

        public int Version { get; set; }
    }
}