using System;
using NgimuApi.SearchForConnections;
using UnityEngine;

namespace NGIMU.Scripts
{
    public class ConnectionAuthority : MonoBehaviour
    {
        private enum Panel
        {
            None,
            ConnectionList, 
            Connecting,
            Connected, 
        }

        public static ConnectionAuthority Instance { get; private set; }

        [SerializeField] private NGIMUConnection connection;

        [Header("UI Panels")] 
        [SerializeField] private RectTransform ConnectionList;
        [SerializeField] private RectTransform Connecting;
        [SerializeField] private RectTransform Connected;

        private Panel currentPanel = Panel.None; 

        public void ConnectTo(ConnectionSearchResult searchResult)
        {
            connection.ConnectTo(searchResult);
            
            ShowPanel(Panel.Connected);
        }

        public void ShowConnectionList()
        {
            ShowPanel(Panel.ConnectionList);
        }

        private void ShowPanel(Panel panel)
        {
            currentPanel = panel;

            ConnectionList.gameObject.SetActive(panel == Panel.ConnectionList); 
            Connecting.gameObject.SetActive(panel == Panel.Connecting); 
            Connected.gameObject.SetActive(panel == Panel.Connected); 
            
            if (panel == Panel.ConnectionList)
            {
                DeviceFinder.Instance.Start();
            }
            else
            {
                DeviceFinder.Instance.Stop();
            }

            if (panel != Panel.Connected && panel != Panel.Connecting)
            {
                connection.Disconnect();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) != true)
            {
                return;
            }

            switch (currentPanel)
            {
                case Panel.None: 
                case Panel.ConnectionList:
                    Application.Quit();
                    return;
                case Panel.Connecting: 
                case Panel.Connected:
                    ShowConnectionList();
                    return;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("More than one ConnectionAuthority is present in the scene!");
                return; 
            }

            Instance = this;

            ShowConnectionList();
        }

        private void OnDestroy()
        {
            ShowPanel(Panel.None);
            
            if (Instance != this)
            {
                return; 
            }

            Instance = null; 
        }
    }
}