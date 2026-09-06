using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cdm.Models
{
    public class UserDepartment
    {
        [Key]
        public int UserDepartmentID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [ForeignKey("Department")]
        public int DepartmentID { get; set; }

        public DateTime AssignedAt { get; set; }

        public virtual User User { get; set; }
        public virtual Department Department { get; set; }
    }
}