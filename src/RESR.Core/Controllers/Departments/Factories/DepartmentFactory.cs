using RESR.Models.Departments;

namespace RESR.Core.Controllers.Departments.Factories;

public sealed class DepartmentFactory : IDepartmentFactory
{
    public Department Create(int idDepartment, string name, int code) =>
        new()
        {
            IdDepartment = idDepartment,
            Name = name,
            Code = code
        };
}