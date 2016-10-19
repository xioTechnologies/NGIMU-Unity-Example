using NgimuApi.SearchForConnections;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionListItem : MonoBehaviour
{
    public delegate void ConnectionListItemEvent(ConnectionListItem item, ConnectionSearchInfo connectionSearchInfo);

    private Button button;
    private Text text;

    [HideInInspector]
    public RectTransform RectTransform;

    public ConnectionSearchInfo ConnectionSearchInfo { get; set; }

    public event ConnectionListItemEvent ItemSelected;

    private void Awake()
    {
        RectTransform = transform as RectTransform;
    }

    private void Start()
    {
        text = GetComponentInChildren<Text>();
        button = GetComponentInChildren<Button>();

        button.onClick.AddListener(() => DoClick());

        text.text = string.Empty;
    }

    private void Update()
    {
        if (ConnectionSearchInfo == null)
        {
            text.text = string.Empty;
        }

        text.text = ConnectionSearchInfo.DeviceDescriptor + "\n" + ConnectionSearchInfo.ConnectionInfo.ToString();
    }

    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }

    private void DoClick()
    {
        if (ItemSelected != null)
        {
            ItemSelected(this, ConnectionSearchInfo);
        }
    }
}