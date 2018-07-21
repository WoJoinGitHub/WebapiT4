//------------------------------------------------------------------------------
//     此代码由T4模板自动生成
//       生成时间 2018-07-21 11:22:58 by ShiJun Liu
//     对此文件的更改可能会导致不正确的行为，并且如果重新生成代码，这些更改将会丢失。
//     如需更改 请使用部分类
//------------------------------------------------------------------------------
using DAL;
using IBLL;
using Model;
using System;
using System.Linq.Expressions;
namespace BLL
{

    public partial class  StudentService:BaseService<Student>,IStudentService
    {
        public  StudentService() : base(RepositoryFactory.StudentRepository)
        {

        }
       
    }
   
}
 