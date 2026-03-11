using RESR.Models.Departments;

namespace RESR.Core.Controllers.Departments.Factories;

public interface IDepartmentFactory
{
    Department Create(int idDepartment, string name, int code);
}