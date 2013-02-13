
Partial Class Roles_ManageRoles
    Inherits System.Web.UI.Page


    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            DisplayRolesInGrid()
        End If
    End Sub

    Private Sub DisplayRolesInGrid()
        RoleList.DataSource = Roles.GetAllRoles()
        RoleList.DataBind()
    End Sub

    Protected Sub CreateRoleButton_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles CreateRoleButton.Click
        Dim newRoleName As String = RoleName.Text.Trim()

        If Not Roles.RoleExists(newRoleName) Then
            ' Create the role
            Roles.CreateRole(newRoleName)

            ' Refresh the RoleList Grid
            DisplayRolesInGrid()
        End If

        RoleName.Text = String.Empty
    End Sub

    Protected Sub RoleList_RowDeleting(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewDeleteEventArgs) Handles RoleList.RowDeleting
        ' Get the RoleNameLabel
        Dim RoleNameLabel As Label = CType(RoleList.Rows(e.RowIndex).FindControl("RoleNameLabel"), Label)

        ' Delete the role
        Roles.DeleteRole(RoleNameLabel.Text, False)

        ' Rebind the data to the RoleList grid
        DisplayRolesInGrid()
    End Sub
End Class
