using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace SQLProj.Models
{
    public class Employee
    {
        public Employee()
        {
        }

        public Employee(SqlDataReader reader)
        {
            Reader = reader;
        }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        public SqlDataReader Reader { get; }
    }
}
