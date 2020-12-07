using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciChartWrapper.Subprofiling
{
    public interface ISubprofilingTool
    {
        object Owner { get; set; }
        int No { get; set; }
        string DrawingType { get; set; }
        string Value1 { get; set; }
        string Value2 { get; set; }
        string Value3 { get; set; }
        string Value4 { get; set; }
    }
}
