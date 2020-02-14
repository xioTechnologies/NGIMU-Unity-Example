using NgimuApi.SearchForConnections;
using TMPro;
using UnityEngine;

namespace NGIMU.Scripts
{
    public class ConnectionItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text detailsText;

        public ConnectionSearchResult ConnectionSearchInfo { get; set; }

        public void Connect()
        {
            ConnectionAuthority.Instance.ConnectTo(ConnectionSearchInfo);
        }

        public void SetText()
        {
            nameText.text = ConnectionSearchInfo.DeviceDescriptor;
            
            detailsText.text = ConnectionSearchInfo.ConnectionInfo.ToString();
        }
    }
}