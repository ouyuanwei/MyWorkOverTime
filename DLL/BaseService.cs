using AutoMapper;
using AutoMapper.Mappers;
using DBHelp;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DLL
{
   public  class BaseService
    {
        private const string SoftDeleteProperty = "IsDelete";
        private const string UpdateByProperty = "UpdateBy";
        private const string UpdateTimeteProperty = "UpdateTime";
        private static readonly object LockSequence = new object();

        public UnitOfWork _unitOfWork=new UnitOfWork();

       

        public virtual BaseResponse<IList<T>> Query<S, T>(BaseRequest request, Func<IQueryable<S>, IQueryable<S>> whereAndOrderBy, ConfigurationStore mapperConfig = null) where S : class
        {
            BaseResponse<IList<T>> response = new BaseResponse<IList<T>>();
            var q = from m in _unitOfWork.GetRepository<S>().dbSet
                    select m;

            if (whereAndOrderBy != null)
            {
                q = whereAndOrderBy(q);
            }
            response.RecordsCount = q.Count();
            List<S> list = null;
            if (request != null && request.PageSize > 0)
            {
                list = q.Skip((request.CurrentPage - 1) * request.PageSize).Take(request.PageSize).ToList();
                response.PagesCount = GetPagesCount(request.PageSize, response.RecordsCount);
            }
            else
            {
                list = q.ToList();
            }
            if (mapperConfig == null)
            {
                Mapper.CreateMap<S, T>();
                response.Data = Mapper.Map<IList<T>>(list);
            }
            else
            {
                var engine = new MappingEngine(mapperConfig);
                response.Data = engine.Map<IList<T>>(list);
                engine.Dispose();
            }

            return response;
        }
        public virtual BaseResponse<T> Get<S, T>(Func<S, bool> where, ConfigurationStore mapperConfig = null)
            where S : class
            where T : class
        {
            BaseResponse<T> response = new BaseResponse<T>();
            var findItem = _unitOfWork.GetRepository<S>().dbSet.FirstOrDefault(where);
            if (findItem != null)
            {
                if (mapperConfig == null)
                {
                    Mapper.CreateMap<S, T>();
                    response.Data = Mapper.Map<T>(findItem);
                }
                else
                {
                    var engine = new MappingEngine(mapperConfig);
                    response.Data = engine.Map<T>(findItem);
                    engine.Dispose();
                }
            }
            return response;
        }
        public virtual BaseResponse<IList<T>> GetList<S, T>(Func<S, bool> where)
            where S : class
            where T : class
        {
            Mapper.CreateMap<S, T>();
            BaseResponse<IList<T>> response = new BaseResponse<IList<T>>();
            var findItem = _unitOfWork.GetRepository<S>().dbSet.Where(where);
            if (findItem != null)
            {
                response.Data = Mapper.Map<IList<T>>(findItem);
            }
            return response;
        }

        public BaseResponse<T> Save<S, T>(T request, Func<S, bool> where)
            where S : class
            where T : class
        {
            return this.Save<S, T>(request, where, null, false);
        }

        public BaseResponse<T> Save<S, T>(T request, Func<S, bool> where, List<string> fields)
            where S : class
            where T : class
        {
            return this.Save<S, T>(request, where, fields, false);
        }

        public BaseResponse<T> Save<S, T>(T request, Func<S, bool> where, List<string> fields, bool reverse)
            where S : class
            where T : class
        {
            BaseResponse<T> response = new BaseResponse<T>();
            MappingEngine engine = null;
            Mapper.CreateMap<T, S>();
            if (fields != null)
            {
                var config = CreateMapperConfig();
                var cm = config.CreateMap<T, S>();
                if (reverse)
                {
                    cm.ForAllMembers(it => it.Condition(m => !fields.Contains(m.PropertyMap.SourceMember.Name)));
                }
                else
                {
                    cm.ForAllMembers(it => it.Condition(m => fields.Contains(m.PropertyMap.SourceMember.Name)));
                }
                engine = new MappingEngine(config);
            }
            Mapper.CreateMap<S, T>();
            var model = _unitOfWork.GetRepository<S>().dbSet.FirstOrDefault(where);
            if (model == null)
            {
                if (engine == null)
                {
                    model = Mapper.Map<S>(request);
                }
                else
                {
                    model = engine.Map<S>(request);
                }

                _unitOfWork.GetRepository<S>().Insert(model);
            }
            else
            {
                if (engine == null)
                {
                    Mapper.Map(request, model);
                }
                else
                {
                    engine.Map(request, model);
                }
                _unitOfWork.GetRepository<S>().Update(model);
            }
            _unitOfWork.Save();
            Mapper.Map(model, request);
            response.Data = request;
            return response;
        }

        public BaseResponse<IList<T>> Save<S, T>(IList<T> request, Func<S, bool> where, List<string> fields = null, bool reverse = false)
            where S : class
            where T : class
        {
            BaseResponse<IList<T>> response = new BaseResponse<IList<T>>();
            var cm = Mapper.CreateMap<T, S>();
            if (fields != null)
            {
                if (reverse)
                {
                    cm.ForAllMembers(it => it.Condition(m => !fields.Contains(m.PropertyMap.SourceMember.Name)));
                }
                else
                {
                    cm.ForAllMembers(it => it.Condition(m => fields.Contains(m.PropertyMap.SourceMember.Name)));
                }
            }
            Mapper.CreateMap<S, T>();
            foreach (var item in request)
            {
                var model = _unitOfWork.GetRepository<S>().dbSet.FirstOrDefault(where);
                if (model == null)
                {
                    model = Mapper.Map<S>(item);
                    _unitOfWork.GetRepository<S>().Insert(model);
                }
                else
                {
                    Mapper.Map(item, model);
                    _unitOfWork.GetRepository<S>().Update(model);
                }
            }
            _unitOfWork.Save();
            response.Data = request;
            return response;
        }



        public virtual BaseResponse Delete<S>(object key) where S : class
        {
            BaseResponse response = new BaseResponse();
            _unitOfWork.GetRepository<S>().Delete(key);
            _unitOfWork.Save();
            return response;
        }


        public virtual int Delete<S>(Expression<Func<S, bool>> filter) where S : class
        {
            var rep = _unitOfWork.GetRepository<S>();
            var result = rep.Delete(filter);
            _unitOfWork.Save();
            return result;
        }

        public virtual BaseResponse SoftDelete<S>(object key, int updateby) where S : class
        {
            BaseResponse response = new BaseResponse();
            var softDeleteProperty = typeof(S).GetProperty(SoftDeleteProperty);
            var updateByProperty = typeof(S).GetProperty(UpdateByProperty);
            var updateTimeProperty = typeof(S).GetProperty(UpdateTimeteProperty);
            if (null == softDeleteProperty
                || null == updateByProperty
                || null == updateTimeProperty)
            {
                throw new ArrayTypeMismatchException(string.Format("{0} has no SoftDelete properties, can not be SoftDelete!", typeof(S).Name));
            }

            var toSoftDelete = _unitOfWork.GetRepository<S>().Get(key);

            softDeleteProperty.SetValue(toSoftDelete, true);
            //updateByProperty.SetValue(toSoftDelete, SecurityHelper.CurrentPrincipal.EmpNo.ToString());
            updateByProperty.SetValue(toSoftDelete, updateby);  //to-do for xiaobo fix updateby ....
            updateTimeProperty.SetValue(toSoftDelete, DateTime.Now);

            _unitOfWork.GetRepository<S>().Update(toSoftDelete);
            _unitOfWork.Save();
            return response;
        }
        public int GetPagesCount(int pageSize, int total)
        {
            if (pageSize <= 0)
            {
                return 1;
            }
            var count = total / pageSize;
            if (total % pageSize > 0)
            {
                count += 1;
            }
            return count;
        }
        protected ConfigurationStore CreateMapperConfig()
        {
            return new ConfigurationStore(new TypeMapFactory(), MapperRegistry.Mappers);
        }
    }
}
