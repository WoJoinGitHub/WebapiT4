using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Helper;
using BLL;
using Model;

namespace kjyl.Controllers
{
    public class Interest_OrderOtherController : ApiController
    {
        // GET: api/Interest_OrderOther
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet]
        [Route("api/Interest_OrderOther/liyi")]
        // GET: api/Interest_OrderOther/5
        public async System.Threading.Tasks.Task<IHttpActionResult> Get(string id, string pno)
        {
            INTEREST_ORDEROTHERService bill = new INTEREST_ORDEROTHERService();
            AjaxResultT<Interest_OrderOtherList> re = new AjaxResultT<Interest_OrderOtherList>
            {
                code = 0,
                msg = "获取失败"

            };
            try
            {
                var relist = new Interest_OrderOtherList();
                
                string[] list = new string[]
                {
                    "意外伤害保险责任",
                    "全球紧急救援保险责任",
                    "学业中断保险责任",
                    "旅行不便保险责任",
                    "个人第三者保险责任",
                };
                for (int i = 0; i < list.Length; i++)
                {
                    INTEREST_ORDEROTHER model = new INTEREST_ORDEROTHER();
                    var name = list[i];
                    INTEREST_ORDEROTHER modelIO = await bill.Select(p => p.SALEDETAILNO == id && p.INTEREST_ORDEROTHER_NAME == name);
                    if (modelIO == null)
                    {                       
                        INTEREST_PRODUCEOTHERService probill = new INTEREST_PRODUCEOTHERService();
                        var model2 = await probill.Select(p => p.PRODUCTNO == pno && p.IPOTHER_NAME == name);
                        if (model2 == null)
                        {
                            continue;
                        }
                        model.INTEREST_ORDEROTHER_ID = model2.IPOTHER_ID;
                        model.INTEREST_ORDEROTHER_MONEY = model2.IPOTHER_MONEY;
                        model.INTEREST_ORDEROTHER_NAME = model2.IPOTHER_NAME;
                        foreach (var item in model2.IPOTHER_ITEM)
                        {
                            IO_OTHER_ITEM ioItem = new IO_OTHER_ITEM();
                            ioItem.IO_OTHER_ITEM_ID = item.IPOTHER_ITEM_ID;
                            ioItem.IO_OTHER_ITEM_MONEY = item.IPOTHER_ITEM_MONEY;
                            ioItem.IO_OTHER_ITEM_NAME = item.IPOTHER_ITEM_NAME;
                            ioItem.IO_OTHER_ITEM_ZFBL = item.IPOTHER_ITEM_ZFBL;
                            ioItem.IO_OTHER_ITEM_ZFMONEY = item.IPOTHER_ITEM_ZFMONEY;
                            model.IO_OTHER_ITEM.Add(ioItem);
                        }
                    }
                    else
                    {
                        model.INTEREST_ORDEROTHER_ID = modelIO.INTEREST_ORDEROTHER_ID;
                        model.INTEREST_ORDEROTHER_MONEY = modelIO.INTEREST_ORDEROTHER_MONEY;
                        model.INTEREST_ORDEROTHER_NAME = modelIO.INTEREST_ORDEROTHER_NAME;
                        foreach (var item in modelIO.IO_OTHER_ITEM)
                        {
                            IO_OTHER_ITEM ioItem = new IO_OTHER_ITEM();
                            ioItem.IO_OTHER_ITEM_ID = item.IO_OTHER_ITEM_ID;
                            ioItem.IO_OTHER_ITEM_MONEY = item.IO_OTHER_ITEM_MONEY;
                            ioItem.IO_OTHER_ITEM_NAME = item.IO_OTHER_ITEM_NAME;
                            ioItem.IO_OTHER_ITEM_ZFBL = item.IO_OTHER_ITEM_ZFBL;
                            ioItem.IO_OTHER_ITEM_ZFMONEY = item.IO_OTHER_ITEM_ZFMONEY;
                            model.IO_OTHER_ITEM.Add(ioItem);
                        }
                    }
                    switch (i)
                    {
                        case 0:
                            relist.accidental = model;
                            break;
                        case 1:
                            relist.emergency = model;
                            break;
                        case 2:
                            relist.study = model;
                            break;
                        case 3:
                            relist.travel = model;
                            break;
                        case 4:
                            relist.other = model;
                            break;
                    }
                }
                var relistList = new List<Interest_OrderOtherList>()
                {
                    relist
                };
                re.result = relistList;
                re.code = 1;
                re.msg = "获取成功";

            }
            catch (Exception e)
            {
                re.msg = e.Message;
            }
            return Json(re);
        }

        // POST: api/Interest_OrderOther
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Interest_OrderOther/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Interest_OrderOther/5
        public void Delete(int id)
        {
        }
    }
}
