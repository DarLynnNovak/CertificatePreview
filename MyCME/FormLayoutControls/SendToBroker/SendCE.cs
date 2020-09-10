﻿using Aptify.Framework.Application;
using Aptify.Framework.DataServices;
using Aptify.Framework.ExceptionManagement;
using Aptify.Framework.WindowsControls;
using Aptify.Framework.BusinessLogic.GenericEntity;
using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml;
using System.Web;

namespace ACSMyCMEFormDLLs.FormLayoutControls.SendToBroker
{ 
   public class SendCE : FormTemplateLayout
    {

        private DataAction m_oda = new DataAction();
        private AptifyProperties m_oProps = new AptifyProperties();
        private AptifyApplication m_oApp = new AptifyApplication();
        private HttpPostedFileBase file;

        private AptifyGenericEntityBase AttachmentsGE;
        private AptifyLinkBox _senderIdLinkBox;
        private AptifyActiveButton _sendToBrokerBtn;
        private AptifyTextBox _eventStartDate;
        private AptifyTextBox _eventEndDate;
        XDocument xDoc = new XDocument();
        string saveLocation = "C:/Code CSharp/ACSMyCMEFormDLLs/MyCME/FormLayoutControls/XML/XMLCEData.xml";
        string attachmentCatIdSql;
        int attachmentCatId;
        string entityIdSql;
        int entityId;
        string senderIdSql;
        int senderId;
        long userId;
        long recordId;
        string filename = null;
        string result = "Failed";
        public void Config()
        {
            try
            {
                m_oApp = ApplicationObject;
                this.m_oda = new Aptify.Framework.DataServices.DataAction(this.m_oApp.UserCredentials);
                userId = m_oda.UserCredentials.AptifyUserID;
            }
            catch (Exception ex)
            {
                Aptify.Framework.ExceptionManagement.ExceptionManager.Publish(ex);
            }
        }

        protected override void OnFormTemplateLoaded(FormTemplateLoadedEventArgs e)
        {
            base.OnFormTemplateLoaded(e);
            try
            {
                if (e.Operation == FormTemplateLoadedOperation.LoadTemplate)
                {
                    Config();
                    BindControls();
                    getSenderId();
                   
                }
                
                //   _sendToBrokerBtn.Click += _sendToBrokerBtn_Click;
            }
        
            catch (Exception ex)
            {
                ExceptionManager.PublishAndDisplayException(this, ex);
            }
        }//End OnFormTemplateLoaded\


        protected virtual void BindControls()
        {
            try
            {
                if (_sendToBrokerBtn == null || _sendToBrokerBtn.IsDisposed)
                {
                    _sendToBrokerBtn = GetFormComponentByLayoutKey(this, "ACS.ACSCMESendToBroker.Form.Active Button.1") as AptifyActiveButton;
                }
                if (_senderIdLinkBox == null || _senderIdLinkBox.IsDisposed)
                {
                    _senderIdLinkBox = GetFormComponentByLayoutKey(this, "ACS.ACSCMESendToBroker.Form.SenderId") as AptifyLinkBox;
                }
                if (_eventStartDate == null || _eventStartDate.IsDisposed)
                {
                    _eventStartDate = GetFormComponentByLayoutKey(this, "ACSCMESendToBroker.EventStartDate") as AptifyTextBox;
                }
                if (_eventEndDate == null || _eventEndDate.IsDisposed)
                {
                    _eventEndDate = GetFormComponentByLayoutKey(this, "ACSCMESendToBroker.EventEndDate") as AptifyTextBox;
                }
                _sendToBrokerBtn.Click += _sendToBrokerBtn_Click;
                recordId = FormTemplateContext.GE.RecordID;

            }
            catch (Exception ex)
            {
                ExceptionManager.Publish(ex);
            }
        }//End Bind Controls
        private void getSenderId()
        {
            if (userId != 11)
            { 
            senderIdSql = "select e.linkedpersonid from vwUserEntityRelations uer join vwemployees e on e.id = uer.EntityRecordID join vwusers u on u.id = uer.userid where u.id = " + userId;
            senderId = Convert.ToInt32(m_oda.ExecuteScalar(senderIdSql));

            }
        }
        void _sendToBrokerBtn_Click(object sender, System.EventArgs e)
        {
            CreateXml();
            MessageBox.Show("Hello Julie ");

           // CreateAttachment();
        }//End BtnClick

        private void CreateXml()
        {
            xDoc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("Courses",
                    new XAttribute("id_parent_provider", "2"), 
                    new XAttribute("upload_key", "1234"),
                    new XElement("Course")
                    )
                );
            MessageBox.Show(Convert.ToString(xDoc));
            xDoc.Save(saveLocation);
            this.FormTemplateContext.GE.SetValue("XmlData", Convert.ToString(xDoc));
            this.FormTemplateContext.GE.Save();
        }
       
        private void CreateAttachment()
        {
            entityIdSql = "select ID from Entities where name like 'ACSCMESendToBroker'";
            entityId = Convert.ToInt32(m_oda.ExecuteScalar(entityIdSql));
            attachmentCatIdSql = "select ID from vwAttachmentCategories where name like 'MyCMEXML'";
            attachmentCatId = Convert.ToInt32(m_oda.ExecuteScalar(attachmentCatIdSql));
            //need to get the file that gets created and read it back into
        
            filename = Path.GetFileName(saveLocation);

            byte[] array = File.ReadAllBytes(saveLocation);
            var lTemp = new byte[array.Length];
            
            
            //file.InputStream.Read(lTemp, 0, array.Length);

            MessageBox.Show("Filename = " + filename);
            MessageBox.Show("fileBytes = " + array.Length);

            AttachmentsGE = m_oApp.GetEntityObject("Attachments", -1);
            AttachmentsGE.SetValue("Name", "XMLCEData");
            AttachmentsGE.SetValue("Description", "XMLCEData");
            AttachmentsGE.SetValue("EntityID", entityId);
            AttachmentsGE.SetValue("RecordID", recordId);
            AttachmentsGE.SetValue("CategoryID", attachmentCatId);
            AttachmentsGE.SetValue("LocalFileName", saveLocation);
            AttachmentsGE.SetValue("BlobData", lTemp);
             

            if (!AttachmentsGE.Save(false))
            {
                result = "Error";
                throw new Exception("Problem Saving attachments Record:" + AttachmentsGE.RecordID);

            }
            else
            {
                AttachmentsGE.Save(true);
                result = "Success";

            }



        }

    }//End Class

}//End Namespace