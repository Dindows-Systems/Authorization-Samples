
Partial Class Roles_CreateUserWizardWithRoles
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            ' Reference the SpecifyRolesStep WizardStep
            Dim SpecifyRolesStep As WizardStep = CType(RegisterUserWithRoles.FindControl("SpecifyRolesStep"), WizardStep)

            ' Reference the RoleList CheckBoxList
            Dim RoleList As CheckBoxList = CType(SpecifyRolesStep.FindControl("RoleList"), CheckBoxList)

            ' Bind the set of roles to RoleList
            RoleList.DataSource = Roles.GetAllRoles()
            RoleList.DataBind()
        End If
    End Sub

    Protected Sub RegisterUserWithRoles_ActiveStepChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles RegisterUserWithRoles.ActiveStepChanged
        'Have we JUST reached the Complete step?
        If RegisterUserWithRoles.ActiveStep.Title = "Complete" Then
            ' Reference the SpecifyRolesStep WizardStep
            Dim SpecifyRolesStep As WizardStep = CType(RegisterUserWithRoles.FindControl("SpecifyRolesStep"), WizardStep)

            ' Reference the RoleList CheckBoxList
            Dim RoleList As CheckBoxList = CType(SpecifyRolesStep.FindControl("RoleList"), CheckBoxList)

            ' Add the checked roles to the just-added user
            For Each li As ListItem In RoleList.Items
                If li.Selected Then
                    Roles.AddUserToRole(RegisterUserWithRoles.UserName, li.Text)
                End If
            Next
        End If
    End Sub
End Class
