using WeldAdminPro.Core.Services.Planning;

namespace WeldAdminPro.Data.Services.ProductionEngine
{
    /*
    ==========================================================
    WeldAdmin Pro
    Planning Engine
    ----------------------------------------------------------
    Builds the AI production plan that feeds the
    Production Engine.
    ==========================================================
    */

    public class PlanningEngine
    {
        private readonly ProductionPlannerService _planner;

        public PlanningEngine()
        {
            _planner = new ProductionPlannerService();
        }
    }
}