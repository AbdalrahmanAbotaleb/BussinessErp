using System;

namespace BussinessErp.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public decimal Salary { get; set; }
        public string Position { get; set; }
        public DateTime HireDate { get; set; }
        public string Department { get; set; }
    }
}
