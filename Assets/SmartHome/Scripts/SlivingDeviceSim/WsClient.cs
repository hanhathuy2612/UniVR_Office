using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SlivingDeviceSim
{
    public class MessageEventArgs : EventArgs
    {
        private byte[] _data;
        internal MessageEventArgs(byte[] Data)
        {
            _data = Data;
        }
        public byte[] Data()
        {
            return _data;
        }
    }

    public enum WsState
    {
        ERROR = -1,
        UNKNOWN = 0,
        INIT,
        CONNECTED,
        WAIT_TIMEOUT,
    };

    public class WsClient
    {
        private const int MAX_READ_SIZE = 2 * 1024 * 1024;
        private const int DEFAULT_WAIT_TIMEOUT_MS = 10 * 1000;

        private readonly ClientWebSocket _ws;
        private readonly Uri _url;
        private WsState _wsState;
        private bool _autoReconnect = true;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;
        private int _waitTimeOutMs = DEFAULT_WAIT_TIMEOUT_MS;
        private bool _threadRun = true;

        //private delegate void OnMessageEventHandler(object sender, MessageEventArgs e);
        //private delegate void OnErrorEventHandler(object sender, MessageEventArgs e);
        //private delegate void OnConnectEventHandler(object sender, MessageEventArgs e);
        //private delegate void OnDisconnectEventHandler(object sender, MessageEventArgs e);

        public event EventHandler OnData;
        public event EventHandler OnError;
        public event EventHandler OnConnect;
        public event EventHandler OnDisconnect;

        protected virtual void DispachEvent(EventHandler dispacher, MessageEventArgs e)
        {
            EventHandler handler = dispacher;
            handler?.Invoke(this, e);
        }


        public WsClient(string ServerURI)
        {
            _ws = new ClientWebSocket();
            _url = new Uri(ServerURI);
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            _wsState = WsState.INIT;
        }

        public void start(bool AutoReconnect = true)
        {
            _autoReconnect = AutoReconnect;
            Task.Run(WebSocketThread);
        }

        public void stop()
        {
            _autoReconnect = false;
            _threadRun = false;
            _ws?.Abort();
            _cancellationTokenSource.Cancel();
        }

        #region Websocket Management
        private void WebSocketAbort()
        {
            _ws.Abort();
            _waitTimeOutMs = DEFAULT_WAIT_TIMEOUT_MS;
            _wsState = WsState.WAIT_TIMEOUT;
            DispachEvent(OnDisconnect, null);
        }


        private async void WebSocketThread()
        {
            var buffer = new byte[MAX_READ_SIZE];
            var bufferList = new List<byte>();
            
            while (_threadRun)
            {
                if (_cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                switch (_wsState)
                {
                    case WsState.INIT:
                        try
                        {
                            Log.Debug("Websocket connecting...");
                            await _ws.ConnectAsync(_url, _cancellationToken);
                            _wsState = WsState.CONNECTED;
                            DispachEvent(OnConnect, null);
                        }
                        catch (Exception)
                        {
                            Log.Debug("Websocket error connecting");
                            WebSocketAbort();
                        }

                        break;
                    case WsState.CONNECTED:
                        if (_ws.State != WebSocketState.Open)
                        {
                            Log.Debug("Websocket invalid state");
                            WebSocketAbort();
                            break;
                        }

                        WebSocketReceiveResult result;

                        do
                        {
                            try
                            {
                                result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationToken);
                            } catch (Exception e)
                            {
                                Log.Debug("ReceiveAsync exception " + e.ToString());
                                WebSocketAbort();
                                break;
                            }
                            

                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                await
                                    _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                                DispachEvent(OnDisconnect, null);
                            }
                            else if (result.MessageType == WebSocketMessageType.Binary)
                            {
                                for (int i = 0; i< result.Count; i++)
                                {
                                    bufferList.Add(buffer[i]);
                                }
                                
                                //var str = Encoding.UTF8.GetString(buffer, 0, result.Count);
                                //stringResult.Append(str);
                            } else
                            {
                                Log.Debug("Other msg type");
                            }

                        } while (!result.EndOfMessage);

                        DispachEvent(OnData, new MessageEventArgs(bufferList.ToArray()));
                        bufferList.Clear();
                        break;
                    case WsState.WAIT_TIMEOUT:
                        if (!_autoReconnect)
                        {
                            _threadRun = false;
                            _cancellationTokenSource.Cancel();
                            Log.Debug("stoped...");
                            break;
                        }
                        Thread.Sleep(_waitTimeOutMs);
                        _wsState = WsState.INIT;
                        Log.Debug("Reconnecting...");
                        break;
                }
            }
        }
        public void SendMessage(string message)
        {
            var messageBuffer = Encoding.UTF8.GetBytes(message);
            SendMessage(messageBuffer);
        }

        public async void SendMessage(byte[] message)
        {
            await SendMessageAsync(message);
        }

        public async Task SendMessageAsync(byte[] message)
        {
            if (_ws.State != WebSocketState.Open || _wsState != WsState.CONNECTED)
            {
                throw new Exception("Connection is not open.");
            }

            var messageBuffer = message;
            var messagesCount = (int)Math.Ceiling((double)messageBuffer.Length / MAX_READ_SIZE);
            Debug.Log("messagesCount: " + messagesCount);
            for (var i = 0; i < messagesCount; i++)
            {
                var offset = (MAX_READ_SIZE * i);
                var count = MAX_READ_SIZE;
                var lastMessage = ((i + 1) == messagesCount);

                if ((count * (i + 1)) > messageBuffer.Length)
                {
                    count = messageBuffer.Length - offset;
                }
                Debug.Log("Index: " + i);
                await _ws.SendAsync(new ArraySegment<byte>(messageBuffer, offset, count), WebSocketMessageType.Binary, lastMessage, _cancellationToken);
            }
        }
        #endregion

    }
}