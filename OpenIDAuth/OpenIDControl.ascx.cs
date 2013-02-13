using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using ExtremeSwank.Authentication.OpenID;
using ExtremeSwank.Authentication.OpenID.Plugins.Extensions;

public partial class OpenIDControl : System.Web.UI.UserControl
{
    /// <summary>
    /// Executed every time page is loaded. Checks for OpenID-related requests,
    /// and processes, if present.
    /// </summary>
    /// <param name="sender">Object invoking this method.</param>
    /// <param name="e">EventArgs associated with the request.</param>
    protected void Page_Load(object sender, EventArgs e)
    {
        // need to store openid, email, newsletter bool etc. in database for first time user.
        if (!IsPostBack)
        {
            OpenIDConsumer openid = new OpenIDConsumer();
            switch (openid.RequestedMode) {
                case RequestedMode.IdResolution:
                    openid.Identity = this.Identity;
                    SetAuthMode(openid);
                    if (openid.Validate())
                    {
                        _UserObject = openid.RetrieveUser();
                        FormPanel.Visible = false;
                        StatusPanel.Visible = true;
                        OnValidateSuccess(e);
                    }
                    else
                    {
                        FormPanel.Visible = true;
                        StatusPanel.Visible = false;
                        LLabel.Text = openid.GetError();
                        OnValidateFail(e);
                    }
                    break;
                case RequestedMode.CancelledByUser:
                    FormPanel.Visible = true;
                    StatusPanel.Visible = false;
                    LLabel.Text = "Login request cancelled.";
                    OnRemoteCancel(e);
                    break;
                case RequestedMode.None:
                    if (UserObject != null)
                    {
                        FormPanel.Visible = false;
                        StatusPanel.Visible = true;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Event fires upon successful validation received from 
    /// Identity Provider.
    /// </summary>
    public event EventHandler ValidateSuccess;

    /// <summary>
    /// Fires when successful validation is received from
    /// Identity Provider.
    /// </summary>
    /// <param name="e">EventArgs</param>
    protected virtual void OnValidateSuccess(EventArgs e)
    {
        if (ValidateSuccess != null)
        {
            ValidateSuccess(this, e);
        }
    }

    /// <summary>
    /// Event fires when unsuccessful validation is received frmo
    /// Identity Provider
    /// </summary>
    public event EventHandler ValidateFail;

    /// <summary>
    /// Fires when unsuccessful validation is received from
    /// Identity Provider
    /// </summary>
    /// <param name="e">EventArgs</param>
    protected virtual void OnValidateFail(EventArgs e)
    {
        if (ValidateFail != null)
        {
            ValidateFail(this, e);
        }
    }
    /// <summary>
    /// Event fires when user cancels request at Identity Provider
    /// and is redirected back to this application.
    /// </summary>
    public event EventHandler RemoteCancel;

    /// <summary>
    /// Fires when user cancels request at Identity Provider
    /// and is redirected back to this application.
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnRemoteCancel(EventArgs e)
    {
        if (RemoteCancel != null)
        {
            RemoteCancel(this, e);
        }
    }

    /// <summary>
    /// Event fires after user has submitted login form,
    /// but before performing authentication-related functions.
    /// </summary>
    public event EventHandler Login;

    /// <summary>
    /// Fires after user has submitted login form, but
    /// before performing authentication-related functions.
    /// </summary>
    /// <param name="e">EventArgs</param>
    protected virtual void OnLogin(EventArgs e)
    {
        if (Login != null)
        {
            Login(this, e);
        }
    }

    /// <summary>
    /// Event fires after user has used the "log out" function.
    /// </summary>
    public event EventHandler Logout;

    /// <summary>
    /// Fires after user has used the "log out" function.
    /// </summary>
    /// <param name="e">EventArgs</param>
    protected virtual void OnLogout(EventArgs e)
    {
        if (Logout != null)
        {
            Logout(this, e);
        }
    }

    private SessionPersister Persister;

    /// <summary>
    /// Sets the authentication mode to be used. 
    /// Supports either "stateful" or "stateless".
    /// Defaults to "stateful".
    /// </summary>
    public string AuthMode
    {
        get { return (string)ViewState["AuthMode"]; }
        set 
        {
            if (value == "stateful" || value == "stateless")
            {
                ViewState["AuthMode"] = value;
            }
            else
            {
                throw new Exception("AuthMode must be set to either 'stateful' or 'stateless'");
            }
        }
    }

    /// <summary>
    /// From Simple Registration Extension. Comma-delimited list of Simple Registration
    /// fields that the Identity Provider should require the user to provide.
    /// </summary>
    public string RequiredFields
    {
        get { return (string)ViewState["RequiredFields"]; }
        set { ViewState["RequiredFields"] = value; }
    }

    /// <summary>
    /// From Simple Registration Extension. Comma-delimited list of Simple Registration
    /// fields that the Identity Provider can optionally ask the user to provide.
    /// </summary>
    public string OptionalFields
    {
        get { return (string)ViewState["OptionalFields"]; }
        set { ViewState["OptionalFields"] = value; }
    }

    /// <summary>
    /// From Simple Registration Extension. URL of this site's privacy policy to send
    /// to the Identity Provider.
    /// </summary>
    public string PolicyURL
    {
        get { return (string)ViewState["Identity"]; }
        set { ViewState["Identity"] = value; }
    }

    /// <summary>
    /// Optional. Base URL of this site. Sets the scope of the authentication request. 
    /// </summary>
    public string Realm
    {
        get { return (string)ViewState["TrustRoot"]; }
        set { ViewState["TrustRoot"] = value; }
    }

    /// <summary>
    /// OpenID identitier.
    /// </summary>
    private string Identity
    {
        get { return (string)Persister["OpenID_Identity"]; }
        set { Persister["OpenID_Identity"] = value; }
    }

    private OpenIDUser _UserObject
    {
        get { return (OpenIDUser)Persister["UserObject"]; }
        set { Persister["UserObject"] = value; }
    }
    /// <summary>
    /// OpenIDUser object that represents the authenticated user and all
    /// information received from the Identity Provider.
    /// </summary>
    public OpenIDUser UserObject
    {
        get { return _UserObject; }
    }
    private void SetAuthMode(OpenIDConsumer openid)
    {
        if (this.AuthMode == "stateless")
        {
            openid.AuthMode = AuthorizationMode.Stateless;
        }
        else
        {
            openid.AuthMode = AuthorizationMode.Stateful;
        }
    }

    /// <summary>
    /// User has clicked the login button. Sets up a new OpenIDConsumer
    /// object and begins the authentication sequence. 
    /// Fires the OnLogin event. 
    /// </summary>
    /// <param name="sender">Object invoking this method.</param>
    /// <param name="e">EventArgs related to this request.</param>
    protected void Button_Click(object sender, EventArgs e)
    {
        OpenIDConsumer openid = new OpenIDConsumer();
        SetAuthMode(openid);
        SimpleRegistration sr = new SimpleRegistration(openid);
        if (this.RequiredFields != null) { sr.RequiredFields = this.RequiredFields; }
        if (this.OptionalFields != null) { sr.OptionalFields = this.OptionalFields; }
        if (this.PolicyURL != null) { sr.PolicyURL = this.PolicyURL; }
        openid.Identity = openid_url.Text;
        this.Identity = openid.Identity;
        OnLogin(e);
        openid.BeginAuth();

        if (openid.IsError())
        {
            LLabel.Text = openid.GetError();
        }
        else
        {
            LLabel.Text = "";
        }
    }
    /// <summary>
    /// User has clicked the "log out" button. Removes all information
    /// about the user from the Session state.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void LogOutButton_Click(object sender, EventArgs e)
    {
        OnLogout(e);
        SessionPersister.Cleanup();
        FormPanel.Visible = true;
        StatusPanel.Visible = false;
    }
    /// <summary>
    /// Creates a new instance of OpenIDControl.
    /// </summary>
    public OpenIDControl()
    {
        Persister = new SessionPersister();
    }


}
