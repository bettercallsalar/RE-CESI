// Author: Salar
// Created Date: 04/01/2025
/// <summary>
/// Represents the roles that can be assigned to a token.
/// </summary>
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
    User,

    /// <summary>
    /// Represents a customer role.
    /// </summary>
    Customer,
    /// <summary>
    /// Represents an administrator role.
    /// </summary>
    Admin = 4
}
