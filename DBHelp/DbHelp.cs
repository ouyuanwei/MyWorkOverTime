using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Reflection;
using System.Collections;
using System.Data.Entity;
using System.Data;
using System.Linq.Expressions;
using System.Data.Entity.Validation;

namespace DBHelp
{
    public class DbHelp 
    {
        private ECContext context = new ECContext();
        private Stack saveList = new Stack();

        public GenericRepository<T> GetRepository<T>() where T : class
        {
            return new GenericRepository<T>(context);
        }
    }
    public class UnitOfWork : IDisposable
    {
        private ECContext context = new ECContext();
        private Stack saveList = new Stack();

        public GenericRepository<T> GetRepository<T>() where T : class
        {
            return new GenericRepository<T>(context);
        }

        public void BeginTransaction()
        {
            this.saveList.Push(null);
        }

        public void Commit()
        {
            if (this.saveList.Count > 0)
            {
                this.saveList.Pop();
            }
            this.Save();
        }

        public void Save()
        {
            try
            {
                if (this.saveList.Count == 0)
                {
                    this.context.SaveChanges();
                }
            }
            catch (DbEntityValidationException dbEve)
            {
                StringBuilder exMsg = new StringBuilder();
                exMsg.AppendLine("");
                foreach (var item in dbEve.EntityValidationErrors)
                {
                    exMsg.AppendLine(item.Entry.Entity.ToString());
                    foreach (var v in item.ValidationErrors)
                    {
                        exMsg.AppendLine(v.ErrorMessage);
                    }
                }
                throw new Exception(exMsg.ToString());
            }
            catch (Exception ex)
            {
                //其他种类的exception没有被catch, 导致开发人员定位不到错误, 暂时加上,便于调试
                //TODO 增加LOG记录错误信息, 替代throw ex;
                throw ex;
            }
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void RollBack()
        {
            this.context.ResetChanges();
            if (this.saveList.Count > 0)
            {
                this.saveList.Pop();
            }
        }
    }
    public partial class ECContext : workovertimeEntities
    {
        public virtual DbSet<TEntity> GetEntitySet<TEntity>() where TEntity : class
        {
            return base.Set<TEntity>();
        }

        public override int SaveChanges()
        {
            UpdateAuditableProperties();
            return base.SaveChanges();
        }

        public void ResetChanges()
        {
            foreach (var entry in this.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    //case EntityState.Modified:
                    //case EntityState.Deleted:
                    //    entry.State = EntityState.Modified;
                    //    entry.State = EntityState.Unchanged;
                    //    break;
                    //case EntityState.Added:
                    //    entry.State = EntityState.Detached;
                    //    break;
                }
            }
        }

        //public override async Task<int> SaveChangesAsync()
        //{
        //    UpdateAuditableProperties();
        //    return await base.SaveChangesAsync();
        //}

        //public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        //{
        //    UpdateAuditableProperties();
        //    return await base.SaveChangesAsync(cancellationToken);
        //}

        protected virtual void UpdateAuditableProperties()
        {
           
        }
    }
    public class GenericRepository<TEntity> where TEntity : class
    {
        internal ECContext context;
        public DbSet<TEntity> dbSet;
        private GenericRepository()
        {

        }

        public GenericRepository(ECContext context)
        {
            this.context = context;
            this.dbSet = context.GetEntitySet<TEntity>();
        }
        /// <summary>
        /// 执行sql added by CHEN LEI
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public virtual IEnumerable<TEntity> SqlQuery(string sql)
        {
            return context.Database.SqlQuery<TEntity>(sql).AsEnumerable();
        }

        /// <summary>
        /// 执行sql返回Datatable added by CHEN LEI
        /// Update: connectoin用完之后需要关闭掉，by Dennis yang 2016-04-09 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public virtual DataTable SqlQueryForDataTatable(string sql, SqlParameter[] parameters)
        {
            DataTable table = new DataTable();
            SqlConnection conn = new System.Data.SqlClient.SqlConnection();
            SqlCommand cmd = new SqlCommand();
            try
            {
                conn.ConnectionString = context.Database.Connection.ConnectionString;
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                cmd.Connection = conn;
                cmd.CommandText = sql;
                if (parameters != null && parameters.Length > 0)
                {
                    foreach (var item in parameters)
                    { cmd.Parameters.Add(item); }
                }
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                adapter.Fill(table);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                conn.Close();
            }
            return table;
        }

        public virtual TEntity Get(object id)
        {
            return dbSet.Find(id);
        }


        public virtual TEntity Get(Expression<Func<TEntity, bool>> filter)
        {
            IQueryable<TEntity> query = dbSet;


            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query.FirstOrDefault();
        }

        public virtual void Insert(TEntity entity)
        {
            dbSet.Add(entity);
        }
        /// <summary>
        /// 批量插入 added by CHEN LEI
        /// </summary>
        /// <param name="entity"></param>
        public virtual void InsertRange(List<TEntity> entity)
        {
            //dbSet.AddRange(entity);
        }

        //public virtual void InsertOrUpate(TEntity entity)
        //{
        //    if (entity.ID == 0)
        //    {
        //        this.Insert(entity);
        //    }
        //    else
        //    {
        //        this.Update(entity);
        //    }
        //}

        /// <summary>
        /// 批理删除
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual int Delete(Expression<Func<TEntity, bool>> filter)
        {
            var deleteList = dbSet.Where(filter).ToList();
            deleteList.ForEach(m => this.Delete(m));
            return deleteList.Count;
        }

        public virtual void Delete(object id)
        {
            TEntity entityToDelete = dbSet.Find(id);
            Delete(entityToDelete);
        }

        public virtual void Delete(TEntity entityToDelete)
        {
            if (context.Entry(entityToDelete).State == EntityState.Detached)
            {
                dbSet.Attach(entityToDelete);
            }
            dbSet.Remove(entityToDelete);
        }

        public virtual void Update(TEntity entityToUpdate)
        {
            dbSet.Attach(entityToUpdate);
            context.Entry(entityToUpdate).State = EntityState.Modified;
        }

        public virtual IQueryable<TEntity> Query(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return orderBy(query);
            }
            else
            {
                return query;
            }
        }

        public IQueryable<TEntity> Query(
            out int pagesCount,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            List<Expression<Func<TEntity, object>>> includes = null,
            int? page = null,
            int? pageSize = null)
        {
            IQueryable<TEntity> query = dbSet;
            pagesCount = 0;
            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            if (orderBy != null)
            {
                query = orderBy(query);
            }
            if (filter != null)
            {
                //query = query.AsExpandable().Where(filter);
                query = query.Where(filter);
            }
            if (page != null && pageSize != null)
            {
                /*
                int count = query.Count();
                if (pageSize > 0)
                {
                    q = q.Skip((request.CurrentPage - 1) * request.PageSize).Take(request.PageSize);
                    count = count / request.PageSize;
                    if (count % request.PageSize > 0)
                    {
                        count = +1;
                    }
                }
                else
                {
                    count = 1;
                }
                */
                query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }
            return query;
        }

        public virtual IQueryable<TEntity> SelectQuery(string query, params object[] parameters)
        {
            return dbSet.SqlQuery(query, parameters).AsQueryable();
        }


        public virtual bool Exists(Expression<Func<TEntity, bool>> predicate = null)
        {
            return predicate == null ? dbSet.Any() : dbSet.Any(predicate);
        }

        public virtual bool ExecuteSqlCommand(string sql, params object[] parameters)
        {
            return context.Database.ExecuteSqlCommand(sql, parameters) > 0;
        }
        public virtual bool ExecuteSqlCommand(string sql)
        {
            return context.Database.ExecuteSqlCommand(sql) > 0;
        }
    }
}
