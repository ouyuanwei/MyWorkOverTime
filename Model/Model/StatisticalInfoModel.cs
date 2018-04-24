using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Model
{
   public  class StatisticalInfoModel
    {

        public int? AllHour { set; get; }
        public int? UseHour { set; get; }
        public int? OverdueHour { set; get; }
        public int? EffectiveHour { set; get; }
        public List<WorkRecModel> RecList { set; get; }

    }
}
