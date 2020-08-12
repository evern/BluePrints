using BaseModel.DataModel;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Resources;
using BluePrints.Data;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using ActiveDirectory;
using System.Timers;

namespace BluePrints.Common
{
    public static class EmailServices
    {
        public static void ExchangeLoginAsync(string sUserName, string sPassword)
        {
#if DEBUG

#else
            try
            {
                string fullEmailAddress = sUserName + BluePrintsResources.DefaultAuthenticateDomain;
                //exService.Credentials = new WebCredentials(fullEmailAddress, sPassword);
                exService.UseDefaultCredentials = true;
                exService.AutodiscoverUrl(fullEmailAddress, RedirectionCallback);
            }
            catch(Exception ex)
            {
                string s = ex.ToString();
            }
#endif

        }

        public static ExchangeService exService = new ExchangeService() { KeepAlive = true, PreAuthenticate = true };
        public static Timer EmailTimer = new Timer(60000) { AutoReset = true }; 
        public static void SendEmail(string fromName, string body, string subject, bool lowPriority = false, string toRecipient = "doc.control@primero.com.au")
        {
            if (lowPriority && EmailTimer.Enabled)
                return;

            EmailMessage msg = new EmailMessage(exService);
            msg.Subject = subject;
            msg.Body = body;
            msg.ToRecipients.Add(new Microsoft.Exchange.WebServices.Data.EmailAddress(toRecipient, toRecipient));

            //to prevent spamming
            if(!EmailTimer.Enabled)
            {
                EmailTimer.Elapsed -= EmailTimer_Elapsed;
                EmailTimer.Elapsed += EmailTimer_Elapsed;
                EmailTimer.Start();
            }

            try
            {
                if (msg.ToRecipients.Count > 0)
                    msg.SendAndSaveCopy();
            }
            catch(Exception ex)
            {
                string s = ex.ToString();
            }
        }

        public static void SendEmailFromDocControl(string fromName, string body, string subject, string toRecipient)
        {
            string authenticationName = "document.control@primerogroup.com.au";
            string docControlName = "Doc.Control";
            string fullEmailAddress = docControlName + BluePrintsResources.DefaultAuthenticateDomain;
            ExchangeService docControlExService = new ExchangeService() { KeepAlive = true, PreAuthenticate = true };
            docControlExService.UseDefaultCredentials = false;
            docControlExService.Credentials = new WebCredentials(authenticationName, "dcPW2018++");
            docControlExService.AutodiscoverUrl(authenticationName, RedirectionCallback);

            EmailMessage msg = new EmailMessage(docControlExService);
            msg.Subject = subject;
            msg.Body = body;
            msg.ToRecipients.Add(new Microsoft.Exchange.WebServices.Data.EmailAddress(toRecipient, toRecipient));

            try
            {
                if (msg.ToRecipients.Count > 0)
                    msg.SendAndSaveCopy();
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
            }
        }

        static bool RedirectionCallback(string url)
        {
            // Return true if the URL is an HTTPS URL.
            return url.ToLower().StartsWith("https://");
        }

        private static void EmailTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            EmailTimer.Stop();
        }

        public static List<USER> GetUSERS()
        {
            List<AdUser> adUSERs = ActiveDirectory.ActiveDirectory.GetUSERS();
            List<USER> USERS = new List<USER>();
            foreach(AdUser adUSER in adUSERs)
            {
                USER newUSER = new USER();
                newUSER.FIRST_NAME = adUSER.FIRST_NAME;
                newUSER.LAST_NAME = adUSER.LAST_NAME;
                newUSER.NAME = adUSER.NAME;
                newUSER.DESCRIPTION = adUSER.DESCRIPTION;
                newUSER.DEPARTMENT = adUSER.DEPARTMENT;
                newUSER.TITLE = adUSER.TITLE;
                USERS.Add(newUSER);
            }

            return USERS;
        }
    }
}