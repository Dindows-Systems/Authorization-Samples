<%@ Control Language="C#" AutoEventWireup="true" Inherits="OpenIDControl" Codebehind="OpenIDControl.ascx.cs" %>
<asp:Panel ID="FormPanel" runat="server">
    <b>OpenID Login</b><br />
    <asp:TextBox ID="openid_url" runat="server" CssClass="openid_url"></asp:TextBox>
    <asp:Button ID="LoginButton" runat="server" CssClass="openid_submit" Text="&gt;&gt;" OnClick="Button_Click" UseSubmitBehavior="true" /><br />
   <asp:HyperLink ID=hlGetOpenId runat=server 
        NavigateUrl="https://www.myopenid.com/signup" Target="_blank" 
        ToolTip="Get an Open ID!" >Get an OpenID!</asp:HyperLink>
    <asp:Label ID="LLabel" runat="server"></asp:Label>
</asp:Panel>
<asp:Panel ID="StatusPanel" visible="false" runat="server">
    <asp:Label ID="SLabel" runat="server">Welcome, <%=UserObject.Identity %>.</asp:Label>
    <asp:LinkButton ID="LogOutButton" runat="server" OnClick="LogOutButton_Click">Log Out</asp:LinkButton>
</asp:Panel>
