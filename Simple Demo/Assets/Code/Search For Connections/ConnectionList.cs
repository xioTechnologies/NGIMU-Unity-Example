using System;
using System.Collections.Generic;
using NgimuApi.SearchForConnections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ConnectionList : MonoBehaviour
{
    [SerializeField]
    public ConnectionListItem ListItemPrefab;

    private readonly Dictionary<ConnectionSearchResult, ConnectionListItemStub> foundConnections = new Dictionary<ConnectionSearchResult, ConnectionListItemStub>();
    private readonly List<ConnectionSearchResult> shouldRemove = new List<ConnectionSearchResult>();
    private readonly object syncObject = new object();

    private SearchForConnections autoConnector;
    private RectTransform listItemContainer;
    private ScrollRect scrollRect;

    public static ConnectionList Instance { get; private set; }

    /// <summary>
    /// Activate the connection list and start searching.
    /// </summary>
    public void Activate()
    {
        gameObject.SetActive(true);

        Start();
    }

    /// <summary>
    /// Deactivate the connection list and clear the search results.
    /// </summary>
    public void Deactivate()
    {
        Stop(); 

        gameObject.SetActive(false);
    }

    #region Normal Unity Events

    private void Awake()
    {
        Instance = this;

        scrollRect = GetComponent<ScrollRect>();

        listItemContainer = scrollRect.content;
    }

    private void OnDestroy()
    {
        Stop();

        if (autoConnector != null)
        {
            autoConnector.Dispose();
            autoConnector = null;
        }

        Instance = null;
    }

    private void Start()
    {
        Stop();

        try
        {
            autoConnector = new SearchForConnections();

            autoConnector.DeviceDiscovered += AutoConnector_DeviceDiscovered;
            autoConnector.DeviceExpired += AutoConnector_DeviceExpired;

            autoConnector.BeginSearch();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private void Stop()
    {
        if (autoConnector != null)
        {
            try
            {
                Debug.Log("Stop searching for connections.");

                autoConnector.DeviceDiscovered -= AutoConnector_DeviceDiscovered;
                autoConnector.DeviceExpired -= AutoConnector_DeviceExpired;

                autoConnector.EndSearch();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                autoConnector.Dispose();
                autoConnector = null;
            }
        }

        ManageListItems(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) == true)
        {
            Stop();

            Application.Quit();
        }

        ManageListItems(false);
    }

    #endregion Normal Unity Events

    private void AlignItem(ref float totalHeight, ConnectionListItem connectionListItem)
    {
        float itemHeight = connectionListItem.RectTransform.offsetMax.y - connectionListItem.RectTransform.offsetMin.y;

        connectionListItem.RectTransform.offsetMax = new Vector2(listItemContainer.offsetMax.x, -totalHeight);

        totalHeight += itemHeight;

        connectionListItem.RectTransform.offsetMin = new Vector2(0, -totalHeight);

        connectionListItem.RectTransform.localScale = Vector3.one;
    }

    private void AutoConnector_DeviceDiscovered(ConnectionSearchResult obj)
    {
        Debug.Log("Device Discovered: " + obj.DeviceDescriptor);

        lock (syncObject)
        {
            ConnectionListItemStub foundItem;

            if (foundConnections.TryGetValue(obj, out foundItem) == false)
            {
                foundConnections.Add(obj, new ConnectionListItemStub() { Info = obj });

                return;
            }

            foundItem.ShouldRemove = false;
        }
    }

    private void AutoConnector_DeviceExpired(ConnectionSearchResult obj)
    {
        Debug.Log("Device Expired: " + obj.DeviceDescriptor);

        lock (syncObject)
        {
            ConnectionListItemStub foundItem;

            if (foundConnections.TryGetValue(obj, out foundItem) == false)
            {
                return;
            }

            foundItem.ShouldRemove = true;
        }
    }

    private void ConnectionListItem_Selected(ConnectionListItem item, ConnectionSearchResult connectionSearchInfo)
    {
        NGIMUConnection connection = FindObjectOfType<NGIMUConnection>();

        if (connection == null)
        {
            Debug.LogError("No NGIMUConnection object could be found in the scene hierarchy.");

            return;
        }

        Deactivate();

        connection.ConnectTo(connectionSearchInfo);
    }

    private void ManageListItems(bool shouldClear)
    {
        float totalHeight = 0;

        lock (syncObject)
        {
            shouldRemove.Clear();

            foreach (ConnectionListItemStub connectionListItemStub in foundConnections.Values)
            {
                if (connectionListItemStub.ShouldRemove == true || shouldClear == true)
                {
                    if (connectionListItemStub.HasConnectionListItemBeenCreated == true)
                    {
                        connectionListItemStub.Item.ItemSelected -= ConnectionListItem_Selected;

                        Destroy(connectionListItemStub.Item.gameObject);
                    }

                    // remove the item from the foundConnections collection
                    shouldRemove.Add(connectionListItemStub.Info);

                    continue;
                }
                else if (connectionListItemStub.HasConnectionListItemBeenCreated == false)
                {
                    ConnectionListItem connectionListItem = Instantiate<ConnectionListItem>(ListItemPrefab);

                    connectionListItem.ConnectionSearchInfo = connectionListItemStub.Info;

                    connectionListItem.transform.SetParent(listItemContainer);

                    connectionListItem.ItemSelected += ConnectionListItem_Selected;

                    connectionListItemStub.Item = connectionListItem;
                }

                AlignItem(ref totalHeight, connectionListItemStub.Item);
            }

            foreach (ConnectionSearchResult info in shouldRemove)
            {
                foundConnections.Remove(info);
            }
        }

        listItemContainer.sizeDelta = new Vector2(listItemContainer.sizeDelta.x, totalHeight);
    }

    private class ConnectionListItemStub
    {
        public ConnectionSearchResult Info;
        public ConnectionListItem Item;

        public bool ShouldRemove;

        public bool HasConnectionListItemBeenCreated { get { return Item != null; } }
    }
}