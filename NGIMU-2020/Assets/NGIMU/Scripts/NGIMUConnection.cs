using System;
using System.Text;
using System.Threading;
using NgimuApi;
using NgimuApi.SearchForConnections;
using TMPro;
using UnityEngine;

namespace NGIMU.Scripts
{
    public class NGIMUConnection : MonoBehaviour
    {
        public enum ConnectionTab
        {
            None = 0,
            ModelView = 1, 
            ConsoleView = 2,
            SettingsView = 3,
            ErrorsView = 4,
        }
        
        private static readonly NgimuApi.Maths.Quaternion zero = new NgimuApi.Maths.Quaternion(0, 0, 0, 0);
        
        [Header("Header")]
        [SerializeField, Tooltip("Device name text", order = 1)] private TMP_Text HeaderName;
        [SerializeField, Tooltip("Device details", order =  2)] private TMP_Text HeaderDetails;
        
        [Header("Model")]
        [SerializeField, Tooltip("Model to transform", order = 10)] public Transform ModelTransform;
        
        [Header("Tabs")]
        [SerializeField, Tooltip("3d model view", order = 20)] public GameObject ModelView;
        [SerializeField, Tooltip("Console view", order = 21)] public GameObject ConsoleView;
        [SerializeField, Tooltip("Settings view", order = 22)] public GameObject SettingsView;
        [SerializeField, Tooltip("Errors view", order = 23)] public GameObject ErrorsView;
        
        [Header("Text")]
        [SerializeField, Tooltip("Settings text", order = 30)] public TMP_Text SettingsText;
        [SerializeField, Tooltip("Errors text", order = 31)] public TMP_Text ErrorsText;

        private readonly UnityReporter reporter = new UnityReporter();

        private Connection connection;

        public void SelectTab(int tab)
        {
            SelectTab((ConnectionTab) tab);
        }

        public void SelectTab(ConnectionTab tab)
        {
            ModelView.SetActive(tab == ConnectionTab.ModelView);
            ConsoleView.SetActive(tab == ConnectionTab.ConsoleView);
            SettingsView.SetActive(tab == ConnectionTab.SettingsView);
            ErrorsView.SetActive(tab == ConnectionTab.ErrorsView);

            if (connection != null)
            {
                StringBuilder sb = new StringBuilder();
                
                foreach (var setting in connection.Settings.Values)
                {
                    sb.AppendLine(setting.Message.ToString()); 
                }

                SettingsText.text = sb.ToString();
            }
        }

        public void ConnectTo(ConnectionSearchResult info)
        { 
            SelectTab(ConnectionTab.ModelView);
            ErrorsText.text = "No errors"; 
            
            reporter.Clear();

            Debug.Log($"ConnectTo ({info.DeviceDescriptor})");

            HeaderName.text = info.DeviceDescriptor;
            HeaderDetails.text = info.ConnectionInfo.ToIDString(); 

            switch (info.ConnectionType)
            {
                case ConnectionType.Udp:
                    UdpConnectionInfo udpInfo = info.ConnectionInfo as UdpConnectionInfo;

                    Thread thread = new Thread(
                        delegate()
                        {
                            try
                            {
                                Debug.Log("Configure unique UDP connection");

                                UdpConnectionInfo newInfo = Connection.ConfigureUniqueUdpConnection(udpInfo, reporter);

                                ConnectTo(newInfo);
                            }
                            catch (Exception ex)
                            {
                                reporter.OnException(this, new ExceptionEventArgs("The device could not be configured to use a unique connection. " + ex.Message, ex));
                            }
                        }
                    );
                    thread.Name = "Configure Unique UDP Connection";
                    thread.Start();

                    break;

                case ConnectionType.Serial:
                    ConnectTo(info.ConnectionInfo as SerialConnectionInfo);
                    break;

                default:
                    break;
            }
        }

        private void ConnectTo(IConnectionInfo connectionInfo)
        {
            Debug.Log("ConnectTo: " + connectionInfo.ToString());

            connection = new Connection(connectionInfo);

            connection.Connected += reporter.OnConnected;
            connection.Disconnected += reporter.OnDisconnected;
            connection.Error += ConnectionOnError;
            connection.Exception += ConnectionOnException;
            connection.Info += reporter.OnInfo;
            connection.Message += reporter.OnMessage;
            connection.UnknownAddress += reporter.OnUnknownAddress;

            connection.Connect();

            connection.Settings.ReadAync(new ISettingItem[] {connection.Settings}, reporter, 100, 3);
        }

        private void ConnectionOnException(object sender, ExceptionEventArgs e)
        {
            reporter.OnException(sender, e);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Exception");
            
            sb.AppendLine(e.Message);

            if (e.Exception != null)
            {
                sb.AppendLine(e.Exception.ToString());
            }
            
            ErrorsText.text = sb.ToString();
        }

        private void ConnectionOnError(object sender, MessageEventArgs e)
        {
            reporter.OnError(sender, e); 
            
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Error");
            
            sb.AppendLine(e.Message);

            if (e.Detail != null)
            {
                sb.AppendLine(e.Detail);
            }

            ErrorsText.text = sb.ToString();
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        public void Disconnect()
        {
            SelectTab(ConnectionTab.None);
            
            if (connection == null)
            {
                return;
            }

            Debug.Log("Disconnect");

            connection.Connected -= reporter.OnConnected;
            connection.Disconnected -= reporter.OnDisconnected;
            connection.Error -= ConnectionOnError;
            connection.Exception -= ConnectionOnException;
            connection.Info -= reporter.OnInfo;
            connection.Message -= reporter.OnMessage;
            connection.UnknownAddress -= reporter.OnUnknownAddress;
            connection.Dispose();
            connection = null;
        }

        private void Update()
        {
            if (connection == null)
            {
                return;
            }

            HeaderDetails.text = connection.Name;
            
            if (connection.Quaternion.Quaternion != zero)
            {
                ModelTransform.rotation = NgimuMathUtils.NgimuToUnityQuaternion(connection.Quaternion.Quaternion);
            }
        }
    }
}