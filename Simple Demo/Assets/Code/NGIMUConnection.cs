using System;
using System.Threading;
using NgimuApi;
using NgimuApi.SearchForConnections;
using UnityEngine;

public class NGIMUConnection : MonoBehaviour
{
    [SerializeField]
    public Transform ModelTransform;

    private readonly UnityReporter reporter = new UnityReporter();

    private Connection connection;

    public void ConnectTo(ConnectionSearchInfo info)
    {
        reporter.Clear(); 

        Debug.Log("ConnectTo (ConnectionSearchInfo)");

        switch (info.ConnectionType)
        {
            case ConnectionType.Udp:
                UdpConnectionInfo udpInfo = info.ConnectionInfo as UdpConnectionInfo;

                Thread thread = new Thread(delegate ()
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
                });
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

    public void ConnectTo(IConnectionInfo connectionInfo)
    {
        Debug.Log("ConnectTo: " + connectionInfo.ToString());

        connection = new Connection(connectionInfo);

        connection.Connected += reporter.OnConnected;
        connection.Disconnected += reporter.OnDisconnected;
        connection.Error += reporter.OnError;
        connection.Exception += reporter.OnException;
        connection.Info += reporter.OnInfo;
        connection.Message += reporter.OnMessage;
        connection.UnknownAddress += reporter.OnUnknownAddress;

        connection.Connect();

        connection.Settings.ReadAync(new ISettingItem[] { connection.Settings }, reporter, 100, 3);
    }

    private void OnDestroy()
    {
        Disconnect();
    }

    public void Disconnect()
    {
        if (connection != null)
        {
            Debug.Log("Disconnect");

            connection.Connected -= reporter.OnConnected;
            connection.Disconnected -= reporter.OnDisconnected;
            connection.Error -= reporter.OnError;
            connection.Exception -= reporter.OnException;
            connection.Info -= reporter.OnInfo;
            connection.Message -= reporter.OnMessage;
            connection.UnknownAddress -= reporter.OnUnknownAddress;
            connection.Dispose();
            connection = null;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) == true)
        {
            Disconnect();

            ShowConnectionList(); 
        }

        if (connection == null)
        {
            return;
        }

        if (connection.Quaternion.Quaternion != new NgimuApi.Maths.Quaternion(0, 0, 0, 0)) 
        {
            ModelTransform.rotation = UnityHelper.NgimuToUnityQuaternion(connection.Quaternion.Quaternion);
        }
    }

    private void ShowConnectionList()
    {
        if (ConnectionList.Instance == null)
        {
            return; 
        }

        ConnectionList.Instance.Activate(); 
    }
}