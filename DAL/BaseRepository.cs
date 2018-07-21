using IDAL;
using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace DAL
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected GitHubEntities db = DBFactory.GetCurrentDbContext();


        public async Task<T> Add(T entity)
        {
            db.Entry<T>(entity).State = EntityState.Added;
            await db.SaveChangesAsync();
            return entity;
        }
        public async Task<bool> SqlGet(string sql, params object[] param)
        {
            int k = await db.Database.ExecuteSqlCommandAsync(sql, param);
            if (k >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public async Task<List<T>> SqlGetDate(string sql, params object[] param)
        {
            List<T> list = new List<T>();
            await Task.Run(()=> list=db.Database.SqlQuery<T>(sql,param).ToList());
            return list;
        }
        public async Task<List<T>> AddAll(List<T> list)
        {
            foreach (var item in list)
            {
                db.Entry<T>(item).State = EntityState.Added;
            }
            await db.SaveChangesAsync();
            return list;
        }

        public async Task<bool> Delete(T entity)
        {
            db.Entry<T>(entity).State = EntityState.Deleted;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Exist(Expression<Func<T, bool>> anyLamda)
        {
            return await db.Set<T>().AnyAsync(anyLamda);
        }

        public async Task<IQueryable<T>> SelectList(Expression<Func<T, bool>> whereLamda)
        {
            return await Task.Run(() => db.Set<T>().Where<T>(whereLamda));
        }

        public async Task<T> SelectOne(Expression<Func<T, bool>> whereLamda)
        {
            return await db.Set<T>().FirstOrDefaultAsync<T>(whereLamda);

        }
        public async Task<IQueryable<object>> SelectOneArear(Expression<Func<T, bool>> whereLamda, Expression<Func<T, object
            >> arareLamda)
        {
            return await Task.Run(() => db.Set<T>().Where(whereLamda).Select(arareLamda));

        }
        public async Task<Tuple<IQueryable<T>, int>> SelectPageList<Tkey>(Expression<Func<T, bool>> whereLamda, Expression<Func<T, Tkey>> orderLamda, int pageIndex, int pageSize)
        {
            IQueryable<T> _list = null;

            await Task.Run(() => _list=db.Set<T>().Where(whereLamda).OrderByDescending(orderLamda));
            int totalRecord = _list.Count();
            int startIndex = (pageIndex - 1) * pageSize;
            startIndex = startIndex < 0 ? 0 : startIndex;
            return new Tuple<IQueryable<T>, int>(_list.Skip<T>(startIndex).Take<T>(pageSize), totalRecord);
        }


        public async Task<bool> Updata(T entity)
        {
            db.Set<T>().Attach(entity);
            db.Entry<T>(entity).State = EntityState.Modified;
            await db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdataAll(List<T> list)
        {
            foreach (var item in list)
            {
                db.Set<T>().Attach(item);
                db.Entry<T>(item).State = EntityState.Modified;
            }
            await db.SaveChangesAsync();
            return true;
        }       
        public async Task<Tuple<IQueryable<T>, int>> SelectPageListNew<Tkey>(Expression<Func<T, bool>> whereLamda, Expression<Func<T, Tkey>> orderLamda, int pageIndex, int pageSize, bool isAsc)
        {
            int startIndex = (pageIndex - 1) * pageSize;
            startIndex = startIndex < 0 ? 0 : startIndex;
            IQueryable<T> _listResult = null;
            if (isAsc)
            {
                await Task.Run(() => _listResult = db.Set<T>().Where<T>(whereLamda).OrderBy<T, Tkey>(orderLamda));
            }
            else
            {
                await Task.Run(() => _listResult = db.Set<T>().Where<T>(whereLamda).OrderByDescending<T, Tkey>(orderLamda));
            }
            int totalRecord = _listResult.Count();
            return new Tuple<IQueryable<T>, int>(_listResult.Skip<T>(startIndex).Take<T>(pageSize), totalRecord);
        }

        public async Task<Tuple<IQueryable<R>, int>> SelectPartEntity<R, Tkey>(Expression<Func<T, bool>> whereLamda, Expression<Func<T, Tkey>> orderLamda, Expression<Func<T, R>> selectLamda, int pageIndex, int pageSize, bool isAsc)
        {
            int startIndex = (pageIndex - 1) * pageSize;
            startIndex = startIndex < 0 ? 0 : startIndex;
            IQueryable<R> _listResult = null;
            if (isAsc)
            {
                await Task.Run(() => _listResult = db.Set<T>().Where<T>(whereLamda).OrderBy<T, Tkey>(orderLamda).Select<T, R>(selectLamda));
            }
            else
            {
                await Task.Run(() => _listResult = db.Set<T>().Where<T>(whereLamda).OrderByDescending<T, Tkey>(orderLamda).Select<T, R>(selectLamda));
            }
            int totalRecord = _listResult.Count();
            return new Tuple<IQueryable<R>, int>(_listResult.Skip<R>(startIndex).Take<R>(pageSize), totalRecord);
        }
    }
}
