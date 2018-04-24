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
        public BaseResponse<List<WorkRecModel>> QueryRec(BaseRequest<WorkRecFilter> request)
        {
            var resquest = new BaseResponse<List<WorkRecModel>>();
            try
            {
                base.Query<workrec, WorkRecModel>(request, (q) =>
                {

                    if (request.Data.EndTime.HasValue) q = q.Where(m => m.EndTime < request.Data.EndTime.Value.AddDays(1));
                    if (request.Data.StartTime.HasValue) q = q.Where(m => m.StartTime >= request.Data.StartTime);
                    if (request.Data.IsUse == true) q = q.Where(m => m.IsUse == true);
                    else(request.Data.IsUse==false) q = q.Where(m => m.IsUse == false);
                    q = q.Where(m => m.IsDelete != true);
                    return q;
                });
            }
            catch (Exception ex)
            {

            }

            return resquest;
        }
    }
}
