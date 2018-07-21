using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IDAL
{
    /// <summary>
    /// 基础接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBaseRepository<T> where T : class
    {
        /// <summary>
        /// 存在？
        /// </summary>
        /// <param name="anyLamda"></param>
        /// <returns></returns>
        Task<bool> Exist(Expression<Func<T, bool>> anyLamda);

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<T> Add(T entity);
        /// <summary>
        /// 批量添加
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        Task<List<T>> AddAll(List<T> list);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<bool> Delete(T entity);

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<bool> Updata(T entity);
        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        Task<bool> UpdataAll(List<T> list);
        /// <summary>
        /// 执行sql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
         Task<bool> SqlGet(string sql, params object[] param);
        /// <summary>
        /// 查询实体
        /// </summary>
        /// <param name="whereLamda"></param>
        /// <returns></returns>
        Task<T> SelectOne(Expression<Func<T, bool>> whereLamda);

        /// <summary>
        /// 查询集合
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="whereLamda"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        Task<IQueryable<T>> SelectList(Expression<Func<T, bool>> whereLamda);
        /// <summary>
        /// 查询部分数据
        /// </summary>
        /// <param name="whereLamda"></param>
        /// <param name="arareLamda"></param>
        /// <returns></returns>
        Task<IQueryable<object>> SelectOneArear(Expression<Func<T, bool>> whereLamda, Expression<Func<T, object
            >> arareLamda);
        /// <summary>
        /// 分页查询集合
        /// </summary>
        /// <param name="whereLamda"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecord"></param>
        /// <returns></returns>
        Task<Tuple<IQueryable<T>, int>> SelectPageList<Tkey>(Expression<Func<T, bool>> whereLamda, Expression<Func<T, Tkey>> orderLamda, int pageIndex, int pageSize);


        /// <summary>
        /// 获取分页新方法
        /// </summary>
        /// <typeparam name="Tkey">字段</typeparam>
        /// <param name="whereLamda">where语句</param>
        /// <param name="orderLamda">order语句</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页容量</param>
        /// <param name="totalRecord">总数</param>
        /// <param name="isAsc">是否正</param>
        /// <returns></returns>
        Task<Tuple<IQueryable<T>, int>> SelectPageListNew<Tkey>(Expression<Func<T, bool>> whereLamda, Expression<Func<T, Tkey>> orderLamda, int pageIndex, int pageSize, bool isAsc);

        /// <summary>
        /// 分页获取实体部分
        /// </summary>
        /// <typeparam name="R">返回结果</typeparam>
        /// <typeparam name="Tkey">字段</typeparam>
        /// <param name="whereLamda"></param>
        /// <param name="orderLamda"></param>
        /// <param name="selectLamda"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecord"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        Task<Tuple<IQueryable<R>, int>> SelectPartEntity<R, Tkey>(Expression<Func<T, bool>> whereLamda, Expression<Func<T, Tkey>> orderLamda, Expression<Func<T, R>> selectLamda, int pageIndex, int pageSize, bool isAsc);

    }
}
