using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Model
{
  public  class WorkRecModel
    {
        public int Id { set; get; }
        public DateTime? StartTime { set; get; }
        public DateTime? EndTime { set; get; }
        public int? Hour { set; get; }
        public bool IsUse { set; get; }
        public string Rmark { set; get; }
        public bool? IsDelete { set; get; }
    }
}
