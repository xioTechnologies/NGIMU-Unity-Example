using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using NgimuApi.SearchForConnections;
using UnityEngine;

namespace NGIMU.Scripts
{
    /// <summary>
    /// Singleton that maintains a list of possible connections. 
    /// </summary>
    public class DeviceFinder
    {
        public static readonly DeviceFinder Instance = new DeviceFinder();

        private readonly ConcurrentDictionary<int, ConnectionSearchResult> active = new ConcurrentDictionary<int, ConnectionSearchResult>();

        private bool searching;
        private bool inError = false;
        private string errorMessage = string.Empty;
        
        private SearchForConnections autoConnector;

        private long operationId = long.MinValue;

        public IEnumerable<ConnectionSearchResult> Active => active.Values;

        public long OperationId => operationId;

        public string State => inError ? "Error" : searching ? "Searching for devices" : "Idle";

        public string Details => inError ? errorMessage : $"{active.Count} device connections found"; 
        
        private DeviceFinder()
        {
        }

        public void Start()
        {
            Stop();

            inError = false;
            errorMessage = string.Empty;

            try
            {

                autoConnector = new SearchForConnections();

                autoConnector.DeviceDiscovered += AutoConnector_DeviceDiscovered;
                autoConnector.DeviceExpired += AutoConnector_DeviceExpired;

                autoConnector.BeginSearch();
                searching = true;
            }
            catch (Exception ex)
            {
                inError = true;
                errorMessage = ex.Message; 
                
                Interlocked.Increment(ref operationId);
                
                throw; 
            }
        }

        public void Stop()
        {
            autoConnector?.Dispose();

            autoConnector = null;

            active.Clear();

            operationId = long.MinValue;

            searching = false; 
        }

        private void AutoConnector_DeviceDiscovered(ConnectionSearchResult obj)
        {
            Debug.Log("Device Discovered: " + obj.DeviceDescriptor);

            active.TryAdd(obj.ConnectionInfo.GetHashCode(), obj);

            Interlocked.Increment(ref operationId);
        }

        private void AutoConnector_DeviceExpired(ConnectionSearchResult obj)
        {
            Debug.Log("Device Expired: " + obj.DeviceDescriptor);

            active.TryRemove(obj.ConnectionInfo.GetHashCode(), out _);

            Interlocked.Increment(ref operationId);
        }
    }
}