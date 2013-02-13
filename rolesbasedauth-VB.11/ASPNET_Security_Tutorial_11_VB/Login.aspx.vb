
Partial Class Login
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            If Request.IsAuthenticated AndAlso Not String.IsNullOrEmpty(Request.QueryString("ReturnUrl")) Then
                ' This is an unauthorized, authenticated request...
                Response.Redirect("~/UnauthorizedAccess.aspx")
            End If
        End If
    End Sub

    'Protected Sub LoginButton_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles LoginButton.Click
    '    ' Validate the user against the Membership framework user store
    '    If Membership.ValidateUser(UserName.Text, Password.Text) Then
    '        ' Log the user into the site
    '        FormsAuthentication.RedirectFromLoginPage(UserName.Text, RememberMe.Checked)
    '    End If

    '    ' If we reach here, the user's credentials were invalid
    '    InvalidCredentialsMessage.Visible = True
    'End Sub

    Protected Sub myLogin_Authenticate(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.AuthenticateEventArgs) Handles myLogin.Authenticate
        ' Get the email address entered
        Dim EmailTextBox As TextBox = CType(myLogin.FindControl("Email"), TextBox)
        Dim email As String = EmailTextBox.Text.Trim()

        ' Verify that the username/password pair is valid
        If Membership.ValidateUser(myLogin.UserName, myLogin.Password) Then
            ' Username/password are valid, check email
            Dim usrInfo As MembershipUser = Membership.GetUser(myLogin.UserName)
            If usrInfo IsNot Nothing AndAlso String.Compare(usrInfo.Email, email, True) = 0 Then
                ' Email matches, the credentials are valid
                e.Authenticated = True
            Else
                ' Email address is invalid...
                e.Authenticated = False
            End If
        Else
            ' Username/password are not valid...
            e.Authenticated = False
        End If
    End Sub

    Protected Sub myLogin_LoginError(ByVal sender As Object, ByVal e As System.EventArgs) Handles myLogin.LoginError
        ' Determine why the user could not login...        
        myLogin.FailureText = "Your login attempt was not successful. Please try again."

        ' Does there exist a User account for this user?
        Dim usrInfo As MembershipUser = Membership.GetUser(myLogin.UserName)
        If usrInfo IsNot Nothing Then
            ' Is this user locked out?
            If usrInfo.IsLockedOut Then
                myLogin.FailureText = "Your account has been locked out because of too many invalid login attempts. Please contact the administrator to have your account unlocked."
            ElseIf Not usrInfo.IsApproved Then
                myLogin.FailureText = "Your account has not yet been approved. You cannot login until an administrator has approved your account."
            End If
        End If
    End Sub
End Class
