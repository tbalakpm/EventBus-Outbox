using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace PostService.Entities
{
    public class IntegrationEvent
    {
        public int Id { get; set; }

        [StringLength(255), Unicode(false)]
        public string Event { get; set; } = string.Empty;

        [Unicode(false)]
        public string Data { get; set; } = string.Empty;
    }
}