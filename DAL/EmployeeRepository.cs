using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BussinessErp.Helpers;
using BussinessErp.Models;

namespace BussinessErp.DAL
{
    public class EmployeeRepository
    {
        public async Task<List<Employee>> GetAllAsync()
        {
            var list = new List<Employee>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("SELECT Id, Name, Phone, Salary, HireDate, Department, Position FROM Employees ORDER BY Name", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.Add(new Employee
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Phone = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Salary = reader.GetDecimal(3),
                        HireDate = reader.GetDateTime(4),
                        Department = reader.IsDBNull(5) ? "" : reader.GetString(5),
                        Position = reader.IsDBNull(6) ? "" : reader.GetString(6)
                    });
                }
            }
            return list;
        }

        public async Task<Employee> GetByIdAsync(int id)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("SELECT Id, Name, Phone, Salary, HireDate, Department, Position FROM Employees WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Employee
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Phone = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Salary = reader.GetDecimal(3),
                            HireDate = reader.GetDateTime(4),
                            Department = reader.IsDBNull(5) ? "" : reader.GetString(5),
                            Position = reader.IsDBNull(6) ? "" : reader.GetString(6)
                        };
                    }
                }
            }
            return null;
        }

        public async Task<int> AddAsync(Employee emp)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                @"INSERT INTO Employees (Name, Phone, Salary, HireDate, Department, Position)
                  VALUES (@Name, @Phone, @Salary, @HireDate, @Department, @Position);
                  SELECT SCOPE_IDENTITY();", conn))
            {
                cmd.Parameters.AddWithValue("@Name", emp.Name);
                cmd.Parameters.AddWithValue("@Phone", (object)emp.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Salary", emp.Salary);
                cmd.Parameters.AddWithValue("@HireDate", emp.HireDate);
                cmd.Parameters.AddWithValue("@Department", (object)emp.Department ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Position", (object)emp.Position ?? DBNull.Value);
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }

        public async Task UpdateAsync(Employee emp)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                @"UPDATE Employees SET Name=@Name, Phone=@Phone, Salary=@Salary,
                  HireDate=@HireDate, Department=@Department, Position=@Position WHERE Id=@Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", emp.Id);
                cmd.Parameters.AddWithValue("@Name", emp.Name);
                cmd.Parameters.AddWithValue("@Phone", (object)emp.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Salary", emp.Salary);
                cmd.Parameters.AddWithValue("@HireDate", emp.HireDate);
                cmd.Parameters.AddWithValue("@Department", (object)emp.Department ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Position", (object)emp.Position ?? DBNull.Value);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand("DELETE FROM Employees WHERE Id=@Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<Employee>> SearchAsync(string keyword)
        {
            var list = new List<Employee>();
            using (var conn = await DatabaseHelper.GetConnectionAsync())
            using (var cmd = new SqlCommand(
                "SELECT Id, Name, Phone, Salary, HireDate, Department, Position FROM Employees WHERE Name LIKE @Keyword OR Department LIKE @Keyword OR Position LIKE @Keyword ORDER BY Name", conn))
            {
                cmd.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new Employee
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Phone = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Salary = reader.GetDecimal(3),
                            HireDate = reader.GetDateTime(4),
                            Department = reader.IsDBNull(5) ? "" : reader.GetString(5),
                            Position = reader.IsDBNull(6) ? "" : reader.GetString(6)
                        });
                    }
                }
            }
            return list;
        }
    }
}
