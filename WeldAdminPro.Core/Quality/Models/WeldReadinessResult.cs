using System;
using System.Collections.Generic;

namespace WeldAdminPro.Core.Quality.Models
{
    public class WeldReadinessResult
    {
        public Guid WeldId { get; set; }

        public bool HasWps { get; set; }

        public bool HasQualifiedWelder { get; set; }

        public bool MaterialsAvailable { get; set; }

        public bool NdtRequirementsDefined { get; set; }

        public bool RequiredDocumentsPresent { get; set; }

        public bool HoldPointsCleared { get; set; }

        public int ReadinessScore { get; set; }

        public bool IsReady { get; set; }

        public List<string> BlockingReasons
        {
            get;
            set;
        } = new();
    }
}