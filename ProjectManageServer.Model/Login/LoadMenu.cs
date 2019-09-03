using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectManageServer.Model.Login
{
    public class LoadMenu
    {
        /* create or replace view v_flc_menu as
select  m.disp_order,l.lan,l.value as MenuName,m.id as MenuCode, m.is_show,m.is_dir as MenuGrade,m.icon_path as MenuImage,
m.parent_id as MenuParent,nvl(m_r.is_from_model,-1) is_from_model,m_r.assembly,m_r.objectcode,m_r.parameter,m.icon_font
,m.is_sys,m.is_enable,fb.is_enable as object_enable,m.is_admin
from flc_menu m
left join flc_lang l on l.key = m.id
left join flc_menu_relevance m_r on m_r.id = m.id
left join flc_object fb on m_r.objectcode = fb.obj_code;*/


        public string Disp_Order { get; set; }

        public string Lan { get; set; }

        public string MenuName { get; set; }

        public string MenuCode { get; set; }

        public string Is_Show { get; set; }

        public string MenuGrade { get; set; }

        public string MenuImage { get; set; }

        public string MenuParent { get; set; }

        public string Is_Form_Model { get; set; }

        public string Assembly { get; set; }

        public string ObjectCode { get; set; }

        public string Parameter { get; set; }

        public string Icon_Font { get; set; }

        public string Is_Sys { get; set; }

        public string Is_Enable { get; set; }

        public string Object_Enable { get; set; }

        public string Is_Admin { get; set; }

        public string Test { get; set; }
    }

}
