using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace ProjectManageServer.Common
{
    public class AjaxRspJson : ActionResult
    {
        /// <summary>
        /// 返回代码  1 成功  0 失败
        /// </summary>
        public RspStatus RspCode
        {
            get;
            set;
        }

        /// <summary>
        /// 返回信息
        /// </summary>
        public string RspMsg
        {
            get;
            set;
        }

        /// <summary>
        /// 返回数据
        /// </summary>
        private string RspData
        {
            get;
            set;
        }

        public object ObjectData
        {
            get; set;
        }
         
        /// <summary>
        /// Ajax 通信的返回 Json 串
        /// </summary>
        public AjaxRspJson()
        {
            this.RspCode = RspStatus.Failed;

            this.RspMsg = string.Empty;

            this.RspData = string.Empty;
        }

        /// <summary>
        /// 可以通过重写报文，每次传固定的属性值：如CityId等
        /// </summary>
        /// <param context="context">上下文</param>
        public AjaxRspJson(HttpContext context)
        {
            this.RspCode = RspStatus.Failed;

            this.RspMsg = string.Empty;

            this.RspData = string.Empty;
        }

        /// <summary>
        /// 将对象转化为 Json 字符串
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            this.RspData = JsonConvert.SerializeObject(ObjectData);

            JObject jo = new JObject();
            
            jo.Add("RspCode", this.RspCode.ToString());

            jo.Add("RspMsg", this.RspMsg);

            jo.Add("RspData", this.RspData);

            return JsonConvert.SerializeObject(jo, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
        }
         
        public override void ExecuteResult(ActionContext context)
        {
            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.WriteAsync(this.ToJson());
        }
    }
}
