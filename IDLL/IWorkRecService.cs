﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;
using Model.Model;
using Model.Filter;

namespace IDLL
{
   public  interface IWorkRecService
    {
       BaseResponse<StatisticalInfoModel>  QueryRec(BaseRequest<WorkRecFilter> request );
        BaseResponse SaveRec(WorkRecModel request);
    }
}
