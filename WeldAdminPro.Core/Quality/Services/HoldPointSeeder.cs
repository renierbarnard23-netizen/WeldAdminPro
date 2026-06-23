using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Services
{
    public class HoldPointSeeder
    {
        public List<WeldHoldPoint>
            CreateDefault(Guid weldId)
        {
            return new List<WeldHoldPoint>
            {
                new WeldHoldPoint
                {
                    Id = Guid.NewGuid(),
                    WeldId = weldId,

                    HoldPointType =
                        HoldPointType.FitUpInspection,

                    Category =
                        HoldPointCategory.Hold,

                    RequiredApproverRole =
                        HoldPointApproverRole.QA,

                    Status =
                        HoldPointStatus.Pending,

                    IsMandatory = true
                },

                new WeldHoldPoint
                {
                    Id = Guid.NewGuid(),
                    WeldId = weldId,

                    HoldPointType =
                        HoldPointType.VisualInspection,

                    Status =
                        HoldPointStatus.Pending,

                    IsMandatory = true
                },

                new WeldHoldPoint
                {
                    Id = Guid.NewGuid(),
                    WeldId = weldId,

                    HoldPointType =
                        HoldPointType.NdtReview,

                    Status =
                        HoldPointStatus.Pending,

                    IsMandatory = true
                },

                new WeldHoldPoint
                {
                    Id = Guid.NewGuid(),
                    WeldId = weldId,

                    HoldPointType =
                        HoldPointType.FinalRelease,

                    Status =
                        HoldPointStatus.Pending,

                    IsMandatory = true
                }
            };
        }
    }
}