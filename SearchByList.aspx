<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SearchByList.aspx.cs" Inherits="SearchEngineExcel.SearchByList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <section class=" col-12 center-container custom-margin center-content">
        <%--<div class="custom-border custom-division">--%>

        <div class="col ">
            <div class="card d-flex flex-column">
                <h3>Upload Source Files</h3>
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
                <label>Column Name</label>
                <span><asp:Label ID="sourceColumnErrorLabel" runat="server" ForeColor="Red" Visible="false"></asp:Label></span>
                <div class="form-group">
                    <asp:TextBox runat="server" ID="columnSearchTextBox" CssClass="form-control" placeholder="Enter the column name to fetch data from the uploaded source file.."></asp:TextBox>
                </div>
            </div>
        </div>

        <%--<div class="custom-border custom-division">--%>
        <div class="col">
            <div class="card d-flex flex-column">
                <h3>Upload Search List</h3>
                <div class="drop_box">
                    <header>
                        <h4>Select File here</h4>
                    </header>
                    <p>Files Supported: xlsx</p>
                    <div>
                        <asp:FileUpload CssClass="form-control-file" ID="searchFileUpload" runat="server" accept=".xlsx" AllowMultiple="true" ClientIDMode="Static" />

                    </div>
                    <asp:Label ID="searchErrorLabel" runat="server" ForeColor="Red" Visible="false"></asp:Label>

                </div>
                <label>Column Name</label>
                <span><asp:Label ID="searchColumnErrorLabel" runat="server" ForeColor="Red" Visible="false"></asp:Label></span>
                <div class="form-group">
                    <asp:TextBox runat="server" ID="columnSearchInSearchFileTextBox" CssClass="form-control" placeholder="Enter the column name in which data to be searched.."></asp:TextBox>
                </div>
                <div class="d-flex justify-content-center">
                    <asp:Button class="btn" ID="searchButton" runat="server" Text="Search and Process" OnClick="SearchAndProcess_Click" />
                </div>
        </div>

        </div>

        
        
           
    </section>


</asp:Content>
