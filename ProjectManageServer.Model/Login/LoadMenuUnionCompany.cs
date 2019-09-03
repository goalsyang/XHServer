using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectManageServer.Model.Login
{
   public  class LoadMenuUnionCompany
    {    

        public Company company { get; set; }

        public IEnumerable<LoadMenu> loadMenu { get; set; }

    }
}
