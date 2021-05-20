using Check;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SecureCloudStorage
{
    public partial class Login : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(@"Server=tcp:securestoragedatabase.database.windows.net,1433;Initial Catalog=secureUpload;Persist Security Info=False;User ID=krishna;Password=Cheppanu$911;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30");
        //SqlConnection con = new SqlConnection(@"Data Source=localhost\SQLEXPRESS;Initial Catalog=secureUpload;Integrated Security=True");
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Add"] == "Data")
            {
                Page.ClientScript.RegisterStartupScript(GetType(), "msgbox", "alert('Registered Successfully. You may login now.')", true);
                Session["Add"] = "";
            }

            #region System Generated . . .
            Class1 c = new Class1();
            bool c1 = c.checkLoad("S438", con);
            if (!c1)
            {
                Response.Redirect("Login.aspx");
            }
            #endregion
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            string s = "select uid,pwd from reg where email='" + TextBox1.Text + "'and pwd='" + TextBox2.Text + "'";
            SqlDataAdapter sda = new SqlDataAdapter(s, con);
            DataSet ds = new DataSet();
            sda.Fill(ds);
            if (ds.Tables[0].Rows.Count > 0)
            {
                Session["status"] = "li";
                Session["type"] = "user";
                Session["uid"] = ds.Tables[0].Rows[0][0].ToString();
                Response.Redirect("UploadFiles.aspx");
            }
            else
            {
                Page.ClientScript.RegisterStartupScript(GetType(), "msgbox", "alert('Invalid Login!!!')", true);
            }
        }

        protected void LinkButton1_Click(object sender, EventArgs e)
        {
            Response.Redirect("User_Register.aspx");
        }
    }
}