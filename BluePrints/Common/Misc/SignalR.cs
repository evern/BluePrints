using BaseModel.Misc;
using BluePrints.Data;
using BluePrints.ViewModels;
using BluePrints.Views;
using DevExpress.Mvvm;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Windows;

namespace BluePrints.Common
{
    public static class SignalR
    {
        public static IHubProxy HubProxy { get; set; }
        public static HubConnection Connection { get; set; }

        public static async void ConnectAsync(string userName)
        {
            Connection = new HubConnection(System.Configuration.ConfigurationManager.ConnectionStrings["SignalR"].ConnectionString, new Dictionary<string, string> { { "UserName", userName } });
            
            HubProxy = Connection.CreateHubProxy("BluePrintsHub");
            HubProxy.On<string, string, string, string, string>("AddMessage", (entityName, key, messageType, sender, hwid) => Application.Current.Dispatcher.Invoke(() => HubReceiveMessage(entityName, key, messageType, sender, hwid)));
            HubProxy.On<string, string>("AuthenticatedMessage", (key, authenticationResult) => Application.Current.Dispatcher.Invoke(() => AuthenticateMessage(key, authenticationResult)));

            try
            {
                await Connection.Start();
            }
            catch (HttpRequestException)
            {
                //MessageBox.Show(BluePrintsResources.SignalR_UnableToConnect);
                //System.Environment.Exit(1);
            }
        }

        public static void Disconnect()
        {
            if (Connection != null)
            {
                Connection.Stop();
                Connection.Dispose();
            }
        }

        public static void HubSendMessage(string entityName, string key, string messageType, string sender, string hwid)
        {
            if (Connection.State == ConnectionState.Connected)
                HubProxy.Invoke("Send", entityName, key, messageType, sender, LoginCredentials.CurrentUser.NAME, hwid);
            else if (Connection.State == ConnectionState.Disconnected)
                ConnectAsync(LoginCredentials.CurrentUser.NAME);
        }

        public static void HubLogMessage(string message)
        {
            if (Connection.State == ConnectionState.Connected)
                HubProxy.Invoke("Log", message);
            else if (Connection.State == ConnectionState.Disconnected)
                ConnectAsync(LoginCredentials.CurrentUser.NAME);
        }

        public static void HubAuthenticate(string username, string password, string key)
        {
            HubProxy.Invoke("Authenticate", username, password, key);
        }

        public static void HubReceiveMessage(string entityName, string key, string messageType, string sender, string hwid)
        {
            //empty string for message type is reserved for shutdown message
            if(messageType == string.Empty)
            {
                Window active_window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.ToString().Contains("LoginWindow"));
                if (active_window == null)
                    return;

               ((LoginViewModel)((LoginWindow)active_window).DataContext).SignalRShutdown(entityName);
            }

            //ignore messages returned from hub because it was transmitted locally
            if (hwid == LoginCredentials.CurrentHWID)
                return;

            if (key.Length < Guid.Empty.ToString().Length)
                return;

            var MessageType = (EntityMessageType) Enum.Parse(typeof(EntityMessageType), messageType);
            var PrimaryKey = new Guid(key);

            if (entityName == typeof(AREA).ToString())
                ReceiveMessage<AREA, Guid>(PrimaryKey, key, MessageType, sender, hwid);
            else if (entityName == typeof(BASELINE_ITEM).ToString())
                ReceiveMessage<BASELINE_ITEM, Guid>(PrimaryKey, key, MessageType, sender, hwid);
            else if (entityName == typeof(BASELINE).ToString())
                ReceiveMessage<BASELINE, Guid>(PrimaryKey, key, MessageType, sender, hwid);
            else if (entityName == typeof(DEPARTMENT).ToString())
                ReceiveMessage<DEPARTMENT, Guid>(PrimaryKey, key, MessageType, sender, hwid);
            else if (entityName == typeof(DISCIPLINE).ToString())
                ReceiveMessage<DISCIPLINE, Guid>(PrimaryKey, key, MessageType, sender, hwid);
            else if (entityName == typeof(DOCTYPE).ToString())
                ReceiveMessage<DOCTYPE, Guid>(PrimaryKey, key, MessageType, sender, hwid);
            else if (entityName == typeof(ESTIMATE).ToString())
                ReceiveMessage<ESTIMATE, Guid>(PrimaryKey, key, MessageType, sender, hwid);
            else if (entityName == typeof(PHASE).ToString())
                ReceiveMessage<PHASE, Guid>(PrimaryKey, key, MessageType, sender, hwid);
            else if (entityName == typeof(PROGRESS_ITEM).ToString())
                ReceiveMessage<PROGRESS_ITEM, Guid>(PrimaryKey, key, MessageType, sender, hwid);
            else if (entityName == typeof(PROGRESS).ToString())
                ReceiveMessage<PROGRESS, Guid>(PrimaryKey, key, MessageType, sender, hwid);
            else if (entityName == typeof(PROJECT_REPORT).ToString())
                ReceiveMessage<PROJECT_REPORT, Guid>(PrimaryKey, key, MessageType, sender, hwid);
            else if (entityName == typeof(PROJECT).ToString())
                ReceiveMessage<PROJECT, Guid>(PrimaryKey, key, MessageType, sender, hwid);
            else if (entityName == typeof(RATE).ToString())
                ReceiveMessage<RATE, Guid>(PrimaryKey, key, MessageType, sender, hwid);
            else if (entityName == typeof(REGISTER).ToString())
                ReceiveMessage<REGISTER, Guid>(PrimaryKey, key, MessageType, sender, hwid);
            else if (entityName == typeof(ROLE_PERMISSION).ToString())
                ReceiveMessage<ROLE_PERMISSION, Guid>(PrimaryKey, key, MessageType, sender, hwid);
            else if (entityName == typeof(ROLE).ToString())
                ReceiveMessage<ROLE, Guid>(PrimaryKey, key, MessageType, sender, hwid);
            else if (entityName == typeof(SETTINGS_GLOBAL).ToString())
                ReceiveMessage<SETTINGS_GLOBAL, Guid>(PrimaryKey, key, MessageType, sender, hwid);
            else if (entityName == typeof(UOM).ToString())
                ReceiveMessage<UOM, Guid>(PrimaryKey, key, MessageType, sender, hwid);
            else if (entityName == typeof(USER).ToString())
                ReceiveMessage<USER, Guid>(PrimaryKey, key, MessageType, sender, hwid);
            else if (entityName == typeof(VARIATION_ITEM).ToString())
                ReceiveMessage<VARIATION_ITEM, Guid>(PrimaryKey, key, MessageType, sender, hwid);
            else if (entityName == typeof(VARIATION).ToString())
                ReceiveMessage<VARIATION, Guid>(PrimaryKey, key, MessageType, sender, hwid);
            else if (entityName == typeof(SUBJOB_ASSIGNMENT).ToString())
                ReceiveMessage<SUBJOB_ASSIGNMENT, Guid>(PrimaryKey, key, MessageType, sender, hwid);
            else if (entityName == typeof(SUBJOB).ToString())
                ReceiveMessage<SUBJOB, Guid>(PrimaryKey, key, MessageType, sender, hwid);
            else if (entityName == typeof(DELIVERABLES_STATUS).ToString())
                ReceiveMessage<DELIVERABLES_STATUS, Guid>(PrimaryKey, key, MessageType, sender, hwid);
        }

        private static void ReceiveMessage<TEntity, TPrimaryKey>(TPrimaryKey primaryKey, string key, EntityMessageType messageType,
            string sender, string hwid)
            where TEntity : class
        {
            Guid keyGuid = new Guid(key);
            Messenger.Default.Send(new EntityMessage<TEntity, TPrimaryKey>(primaryKey, keyGuid, messageType, sender, hwid));
        }

        private static void AuthenticateMessage(string key, string authenticationResult)
        {
            Messenger.Default.Send(new AuthenticationResult() { Key = key, Result = authenticationResult });
        }
    }

    public class AuthenticationResult
    {
        public string Key { get; set; }
        public string Result { get; set; }
    }
}