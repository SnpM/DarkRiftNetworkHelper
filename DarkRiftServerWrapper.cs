
using UnityEngine;
using System.Threading;
using System.Collections.Generic;

using DarkRift;
using System;

namespace Lockstep.NetworkHelpers.DarkRift
{
    /*
 * This file is designed to launch a DarkRift server from Unity during the Awake() function. 
 * 
 * I had hoped this would be fairly simple but due to the fact DarkRift is very multithreaded
 * and Unity is simply not there is a large amount of code for transfering the event calls to
 * the main thread.
 * 
 * The only part of this file you should really need to change is the forceLoadPlugins field 
 * if you want to add any plugins.
 */
    //Copy-paste of DarkRift.Server
    public sealed class DarkRiftServerWrapper : IManualDataProcessor
    {
        /// <summary>
        ///     The port to host the server on.
        /// </summary>
        [SerializeField]
        int port = 4296;

        /// <summary>
        ///     The max number of connections to allow on the server.
        /// </summary>
        [SerializeField]
        ushort maxConnections = 20;

        /// <summary>
        ///     Should passing data be logged to the server?
        /// </summary>
        [SerializeField]
        bool logData = false;

        /// <summary>
        ///     Add any plugins you want loading into here using typeof(T).
        /// </summary>
        // Example:
        // Type[] forceLoadPlugins = new Type[]{typeof(LoginPlugin)};
        [SerializeField]
        Type[] forceLoadPlugins = new Type[0];

        enum EventCallbackHandler
        {
            Update,
            LateUpdate,
            FixedUpdate
        }

        /// <summary>
        ///     Which Unity routine the server events will be called from.
        /// </summary>
        [SerializeField]
        EventCallbackHandler callEventsFrom = EventCallbackHandler.Update;

        bool closing = false;

        struct QueueItem
        {
            public Action processingMethod;
            public ManualResetEvent reset;

            public QueueItem(Action processingMethod)
            {
                this.processingMethod = processingMethod;
                this.reset = null;
            }

            public QueueItem(Action processingMethod, ManualResetEvent reset)
            {
                this.processingMethod = processingMethod;
                this.reset = reset;
            }
        }

        Queue<QueueItem> updateQueue = new Queue<QueueItem>();

        public void Awake()
        {
            //Start the server in embedded mode so with given parameters. The Debug functions pointers redirect Interface
            //messages to Unity, the this will tell the server not the use the thread pool to dispatch events but instead
            //let this script sort it out.
            DarkRiftServer.Bootstrap(Mode.Embedded, port, maxConnections, logData, Debug.Log, Debug.LogWarning, Debug.LogError, Debug.LogError, this, forceLoadPlugins);
        }

        public void OnApplicationQuit()
        {
            //Because Unity stops calling Update/FixedUpdate/LateUpdate before OnApplicationQuit the wait handles
            //required will cause DarkRift to wait forever and crash Unity, therefore if we're closing we just need
            //to execute stuff as it comes to us.
            closing = true;

            //Close the server when we close unity!
            DarkRiftServer.Close(false);
        }

        //Called by the server to add a processing item.
        public void Enqueue(Action processingMethod)
        {
            if (closing)
            {
                processingMethod.Invoke();
            } else
            {
                lock (updateQueue)
                {
                    updateQueue.Enqueue(new QueueItem(processingMethod));
                }
            }
        }

        //Called by the server to add a processing item.
        public ManualResetEvent EnqueueWaitHandle(Action processingMethod)
        {
            if (closing)
            {
                processingMethod.Invoke();
                return new ManualResetEvent(true);
            } else
            {
                lock (updateQueue)
                {
                    ManualResetEvent reset = new ManualResetEvent(false);
                    updateQueue.Enqueue(new QueueItem(processingMethod, reset));
                    return reset;
                }
            }
        }


        public void Update()
        {
            if (callEventsFrom == EventCallbackHandler.Update)
                ProcessAllQueueItems();
        }

        public void LateUpdate()
        {
            if (callEventsFrom == EventCallbackHandler.LateUpdate)
                ProcessAllQueueItems();
        }

        public void FixedUpdate()
        {
            if (callEventsFrom == EventCallbackHandler.FixedUpdate)
                ProcessAllQueueItems();
        }

        void ProcessAllQueueItems()
        {
            //Empty the queue of items
            while (updateQueue.Count > 0)
            {
                QueueItem item;

                lock (updateQueue)
                    item = updateQueue.Dequeue();

                item.processingMethod.Invoke();

                if (item.reset != null)
                    item.reset.Set();
            }
        }
    }
}