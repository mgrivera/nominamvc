<%@ Page Title="" Language="C#" MasterPageFile="~/Site2.master" AutoEventWireup="true" CodeBehind="SeleccionarCiaContab.aspx.cs" Inherits="NominaASP.Generales.SeleccionarCiaContab.SeleccionarCiaContab" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <ajaxToolkit:UpdatePanelAnimationExtender ID="UpdatePanelAnimationExtender1" runat="server"
        TargetControlID="UpdatePanel1" Enabled="True">
        <Animations>
            <OnUpdating>
                <Parallel duration=".5">
                    <FadeOut minimumOpacity=".5" />
                </Parallel>
            </OnUpdating>
            <OnUpdated>
                <Parallel duration=".5">
                    <FadeIn minimumOpacity=".5" />
                </Parallel>
            </OnUpdated>
        </Animations>
    </ajaxToolkit:UpdatePanelAnimationExtender>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">

        <ContentTemplate>

            <h3 style="margin-top: 15px; ">Por favor, seleccione una compañía de la lista.</h3>

            <asp:ValidationSummary ID="ValidationSummary1" 
                                   runat="server" 
                                   class="errmessage_background generalfont errmessage"
                                   ShowModelStateErrors="true"
                                   ForeColor="" />

            <asp:CustomValidator ID="CustomValidator1" runat="server" ErrorMessage="CustomValidator" Display="None" EnableClientScript="False" />
            
            <asp:GridView ID="GridView1" 
                          runat="server" 

                          DataKeyNames="Numero" 

                          OnSelectedIndexChanged="GridView1_SelectedIndexChanged"
                          OnPageIndexChanging="GridView1_PageIndexChanging"
                                        
                          AutoGenerateColumns="False" 
                          AllowPaging="True" 
                          PageSize="20"  
                          CssClass="Grid">

                <Columns>  

                    <asp:buttonfield CommandName="Select" ImageUrl="../../Pictures/SelectRow.png" ButtonType="Image" Text="Select" />

                    <asp:BoundField DataField="Nombre" HeaderText="Compañía" ReadOnly="True"   
                        SortExpression="Nombre" >  
                        <HeaderStyle HorizontalAlign="Left" Font-Size="Medium" />
                        <ItemStyle HorizontalAlign="Left" Font-Size="Small" />
                    </asp:BoundField>
                   
                </Columns>  

                <EmptyDataTemplate>
                    <br />
                    No hay registros que mostrar; probablemente no existen compañías registradas en la tabla Compañías ...
                </EmptyDataTemplate>
                
                <%--agregamos Font-Size para cambiar el font (smaller) que se indica en el cssclass ...--%>  

                <SelectedRowStyle CssClass="GridSelectedItem" />  
                <AlternatingRowStyle CssClass="GridAltItem" />
				<RowStyle CssClass="GridItem"  />
				<HeaderStyle CssClass="GridHeader" />
				<PagerStyle CssClass="GridPager" />
                <EmptyDataRowStyle CssClass="GridEmptyData" />

            </asp:GridView>



            <%--para mostrar un diálogo que permita al usuario continuar/cancelar--%> 
            <asp:Panel ID="pnlPopup" runat="server" CssClass="modalpopup" style="display:none">
                <div class="popup_container" style="max-width: 500px; ">
                    <div class="popup_form_header" style="overflow: hidden; ">
                        <div id="ModalPopupTitle_div" style="width: 85%; margin-top: 5px;  float: left;  ">
                            <span runat="server" id="ModalPopupTitle_span" style="font-weight: bold; "/> 
                        </div>
                        <div style="width: 15%; float: right; ">
                            <asp:ImageButton ID="ImageButton1" runat="server" OnClientClick="$find('popup').hide(); return false;" ImageUrl="~/Pictures/PopupCloseButton.png" />
                        </div>
                    </div>
                    <div class="inner_container">
                        <div class="popup_form_content">
                            <span runat="server" id="ModalPopupBody_span" /> 
                        </div>
                        <div class="popup_form_footer">
                            <asp:Button ID="btnOk" runat="server" Text="Continuar" OnClick="btnOk_Click" />
                            <asp:Button ID="btnCancel" runat="server" Text="Cancelar" OnClientClick="$find('popup').hide(); return false;" Width="80px"/>
                        </div>   
                    </div>                                          
                </div>
            </asp:Panel>

            <asp:HiddenField ID="HiddenField1" runat="server" />
            <ajaxToolkit:ModalPopupExtender ID="ModalPopupExtender1" 
                                            runat="server" 
                                            BehaviorID="popup" 
                                            TargetControlID="HiddenField1" 
                                            PopupControlID="pnlPopup" 
                                            BackgroundCssClass="modalBackground" 
                                            PopupDragHandleControlID="ModalPopupTitle_div" 
                                            Drag="True" />
        
            </ContentTemplate>

    </asp:UpdatePanel>

</asp:Content>
