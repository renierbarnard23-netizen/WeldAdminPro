using System;

namespace WeldAdminPro.Core.Models
{
	public class Category
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = "";
		public bool IsActive { get; set; }
	}
}
