using System;
using BussinessErp.BLL;
using BussinessErp.Models;

namespace BussinessErp.Helpers
{
    /// <summary>
    /// Centralized Role-Based Access Control (RBAC) enforcer.
    /// </summary>
    public static class RoleGuard
    {
        public static void RequiresAdmin(string actionName)
        {
            if (!AuthService.IsAdmin)
            {
                LogUnauthorized(actionName, "Admin");
                throw new UnauthorizedAccessException($"Access Denied: Action '{actionName}' requires Admin role.");
            }
        }

        public static void RequiresManager(string actionName)
        {
            if (!AuthService.IsManager)
            {
                LogUnauthorized(actionName, "Manager");
                throw new UnauthorizedAccessException($"Access Denied: Action '{actionName}' requires Manager or Admin role.");
            }
        }

        public static bool CanEditSale(Sale sale)
        {
            if (sale == null) return false;
            
            // Managers/Admins can always edit. 
            if (AuthService.IsManager) return true;

            // Employees can only edit if it's "New" (or not yet finalize-locked).
            // Since our Sale model doesn't have a 'Status' field yet, we assume a saved sale 
            // in history (with an ID > 0) is finalized for an Employee unless we add a status.
            // For now, let's treat any sale fetched from DB (ID > 0) as locked for Employees.
            if (sale.Id > 0)
            {
                LogUnauthorized($"Edit Finalized Sale #{sale.Id}", "Manager");
                return false;
            }

            return true;
        }

        private static void LogUnauthorized(string action, string requiredRole)
        {
            string user = AuthService.CurrentUser?.Username ?? "Unknown";
            string role = AuthService.CurrentRole;
            AppLogger.Warn($"UNAUTHORIZED ACCESS ATTEMPT: User '{user}' (Role: {role}) tried to '{action}'. Required: {requiredRole}.");
        }
    }
}
