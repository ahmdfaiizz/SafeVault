
using System;

namespace SafeVault
{
    public class AuthorizationService
    {
        public bool CanAccessAdminDashboard(AuthenticatedUser? user)
        {
            return user != null &&
                   string.Equals(
                       user.Role,
                       "admin",
                       StringComparison.Ordinal
                   );
        }

        public bool CanAccessUserDashboard(AuthenticatedUser? user)
        {
            return user != null &&
                   (
                       string.Equals(
                           user.Role,
                           "user",
                           StringComparison.Ordinal
                       )
                       ||
                       string.Equals(
                           user.Role,
                           "admin",
                           StringComparison.Ordinal
                       )
                   );
        }
    }
}
