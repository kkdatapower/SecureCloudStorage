using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SecureCloudStorage
{
    public partial class ViewDet : System.Web.UI.Page
    {
        public enum State
        {
            Hiding,
            Filling_With_Zeros
        };
        SqlConnection con = new SqlConnection(@"Server=tcp:securestoragedatabase.database.windows.net,1433;Initial Catalog=secureUpload;Persist Security Info=False;User ID=krishna;Password=Cheppanu$911;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30");
        //SqlConnection con = new SqlConnection(@"Data Source=localhost\SQLEXPRESS;Initial Catalog=secureUpload;Integrated Security=True");
        protected void Page_Load(object sender, EventArgs e)
        {
            string ses = (string)Session["status"];
            if (ses == null)
            {
                Response.Redirect("Login.aspx");
            }

            string uuid = Request.QueryString["ID"];
            string ufid = Request.QueryString["FID"];
            string auth = "select * from details where uid='" + uuid + "' and fid='" + ufid + "'";
            SqlDataAdapter sdau = new SqlDataAdapter(auth, con);
            DataSet dsu = new DataSet();
            sdau.Fill(dsu);

            if (dsu.Tables[0].Rows.Count == 0)
            {
                Page.ClientScript.RegisterStartupScript(GetType(), "msgbox", "alert('Cannot Access!!!')", true);
                Session["access"] = "no";
                Response.Redirect("ViewList.aspx");
            }
            else
            {
                Session["access"] = "yes";
            }

            string uid = Request.QueryString["ID"];
            string fid = Request.QueryString["FID"];

            string s = "select * from details where fid='" + fid + "'";
            SqlDataAdapter sda = new SqlDataAdapter(s, con);
            DataSet ds = new DataSet();
            sda.Fill(ds);
            string url;
            string ext = ds.Tables[0].Rows[0][6].ToString();
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
            Image1.ImageUrl = url;
            Label1.Text = ds.Tables[0].Rows[0][2].ToString();
        }
        private byte[] ConvertBitmapToByteArray(Bitmap imageToConvert)
        {
            MemoryStream ms = new System.IO.MemoryStream();
            imageToConvert.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }
        protected void Button2_Click(object sender, EventArgs e)
        {
            string uid = Request.QueryString["ID"];
            string fid = Request.QueryString["FID"];
            string s = "select lsb from details where fid='" + fid + "'";
            SqlDataAdapter sda = new SqlDataAdapter(s, con);
            DataTable dt = new DataTable();
            sda.Fill(dt);
            string image, path;
            image = FileUpload1.FileName;
            path = Server.MapPath("~\\Upload\\");
            FileUpload1.SaveAs(path + image);

            Bitmap b = new Bitmap(path + image);
            byte[] op1 = ConvertBitmapToByteArray(b);

            byte[] ba = (byte[])dt.Rows[0][0];
            int lenn = ba.Length;

            int Clen = op1.Length + 13;
            //int len = ba.Length;

            if (Clen == lenn || Clen - 13 == lenn)
            {
                Bitmap img2 = (Bitmap)byteArrayToImage(ba);
                string res = extractText(img2);
                TextBox1.Text = res;
                Button1.Enabled = true;
                TextBox1.ForeColor = System.Drawing.Color.Black;
            }
            else
            {
                Response.Redirect("dummy.html");
                TextBox1.Text = "Wrong Image";
                Button1.Enabled = false;
                TextBox1.ForeColor = System.Drawing.Color.Red;
            }


        }

        private string RC2Decrypt(string cipherText, string key)
        {
            string EncryptionKey = key.Substring(0, 8);
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (RC2 encryptor = RC2.Create())
            {

                encryptor.Key = ASCIIEncoding.ASCII.GetBytes(EncryptionKey);
                encryptor.IV = ASCIIEncoding.ASCII.GetBytes(EncryptionKey);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = ASCIIEncoding.ASCII.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        private string DESDecrypt(string encryptedMessage, string password)
        {
            password = password.Substring(0, 8);
            // Convert encrypted message and password to bytes
            byte[] encryptedMessageBytes = Convert.FromBase64String(encryptedMessage);
            byte[] passwordBytes = ASCIIEncoding.ASCII.GetBytes(password);

            // Set encryption settings -- Use password for both key and init. vector
            DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
            ICryptoTransform transform = provider.CreateDecryptor(passwordBytes, passwordBytes);
            CryptoStreamMode mode = CryptoStreamMode.Write;

            // Set up streams and decrypt
            MemoryStream memStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memStream, transform, mode);
            cryptoStream.Write(encryptedMessageBytes, 0, encryptedMessageBytes.Length);
            cryptoStream.FlushFinalBlock();

            // Read decrypted message from memory stream
            byte[] decryptedMessageBytes = new byte[memStream.Length];
            memStream.Position = 0;
            memStream.Read(decryptedMessageBytes, 0, decryptedMessageBytes.Length);

            // Encode deencrypted binary data to base64 string
            string message = ASCIIEncoding.ASCII.GetString(decryptedMessageBytes);

            return message;
        }

        private string AESDecryptUsingByte(string encryptedMessage, string key)
        {
            byte[] cipherBytes = Convert.FromBase64String(encryptedMessage);
            string EncryptionKey = key;
            string cipherText;
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = ASCIIEncoding.ASCII.GetString(ms.ToArray());
                }
            }
            return cipherText;
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
        protected void Button1_Click(object sender, EventArgs e)
        {
            try
            {

                string uploadFileName, uploadKey;
                uploadFileName = FileUpload1.FileName.ToString();
                uploadKey = TextBox1.Text.ToString();
                if (String.IsNullOrEmpty(uploadFileName) || String.IsNullOrEmpty(uploadKey))
                {
                    Page.ClientScript.RegisterStartupScript(GetType(), "msgbox", "alert('Please fill the details');", true);
                    return;
                }
                string file1 = "", path = "";
                file1 = FileUpload1.FileName;
                path = Server.MapPath("~\\DecFilesTemp\\");
                FileUpload1.SaveAs(path + file1);

                string images;
                images = FileUpload1.FileName;
                //paths = Server.MapPath("~\\images\\");
                //FileUpload2.SaveAs(path + images);

                Bitmap b = new Bitmap(path + images);
                byte[] op1 = ConvertBitmapToByteArray(b);
                Bitmap b1 = embedText(TextBox1.Text, b);

                byte[] op = ConvertBitmapToByteArray(b1);


                string images1, paths;
                images1 = FileUpload1.FileName;
                paths = Server.MapPath("~\\images\\");
                //FileUpload2.SaveAs(paths + images);

                Bitmap b2 = new Bitmap(paths + images1);
                byte[] op2 = ConvertBitmapToByteArray(b2);
                Bitmap b3 = embedText(TextBox1.Text, b);

                byte[] op3 = ConvertBitmapToByteArray(b3);

                bool res;

                res = StructuralComparisons.StructuralEqualityComparer.Equals(op3, op1);

                if (!res)
                {

                    Page.ClientScript.RegisterStartupScript(GetType(), "msgbox", "alert('Details are not correct ');", true);
                    return;
                }

                string uid = Request.QueryString["ID"];
                string fid = Request.QueryString["FID"];

                string s = "select * from details where fid='" + fid + "'";
                SqlDataAdapter sda = new SqlDataAdapter(s, con);
                DataSet ds = new DataSet();
                sda.Fill(ds);


                string fname = ds.Tables[0].Rows[0][2].ToString();


                //Save the Input File, Decrypt it and save the decrypted file in output path.
                //  FileUpload1.SaveAs(input);
                string file = fname, aespath = "", rcpath = "", despath = "";
                aespath = Server.MapPath("~\\AES\\" + file);
                rcpath = Server.MapPath("~\\RC\\" + file);
                despath = Server.MapPath("~\\DES\\" + file);
                string aesEncrypted = File.ReadAllText(aespath);
                string desEncrypted = File.ReadAllText(despath);
                string rc2Encrypted = File.ReadAllText(rcpath);

                string keyVal = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456";

                string aesDecrypt = AESDecryptUsingByte(aesEncrypted, keyVal);
                string desDecrypt = DESDecrypt(desEncrypted, keyVal);
                string rc2Decrypt = RC2Decrypt(rc2Encrypted, keyVal);

                string decryptText = aesDecrypt + desDecrypt + rc2Decrypt;

                string rootPath = Server.MapPath("~");
                string decryptFilePath = rootPath + "\\Files\\Dec_" + fname;
                File.WriteAllText(decryptFilePath, decryptText);

                Response.Clear();
                Response.AddHeader("Content-Disposition", "attachment; filename=" + "Dec_" + fname);
                Response.AddHeader("Content-Length", decryptText.Length.ToString());
                Response.ContentType = "application/octet-stream";
                Response.WriteFile(decryptFilePath);
                //Response.End();
                Response.Flush();

                //WebClient req = new WebClient();
                //HttpResponse response = HttpContext.Current.Response;
                ////response.Flush();
                //response.Clear();
                //response.ClearContent();
                //response.ClearHeaders();
                ////response.Buffer = true;
                //response.ContentType = "application/octet-stream";
                //response.AddHeader("Content-Disposition", "attachment;filename=" + "Dec_" + fname);
                //byte[] data = req.DownloadData(decryptFilePath);
                ////Response.TransmitFile(decryptFilePath);
                //response.BinaryWrite(data);
                ////response.End();
                //response.Flush();
                ////HttpContext.Current.Response.Flush();
                ////HttpContext.Current.Response.SuppressContent = true;
                //HttpContext.Current.ApplicationInstance.CompleteRequest();

            }
            catch (Exception ec)
            {
                /* string uid = Request.QueryString["ID"];
                 string fid = Request.QueryString["FID"];

                 string s = "select * from details where fid='" + fid + "'";
                 SqlDataAdapter sda = new SqlDataAdapter(s, con);
                 DataSet ds = new DataSet();
                 sda.Fill(ds);

                 string Fpath = Server.MapPath("~\\Files\\");
                 string fname = ds.Tables[0].Rows[0][2].ToString();
                 string Ffile = "Dec_w" + fname;
                 File.Create(Fpath+Ffile);File.OpenWrite(Fpath + Ffile);
                 File.WriteAllText(Fpath+Ffile, "This is Wrong File downloaded as the right key couldn't be fetched to have access to the right file");

                 /*Response.Clear();
                 StringWriter oStringWriter = new StringWriter();
                // oStringWriter.WriteLine("Line 1");
                 Response.ContentType = "text/plain";

                 Response.AddHeader("content-disposition", "attachment;filename=" +Ffile );
                 File.WriteAllText(Fpath+Ffile, String.Empty);
                 Response.Clear();


                 StreamWriter strm = File.CreateText(Fpath+Ffile);
                 strm.Flush();
                 strm.Close();*/
            }
            //Response.Redirect(Fpath + Ffile);
        }

        public static string extractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            // holds the text that will be extracted from the image
            string extractedText = String.Empty;

            // pass through the rows
            for (int i = 0; i < bmp.Height; i++)
            {
                // pass through each row
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);

                    // for each pixel, pass through its elements (RGB)
                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                {
                                    // get the LSB from the pixel element (will be pixel.R % 2)
                                    // then add one bit to the right of the current character
                                    // this can be done by (charValue = charValue * 2)
                                    // replace the added bit (which value is by default 0) with
                                    // the LSB of the pixel element, simply by addition
                                    charValue = charValue * 2 + pixel.R % 2;
                                }
                                break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                }
                                break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                }
                                break;
                        }

                        colorUnitIndex++;

                        // if 8 bits has been added,
                        // then add the current character to the result text
                        if (colorUnitIndex % 8 == 0)
                        {
                            // reverse? of course, since each time the process occurs
                            // on the right (for simplicity)
                            charValue = reverseBits(charValue);

                            // can only be 0 if it is the stop character (the 8 zeros)
                            if (charValue == 0)
                            {
                                return extractedText;
                            }

                            // convert the character value from int to char
                            char c = (char)charValue;

                            // add the current character to the result text
                            extractedText += c.ToString();
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n)
        {
            int result = 0;

            for (int i = 0; i < 8; i++)
            {
                result = result * 2 + n % 2;

                n /= 2;
            }

            return result;
        }

        private static byte[] DecryptBytes(SymmetricAlgorithm alg, byte[] message)
        {
            if ((message == null) || (message.Length == 0))
            {
                return message;
            }

            if (alg == null)
            {
                throw new ArgumentNullException("alg");
            }

            using (var stream = new MemoryStream())
            using (var decryptor = alg.CreateDecryptor())
            using (var encrypt = new CryptoStream(stream, decryptor, CryptoStreamMode.Write))
            {
                encrypt.Write(message, 0, message.Length);
                encrypt.FlushFinalBlock();
                return stream.ToArray();
            }
        }

        public System.Drawing.Image byteArrayToImage(byte[] byteArrayIn)
        {

            MemoryStream ms = new MemoryStream(byteArrayIn, 0, byteArrayIn.Length, true);
            ms.Write(byteArrayIn, 0, byteArrayIn.Length);
            System.Drawing.Image returnImage = System.Drawing.Image.FromStream(ms, true);


            //  var fs = new BinaryWriter(new FileStream(@"D:\Imagestegano1\Imagestegano1\images\Enc.gif", FileMode.Create, FileAccess.Write));
            // fs.Write(byteArrayIn,0,327174);
            //fs.Close();

            return (returnImage);
        }
    }
}