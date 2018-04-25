using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDLL;
using DBHelp;
using System.Data.Entity;
using Model;
using Model.Filter;
using Model.Model;

namespace DLL
{
    public class WorkRecService : BaseService, IWorkRecService
    {
        public BaseResponse<StatisticalInfoModel> QueryRec(BaseRequest<WorkRecFilter> request)
        {
            var resquest = new BaseResponse<StatisticalInfoModel>();
            try
            {
                var pageSize = request.PageSize;
                request.PageSize = -1;
                var x= base.Query<workrec, WorkRecModel>(request, (q) =>
                  {
                      if (request.Data.EndTime.HasValue) q = q.Where(m => m.EndTime < request.Data.EndTime);
                      if (request.Data.StartTime.HasValue) q = q.Where(m => m.StartTime >= request.Data.StartTime);
                      if (request.Data.IsUse == true) q = q.Where(m => m.IsUse == true);
                      else if(request.Data.IsUse == false) q = q.Where(m => m.IsUse == false);
                      q = q.Where(m => m.IsDelete != true);
                      q = q.OrderBy(m => m.StartTime);
                      return q;
                  }).Data;
                resquest.Data = new StatisticalInfoModel();
                resquest.Data.RecList =x.ToList();
                resquest.RecordsCount = x.Count;
                var date = DateTime.Now.AddMonths(-1);

                resquest.Data.AllHour = x.Sum(m => m.Hour);
                resquest.Data.UseHour = x.Where(m => m.IsUse == true).Sum(m => m.Hour);
                resquest.Data.OverdueHour = x.Where(m => m.IsUse != true && m.StartTime < date).Sum(m => m.Hour);
                resquest.Data.EffectiveHour= x.Where(m => m.IsUse != true && m.StartTime >= date).Sum(m => m.Hour);

                request.PageSize = pageSize;
                if (request!=null && request.PageSize > 0)
                {
                    resquest.Data.RecList = x.Skip((request.CurrentPage - 1) * request.PageSize).Take(request.PageSize).ToList();
                    resquest.PagesCount = GetPagesCount(request.PageSize, resquest.RecordsCount);
                }
            }
            catch (Exception ex)
            {
                resquest.ResultCode = -1;
                resquest.ResultMessage = "操作失败";
            }
            return resquest;
        }

        public BaseResponse SaveRec(WorkRecModel request)
        {
            return base.Save<workrec, WorkRecModel>(request, q => q.Id == request.Id);
        }
    }
}
