<%@ Page Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="false" CodeFile="ManageRoles.aspx.vb" Inherits="Roles_ManageRoles" title="Untitled Page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" Runat="Server">
    <h2>Manage Roles</h2>
    <p>
        <b>Create a New Role: </b>
        <asp:TextBox ID="RoleName" runat="server"></asp:TextBox>
        <asp:RequiredFieldValidator ID="RoleNameReqField" runat="server" 
            ControlToValidate="RoleName" Display="Dynamic" 
            ErrorMessage="You must enter a role name."></asp:RequiredFieldValidator>
        
        <br />
        <asp:Button ID="CreateRoleButton" runat="server" Text="Create Role" />
    </p>
    <p>
        <asp:GridView ID="RoleList" runat="server" AutoGenerateColumns="False">
            <Columns>
                <asp:CommandField DeleteText="Delete Role" ShowDeleteButton="True" />
                <asp:TemplateField HeaderText="Role">
                    <ItemTemplate>
                        <asp:Label runat="server" ID="RoleNameLabel" Text='<%# Container.DataItem %>' />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </p>
</asp:Content>
