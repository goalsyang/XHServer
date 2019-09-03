using System.Collections.Generic;

namespace ProjectManageServer.Model
{
    public class MenuRolesModel
    {
        public string RoleCode { get; set; }

        public string MenuCode { get; set; }

        public string MenuName { get; set; }

        public string MenuIcon { get; set; }

        public string ParentCode { get; set; }

        public string MenuPath { get; set; }

        public string Component { get; set; }

        public string Title { get; set; }

        public string AlwaysShow { get; set; }

        public IEnumerable<MenuRolesModel> Children { get; set; }

    }
}
