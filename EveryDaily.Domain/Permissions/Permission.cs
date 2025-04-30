namespace EveryDaily.Domain.Permissions;

[Flags]
public enum Permission
{
    SuperAdmin = 1,
    User = 2,
}