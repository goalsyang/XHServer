using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectManageServer.Model.Login
{
    public class U8connString
    {
        public string U8DBInstance { get; set; }

        public string U8DBLibname { get; set; }

        public string U8User { get; set; }

        public string U8Password { get; set; }

        public string U8connStrings => string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3} ", this.U8DBInstance, this.U8DBLibname, this.U8User, this.U8Password);
    }
}
