using Microsoft.VisualStudio.TestTools.UnitTesting;
using KJYLServer.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KJYLServer.Controllers.Tests
{
    [TestClass()]
    public class ADMINControllerTests
    {
        [TestMethod]
        public async Task LoginTestAsync()
        {
            var adminControl=new ADMINController();
            var re=  await adminControl.Login("admin", "123456");
            Assert.Fail();
        }
    }
}