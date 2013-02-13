<%@ Page Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.Page" Trace="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
</head>
<body>
<form id="form1" runat="server">

<asp:ScriptManager ID="ScriptManager" runat="server"/>

<div>
	<asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="~/">Home</asp:HyperLink>
</div>

<h1>Login with a Token</h1>

<p>
	A token has been sent to you. Please enter the token in the text box below and click 'Login'.
	Note that tokens are case sensitive.
</p>
<p>
	In the message that was sent to you, you will also find a link back to this site. By
	clicking that link, you will automatically be authenticated. When you have clicked that
	link, this page will automatically be directed to the page you originally were trying to access.
</p>

<asp:Timer ID="Timer1" runat="server" Interval="2000" />
		
<asp:UpdatePanel ID="UpdatePanel1" runat="server" RenderMode="Block">
	<Triggers>
		<asp:AsyncPostBackTrigger ControlID="Timer1" EventName="Tick" />
	</Triggers>
	<ContentTemplate>
		
		<p>Time remaining: <asp:Literal ID="TimeRemainingText" runat="server" /> seconds.</p>
		<div id="RedirDiv" runat="server" visible="false"></div>
		
		<p id="TokenExpiredPara" runat="server" visible="false" style="color: Red;">
			The authentication token has expired. You must now click the 'Resend Token' button
			in order to get a new token that you can use for authentication.
		</p>
	</ContentTemplate>
</asp:UpdatePanel>

<p id="LoginFailedPara" runat="server" visible="false" enableviewstate="false" style="color:Red">
	The given token did not match the token that was sent to you or the token has expired.
	Please try again or click the "Resend Token" button to resend a new token to you.
</p>
<div>
	<asp:Label ID="Label1" runat="server" AssociatedControlID="TokenTextBox">Token:</asp:Label>
	<asp:TextBox ID="TokenTextBox" runat="server" TextMode="Password" />
	
	<asp:UpdatePanel ID="UpdatePanel2" runat="server" RenderMode="Inline">
		<Triggers>
			<asp:AsyncPostBackTrigger ControlID="Timer1" EventName="Tick" />
			<asp:PostBackTrigger ControlID="LoginButton" />
		</Triggers>
		<ContentTemplate>
			<asp:Button ID="LoginButton" runat="server" Text="Login" onclick="LoginButton_Click" />
		</ContentTemplate>
	</asp:UpdatePanel>
	
	<asp:Button ID="ResendTokenButton" runat="server" Text="Resend Token" OnClick="ResendTokenButton_Click" />
</div>

<script type="text/javascript">
	document.getElementById('<%=this.TokenTextBox.ClientID%>').focus();
	
	CheckAuthenticationStatus();
	function CheckAuthenticationStatus() {
		if (null != document.getElementById("RedirDiv")) {
			document.location = '<%=TokenAuthentication.Web.AuthenticationManager.GetOriginalUrl() %>';
		}
		else {
			setTimeout("CheckAuthenticationStatus()", 500);
		}
	}
</script>

</form>
</body>
</html>

<script runat="server">
	protected override void Render(HtmlTextWriter writer)
	{
		TimeSpan remaining = TokenAuthentication.Web.AuthenticationManager.GetPendingTimeRemaining();
		this.TimeRemainingText.Text = string.Format("{0}", (int)remaining.TotalSeconds);

		this.TokenExpiredPara.Visible = remaining <= TimeSpan.Zero;

		this.LoginButton.Enabled = !this.TokenExpiredPara.Visible;
		
		this.RedirDiv.Visible = TokenAuthentication.Web.AuthenticationManager.IsAuthenticated();
		base.Render(writer);
	}
	
	protected void LoginButton_Click(object sender, EventArgs e)
	{
		if (!TokenAuthentication.Web.AuthenticationManager.Authenticate(this.TokenTextBox.Text))
		{
			this.Trace.Warn("Failed!");
			this.LoginFailedPara.Visible = true;
		}
		else
		{
			this.Trace.Write("Succeeded!");
		}
	}
	
	protected void ResendTokenButton_Click(object sender, EventArgs e)
	{
		this.Trace.Write("Resending token...");
		TokenAuthentication.Web.AuthenticationManager.SendToken();
	}
</script>