
Partial Class Roles_UsersAndRoles
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            ' Bind the users and roles
            BindUsersToUserList()
            BindRolesToList()

            ' Check the selected user's roles
            CheckRolesForSelectedUser()

            'Display those users belonging to the currently selected role
            DisplayUsersBelongingToRole()
        End If
    End Sub

    Private Sub BindRolesToList()
        ' Get all of the roles
        Dim roleNames() As String = Roles.GetAllRoles()
        UsersRoleList.DataSource = roleNames
        UsersRoleList.DataBind()

        RoleList.DataSource = roleNames
        RoleList.DataBind()
    End Sub

#Region "'By User' Interface-Specific Methods"
    Private Sub BindUsersToUserList()
        ' Get all of the user accounts
        Dim users As MembershipUserCollection = Membership.GetAllUsers()
        UserList.DataSource = users
        UserList.DataBind()
    End Sub

    Protected Sub UserList_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles UserList.SelectedIndexChanged
        CheckRolesForSelectedUser()
    End Sub

    Private Sub CheckRolesForSelectedUser()
        ' Determine what roles the selected user belongs to
        Dim selectedUserName As String = UserList.SelectedValue
        Dim selectedUsersRoles() As String = Roles.GetRolesForUser(selectedUserName)

        ' Loop through the Repeater's Items and check or uncheck the checkbox as needed
        For Each ri As RepeaterItem In UsersRoleList.Items
            ' Programmatically reference the CheckBox
            Dim RoleCheckBox As CheckBox = CType(ri.FindControl("RoleCheckBox"), CheckBox)

            ' See if RoleCheckBox.Text is in selectedUsersRoles
            If Linq.Enumerable.Contains(Of String)(selectedUsersRoles, RoleCheckBox.Text) Then
                RoleCheckBox.Checked = True
            Else
                RoleCheckBox.Checked = False
            End If
        Next
    End Sub

    Protected Sub RoleCheckBox_CheckChanged(ByVal sender As Object, ByVal e As EventArgs)
        ' Reference the CheckBox that raised this event
        Dim RoleCheckBox As CheckBox = CType(sender, CheckBox)

        ' Get the currently selected user and role
        Dim selectedUserName As String = UserList.SelectedValue
        Dim roleName As String = RoleCheckBox.Text

        ' Determine if we need to add or remove the user from this role
        If RoleCheckBox.Checked Then
            ' Add the user to the role
            Roles.AddUserToRole(selectedUserName, roleName)

            ' Display a status message
            ActionStatus.Text = String.Format("User {0} was added to role {1}.", selectedUserName, roleName)
        Else
            ' Remove the user from the role
            Roles.RemoveUserFromRole(selectedUserName, roleName)

            ' Display a status message
            ActionStatus.Text = String.Format("User {0} was removed from role {1}.", selectedUserName, roleName)
        End If

        ' Refresh the "by role" interface
        DisplayUsersBelongingToRole()
    End Sub
#End Region

#Region "'By Role' Interface-Specific Methods"
    Protected Sub RoleList_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles RoleList.SelectedIndexChanged
        DisplayUsersBelongingToRole()
    End Sub

    Private Sub DisplayUsersBelongingToRole()
        ' Get the selected role
        Dim selectedRoleName As String = RoleList.SelectedValue

        ' Get the list of usernames that belong to the role
        Dim usersBelongingToRole() As String = Roles.GetUsersInRole(selectedRoleName)

        ' Bind the list of users to the GridView
        RolesUserList.DataSource = usersBelongingToRole
        RolesUserList.DataBind()
    End Sub

    Protected Sub RolesUserList_RowDeleting(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewDeleteEventArgs) Handles RolesUserList.RowDeleting
        ' Get the selected role
        Dim selectedRoleName As String = RoleList.SelectedValue

        ' Reference the UserNameLabel
        Dim UserNameLabel As Label = CType(RolesUserList.Rows(e.RowIndex).FindControl("UserNameLabel"), Label)

        ' Remove the user from the role
        Roles.RemoveUserFromRole(UserNameLabel.Text, selectedRoleName)

        ' Refresh the GridView
        DisplayUsersBelongingToRole()

        ' Display a status message
        ActionStatus.Text = String.Format("User {0} was removed from role {1}.", UserNameLabel.Text, selectedRoleName)

        ' Refresh the "by user" interface
        CheckRolesForSelectedUser()
    End Sub

    Protected Sub AddUserToRoleButton_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles AddUserToRoleButton.Click
        ' Get the selected role and username
        Dim selectedRoleName As String = RoleList.SelectedValue
        Dim userToAddToRole As String = UserNameToAddToRole.Text

        ' Make sure that a value was entered
        If userToAddToRole.Trim().Length = 0 Then
            ActionStatus.Text = "You must enter a username in the textbox."
            Exit Sub
        End If

        ' Make sure that the user exists in the system
        Dim userInfo As MembershipUser = Membership.GetUser(userToAddToRole)
        If userInfo Is Nothing Then
            ActionStatus.Text = String.Format("The user {0} does not exist in the system.", userNameToAddToRole)
            Exit Sub
        End If

        ' Make sure that the user doesn't already belong to this role
        If Roles.IsUserInRole(userToAddToRole, selectedRoleName) Then
            ActionStatus.Text = String.Format("User {0} already is a member of role {1}.", UserNameToAddToRole, selectedRoleName)
            Exit Sub
        End If

        ' If we reach here, we need to add the user to the role
        Roles.AddUserToRole(userToAddToRole, selectedRoleName)

        ' Clear out the TextBox
        userNameToAddToRole.Text = String.Empty

        ' Refresh the GridView
        DisplayUsersBelongingToRole()

        ' Display a status message
        ActionStatus.Text = String.Format("User {0} was added to role {1}.", UserNameToAddToRole, selectedRoleName)

        ' Refresh the "by user" interface
        CheckRolesForSelectedUser()
    End Sub
#End Region
End Class
