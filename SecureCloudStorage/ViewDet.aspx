<%@ Page Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="ViewDet.aspx.cs" Inherits="SecureCloudStorage.ViewDet" %>

<asp:Content ID="Body" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div align="center" style="width:100%">
        <table align="center" width="50%">
            <tr>
                <td colspan="2" align="center">
                    <br />
                    <br />
                    <asp:Label ID="Label5" runat="server" Text="FILE DETAILS" Font-Bold="True" Font-Size="16pt" ForeColor="#FCCE54"></asp:Label>
                    <hr />
                    <br />
                </td>
            </tr>
            <tr>
                <td align="Right" width="40%">
                  <asp:Label ID="Label3" runat="server" Text="Image" Font-Bold="True" Font-Size="15pt" ForeColor="#0099CC"></asp:Label> :                   
                    </td>
                <td align="center" width="60%">                  
                    <asp:FileUpload ID="FileUpload1" runat="server" ForeColor="#0099CC" CssClass="txt" />
                    <br />
                    <br />
                   <%--<asp:Button ID="Button2" runat="server" OnClick="Button2_Click" Text="Extract"  ForeColor="White" CssClass="button" />--%>
                </td>
                <td align="center" width="40%">
                    <asp:Image ID="Image1" runat="server" width="95%"/>
                    <br />
                    <asp:Label ID="Label1" runat="server" Text="Label" Font-Bold="True" Font-Size="15pt" ForeColor="#0099CC"></asp:Label>
                    <br />
                    <br />
                </td>
            </tr>
            <tr>
                <td align="Right">
                  <asp:Label ID="Label4" runat="server" Text="Key" Font-Bold="True" Font-Size="15pt" ForeColor="#0099CC"></asp:Label> :           
                    </td>
                <td align="center">
                    <br />
                    <br />
                     <asp:TextBox ID="TextBox1" runat="server" CssClass="txt" Enabled="True"></asp:TextBox>
                    <br />
                    <br />
                        <asp:Button ID="Button1" runat="server" Text="Download" OnClick="Button1_Click" ForeColor="White" CssClass="button" />
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
