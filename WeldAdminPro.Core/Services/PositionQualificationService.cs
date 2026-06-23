using System;
using System.Collections.Generic;
using System.Linq;

namespace WeldAdminPro.Core.Services
{
    public class PositionQualificationService
    {
        private readonly Dictionary<string, List<string>> _qualificationMatrix =
            new(StringComparer.OrdinalIgnoreCase)
            {
                // =====================================
                // GROOVE QUALIFICATIONS
                // =====================================

                { "1G", new List<string> { "1G" } },

                { "2G", new List<string> { "2G" } },

                { "3G", new List<string> { "3G" } },

                { "4G", new List<string> { "4G" } },

                { "5G", new List<string>
                    {
                        "1G",
                        "2G",
                        "5G"
                    }
                },

                { "6G", new List<string>
                    {
                        "1G",
                        "2G",
                        "3G",
                        "4G",
                        "5G",
                        "6G"
                    }
                },

                // =====================================
                // SPECIAL
                // =====================================

                { "ALL", new List<string>
                    {
                        "1G",
                        "2G",
                        "3G",
                        "4G",
                        "5G",
                        "6G"
                    }
                }
            };

        public bool IsQualified(
    string requiredPosition,
    string qualifiedPosition)
        {

            System.Diagnostics.Debug.WriteLine(
    $"Required Position = [{requiredPosition}]");

            System.Diagnostics.Debug.WriteLine(
                $"Qualified Position = [{qualifiedPosition}]");

            requiredPosition =
                requiredPosition?
                .Trim()
                .ToUpper() ?? "";

            qualifiedPosition =
                qualifiedPosition?
                .Trim()
                .ToUpper() ?? "";

            if (string.IsNullOrWhiteSpace(requiredPosition))
                return true;

            if (string.IsNullOrWhiteSpace(qualifiedPosition))
                return true;

            // Exact match
            if (requiredPosition == qualifiedPosition)
                return true;

            // ALL qualifies everything
            if (qualifiedPosition == "ALL")
                return true;

            // Required is ALL
            if (requiredPosition == "ALL")
            {
                return qualifiedPosition switch
                {
                    "6G" => true,
                    _ => false
                };
            }

            // ASME expansions
            return qualifiedPosition switch
            {
                "6G" =>
                    true,

                "5G" =>
                    requiredPosition is
                        "1G" or
                        "2G" or
                        "5G",

                "3G" =>
                    requiredPosition is
                        "1G" or
                        "2G" or
                        "3G",

                _ =>
                    false
            };
        }

        public List<string> GetQualifiedPositions(
            string pqrPosition)
        {
            if (string.IsNullOrWhiteSpace(pqrPosition))
                return new List<string>();

            if (_qualificationMatrix.TryGetValue(
                pqrPosition.ToUpper(),
                out var positions))
            {
                return positions;
            }

            return new List<string>();
        }
    }
}