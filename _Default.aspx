<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="_Default.aspx.cs" Inherits="SearchEngineExcel._Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <section class=" col-12 center-container custom-margin center-content">
        <div class=" custom-border custom-division">
                <h3>Search By Name</h3>
                <br />
                    <p>
                        Search for items in the source file by entering their names in the search box.
                        Multiple items can be searched by providing comma-separated names.
                    </p>
            <br />
                <a href="SearchByName.aspx" class="btn btn-primary">click here >></a>
        </div>
        <hr>
        <div class=" custom-border divider custom-division">
                <h3>Search By List</h3>
            <br />
                <p>Utilize the search function in the source file by incorporating an Excel file containing a list of search items.
                    
                    Recommended for multiple search.
                </p>
            <br />
                <a href="SearchByList.aspx" class="btn btn-primary">click here >></a>
        </div>
    </section>



</asp:Content>




