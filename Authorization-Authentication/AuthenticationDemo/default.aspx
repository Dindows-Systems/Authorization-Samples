<%@ Page Language="C#" AutoEventWireup="true" CodeFile="default.aspx.cs" Inherits="AuthenticationDemo.PageDefault" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head" runat="server">
    <title></title>
    <link rel="Stylesheet" href="default.css" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <h1>Authentication Demo</h1>
        <hr />
        <div id="output" runat="server" /> 
        <hr />
        <div id="exMessage" runat="server" clientidmode="Static" /> 
    </div>
    </form>
</body>
</html>
