using System.Collections.Generic;
using NgimuApi.SearchForConnections;
using TMPro;
using UnityEngine;

namespace NGIMU.Scripts
{
    public class ConnectionList : MonoBehaviour
    {
        private readonly List<ConnectionSearchResult> active = new List<ConnectionSearchResult>();
        private readonly Dictionary<ConnectionSearchResult, ConnectionItem> connections = new Dictionary<ConnectionSearchResult, ConnectionItem>();
        private readonly List<KeyValuePair<ConnectionSearchResult, ConnectionItem>> items = new List<KeyValuePair<ConnectionSearchResult, ConnectionItem>>();

        private long operationId = long.MinValue;
        
        [SerializeField, Header("Header", order = 1), InspectorName("State")] private TMP_Text HeaderState;
        [SerializeField, InspectorName("Details", order =  2)] private TMP_Text HeaderDetails;
        
        [SerializeField, Header("Item List", order = 10)] private ConnectionItem ItemTemplate;
        [SerializeField, InspectorName("Container", order =  11)] private Transform ListContainer;

        private void MaintainListItems()
        {
            active.Clear();
            active.AddRange(DeviceFinder.Instance.Active);

            items.Clear();
            items.AddRange(connections);

            foreach (KeyValuePair<ConnectionSearchResult, ConnectionItem> connection in items)
            {
                if (active.Contains(connection.Key))
                {
                    active.Remove(connection.Key);
                    continue;
                }

                Destroy(connection.Value.gameObject);

                connections.Remove(connection.Key);
            }

            foreach (ConnectionSearchResult connection in active)
            {
                ConnectionItem item = Instantiate(ItemTemplate, ListContainer);

                item.ConnectionSearchInfo = connection;

                item.SetText();

                connections.Add(connection, item);
            }
        }
        
        private void Update()
        {
            if (DeviceFinder.Instance.OperationId == operationId)
            {
                return; 
            }

            operationId = DeviceFinder.Instance.OperationId;

            HeaderState.text = DeviceFinder.Instance.State;
            HeaderDetails.text = DeviceFinder.Instance.Details;

            MaintainListItems();
        }
    }
}