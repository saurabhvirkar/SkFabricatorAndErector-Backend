namespace SkFabricatorAndErector.Domain.Constants;

public static class Permissions
{
    public const string ClaimType = "permission";

    public static class Projects
    {
        public const string Read = "Projects.Read";
        public const string Create = "Projects.Create";
        public const string Update = "Projects.Update";
        public const string Delete = "Projects.Delete";
    }

    public static class Services
    {
        public const string Read = "Services.Read";
        public const string Create = "Services.Create";
        public const string Update = "Services.Update";
        public const string Delete = "Services.Delete";
    }

    public static class Team
    {
        public const string Read = "Team.Read";
        public const string Create = "Team.Create";
        public const string Update = "Team.Update";
        public const string Delete = "Team.Delete";
    }

    public static class Gallery
    {
        public const string Read = "Gallery.Read";
        public const string Create = "Gallery.Create";
        public const string Delete = "Gallery.Delete";
    }

    public static class Clients
    {
        public const string Read = "Clients.Read";
        public const string Create = "Clients.Create";
        public const string Update = "Clients.Update";
        public const string Delete = "Clients.Delete";
    }

    public static class HomeSlider
    {
        public const string Read = "HomeSlider.Read";
        public const string Create = "HomeSlider.Create";
        public const string Delete = "HomeSlider.Delete";
    }

    public static class Inquiries
    {
        public const string Read = "Inquiries.Read";
        public const string Delete = "Inquiries.Delete";
    }

    public static class Users
    {
        public const string Read = "Users.Read";
        public const string Create = "Users.Create";
        public const string Update = "Users.Update";
        public const string Disable = "Users.Disable";
    }

    public static class Roles
    {
        public const string Read = "Roles.Read";
        public const string Assign = "Roles.Assign";
    }

    public static class Audit
    {
        public const string Read = "Audit.Read";
    }

    public static class Security
    {
        public const string Manage = "Security.Manage";
    }
}
