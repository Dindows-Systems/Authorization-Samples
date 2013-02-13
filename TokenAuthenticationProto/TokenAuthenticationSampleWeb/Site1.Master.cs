using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TokenAuthentication.Web;

namespace TokenAuthenticationSampleWeb
{
	public partial class Site1 : System.Web.UI.MasterPage
	{

		protected override void Render(HtmlTextWriter writer)
		{
			this.LogOutSpan.Visible = AuthenticationManager.IsAuthenticated();
			base.Render(writer);
		}

		protected void LogOutLink_OnClick(object sender, EventArgs e)
		{
			AuthenticationManager.SignOut();
		}

	}
}
