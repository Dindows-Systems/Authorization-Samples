Imports System.Security.Permissions

Partial Class Roles_RoleBasedAuthorization
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            BindUserGrid()
        End If
    End Sub

    Private Sub BindUserGrid()
        Dim allUsers As MembershipUserCollection = Membership.GetAllUsers()
        UserGrid.DataSource = allUsers
        UserGrid.DataBind()
    End Sub

#Region "RowCreated Event Handler"
    Protected Sub UserGrid_RowCreated(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewRowEventArgs) Handles UserGrid.RowCreated
        If e.Row.RowType = DataControlRowType.DataRow AndAlso e.Row.RowIndex <> UserGrid.EditIndex Then
            ' Programmatically reference the Edit and Delete LinkButtons
            Dim EditButton As LinkButton = CType(e.Row.FindControl("EditButton"), LinkButton)
            Dim DeleteButton As LinkButton = CType(e.Row.FindControl("DeleteButton"), LinkButton)

            EditButton.Visible = (User.IsInRole("Administrators") OrElse User.IsInRole("Supervisors"))
            DeleteButton.Visible = User.IsInRole("Administrators")
        End If
    End Sub
#End Region

#Region "Editing-related Event Handlers"
    Protected Sub UserGrid_RowEditing(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewEditEventArgs) Handles UserGrid.RowEditing
        'Set the grid's EditIndex and rebind the data
        UserGrid.EditIndex = e.NewEditIndex
        BindUserGrid()
    End Sub

    Protected Sub UserGrid_RowCancelingEdit(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewCancelEditEventArgs) Handles UserGrid.RowCancelingEdit
        'Revert the grid's EditIndex to -1 and rebind the data
        UserGrid.EditIndex = -1
        BindUserGrid()
    End Sub

    <PrincipalPermission(SecurityAction.Demand, Role:="Administrators")> _
    <PrincipalPermission(SecurityAction.Demand, Role:="Supervisors")> _
    Protected Sub UserGrid_RowUpdating(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewUpdateEventArgs) Handles UserGrid.RowUpdating
        'Exit if the page is not valid
        If Not Page.IsValid Then
            Exit Sub
        End If

        ' Determine the username of the user we are editing
        Dim UserName As String = UserGrid.DataKeys(e.RowIndex).Value.ToString()

        ' Read in the entered information and update the user
        Dim EmailTextBox As TextBox = CType(UserGrid.Rows(e.RowIndex).FindControl("Email"), TextBox)
        Dim CommentTextBox As TextBox = CType(UserGrid.Rows(e.RowIndex).FindControl("Comment"), TextBox)

        ' Return information about the user
        Dim UserInfo As MembershipUser = Membership.GetUser(UserName)

        ' Update the User account information
        UserInfo.Email = EmailTextBox.Text.Trim()
        UserInfo.Comment = CommentTextBox.Text.Trim()

        Membership.UpdateUser(UserInfo)

        ' Revert the grid's EditIndex to -1 and rebind the data
        UserGrid.EditIndex = -1
        BindUserGrid()
    End Sub
#End Region

#Region "RowDeleting Event Handler"
    <PrincipalPermission(SecurityAction.Demand, Role:="Administrators")> _
    Protected Sub UserGrid_RowDeleting(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewDeleteEventArgs) Handles UserGrid.RowDeleting
        'Determine the username of the user we are editing
        Dim UserName As String = UserGrid.DataKeys(e.RowIndex).Value.ToString()

        ' Delete the user
        Membership.DeleteUser(UserName)

        ' Revert the grid's EditIndex to -1 and rebind the data
        UserGrid.EditIndex = -1
        BindUserGrid()
    End Sub
#End Region
End Class
