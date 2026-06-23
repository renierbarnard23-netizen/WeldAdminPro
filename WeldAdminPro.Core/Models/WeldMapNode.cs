using System;

namespace WeldAdminPro.Core.Models
{
    public class WeldMapNode
    {
        public Guid WeldId
        {
            get;
            set;
        }

        public string WeldNumber
        {
            get;
            set;
        } = string.Empty;

        public double X
        {
            get;
            set;
        }

        public double Y
        {
            get;
            set;
        }

        public string StatusColor
        {
            get;
            set;
        } = "Gray";
    }
}