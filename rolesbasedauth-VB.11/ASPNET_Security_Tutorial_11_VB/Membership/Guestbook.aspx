<%@ Page Language="VB" MasterPageFile="~/Site.master" AutoEventWireup="false" CodeFile="Guestbook.aspx.vb" Inherits="Membership_Guestbook" title="Untitled Page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" Runat="Server">
    <h2>The Guestbook</h2>
    <h3>Leave a Comment</h3>
    <p>
        <b>Subject:</b> 
        <asp:RequiredFieldValidator ID="SubjectReqValidator" runat="server" ErrorMessage="You must provide a value for Subject" ControlToValidate="Subject" ValidationGroup="EnterComment"></asp:RequiredFieldValidator><br />
        <asp:TextBox ID="Subject" Columns="40" runat="server"></asp:TextBox>
    </p>
    <p>
        <b>Body:</b>
        <asp:RequiredFieldValidator ID="BodyReqValidator" runat="server" ControlToValidate="Body"
            ErrorMessage="You must provide a value for Body" ValidationGroup="EnterComment"></asp:RequiredFieldValidator><br />
        <asp:TextBox ID="Body" TextMode="MultiLine" Width="95%" Rows="8" runat="server"></asp:TextBox>
    </p>
    <p>
        <asp:Button ID="PostCommentButton" runat="server" Text="Post Your Comment" 
            ValidationGroup="EnterComment" />
    </p>
    <hr />
    
    <h3>Guestbook Comments</h3>
    <asp:ListView ID="CommentList" runat="server" DataSourceID="CommentsDataSource" EnableViewState="false">        
        <LayoutTemplate>
            <span ID="itemPlaceholder" runat="server" />
            
            <p>
                <asp:DataPager ID="DataPager1" runat="server">
                    <Fields>
                        <asp:NextPreviousPagerField ButtonType="Button" ShowFirstPageButton="True" 
                            ShowLastPageButton="True" />
                    </Fields>
                </asp:DataPager>
            </p>
        </LayoutTemplate>
        
        <ItemTemplate>
            <h4><asp:Label ID="SubjectLabel" runat="server" Text='<%# Eval("Subject") %>' /></h4>            
            <asp:Label ID="BodyLabel" runat="server" Text='<%# Eval("Body").ToString().Replace(Environment.NewLine, "<br />") %>' />
            
            <p>
                ---<br />
                <asp:Label ID="SignatureLabel" Font-Italic="true" runat="server" Text='<%# Eval("Signature") %>' />
                <br />
                <br />
                My Home Town:
                <asp:Label ID="HomeTownLabel" runat="server" Text='<%# Eval("HomeTown") %>' />
                <br />
                My Homepage:
                <asp:HyperLink ID="HomepageUrlLink" runat="server" NavigateUrl='<%# Eval("HomepageUrl") %>' Text='<%# Eval("HomepageUrl") %>' />
            </p>
            <p align="center">
                Posted by 
                    <asp:Label ID="UserNameLabel" runat="server" Text='<%# Eval("UserName") %>' /> on
                    <asp:Label ID="CommentDateLabel" runat="server" Text='<%# Eval("CommentDate") %>' />
            </p>            
        </ItemTemplate>
        
        <ItemSeparatorTemplate>
            <hr />
        </ItemSeparatorTemplate>
    </asp:ListView>
    <p>
        <asp:SqlDataSource ID="CommentsDataSource" runat="server" 
            ConnectionString="<%$ ConnectionStrings:SecurityTutorialsConnectionString %>" 
            
            SelectCommand="SELECT GuestbookComments.Subject, GuestbookComments.Body, GuestbookComments.CommentDate, UserProfiles.HomeTown, UserProfiles.HomepageUrl, UserProfiles.Signature, aspnet_Users.UserName FROM GuestbookComments INNER JOIN UserProfiles ON GuestbookComments.UserId = UserProfiles.UserId INNER JOIN aspnet_Users ON GuestbookComments.UserId = aspnet_Users.UserId ORDER BY GuestbookComments.CommentDate DESC">
        </asp:SqlDataSource>
    </p>
</asp:Content>

