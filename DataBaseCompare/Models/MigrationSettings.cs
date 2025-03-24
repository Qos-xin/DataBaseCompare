using System;
using System.Collections.Generic;

namespace DataBaseCompare.Models
{
    public class MigrationSettings
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<TableConfig> Tables { get; set; }
    }
} 