using System;
using System.Collections.Generic;

namespace DataBaseCompare.Models
{
    public class CompareSettings
    {
        public LogTableConfig LogTable { get; set; }
        public DateRange CompareDateRange { get; set; }
        public List<TableConfig> Tables { get; set; }
    }

    public class LogTableConfig
    {
        public string Name { get; set; }
        public string OrderNoColumn { get; set; }
        public string DateColumn { get; set; }
    }

    public class DateRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
} 