<%@ Page language="C#" %>
<%@ outputcache location="None" %>
<%@ Import namespace="samples.sforce.com" %>
<%@ Import namespace="System.Xml" %>

<script runat='server'>
void Page_Load(object sender, EventArgs e)
{
    // ensure that the user is authenticated
	if(!Request.IsAuthenticated)
	{
		Response.StatusCode = 401;
		Response.End();
	}
	else
	{
		// we need to convert the default NT username string
		// into an user principal name format, which conviently look
		// like email addresses
		// you'll need to change this to match your environment
		// depending on how you can systematically map from your
		// AD users to their salesforce.com username
		// TODO: change domain part of UPN
		username.Value = Context.User.Identity.Name.Split(new char[] {'\\'})[1] + "@sample.org";
		token.Value    = SingleSignOn.CreateToken(username.Value);

	}
	
	if (Request.Params["xml"] != null) {
		Response.ContentType = "text/xml; charset=UTF-8";
		XmlTextWriter w = new XmlTextWriter(Response.Output);
        w.WriteStartDocument();
		w.WriteStartElement("authentication");
		w.WriteStartElement("username");
		w.WriteString(username.Value);
		w.WriteEndElement();
		w.WriteStartElement("token");
		w.WriteString(token.Value);
		w.WriteEndElement();
		w.WriteEndElement();
        w.WriteEndDocument();
		Response.End();
	}
	else {
        // startURL is used to redirect the user to a URL after they have authenticated.
		startURL.Value = Request.Params["startURL"];
		if (startURL.Value == "") {
			startURL.Visible = false;
		}

		// uncomment out the code lines below if you want to config logout url or have automatic SSO
		// logoutURL allow you to customize the logoutURL
		// logoutURL.Value="http://intranet";
		// ssoStartPage can override the page that a user is redirected to when they are not authenticated.  
		// This can be used to automatically login a user when they click on a link
		// ssoStartPage.Value = "http://intranet/SSOSamples/gotosfdc.aspx";
		
	}
}

</script>


<html>
<head>
</head>
<body onLoad="document.sfdc.submit();">
<form action="https://www.salesforce.com/login.jsp" METHOD="POST" name="sfdc">
<input type="hidden" name="un" runat="server" id="username">
<input type="hidden" name="pw" runat="server" id="token">
<input type="hidden" name="startURL" runat="server" id="startURL">
<input type="hidden" name="logoutURL" runat="server" id="logoutURL">
<input type="hidden" name="ssoStartPage" runat="server" id="ssoStartPage">
<input type="hidden" name="jse" value="0">
<input type="hidden" name="rememberUn" value="1">
<script language="Javascript1.2">
   document.aspPostForm.jse.value = 1;
</script>
</form>
</body>
</html>
