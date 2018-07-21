using IBLL;
using IDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class BaseService<T> : IBaseService<T> where T : class
    {
        protected IBaseRepository<T> CurrentRepository { set; get; }

        public BaseService(IBaseRepository<T> currentRepository)
        {
            CurrentRepository = currentRepository;
        }

        public async Task<T> Add(T entity)
        {
            return await CurrentRepository.Add(entity);
        }

        public async Task<List<T>> AddAll(List<T> list)
        {
            return await CurrentRepository.AddAll(list);
        }

        public async Task<bool> Delete(T entity)
        {
            return await CurrentRepository.Delete(entity);
        }
        public async Task<bool> SqlGet(string sql, params object[] param)
        {
            return await CurrentRepository.SqlGet(sql, param);
        }

        public async Task<bool> Exist(Expression<Func<T, bool>> anyLamda)
        {
            return await CurrentRepository.Exist(anyLamda);
        }

        public async Task<IQueryable<T>> SelectList(Expression<Func<T, bool>> whereLamda)
        {
            return await CurrentRepository.SelectList(whereLamda);
        }

        public async Task<T> SelectOne(Expression<Func<T, bool>> whereLamda)
        {
            return await CurrentRepository.SelectOne(whereLamda);
        }
        public async Task<IQueryable<object>> SelectOneArear(Expression<Func<T, bool>> whereLamda, Expression<Func<T, object
            >> arareLamda)
        {
            return await CurrentRepository.SelectOneArear(whereLamda, arareLamda);
        }
        public async Task<Tuple<IQueryable<T>, int>> SelectPageList<Tkey>(Expression<Func<T, bool>> whereLamda, Expression<Func<T, Tkey>> orderLamda, int pageIndex, int pageSize)
        {
            return await CurrentRepository.SelectPageList(whereLamda, orderLamda, pageIndex, pageSize);
        }
        public async Task<bool> Updata(T entity)
        {
            return await CurrentRepository.Updata(entity);
        }
        public async Task<bool> UpdataAll(List<T> list)
        {
            return await CurrentRepository.UpdataAll(list);
        }
        public async Task<Tuple<IQueryable<T>, int>> SelectPageListNew<Tkey>(Expression<Func<T, bool>> whereLamda, Expression<Func<T, Tkey>> orderLamda, int pageIndex, int pageSize, bool isAsc)
        {
            return await CurrentRepository.SelectPageListNew<Tkey>(whereLamda, orderLamda, pageIndex, pageSize, isAsc);
        }

        public async Task<Tuple<IQueryable<R>, int>> SelectPartEntity<R, Tkey>(Expression<Func<T, bool>> whereLamda, Expression<Func<T, Tkey>> orderLamda, Expression<Func<T, R>> selectLamda, int pageIndex, int pageSize, bool isAsc)
        {
            return await CurrentRepository.SelectPartEntity<R, Tkey>(whereLamda, orderLamda, selectLamda, pageIndex, pageSize, isAsc);
        }

    }
}

