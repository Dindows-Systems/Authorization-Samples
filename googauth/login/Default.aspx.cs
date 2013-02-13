using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using DotNetOpenAuth.OpenId.RelyingParty;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;

public partial class login_Default : System.Web.UI.Page
{


	protected void Page_Load(object sender, EventArgs e) {

        OpenIdRelyingParty rp = new OpenIdRelyingParty();
        var response = rp.GetResponse();
       

        // if this isn't a postback and it isn't an oauth response, push them over to google to login...
        // unless you want to give them a button or option to login
        if (response == null && !IsPostBack)
            GoogleLogin();

      

        // process oauth response
        if (response != null)
        {
    

            switch (response.Status)
            {
                case AuthenticationStatus.Authenticated:
                    FetchResponse fetch = response.GetExtension<FetchResponse>();
                    string email = fetch.Attributes["http://axschema.org/contact/email"].Values[0].ToString();
                    string firstname = fetch.Attributes["http://axschema.org/namePerson/first"].Values[0].ToString();
                    string lastname = fetch.Attributes["http://axschema.org/namePerson/last"].Values[0].ToString();
                    string id = response.ClaimedIdentifier.ToString();

                    // could add a check here to validate user is from a specific domain, like this:
                    //if (!email.EndsWith("@fynydd.com"))
                    //    Response.Redirect("onlyfynydd.aspx", true);

                        


                    // put some code here to record the user login

                        


                    // can't use regular auth redirect because we had to double urlencode the return to...
                    //FormsAuthentication.RedirectFromLoginPage(email, false);

                    FormsAuthentication.SetAuthCookie(email, false);
                        
                    string returnurl = FormsAuthentication.DefaultUrl;
                        
                    if (Request["returnurl"] != null)
                        returnurl = Server.UrlDecode(Request["returnurl"].ToString());
                        
                    if (returnurl.Length ==0) returnurl = "/";
                        
                    Response.Redirect(returnurl, true);

                    break;
                case AuthenticationStatus.Canceled:
                    break;
                case AuthenticationStatus.Failed:
                    break;
            }
        }
	}
 
 

 

    protected void Unnamed1_Click(object sender, ImageClickEventArgs e)
    {
        GoogleLogin();
    }



    private void GoogleLogin() {

        string discoveryUri = "https://www.google.com/accounts/o8/id";

        OpenIdRelyingParty openid = new OpenIdRelyingParty();



        string returnUrl = "";
        if (Request["ReturnUrl"] != null)
            returnUrl = "&ReturnUrl=" + Server.UrlEncode(Server.UrlEncode(Request["returnurl"].ToString())); //if you don't double encode, you get an oatch error?
           
        var URIbuilder = new UriBuilder(Request.Url) { Query = returnUrl };

        try
        {
            var req = openid.CreateRequest(discoveryUri, Realm.AutoDetect, URIbuilder.Uri );

            // request that we get back user's email and name
            FetchRequest fetch = new FetchRequest();
            fetch.Attributes.AddRequired(WellKnownAttributes.Contact.Email);
            fetch.Attributes.AddRequired(WellKnownAttributes.Name.First);
            fetch.Attributes.AddRequired(WellKnownAttributes.Name.Last);
            req.AddExtension(fetch);

            // push to google
            try
            {
                req.RedirectToProvider();
            }
            catch (Exception err)
            {
                // try it one more time...  disabled proxy in web.config to address a common issue, but google may also be timing out sometimes...
                req.RedirectToProvider();
            }
            
        }
        catch (Exception err)
        {
            Response.Write("error:<br><br>");
            Response.Write(Server.HtmlEncode(err.Message).Replace(System.Environment.NewLine,"<br>") + "<br><br>");
            Response.Write(Server.HtmlEncode(URIbuilder.Uri.ToString()) + "<br><br>");
            

        }
    }
}