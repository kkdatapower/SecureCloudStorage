using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SecureCloudStorage
{
    public partial class MasterPage : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (Session["type"]?.ToString() == "user")
            {
                U_Panel.Visible = true;
            }
            else
            {
                U_Panel.Visible = false;
            }
        }
    }
}