<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage_Simple.Master" AutoEventWireup="true" CodeBehind="Nomina_ConstruccionTextFiles.aspx.cs" Inherits="NominaASP.Nomina.Nomina.Nomina_ConstruccionTextFiles" ValidateRequest="false" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <script type="text/javascript">
        function PopupWin(url, w, h) {
            ///Parameters url=page to open, w=width, h=height
            myWindow = window.open(url, "external2", "width=" + w + ",height=" + h + ",resizable=yes,scrollbars=yes,status=no,location=no,toolbar=no,menubar=no,top=10px,left=8px");
            myWindow.focus();
        }
    </script>

     <ajaxToolkit:tabcontainer id="TabContainer1" runat="server" activetabindex="0" >

        <ajaxToolkit:TabPanel HeaderText="Recibos de pago" runat="server" ID="TabPanel1" >
            <ContentTemplate>

                <div class="notsosmallfont" 
                     style="width: 15%; border: 1px solid #C0C0C0; vertical-align: top; background-color: #F7F7F7; float: left; text-align: center; padding: 10px; ">
        
                    <br />

                    <div>
                        <asp:ImageButton ID="DownLoadFile_ImageButton" 
                                     runat="server" 
                                     ImageUrl="~/Pictures/download2_25x25.png" 
                                     OnClick="DownLoadFile_ImageButton_Click" />
                    </div>

                    <div>
                        <asp:LinkButton ID="DownLoadFile2_LinkButton" 
                                    runat="server" 
                                    OnClick="DownLoadFile2_LinkButton_Click1">Download
                    </asp:LinkButton>
                    </div>

                    <hr />

                    <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Always">
                        <ContentTemplate>

                            <div>
                                <asp:ImageButton ID="ObtenerEmails_ImageButton" 
                                                    runat="server" 
                                                    ImageUrl="~/Pictures/email_25x25.png" 
                                                    OnClick="ObtenerEmails_ImageButton_Click" />
                            </div>
                            <div>
                                <asp:LinkButton ID="ObtenerEmails_LinkButton" 
                                                runat="server" 
                                                OnClick="ObtenerEmails_LinkButton_Click">e-mails
                                </asp:LinkButton>
                            </div>

                        </ContentTemplate>
                    </asp:UpdatePanel>
           
                </div>

                <div style="float: right; width: 75%;">

                    <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Always">
                        <ContentTemplate>
    
                            <asp:ValidationSummary ID="ValidationSummary1" 
                                                    runat="server" 
                                                    class="errmessage_background generalfont errmessage"
                                                    ShowModelStateErrors="true"
                                                    ForeColor="" />

                            <asp:CustomValidator ID="CustomValidator1" 
                                                    runat="server" 
                                                    ErrorMessage="CustomValidator" 
                                                    Display="None" 
                                                    EnableClientScript="False" />

        
                            <fieldset style="padding: 0px 25px 0px 25px; text-align: center; " class="generalfont">
                                <legend>Construcción del archivo: </legend>
                                <br />
                                <asp:DropDownList ID="OpcionesMailMerge_DropDownList" 
                                                    runat="server" 
                                                    OnSelectedIndexChanged="OpcionesMailMerge_DropDownList_SelectedIndexChanged" AutoPostBack="True">
                                    <asp:ListItem Value="0">Seleccione una opción ...</asp:ListItem>
                                    <asp:ListItem Value="1">Recibos de pago</asp:ListItem>
                                </asp:DropDownList>       

                                <br /><br /><br />

                                <asp:Button ID="GenerarMailMergeFile_Button" 
                                            runat="server" 
                                            Text="Construir archivo" 
                                            onclick="GenerarMailMergeFile_Button_Click" />

                                <br /><br />

                                <div id="OpcionesFormato_RecibosPago_Div" runat="server" style="text-align: center; ">

                                    <fieldset style="margin-top: 10px; margin-bottom: 10px; ">
                                        <legend style="white-space: nowrap; ">Opciones generación Email: </legend>
                                            <div style="margin: 10px; text-align: left; ">
                                                <p style="white-space: nowrap">Enviar correo generado a: </p>
                                                <asp:CheckBox ID="Email_EnviarEmpleado_CheckBox" runat="server" Text="Empleados" />
                                                <asp:CheckBox ID="Email_EnviarUsuario_CheckBox" runat="server" Text="Usuario" />
                                            </div>
                                    </fieldset>

                                </div>
                            </fieldset>

                            <br />

                            <%--para mostrar un diálogo que permita al usuario continuar/cancelar--%> 
                            <asp:Panel ID="pnlPopup" runat="server" CssClass="modalpopup" style="display:none">
                                <div class="popup_container" style="width: 500px; ">
                                    <div class="popup_form_header" style="overflow: hidden; ">
                                        <div id="ModalPopupTitle_div" style="width: 85%; margin-top: 5px;  float: left; ">
                                            <span runat="server" id="ModalPopupTitle_span" style="font-weight: bold; display:block; text-align: left;"/> 
                                        </div>
                                        <div style="width: 15%; float: right; ">
                                            <asp:ImageButton ID="ImageButton1" runat="server" OnClientClick="$find('popup').hide(); return false;" ImageUrl="~/Pictures/PopupCloseButton.png" />
                                        </div>
                                    </div>
                                    <div class="inner_container">
                                        <div class="popup_form_content">
                                            <span runat="server" id="ModalPopupBody_span" style="display:block; text-align: left; "/> 
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
                </div>

            </ContentTemplate>
        </ajaxToolkit:TabPanel>

        <ajaxToolkit:TabPanel HeaderText="Notas" runat="server" ID="TabPanel2" >
            <ContentTemplate>
                <asp:Accordion ID="MyAccordion"
                                runat="Server"
                                SelectedIndex="0"
                                cssClass="accordion"
                                HeaderCssClass="accordionHeader"
                                HeaderSelectedCssClass="accordionHeaderSelected"
                                ContentCssClass="accordionContent"
                                AutoSize="None"
                                FadeTransitions="true"
                                TransitionDuration="250"
                                FramesPerSecond="40"
                                RequireOpenedPane="false"
                                SuppressHeaderPostbacks="true">
                    <Panes>
                        <asp:AccordionPane ID="AccordionPane1" runat="server">
                            <Header>Introducción</Header>
                            <Content>
                                <p>
                                    Este proceso construye los recibos de pago para los empleados que corresponden a la nómina que Ud. seleccione en la lista. 
                                    Los recibos de pago se construyen en base a una plantilla que debe existir en el servidor. 
                                    El nombre de la plantilla es: <em>RecibosPago.docx</em> y debe existir en el directorio 
                                    <em>'/Temp/RecibosPago/<ciaContab>'</em> en el servidor y para este programa. 
                                </p>
                                <p>
                                    Ud. puede obtener una copia del documento mencionado y modificarla según le parezca adecuado; 
                                    luego reemplazar la original para que se mantengan sus cambios. 
                                </p>
                                <p>
                                    Note que existe una plantilla (RecibosPago.docx) para cada Cia Contab. Esto permite mantener un formato diferente para 
                                    cada compañía. 
                                </p>
                            </Content>
                        </asp:AccordionPane>
                        <asp:AccordionPane ID="AccordionPane2" runat="server">
                            <Header>Obtención de correos electronicos</Header>
                            <Content>
                                <p>
                                    El proceso genera una copia de cada recibo de empleados en formato pdf. Si Ud. decide enviar 
                                    los recibos usando correos electrónicos, 
                                    este programa genera un correo para cada empleado y adjunta su recibo en formato pdf. 
                                </p>
                                <p>
                                    <b>Notas: </b><br />
                                    <ul>
                                        <li>
                                            Si Ud. desea enviar correos electrónicos a los empleados mediante este proceso, <b>siempre</b>
                                            obtenga los recibos primero; solo luego construya y envie los e-mails. 
                                        </li>
                                        <li>
                                            Ud. debe indicar si desea enviar los correos a cada uno de los empleados; además, 
                                            Ud. puede indicar que desea que se le envie una 
                                            copia de cada correo. Ud. debe marcar al menos una de las opciones, para que este 
                                            proceso se ejecute en forma normal. 
                                        </li>
                                        <li>
                                            La configuración necesaria para el envio de correos electrónicos debe ser registrada en la tabla
                                            <em>Compañías</em> para la <em>Cia Contab</em> a la cual corresponde la nómina de pago. 
                                        </li>
                                        <li>
                                            El usuario que ejecuta este proceso, debe tener su dirección de correo electrónico registrada
                                            en la tabla <em>Usuarios</em>. Esto permite que el usuario pueda recibir una copia, si lo desea, 
                                            de cada correo enviado. 
                                        </li>
                                        <li>
                                            Cada empleado debe tener su dirección de correo electrónico registrado en la tabla <em>Empleados</em>. 
                                            Si un empleado no tiene una dirección de correo registrada en esta tabla, su correo electrónico
                                            no será enviado; es decir, el envío de su correo será obviado por este proceso. 
                                        </li>
                                    </ul>
                                </p>
                            </Content>
                        </asp:AccordionPane>
                    </Panes>
                </asp:Accordion>
            </ContentTemplate>
        </ajaxToolkit:TabPanel>

    </ajaxToolkit:tabcontainer>
   
</asp:Content>
