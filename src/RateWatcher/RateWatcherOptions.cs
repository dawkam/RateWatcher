using System;
using System.Collections.Generic;
using System.Text;

namespace RateWatcher
{
    public sealed class RateWatcherOptions
    {
        public string[] Currency { get; set; } = [];
        public int RefreshInterval { get; set; }
        public decimal ValueChangeThreshold { get; set; }
    }
}
