// Author: Salar
// Created Date: 04/01/2025
/// <summary>
/// Represents the roles that can be assigned to a token.
/// </summary>
namespace RESR.WebAPI.Security;

[Flags]
public enum TokenRole
{
    /// <summary>
    /// Represents no role.
    /// </summary>
    None = 0,
    /// <summary>
    /// Represents a user role.
    /// </summary>
    User = 1,

    /// <summary>
    /// Represents a customer role.
    /// </summary>
    Customer = 2,
    /// <summary>
    /// Represents an administrator role.
    /// </summary>
    Admin = 4
}
