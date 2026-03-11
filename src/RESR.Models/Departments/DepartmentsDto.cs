namespace RESR.Models.Departments;

public sealed record DepartmentResponse(
    int IdDepartment,
    string Name,
    int Code
);