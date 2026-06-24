using System;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public class WeldReadinessEngine
        : IWeldReadinessEngine
    {
        public WeldReadinessResult Evaluate(
            Weld weld)
        {
            var result =
                new WeldReadinessResult
                {
                    WeldId = weld.Id
                };

            return result;
        }
    }
}