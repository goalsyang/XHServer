using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectManageServer.Model.Authorization
{

    public class GetAllAuthByRole
    {
        public IEnumerable<V_FLC_MENU> dtNodes { get; set; }

        public IEnumerable<FLC_OBJ_OPERATION> dtOperation { get; set; }
    }

    public class SetAuthToRole
    {
        public string Role { get; set; }

        public List<FLC_AUTHORIZATION_ROLE> fLC_AUTHORIZATION_ROLEs { get; set; }

    }

    public class SaveMenuAuthToRole
    {
        public string role_code { get; set; }

        public List<FLC_MENU_AUTH> fLC_MENU_AUTHs { get; set; }

    }

    public class SaveOperationAuthToRole
    {
        public string role_code { get; set; }

        public List<FLC_OPEERATION_AUTH>  fLC_OPEERATION_AUTHs { get; set; }

    }

    public class GetOperationAuthByUser
    {
        
        public IEnumerable<dynamic> objects { get; set; }

        public IEnumerable<FLC_OBJ_OPERATION> operation { get; set; }
    }


    public class GetOperationAuthByRole
    {

        public IEnumerable<FLC_OBJ_OPERATION> operation { get; set; }

        public IEnumerable<FLC_OBJECT> objects { get; set; }

    }

}
