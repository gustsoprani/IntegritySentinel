using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegritySentinel.Worker.Configuration;

public class MonitorSettings
{
    public string TargetPath { get; set; } = string.Empty;
    public int IntervalInSeconds { get; set; } = 5;
    public string IgnoredExtensions { get; set; } = string.Empty;
}
