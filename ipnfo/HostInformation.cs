﻿using csutils.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;

namespace ipnfo
{
    public class HostInformation : Base
    {
        /// <summary>
        /// For serialisation
        /// </summary>
        public HostInformation()
        {

        }

        public HostInformation(long ip)
        {
            OpenPorts = new List<PortInformation>();
            IP = ip;
        }

        public HostInformation(string ip)
        {
            OpenPorts = new List<PortInformation>();
            IP = IPAddress.Parse(ip).ToLong();
        }

        public long IP
        {
            get { return Get<long>("IP"); }
            set { Set("IP", value); OnPropertyChanged("IP"); }
        }


        public byte[] MACBytes
        {
            get { return MAC.GetAddressBytes(); }
            set { Set("MAC", new PhysicalAddress(value)); OnPropertyChanged("MAC"); }
        }

        [XmlIgnore]
        public PhysicalAddress MAC
        {
            get { return Get<PhysicalAddress>("MAC"); }
            set { Set("MAC", value); OnPropertyChanged("MAC"); OnPropertyChanged("CanWOL"); }
        }

        [XmlIgnore]
        public HostStatus Status
        {
            get { return Get<HostStatus>("Status"); }
            set { Set("Status", value); OnPropertyChanged("Status"); OnPropertyChanged("VisibleStatus"); OnPropertyChanged("CanWOL"); }
        }

        [XmlIgnore]
        public bool Pending
        {
            get { return Get<bool>("Pending"); }
            set { Set("Pending", value); OnPropertyChanged("Pending"); OnPropertyChanged("VisibleStatus"); }
        }

        [XmlIgnore]
        public int Ping
        {
            get { return Get<int>("Ping"); }
            set { Set("Ping", value); OnPropertyChanged("Ping"); }
        }

        public string Hostname
        {
            get { return Get<string>("Hostname"); }
            set { Set("Hostname", value); OnPropertyChanged("Hostname"); }
        }

        public bool CanWOL
        {
            get
            {
                return MAC != null && (Status == HostStatus.Offline ||  Status == HostStatus.Unknown);
            }
        }


        public HostStatus VisibleStatus
        {
            get
            {
                return Pending ? HostStatus.Pending : Status;
            }
        }

        public string Text
        {
            get
            {
                return IP.ToIP().ToString();
            }
        }

        public string LastOctett
        {
            get
            {
                return string.Format("{0}", (int)IP.ToIP().GetAddressBytes()[3]);
            }
        }

        public override string ToString()
        {
            return Text/*+", "+string.Join("|",OpenPorts.Select(s=>s.ShortName))*/;
        }

        [XmlIgnore]
        public List<PortInformation> OpenPorts
        {
            get { return Get<List<PortInformation>>("OpenPorts"); }
            set { Set("OpenPorts", value); OnPropertyChanged("OpenPorts"); }
        }

        private ICommand cmdWakeOnLan;
        /// <summary>
        /// WakeOnLan Command
        /// </summary>
        public ICommand WakeOnLanCommand
        {
            get
            {
                if (cmdWakeOnLan == null)
                    cmdWakeOnLan = new RelayCommand(p => OnWakeOnLan(p), p => CanWakeOnLan());
                return cmdWakeOnLan;
            }
        }

        private bool CanWakeOnLan()
        {
            return CanWOL;
        }

        private void OnWakeOnLan(object parameter)
        {
            WakeOnLan(MAC.GetAddressBytes());
            Task.Run(() => { CheckWOL(); });
        }
	

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        private async void CheckWOL()
        {
            Status = HostStatus.Checking;

            for(int i=0; i<10; i++)
            {
                await Task.Delay(1000);

                HostInformation hi = await MainViewModel.CheckHost(this, null, 1000, false, HostStatus.Checking);
                if(hi.Status == HostStatus.Online)
                {
                    Status = HostStatus.Online;
                    return;
                }
            }

            Status = HostStatus.Offline;
        }

        /// Sends a Wake-On-Lan packet to the specified MAC address.
        /// </summary>
        /// <param name="mac">Physical MAC address to send WOL packet to.</param>
        private static void WakeOnLan(byte[] mac)
        {
            // WOL packet is sent over UDP 255.255.255.0:40000.
            UdpClient client = new UdpClient();
            client.Connect(IPAddress.Broadcast, 40000);

            // WOL packet contains a 6-bytes trailer and 16 times a 6-bytes sequence containing the MAC address.
            byte[] packet = new byte[17 * 6];

            // Trailer of 6 times 0xFF.
            for (int i = 0; i < 6; i++)
                packet[i] = 0xFF;

            // Body of magic packet contains 16 times the MAC address.
            for (int i = 1; i <= 16; i++)
                for (int j = 0; j < 6; j++)
                    packet[i * 6 + j] = mac[j];

            // Send WOL packet.
            client.Send(packet, packet.Length);
        }
    }
}
