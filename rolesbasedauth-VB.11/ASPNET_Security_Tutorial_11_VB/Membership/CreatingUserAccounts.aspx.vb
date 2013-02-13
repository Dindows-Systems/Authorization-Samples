
Partial Class Membership_CreatingUserAccounts
    Inherits System.Web.UI.Page

    Const passwordQuestion As String = "What is your favorite color"

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            SecurityQuestion.Text = passwordQuestion
        End If
    End Sub

    Protected Sub CreateAccountButton_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles CreateAccountButton.Click
        Dim createStatus As MembershipCreateStatus

        Dim newUser As MembershipUser = _
                Membership.CreateUser(Username.Text, Password.Text, _
                                   Email.Text, passwordQuestion, _
                                   SecurityAnswer.Text, True, _
                                   createStatus)

        Select Case createStatus
            Case MembershipCreateStatus.Success
                CreateAccountResults.Text = "The user account was successfully created!"

            Case MembershipCreateStatus.DuplicateUserName
                CreateAccountResults.Text = "There already exists a user with this username."

            Case MembershipCreateStatus.DuplicateEmail
                CreateAccountResults.Text = "There already exists a user with this email address."

            Case MembershipCreateStatus.InvalidEmail
                CreateAccountResults.Text = "There email address you provided in invalid."

            Case MembershipCreateStatus.InvalidAnswer
                CreateAccountResults.Text = "There security answer was invalid."

            Case MembershipCreateStatus.InvalidPassword
                CreateAccountResults.Text = "The password you provided is invalid. It must be seven characters long and have at least one non-alphanumeric character."

            Case Else
                CreateAccountResults.Text = "There was an unknown error; the user account was NOT created."
        End Select
    End Sub

    Protected Sub RegisterUser_CreatingUser(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.LoginCancelEventArgs) Handles RegisterUser.CreatingUser
        Dim trimmedUserName As String = RegisterUser.UserName.Trim()
        If RegisterUser.UserName.Length <> trimmedUserName.Length Then
            ' Show the error message
            InvalidUserNameOrPasswordMessage.Text = "The username cannot contain leading or trailing spaces."
            InvalidUserNameOrPasswordMessage.Visible = True

            ' Cancel the create user workflow
            e.Cancel = True
        Else
            ' Username is valid, make sure that the password does not contain the username
            If RegisterUser.Password.IndexOf(RegisterUser.UserName, StringComparison.OrdinalIgnoreCase) >= 0 Then
                ' Show the error message
                InvalidUserNameOrPasswordMessage.Text = "The username may not appear anywhere in the password."
                InvalidUserNameOrPasswordMessage.Visible = True

                ' Cancel the create user workflow
                e.Cancel = True
            End If
        End If
    End Sub
End Class
