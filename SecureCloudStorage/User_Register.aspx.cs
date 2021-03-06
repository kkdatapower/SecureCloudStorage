using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SecureCloudStorage
{
    public partial class User_Register : System.Web.UI.Page
    {
        static string strcon = ConfigurationManager.AppSettings["DatabaseConnectionString"].ToString();

        SqlConnection con = new SqlConnection(strcon);
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            string strcon = ConfigurationManager.AppSettings["DatabaseConnectionString"].ToString();

            string spName = ConfigurationManager.AppSettings["DistinctUID"].ToString();

            using (SqlConnection conn = new SqlConnection(strcon))
            {
                SqlCommand sqlComm = new SqlCommand(spName, conn);
                //sqlComm.Parameters.AddWithValue("@eml", TextBox1?.Text);

                sqlComm.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = sqlComm;
                DataSet ds = new DataSet();
                da.Fill(ds);

                string uuid = "";
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string uid = ds.Tables[0].Rows[0][0].ToString();
                    int i = Convert.ToInt16(uid);
                    i = i + 1;
                    uuid = i.ToString();
                }
                else
                {
                    uuid = "101";
                }

             string spName1 = ConfigurationManager.AppSettings["AddUserDetails"].ToString();
               
                    SqlCommand sqlComm1 = new SqlCommand(spName1, conn);
                    sqlComm1.Parameters.AddWithValue("@uuid", uuid);
                    sqlComm1.Parameters.AddWithValue("@name", TextBox1?.Text);
                    sqlComm1.Parameters.AddWithValue("@eml", TextBox2?.Text);
                    sqlComm1.Parameters.AddWithValue("@addr", TextBox3?.Text);
                    sqlComm1.Parameters.AddWithValue("@pw", TextBox4?.Text);

                    sqlComm1.CommandType = CommandType.StoredProcedure;

                    SqlDataAdapter da1 = new SqlDataAdapter();
                    da1.SelectCommand = sqlComm1;
                    DataSet ds1 = new DataSet();
                    da1.Fill(ds1);

                    Session["Add"] = "Data";
                    Response.Redirect("Login.aspx");
                
            }
        }
    }
}