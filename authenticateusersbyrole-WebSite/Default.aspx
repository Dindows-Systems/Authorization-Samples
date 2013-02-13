<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <ul>
            <asp:Repeater ID="TopMenuRepeater" runat="server" DataSourceID="SiteMapDataSource">
                <ItemTemplate>
                    <li>
                        <asp:HyperLink runat="server" ID="MenuLink" NavigateUrl='<%# Eval("Url") %>'><em><b><%# Eval("Title") %></b></em></asp:HyperLink>
                    </li>
                </ItemTemplate>
            </asp:Repeater>
        </ul>
    </div>
    </form>
    <asp:SiteMapDataSource ID="SiteMapDataSource" runat="server" ShowStartingNode="False" />
</body>
</html>
