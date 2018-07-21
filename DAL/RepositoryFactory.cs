 
  


using IDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace DAL
{
    public static class RepositoryFactory
    {
	        public static IAdminRepository AdminRepository
         {
            get
            {
                return new AdminRepository();
            }
         }
            public static IStudentRepository StudentRepository
         {
            get
            {
                return new StudentRepository();
            }
         }
     
    }
}
 