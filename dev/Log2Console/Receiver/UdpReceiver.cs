using Log2Console.Log;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace Log2Console.Receiver
{
    [Serializable]
    [DisplayName("UDP (IP v4 and v6)")]
    public class UdpReceiver : BaseReceiver
    {
        [NonSerialized]
        private Thread _worker;
        [NonSerialized]
        private UdpClient _udpClient;
        [NonSerialized]
        private IPEndPoint _remoteEndPoint;

        [Category("Configuration")]
        [DisplayName("UDP Port Number")]
        //[DefaultValue(7071)]
        public int Port { get; set; } = 7071;

        [Category("Configuration")]
        [DisplayName("Use IPv6 Addresses")]
        //[DefaultValue(false)]
        public bool IpV6 { get; set; }

        [Category("Configuration")]
        [DisplayName("Multicast Group Address (Optional)")]
        public string Address { get; set; } = string.Empty;

        [Category("Configuration")]
        [DisplayName("Receive Buffer Size")]
        public int BufferSize { get; set; } = 10000;


        #region IReceiver Members

        [Browsable(false)]
        public override string SampleClientConfig
        {
            get
            {
                return
                    "Configuration for log4net:" + Environment.NewLine +
                    "<appender name=\"UdpAppender\" type=\"log4net.Appender.UdpAppender\">" + Environment.NewLine +
                    "    <remoteAddress value=\"localhost\" />" + Environment.NewLine +
                   $"    <remotePort value=\"{Port}\" />" + Environment.NewLine +
                    "    <layout type=\"log4net.Layout.XmlLayoutSchemaLog4j\" />" + Environment.NewLine +
                    "</appender>";
            }
        }

        public override void Initialize()
        {
            if ((_worker != null) && _worker.IsAlive)
            {
                return;
            }

            // Init connexion here, before starting the thread, to know the status now
            _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            _udpClient = IpV6 ? new UdpClient(Port, AddressFamily.InterNetworkV6) : new UdpClient(Port);
            _udpClient.Client.ReceiveBufferSize = BufferSize;
            if (!String.IsNullOrEmpty(Address))
            {
                _udpClient.JoinMulticastGroup(IPAddress.Parse(Address));
            }

            // We need a working thread
            _worker = new Thread(Start)
            {
                IsBackground = true
            };
            _worker.Start();
        }

        public override void Terminate()
        {
            if (_udpClient != null)
            {
                _udpClient.Close();
                _udpClient = null;

                _remoteEndPoint = null;
            }

            if ((_worker != null) && _worker.IsAlive)
            {
                _worker.Abort();
            }

            _worker = null;
        }

        #endregion

        public void Clear()
        {
        }

        private void Start()
        {
            while ((_udpClient != null) && (_remoteEndPoint != null))
            {
                try
                {
                    byte[] buffer = _udpClient.Receive(ref _remoteEndPoint);
                    string loggingEvent = Encoding.UTF8.GetString(buffer);

                    //Console.WriteLine(loggingEvent);
                    //  Console.WriteLine("Count: " + count++);

                    if (Notifiable == null)
                    {
                        continue;
                    }

                    LogMessage logMsg = ReceiverUtils.ParseLog4JXmlLogEvent(loggingEvent, "UdpLogger");
                    logMsg.RootLoggerName = _remoteEndPoint.Address.ToString().Replace(".", "-");
                    logMsg.LoggerName = string.Format("{0}_{1}", _remoteEndPoint.Address.ToString().Replace(".", "-"), logMsg.LoggerName);
                    Notifiable.Notify(logMsg);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return;
                }
            }
        }

    }
}
