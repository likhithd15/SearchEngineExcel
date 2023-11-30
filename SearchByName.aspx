<%@ Page Language="C#" MasterPageFile="~/Site.Master" EnableViewState="true" AutoEventWireup="true" CodeBehind="SearchByName.aspx.cs" Inherits="SearchEngineExcel.SearchByName" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
   
    <section class=" col-12 center-container custom-margin center-content">
        <%--<div class="custom-border custom-division">--%>

            <div class="col ">
                <div class="card">
                    <h3>Upload Files</h3>
                    <div class="drop_box">
                        <header>
                            <h4>Select File here</h4>
                        </header>
                        <p>Files Supported: xlsx</p>
                        <div>
                        <asp:FileUpload CssClass="form-control-file" ID="fileUpload" runat="server" accept=".xlsx" AllowMultiple="true" ClientIDMode="Static" />
                    </div>
                        <asp:Label ID="sourceErrorLabel" runat="server" ForeColor="Red" Visible="false"></asp:Label>


                    </div>
                </div>
            </div>
            
            <%--<div class="custom-border custom-division">--%>
                <div class="col">
                    <div class="card">

                        <label>Column Name</label>
                        <span><asp:Label ID="columnErrorLabel" runat="server" ForeColor="Red" Visible="false"></asp:Label></span>
                        <div class="form-group">
                            <asp:TextBox runat="server" ID="columnSearchTextBox" CssClass="form-control" placeholder="Enter the column name to fetch data from the uploaded source file.."></asp:TextBox>
                        </div>
                        
                        <label>Search Box</label>
                        <span><asp:Label ID="searchErrorLabel" runat="server" ForeColor="Red" Visible="false"></asp:Label></span>
                        <div class="form-group">
                            <asp:TextBox runat="server" ID="searchQuery" CssClass="form-control" placeholder="Search here.."></asp:TextBox>
                        </div>
                        <center>
                        <asp:Button class="btn" ID="searchButton" runat="server" Text="Search and Process" onClick="SearchAndProcess_Click"/>
                            </center>
                    </div>
               <%-- </div>
            </div>--%>
        </div>
    </section>

</asp:Content>

