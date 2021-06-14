using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SecureCloudStorage
{
    public partial class ViewFiles : System.Web.UI.Page
    {
        static string strcon = ConfigurationManager.AppSettings["DatabaseConnectionString"].ToString();
        SqlConnection con = new SqlConnection(strcon);

        protected void Page_Load(object sender, EventArgs e)
        {
            string ses = (string)Session["status"];
            string uid = (string)Session["uid"];
            if (ses == null)
            {
                Response.Redirect("Login.aspx");
            }
            if ((string)Session["access"] == "no")
            {
                Page.ClientScript.RegisterStartupScript(GetType(), "msgbox", "alert('Cannot Access!!!')", true);
            }

            string strcon = ConfigurationManager.AppSettings["DatabaseConnectionString"].ToString();

            string spName = ConfigurationManager.AppSettings["ViewFilesUID"].ToString();

            using (SqlConnection conn = new SqlConnection(strcon))
            {
                SqlCommand sqlComm = new SqlCommand(spName, conn);
                sqlComm.Parameters.AddWithValue("@uuid", uid);

                sqlComm.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = sqlComm;
                DataSet ds = new DataSet();
                da.Fill(ds);

                int count = ds.Tables[0].Rows.Count;
                StringBuilder strBld = new StringBuilder();
                strBld.Append("<table align='center' width='70%'> <tr>");
                
                string url = "", name = "", fid, id;
                for (int i = 0; i < count; i++)
                {
                    string ext = ds.Tables[0].Rows[i][6].ToString();
                    if (ext == ".txt")
                    {
                        url = "icons\\txt.png";
                    }
                    else if (ext == ".mp3")
                    {
                        url = "icons\\mp3.png";
                    }
                    else if (ext == ".mp4")
                    {
                        url = "icons\\mp4.png";
                    }
                    else if (ext == ".pdf")
                    {
                        url = "icons\\pdf.png";
                    }
                    else
                    {
                        url = "icons\\default.png";
                    }
                    name = ds.Tables[0].Rows[i][2].ToString();
                    fid = ds.Tables[0].Rows[i][1].ToString();
                    id = ds.Tables[0].Rows[i][0].ToString();
                    if ((i % 3) == 0)
                    {
                        strBld.Append("</tr>");
                        strBld.Append("<tr> <td width = '32%' align = 'center' >");
                        strBld.Append("<a href='ViewDet.aspx?ID=" + id + "&FID=" + fid + "'>");
                        strBld.Append("<img src=" + url + " width='220' height='170'/><br />");
                        strBld.Append("<span style='width:100%; font-family:'Bell MT'; font-size:22px; color:dimgray'>" + name + "</span></a>");
                        strBld.Append("</td>");
                        strBld.Append("<td width='2%'></td>");
                    }
                    else
                    {
                        strBld.Append("<td width = '32%' align = 'center' >");
                        strBld.Append("<a href='ViewDet.aspx?ID=" + id + "&FID=" + fid + "'>");
                        strBld.Append("<img src=" + url + " width='220' height='170'/><br />");
                        strBld.Append("<span style='width:100%; font-family:'Bell MT'; font-size:22px; color:dimgray'>" + name + "</span></a>");
                        strBld.Append("</td>");
                        strBld.Append("<td width='2%'></td>");
                    }
                }

                strBld.Append("</table>");
                Label1.Text = strBld.ToString();
            }
        }
    }
}