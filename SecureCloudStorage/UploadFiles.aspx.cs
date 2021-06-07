using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;

namespace SecureCloudStorage
{
    public partial class UploadFiles : System.Web.UI.Page
    {
        public enum State
        {
            Hiding,
            Filling_With_Zeros
        };
        static string strcon = ConfigurationManager.AppSettings["DatabaseConnectionString"].ToString();

        SqlConnection con = new SqlConnection(strcon);
        //SqlConnection con = new SqlConnection(@"Server=tcp:securestoragedatabase.database.windows.net,1433;Initial Catalog=secureUpload;Persist Security Info=False;User ID=krishna;Password=Cheppanu$911;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30");
        //SqlConnection con = new SqlConnection(@"Data Source=localhost\SQLEXPRESS;Initial Catalog=secureUpload;Integrated Security=True");
        protected void Page_Load(object sender, EventArgs e)
        {
            string ses = (string)Session["status"];
            if (ses == null)
            {
                Response.Redirect("Login.aspx");
            }
            string uid = (string)Session["uid"];
            if (!IsPostBack)
            {
                /*string s = "select distinct uid from reg where uid!='" + uid + "'";
                SqlDataAdapter sda = new SqlDataAdapter(s, con);
                DataSet ds = new DataSet();
                sda.Fill(ds);*/
                string strcon = ConfigurationManager.AppSettings["DatabaseConnectionString"].ToString();

                string spName = ConfigurationManager.AppSettings["UploadFilesDistinctUid"].ToString();

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
                   
                    DropDownList1.DataSource = ds.Tables[0];
                    DropDownList1.DataTextField = "Name";
                    DropDownList1.DataValueField = "uid";
                    DropDownList1.DataBind();
                    DropDownList1.Items.Insert(0, "--Select--");

                    //for (int i = 0; i < count; i++)
                    //{
                    //    DropDownList1.Items.Add(new SelectListItem { Text = });
                    //        //ds.Tables[0].Rows[i][0].ToString(),);
                    //}
                }
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            string file = "", path = "";
            try
            {
                file = FileUpload1.FileName;
                path = Server.MapPath("~\\Files\\");
                FileUpload1.SaveAs(path + file);
                string keyVal = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456";

                byte[] fileByteArr = File.ReadAllBytes(path + file);

                int encryptFileLen = fileByteArr.Length / 3;
                int[] lenPerEncryption = new int[] { encryptFileLen, encryptFileLen, encryptFileLen };
                if (fileByteArr.Length % 3 != 0)
                {
                    int modDiff = fileByteArr.Length % 3;
                    int inc = 0;
                    while (modDiff != 0)
                    {
                        if (inc == 3) inc = 0;
                        lenPerEncryption[inc]++;
                        inc++;
                        modDiff--;
                    }
                }
                byte[] aesEncryptArr = new byte[lenPerEncryption[0]];
                byte[] desEncryptArr = new byte[lenPerEncryption[1]];
                byte[] rc2EncryptArr = new byte[lenPerEncryption[2]];

                int incbyte = 0;
                for (int i = 0; i < aesEncryptArr.Length; i++)
                {
                    aesEncryptArr[i] = fileByteArr[incbyte++];
                }
                for (int i = 0; i < desEncryptArr.Length; i++)
                {
                    desEncryptArr[i] = fileByteArr[incbyte++];
                }
                for (int i = 0; i < rc2EncryptArr.Length; i++)
                {
                    rc2EncryptArr[i] = fileByteArr[incbyte++];
                }

                

                string encryptAes = AESEncryptUsingByte(aesEncryptArr, keyVal);
                string encryptDes = DESEncrypt(desEncryptArr, keyVal);
                string encryptRc2 = RC2Encrypt(rc2EncryptArr, keyVal);


                path = Server.MapPath("~//AES//");
                File.WriteAllText(path + file, encryptAes);

                path = Server.MapPath("~//DES//");
                File.WriteAllText(path + file, encryptDes);

                path = Server.MapPath("~//RC//");
                File.WriteAllText(path + file, encryptRc2);

            }
            catch (Exception ep)
            {
                Page.ClientScript.RegisterStartupScript(GetType(), "msgbox", "alert('Please Select a File');", true);
                throw ep;
            }


            string images, paths;
            images = FileUpload2.FileName;
            paths = Server.MapPath("~\\images\\");
            FileUpload2.SaveAs(paths + images);

            Bitmap b = new Bitmap(paths + images);
            //byte[] op1 = ConvertBitmapToByteArray(b);
            Bitmap b1 = embedText(TextBox1.Text, b);

            byte[] op = ConvertBitmapToByteArray(b1);

            using (MemoryStream memstr = new MemoryStream(op))
            {
                Image img1 = Image.FromStream(memstr);
                
            
            //System.Drawing.Image img1 = byteArrayToImage(op);
            string spath = Server.MapPath("~\\Password\\");

            img1.Save(spath + FileUpload2.FileName);
            Image2.ImageUrl = spath + FileUpload2.FileName;
            }
            string com = "";
            SqlCommand cmd;
            com = "insert into lsb(op) values (@data);";
            con.Open();
            cmd = new SqlCommand(com, con);
            cmd.Parameters.AddWithValue("@data", op);
            cmd.ExecuteNonQuery();
            con.Close();

            Page.ClientScript.RegisterStartupScript(GetType(), "msgbox", "alert('File encrypted successfully')", true);

            //************************************************Insert**************************

            /*string s = "select fid from details order by fid desc";
            SqlDataAdapter sds = new SqlDataAdapter(s, con);
            DataSet dss = new DataSet();
            sds.Fill(dss);*/

            string strcon = ConfigurationManager.AppSettings["DatabaseConnectionString"].ToString();

            string spName1 = ConfigurationManager.AppSettings["UploadFilesFidInfo"].ToString();

            using (SqlConnection conn = new SqlConnection(strcon))
            {
                SqlCommand sqlComm1 = new SqlCommand(spName1, conn);

                sqlComm1.CommandType = CommandType.StoredProcedure; 

                SqlDataAdapter da1 = new SqlDataAdapter();
                da1.SelectCommand = sqlComm1;
                DataSet ds1 = new DataSet();
                da1.Fill(ds1);

                string ffid = "";
                if (ds1.Tables[0].Rows.Count > 0)
                {
                    string fid = ds1.Tables[0].Rows[0][0].ToString();
                    int fidd = Convert.ToInt16(fid);
                    fidd = fidd + 1;
                    ffid = fidd.ToString();
                }
                else
                {
                    ffid = "101";
                }


                string aespath1 = Server.MapPath("~\\AES\\" + file);
                string rcpath1 = Server.MapPath("~\\RC\\" + file);
                string despath1 = Server.MapPath("~\\DES\\" + file);

                string fileExtension = Path.GetExtension(FileUpload1.PostedFile.FileName);

                string uid = (string)Session["uid"];
                /*string com1 = "";
                SqlCommand cmd1;
                com1 = "insert into details(uid,fid,filenm,aes,des,rc6,type,lsb) values (@uid,@fid,@fn,@aes,@des,@rc6,@type,@lsb);";
                con.Open();
                cmd1 = new SqlCommand(com1, con);
                cmd1.Parameters.AddWithValue("@uid", uid);
                cmd1.Parameters.AddWithValue("@fid", ffid);
                cmd1.Parameters.AddWithValue("@fn", FileUpload1.FileName);
                cmd1.Parameters.AddWithValue("@aes", aespath1);
                cmd1.Parameters.AddWithValue("@des", despath1);
                cmd1.Parameters.AddWithValue("@rc6", rcpath1);
                cmd1.Parameters.AddWithValue("@type", fileExtension);
                cmd1.Parameters.AddWithValue("@lsb", op);
                cmd1.ExecuteNonQuery();
                con.Close();*/

                string spName2 = ConfigurationManager.AppSettings["UploadFilesInfo"].ToString();

                using (SqlConnection conn1 = new SqlConnection(strcon))
                {
                    SqlCommand sqlComm2 = new SqlCommand(spName2, conn1);
                    sqlComm2.Parameters.AddWithValue("@uid", uid);
                    sqlComm2.Parameters.AddWithValue("@fid", ffid);
                    sqlComm2.Parameters.AddWithValue("@filenm", FileUpload1?.FileName);
                    sqlComm2.Parameters.AddWithValue("@aes", aespath1);
                    sqlComm2.Parameters.AddWithValue("@des", despath1);
                    sqlComm2.Parameters.AddWithValue("@rc", rcpath1);
                    sqlComm2.Parameters.AddWithValue("@type", fileExtension);
                    sqlComm2.Parameters.AddWithValue("@lsb", op);

                    sqlComm2.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter da2 = new SqlDataAdapter();
                    da2.SelectCommand = sqlComm2;
                    DataSet ds2 = new DataSet();
                    da2.Fill(ds2);
                }

                /*string ss = "select fid from details order by fid desc";
                SqlDataAdapter sdss = new SqlDataAdapter(ss, con);
                DataSet dsss = new DataSet();
                sdss.Fill(dsss);*/

                string spName3 = ConfigurationManager.AppSettings["UploadFilesFidInfo"].ToString();

                SqlCommand sqlComm3 = new SqlCommand(spName3, conn);

                sqlComm1.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter da3 = new SqlDataAdapter();
                da3.SelectCommand = sqlComm3;
                DataSet dsss = new DataSet();
                da3.Fill(dsss);

                string ffids = "";
                if (dsss.Tables[0].Rows.Count > 0)
                {
                    string fids = dsss.Tables[0].Rows[0][0].ToString();
                    int fidds = Convert.ToInt16(fids);
                    fidds = fidds + 1;
                    ffids = fidds.ToString();
                }
                else
                {
                    ffids = "101";
                }

                /*string com2 = "";
                SqlCommand cmd2;
                com2 = "insert into details(uid,fid,filenm,aes,des,rc6,type,lsb) values (@uid,@fid,@fn,@aes,@des,@rc6,@type,@lsb);";
                con.Open();
                cmd2 = new SqlCommand(com2, con);
                cmd2.Parameters.AddWithValue("@uid", DropDownList1.Text);
                cmd2.Parameters.AddWithValue("@fid", ffid);
                cmd2.Parameters.AddWithValue("@fn", FileUpload1.FileName);
                cmd2.Parameters.AddWithValue("@aes", aespath1);
                cmd2.Parameters.AddWithValue("@des", despath1);
                cmd2.Parameters.AddWithValue("@rc6", rcpath1);
                cmd2.Parameters.AddWithValue("@type", fileExtension);
                cmd2.Parameters.AddWithValue("@lsb", op);
                cmd2.ExecuteNonQuery();
                con.Close();*/
                if (DropDownList1?.SelectedValue?.ToString() != "--Select--") { 
                using (SqlConnection conn1 = new SqlConnection(strcon))
                {
                    SqlCommand sqlComm4 = new SqlCommand(spName2, conn1);
                    sqlComm4.Parameters.AddWithValue("@uid", DropDownList1?.SelectedValue?.ToString());
                    sqlComm4.Parameters.AddWithValue("@fid", ffid);
                    sqlComm4.Parameters.AddWithValue("@filenm", FileUpload1?.FileName);
                    sqlComm4.Parameters.AddWithValue("@aes", aespath1);
                    sqlComm4.Parameters.AddWithValue("@des", despath1);
                    sqlComm4.Parameters.AddWithValue("@rc", rcpath1);
                    sqlComm4.Parameters.AddWithValue("@type", fileExtension);
                    sqlComm4.Parameters.AddWithValue("@lsb", op);

                    sqlComm4.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter da4 = new SqlDataAdapter();
                    da4.SelectCommand = sqlComm4;
                    DataSet ds4 = new DataSet();
                    da4.Fill(ds4);
                }
                }

                /*string sem = "select email from reg where uid='" + DropDownList1.Text + "'";
                SqlDataAdapter sda = new SqlDataAdapter(sem, con);
                DataSet ds = new DataSet();
                sda.Fill(ds);*/
                string spName5 = ConfigurationManager.AppSettings["EmailInfo"].ToString();
                using (SqlConnection conn1 = new SqlConnection(strcon))
                {
                    SqlCommand sqlComm5 = new SqlCommand(spName5, conn1);
                    sqlComm5.Parameters.AddWithValue("@uid", DropDownList1?.SelectedValue?.ToString());

                    sqlComm5.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter da5 = new SqlDataAdapter();
                    da5.SelectCommand = sqlComm5;
                    DataSet ds5 = new DataSet();
                    da5.Fill(ds5);

                    string smtpServer = ConfigurationManager.AppSettings["SMTPServerName"].ToString();
                    string smtpUserName = ConfigurationManager.AppSettings["SMTPUserName"].ToString();
                    string smtpPassword = ConfigurationManager.AppSettings["SMTPPassword"].ToString();

                    MailMessage mail = new MailMessage();
                    SmtpClient SmtpServer = new SmtpClient(smtpServer);
                    mail.From = new MailAddress(smtpUserName);
                    string email = "";
                    if (ds5.Tables[0].Rows.Count > 0)
                    {
                        string eml = ds5.Tables[0].Rows[0][0].ToString();
                        //int eml1 = Convert.ToInt16(eml);
                        //eml1 = eml1 + 1;
                        email = eml.ToString();
                    }
                    else
                    {
                        email = Session["usrEmail"]?.ToString();
                            //"krishnakreddy77@gmail.com";
                    }

                    //string email = ds.Tables[0].Rows[0][0].ToString();
                    mail.To.Add(email);
                    mail.Subject = "Image";
                    mail.IsBodyHtml = true;
                    mail.Body = "Key is : " + TextBox1.Text + "<br /> <br /> File Name is : " + FileUpload1?.FileName;

                    SmtpServer.Port = 587;
                    SmtpServer.EnableSsl = true;
                    SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                    SmtpServer.UseDefaultCredentials = false;
                    SmtpServer.Credentials = new System.Net.NetworkCredential(smtpUserName, smtpPassword);

                    //Attachment
                    System.Net.Mail.Attachment attachment;

                    attachment = new System.Net.Mail.Attachment(Image2.ImageUrl);
                    mail.Attachments.Add(attachment);

                    SmtpServer.Send(mail);

                    string fname = FileUpload1.FileName;
                    string Fpath = Server.MapPath("~\\Files\\");
                    if (File.Exists(Fpath + fname))
                    {
                        File.Delete(Fpath + fname);
                    }
                }
            }
        }




        private string AESEncryptUsingByte(byte[] clearBytes, string key)
        {
            string EncryptionKey = key;
            string clearText;
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }


        private string DESEncrypt(byte[] messageBytes, string password)
        {
            password = password.Substring(0, 8);
            // Encode message and password
            //byte[] messageBytes = ASCIIEncoding.ASCII.GetBytes(message);
            byte[] passwordBytes = ASCIIEncoding.ASCII.GetBytes(password);

            // Set encryption settings -- Use password for both key and init. vector
            DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
            ICryptoTransform transform = provider.CreateEncryptor(passwordBytes, passwordBytes);
            CryptoStreamMode mode = CryptoStreamMode.Write;

            // Set up streams and encrypt
            MemoryStream memStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memStream, transform, mode);
            cryptoStream.Write(messageBytes, 0, messageBytes.Length);
            cryptoStream.FlushFinalBlock();

            // Read the encrypted message from the memory stream
            byte[] encryptedMessageBytes = new byte[memStream.Length];
            memStream.Position = 0;
            memStream.Read(encryptedMessageBytes, 0, encryptedMessageBytes.Length);

            // Encode the encrypted message as base64 string
            string encryptedMessage = Convert.ToBase64String(encryptedMessageBytes);

            return encryptedMessage;
        }


        private string RC2Encrypt(byte[] clearBytes, string key)
        {
            string EncryptionKey = key.Substring(0, 8);
            string clearText;
            //byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (RC2 encryptor = RC2.Create())
            {

                encryptor.Key = ASCIIEncoding.ASCII.GetBytes(EncryptionKey);
                encryptor.IV = ASCIIEncoding.ASCII.GetBytes(EncryptionKey);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public System.Drawing.Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn, 0, byteArrayIn.Length, true);
            ms.Write(byteArrayIn, 0, byteArrayIn.Length);
            using (System.Drawing.Image returnImage = System.Drawing.Image.FromStream(ms, true))
                return (returnImage);
        }

        private byte[] ConvertBitmapToByteArray(Bitmap imageToConvert)
        {
            MemoryStream ms = new System.IO.MemoryStream();
            imageToConvert.Save(ms, ImageFormat.Png);

            return ms.ToArray();
        }

        public static Bitmap embedText(string text, Bitmap bmp)
        {
            // initially, we'll be hiding characters in the image
            State state = State.Hiding;

            // holds the index of the character that is being hidden
            int charIndex = 0;

            // holds the value of the character converted to integer
            int charValue = 0;

            // holds the index of the color element (R or G or B) that is currently being processed
            long pixelElementIndex = 0;

            // holds the number of trailing zeros that have been added when finishing the process
            int zeros = 0;

            // hold pixel elements
            int R = 0, G = 0, B = 0;

            // pass through the rows
            for (int i = 0; i < bmp.Height; i++)
            {
                // pass through each row
                for (int j = 0; j < bmp.Width; j++)
                {
                    // holds the pixel that is currently being processed
                    Color pixel = bmp.GetPixel(j, i);

                    // now, clear the least significant bit (LSB) from each pixel element
                    R = pixel.R - pixel.R % 2;
                    G = pixel.G - pixel.G % 2;
                    B = pixel.B - pixel.B % 2;

                    // for each pixel, pass through its elements (RGB)
                    for (int n = 0; n < 3; n++)
                    {
                        // check if new 8 bits has been processed
                        if (pixelElementIndex % 8 == 0)
                        {
                            // check if the whole process has finished
                            // we can say that it's finished when 8 zeros are added
                            if (state == State.Filling_With_Zeros && zeros == 8)
                            {
                                // apply the last pixel on the image
                                // even if only a part of its elements have been affected
                                if ((pixelElementIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                // return the bitmap with the text hidden in
                                return bmp;
                            }

                            // check if all characters has been hidden
                            if (charIndex >= text.Length)
                            {
                                // start adding zeros to mark the end of the text
                                state = State.Filling_With_Zeros;
                            }
                            else
                            {
                                // move to the next character and process again
                                charValue = text[charIndex++];
                            }
                        }

                        // check which pixel element has the turn to hide a bit in its LSB
                        switch (pixelElementIndex % 3)
                        {
                            case 0:
                                {
                                    if (state == State.Hiding)
                                    {
                                        // the rightmost bit in the character will be (charValue % 2)
                                        // to put this value instead of the LSB of the pixel element
                                        // just add it to it
                                        // recall that the LSB of the pixel element had been cleared
                                        // before this operation
                                        R += charValue % 2;

                                        // removes the added rightmost bit of the character
                                        // such that next time we can reach the next one
                                        charValue /= 2;
                                    }
                                }
                                break;
                            case 1:
                                {
                                    if (state == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                }
                                break;
                            case 2:
                                {
                                    if (state == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }
                                break;
                        }

                        pixelElementIndex++;

                        if (state == State.Filling_With_Zeros)
                        {
                            // increment the value of zeros until it is 8
                            zeros++;
                        }
                    }
                }
            }

            return bmp;
        }
    }
}