<%@ Page Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="false" CodeFile="EnhancedCreateUserWizard.aspx.vb" Inherits="Membership_EnhancedCreateUserWizard" title="Untitled Page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" Runat="Server">
    <h2>Create a New User Account (and a Record in <code>UserProfiles</code>)</h2>
    <p>
        This CreateUserWizard control not only adds the new user account to the
        Membership framework, but also adds a corresponding record to the
        <code>UserProfiles</code> table.
    </p>
    <asp:CreateUserWizard ID="NewUserWizard" runat="server" 
        ContinueDestinationPageUrl="~/Membership/AdditionalUserInfo.aspx">
        <WizardSteps>
            <asp:CreateUserWizardStep ID="CreateUserWizardStep1" runat="server">
            </asp:CreateUserWizardStep>
            <asp:WizardStep ID="UserSettings" runat="server" StepType="Step" Title="Your Settings">
                <p>
                    <b>Home Town:</b><br />
                    <asp:TextBox ID="HomeTown" runat="server"></asp:TextBox>
                </p>
                <p>
                    <b>Homepage URL:</b><br />
                    <asp:TextBox ID="HomepageUrl" Columns="40" runat="server"></asp:TextBox>
                </p>
                <p>
                    <b>Signature:</b><br />
                    <asp:TextBox ID="Signature" TextMode="MultiLine" Width="95%" Rows="5" runat="server"></asp:TextBox>
                </p>
            </asp:WizardStep>
            <asp:CompleteWizardStep ID="CompleteWizardStep1" runat="server">
            </asp:CompleteWizardStep>
        </WizardSteps>
    </asp:CreateUserWizard>
</asp:Content>

