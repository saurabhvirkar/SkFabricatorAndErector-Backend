namespace SkFabricatorAndErector.Domain.Constants;

public static class UserRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Employee = "Employee";
    public const string Customer = "Customer";

    public const string AdminOrManager = "Admin,Manager";
    public const string ElevateRoles = "SuperAdmin,Admin";
}
