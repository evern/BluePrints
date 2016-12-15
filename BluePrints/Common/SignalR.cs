using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.ViewModel;
using BluePrints.Data;
using DevExpress.Mvvm;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BluePrints.Common
{
    public static class SignalR
    {

        public static IHubProxy HubProxy { get; set; }
        public const string ServerURI = "http://localhost:6060/signalr";
        public static HubConnection Connection { get; set; }

        public static async void ConnectAsync()
        {
            SignalR.Connection = new HubConnection(SignalR.ServerURI);
            SignalR.HubProxy = SignalR.Connection.CreateHubProxy("MyHub");

            SignalR.HubProxy.On<string, string, string, string>("AddMessage", (entityName, key, messageType, sender) =>
                Application.Current.Dispatcher.Invoke(
                () => SignalR.HubReceiveMessage(entityName, key, messageType, sender)
                )
            );

            try
            {
                await SignalR.Connection.Start();
            }
            catch (HttpRequestException)
            {
                //MessageBox.Show(CommonResources.SignalR_UnableToConnect);
                //System.Environment.Exit(1);
            }
        }

        public static void Disconnect()
        {
            if(Connection != null)
            {
                Connection.Stop();
                Connection.Dispose();
            }
        }

        public static void HubSendMessage(string entityName, string key, string messageType, string sender)
        {
            if (SignalR.Connection.State == ConnectionState.Connected)
                HubProxy.Invoke("Send", entityName, key, messageType, sender, LoginCredentials.CurrentUser.NAME);
            else if (SignalR.Connection.State == ConnectionState.Disconnected)
                ConnectAsync();
        }

        public static void HubReceiveMessage(string entityName, string key, string messageType, string sender)
        {
            EntityMessageType MessageType = (EntityMessageType)Enum.Parse(typeof(EntityMessageType), messageType);
            Guid PrimaryKey = new Guid(key);

            if (entityName == typeof(AREA).ToString())
                ReceiveMessage<AREA, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(BASELINE_ITEM).ToString())
                ReceiveMessage<BASELINE_ITEM, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(BASELINE).ToString())
                ReceiveMessage<BASELINE, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(COMMODITY_CODE).ToString())
                ReceiveMessage<COMMODITY_CODE, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(COMMODITY_GROUP_DIRECT).ToString())
                ReceiveMessage<COMMODITY_GROUP_DIRECT, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(DEPARTMENT).ToString())
                ReceiveMessage<DEPARTMENT, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(DISCIPLINE).ToString())
                ReceiveMessage<DISCIPLINE, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(DOCTYPE).ToString())
                ReceiveMessage<DOCTYPE, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(ESTIMATION_ITEM).ToString())
                ReceiveMessage<ESTIMATION_ITEM, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(ESTIMATION).ToString())
                ReceiveMessage<ESTIMATION, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(INDIRECT_TYPE).ToString())
                ReceiveMessage<INDIRECT_TYPE, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(PHASE).ToString())
                ReceiveMessage<PHASE, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(PROGRESS_ITEM).ToString())
                ReceiveMessage<PROGRESS_ITEM, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(PROGRESS).ToString())
                ReceiveMessage<PROGRESS, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(PROJECT_REPORT).ToString())
                ReceiveMessage<PROJECT_REPORT, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(PROJECT).ToString())
                ReceiveMessage<PROJECT, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(RATE).ToString())
                ReceiveMessage<RATE, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(REGISTER).ToString())
                ReceiveMessage<REGISTER, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(ROLE_PERMISSION).ToString())
                ReceiveMessage<ROLE_PERMISSION, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(ROLE).ToString())
                ReceiveMessage<ROLE, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(SETTINGS_GLOBAL).ToString())
                ReceiveMessage<SETTINGS_GLOBAL, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(UOM).ToString())
                ReceiveMessage<UOM, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(USER).ToString())
                ReceiveMessage<USER, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(VARIATION_ITEM).ToString())
                ReceiveMessage<VARIATION_ITEM, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(VARIATION).ToString())
                ReceiveMessage<VARIATION, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(WORKPACK_ASSIGNMENT).ToString())
                ReceiveMessage<WORKPACK_ASSIGNMENT, Guid>(PrimaryKey, MessageType, sender);
            else if (entityName == typeof(WORKPACK).ToString())
                ReceiveMessage<WORKPACK, Guid>(PrimaryKey, MessageType, sender);
        }


        private static void ReceiveMessage<TEntity, TPrimaryKey>(TPrimaryKey primaryKey, EntityMessageType messageType, string sender)
            where TEntity : class
        {
            Messenger.Default.Send(new EntityMessage<TEntity, TPrimaryKey>(primaryKey, messageType, sender));
        }
    }
}
