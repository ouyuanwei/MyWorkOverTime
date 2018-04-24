using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Filter
{
  public   class WorkRecFilter
    {
        public DateTime? StartTime { set; get; }
        public DateTime? EndTime { set; get; }
        public bool? IsUse { set; get; }

    }
}
