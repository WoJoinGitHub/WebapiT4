using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace System.Web.Http
{
    public static class ApiControllerExtensions
    {
        public static JsonResult<T> JsonMy<T>(this ApiController apicontroller, T content)
        {
            return new JsonResult<T>(content, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }, Encoding.Default, apicontroller);
        }
    }
}
