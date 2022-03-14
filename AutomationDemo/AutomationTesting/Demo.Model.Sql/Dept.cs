using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Demo.Model.Sql
{
    public partial class Dept
    {
        public Dept()
        {
            Users = new HashSet<User>();
        }

        [Key]
        public int Id { get; set; }
        [StringLength(255)]
        public string Name { get; set; } = null!;

        [InverseProperty(nameof(User.Dept))]
        public virtual ICollection<User> Users { get; set; }
    }
}
