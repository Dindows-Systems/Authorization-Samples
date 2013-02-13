Imports System.IO
Imports System.Security.Permissions

Partial Class Membership_UserBasedAuthorization
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            ' Is this Tito visiting the page?
            Dim userName As String = User.Identity.Name
            If String.Compare(userName, "Tito", True) = 0 Then
                ' This is Tito, SHOW the Delete column
                FilesGrid.Columns(1).Visible = True
            Else
                ' This is NOT Tito, HIDE the Delete column
                FilesGrid.Columns(1).Visible = False
            End If

            Dim appPath As String = Request.PhysicalApplicationPath
            Dim dirInfo As New DirectoryInfo(appPath)

            Dim files() As FileInfo = dirInfo.GetFiles()

            FilesGrid.DataSource = files
            FilesGrid.DataBind()
        End If
    End Sub

    <PrincipalPermission(SecurityAction.Demand, Authenticated:=True)> _
    Protected Sub FilesGrid_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles FilesGrid.SelectedIndexChanged
        ' Open the file and display it
        Dim fullFileName As String = FilesGrid.SelectedValue.ToString()
        Dim contents As String = File.ReadAllText(fullFileName)

        Dim FileContentsTextBox As TextBox = CType(LoginViewForFileContentsTextBox.FindControl("FileContents"), TextBox)
        FileContentsTextBox.Text = contents
    End Sub

    <PrincipalPermission(SecurityAction.Demand, Name:="Tito")> _
    Protected Sub FilesGrid_RowDeleting(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewDeleteEventArgs) Handles FilesGrid.RowDeleting
        Dim fullFileName As String = FilesGrid.DataKeys(e.RowIndex).Value.ToString()

        Dim FileContentsTextBox As TextBox = CType(LoginViewForFileContentsTextBox.FindControl("FileContents"), TextBox)
        FileContentsTextBox.Text = String.Format("You have opted to delete {0}.", fullFileName)

        ' To actually delete the file, uncomment the following line
        ' File.Delete(fullFileName)
    End Sub
End Class
