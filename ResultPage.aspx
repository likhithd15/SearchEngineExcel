<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="ResultPage.aspx.cs" Inherits="SearchEngineExcel.ResultPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <%--<section class="container mx-auto">--%>
    <div class="container">
        <div class="row">
            <div class="col-sm-12">
                <center>
                    <h3>Result</h3>
                </center>
                <div class="text-right">
                <button type="button" class="btn btn-primary" id="existingSheet">Save in existing sheet</button>
                <button type="button" class="btn btn-primary" id="newSheet">Save in new sheet</button>
            </div>

                <div class="row">
                    <div class="col-sm-12 col-md-12">
                        <asp:Panel class="alert alert-success" role="alert" ID="Panel1" runat="server" Visible="False">
                            <asp:Label ID="Label1" runat="server" Text="Label"></asp:Label>
                        </asp:Panel>
                    </div>
                </div>
                <br />
                <div class="row">
                    <div class="card w-100">
                        <div class="card-body">

                            <div class="row">

                                <div class="col">
                                    <div class="table-responsive">
                                        <!-- Wrap the GridView in a div with the 'table-responsive' class -->
                                        <asp:GridView class="table table-striped table-bordered" ID="GridView1" runat="server">
                                        </asp:GridView>
                                        <center>
                                            <asp:Label ID="errorLabel1" runat="server" Visible="false"></asp:Label>
                                        </center>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="modal" id="existingModal">
    <div class="modal-dialog">
        <div class="modal-content">

            <!-- Modal Header -->
            <div class="modal-header">
                <button type="button" class="close" data-dismiss="modal">&times;</button>
            </div>

            <!-- Modal Body -->
            <div class="modal-body">
                 <div class="row">
                     <div class="col">
                        <label>Destination File Path</label>
                        <div class="form-group">
                           <asp:TextBox ID="destinationFile" CssClass="form-control" runat="server" placeholder="Enter the File Path"></asp:TextBox>
            <asp:Label ID="destinationErrorLabel" runat="server" ForeColor="Red" Visible="false"></asp:Label>
                        </div>
                        <label>Sheet Name</label>
                        <div class="form-group">
                           <asp:TextBox ID="sheetNameTextBox" CssClass="form-control col-12" runat="server" placeholder="Enter the existing Sheet name"></asp:TextBox>
            <asp:Label ID="errorLabel" runat="server" CssClass="error"></asp:Label>
                        </div>
                        <div class="form-group">
                           <asp:Button class="btn btn-success btn-block btn-lg" runat="server" Text="Save" OnClick="ExistingSheet_Click" />
                        </div>
                     </div>
                  </div>

                
            </div>

            

        </div>
    </div>
</div>

    <div class="modal" id="newModal">
    <div class="modal-dialog">
        <div class="modal-content">

            <!-- Modal Header -->
            <div class="modal-header">
                <button type="button" class="close" data-dismiss="modal">&times;</button>
            </div>

            <!-- Modal Body -->
            <div class="modal-body">
                 <div class="row">
                     <div class="col">
                        <label>Destination File Path</label>
                        <div class="form-group">
                           <asp:TextBox ID="destination" CssClass="form-control" runat="server" placeholder="Enter the File Path"></asp:TextBox>
            <asp:Label ID="destinationError" runat="server" ForeColor="Red" Visible="false"></asp:Label>
                        </div>
                        
                        <div class="form-group">
                           <asp:Button class="btn btn-success btn-block btn-lg" runat="server" Text="Save" OnClick="NewSheet_Click" />
                        </div>
                     </div>
                  </div>

                
            </div>

            

        </div>
    </div>
</div>


</asp:Content>
