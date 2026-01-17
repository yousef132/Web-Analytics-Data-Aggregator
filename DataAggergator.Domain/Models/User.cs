using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAggergator.Domain.Models
{
    public class User
    {
        public int Id { get; set; }

        // Keycloak user id (UUID)
        public string KeycloakUserId { get; set; } = default!;

        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;

        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}

