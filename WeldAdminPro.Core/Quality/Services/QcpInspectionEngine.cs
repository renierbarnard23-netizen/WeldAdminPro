using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public class QcpInspectionEngine
    {
        private readonly Random
            _random = new();

        public bool RequiresInspection(
            Weld weld,
            QcpInspectionRule rule)
        {
            if (weld.WeldType
                != rule.WeldType)
            {
                return false;
            }

            var randomValue =
                _random.NextDouble() * 100;

            return randomValue
                <= rule.InspectionPercentage;
        }
    }
}