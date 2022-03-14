using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Demo.Model.Sql
{
    [Table("User")]
    public partial class User
    {
        [Key]
        public int Id { get; set; }
        [StringLength(50)]
        public string FirstName { get; set; } = null!;
        [StringLength(50)]
        public string? MiddleName { get; set; }
        [StringLength(50)]
        public string LastName { get; set; } = null!;
        public short? Age { get; set; }
        public int DeptId { get; set; }

        [ForeignKey(nameof(DeptId))]
        [InverseProperty("Users")]
        public virtual Dept Dept { get; set; } = null!;
    }
}
