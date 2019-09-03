using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectManageServer.Model.CreateObject
{
    public class FLC_OBJECTEVENT
    {
        /// <summary>
        /// 对象编码
        /// </summary>
        public string OBJ_CODE { get; set; }

        /// <summary>
        /// 0单据1列表
        /// </summary>
        public int? OBJ_EVENTTYPE { get; set; }

        /// <summary>
        /// dll路径;命名空间.类名
        /// </summary>
        public string ASSEMBLYPATH { get; set; }

        /// <summary>
        /// 执行顺序
        /// </summary>
        public int? RUN_INDEX { get; set; }

        /// <summary>
        /// 1启用 0 禁用
        /// </summary>
        public int? RUN_ENABLE { get; set; }

    }
}
