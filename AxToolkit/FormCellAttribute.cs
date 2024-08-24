using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxToolkit
{
    public abstract class FormCellAttribute : Attribute
    {
        public FormCellAttribute(int order, string label, int group = 0)
        {
            Order = order;
            Label = label;
            Group = group;
        }

        public int Order { get; set; }
        public int Group { get; set; }
        public string Label { get; set; }
    }

    public class FormEntryCellAttribute : FormCellAttribute
    {
        public FormEntryCellAttribute(int order, string label, int group = 0)
            : base(order, label, group)
        {
        }

        public string Prefix { get; set; }
        public string Sufix { get; set; }
    }

    public class FormEnumCellAttribute : FormCellAttribute
    {
        public FormEnumCellAttribute(int order, string label, int group = 0)
            : base(order, label, group)
        {
        }

        public string[] Values { get; set; }
    }
    public class FormPrintCellAttribute : FormCellAttribute
    {
        public FormPrintCellAttribute(int order, string label, string format = null, int group = 0)
            : base(order, label, group)
        {
            Format = format;
        }

        public string Format { get; set; }
    }
}
