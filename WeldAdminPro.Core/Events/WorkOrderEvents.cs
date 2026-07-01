using System;

namespace WeldAdminPro.Core.Events
{
    public static class WorkOrderEvents
    {
        public static event Action? Changed;

        public static void RaiseChanged()
        {
            Changed?.Invoke();
        }
    }
}