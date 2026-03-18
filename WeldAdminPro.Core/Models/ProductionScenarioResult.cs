using System;
using System.Collections.Generic;
using WeldAdminPro.Core.Analytics.Production;

public class ProductionScenarioResult
{
	public List<ProductionCompletionPrediction> Predictions { get; set; } = new();
	public int LateJobs { get; set; }
	public int TotalDelayDays { get; set; }


}