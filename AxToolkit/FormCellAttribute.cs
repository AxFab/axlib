// This file is part of AxLib.
// 
// AxLib is free software: you can redistribute it and/or modify it under the
// terms of the GNU General Public License as published by the Free Software 
// Foundation, either version 3 of the License, or (at your option) any later 
// version.
// 
// AxLib is distributed in the hope that it will be useful, but WITHOUT ANY 
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more 
// details.
// 
// You should have received a copy of the GNU General Public License along 
// with AxLib. If not, see <https://www.gnu.org/licenses/>.
// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
namespace AxToolkit;

public abstract class FormCellAttribute : Attribute
{
    protected FormCellAttribute(int order, string label) : this(order, label, 0) { }
    protected FormCellAttribute(int order, string label, int group)
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
    public FormEntryCellAttribute(int order, string label) : this(order, label, 0) { }
    public FormEntryCellAttribute(int order, string label, int group)
        : base(order, label, group)
    {
    }

    public string Prefix { get; set; }
    public string Sufix { get; set; }
}

public class FormEnumCellAttribute : FormCellAttribute
{
    public FormEnumCellAttribute(int order, string label) : this(order, label, 0) { }
    public FormEnumCellAttribute(int order, string label, int group)
        : base(order, label, group)
    {
    }

    public string[] Values { get; set; }
}
public class FormPrintCellAttribute : FormCellAttribute
{
    public FormPrintCellAttribute(int order, string label) : this(order, label, null, 0) { }
    public FormPrintCellAttribute(int order, string label, string format) : this(order, label, format, 0) { }
    public FormPrintCellAttribute(int order, string label, string format, int group)
        : base(order, label, group)
    {
        Format = format;
    }

    public string Format { get; set; }
}
