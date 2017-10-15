using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RobSense_Drone_Swarm_Control_Station
{
    class Mavlink
    {
        swarmlink.loraframe _loraframe = new swarmlink.loraframe();
        public float lat;
        public float lon;
        public float hdg;
        public Label lab_statue;
        public ListBox listBox2;
        public byte[,] Gps_foreach = new byte[100, 38];
        public List<byte> Gps_data = new List<byte>(1000);
        public struct Node_gps
        {
            public byte[] node;
            public float lat;
            public float lon;
        }
        public List<Node_gps> node_gps = new List<Node_gps>(100);
        public byte[] Data_Anlyze(List<byte> data)
        {
            List<byte> get_data = new List<byte>();
            if (data.Count > 24)
            {
                get_data = _loraframe.analyze(data);
            }
            byte Length = get_data[0];
            byte[] Addr = new byte[2];
            Addr[0] = data[1];
            Addr[1] = data[2];
            byte[] MAV_Data = new byte[Length];
            data.CopyTo(3, MAV_Data, 0, Length);
            if (Length == 36)
            {
                lat = (float)(((MAV_Data[10]) | (MAV_Data[11] << 8) | (MAV_Data[12] << 16) | (MAV_Data[13] << 24)) / 10000000.0);
                lon = (float)(((MAV_Data[14]) | (MAV_Data[15] << 8) | (MAV_Data[16] << 16) | (MAV_Data[17] << 24)) / 10000000.0);
                //hdg = (float)(((MAV_Data[32]) | (MAV_Data[33] << 8)) / 100.0);
                func(Addr, lat, lon);
            }
            return Frame_Last_Send(Addr, MAV_Data);
        }
        private void func(byte[] addr, float lat, float lon)
        {
            Node_gps demo;
            demo.node = new byte[2];
            demo.node[0] = addr[0];
            demo.node[1] = addr[1];
            demo.lat = lat;
            demo.lon = lon;
            bool flag = true;
            //
            string a,b;                        
            if (addr[0] == 0)
            {
                a = "00";
            }
            else
            {
                a = addr[0].ToString("X");
            }
            if (addr[1] < 0x10)
            {
                b = "0" + addr[1].ToString("X");
            }
            else
            {
                b = addr[1].ToString("X");
            }
            if (!listBox2.Items.Contains(a+b))
            {
                listBox2.Items.Add(a + b);
            }
            for (int i = 0; i < node_gps.Count; i++)
            {
                if ((demo.node[0] == node_gps[i].node[0]) && (demo.node[1] == node_gps[i].node[1]))
                {
                    node_gps[i] = demo;
                    flag = false;
                }
            }
            //Console.Write("{0} {1} flag:", demo.node[0], demo.node[1]);
            //Console.WriteLine(flag);
            if (flag)
            {
                node_gps.Add(demo);
            }
            for (int i = 0; i < node_gps.Count; i++)
            {
                Console.WriteLine("GET:   addr: {0:X} {1:X}\t lat:{2}\t lon:{3}\t number:{4}", node_gps[i].node[0], node_gps[i].node[1], lat, lon, node_gps.Count);
            }
            lab_statue.Text = "已连接飞行器数量：" + node_gps.Count.ToString();
            //Console.WriteLine("GET:   addr: {0:X} {1:X}\t lat:{2}\t lon:{3}\t number:{4}", node_gps[0].node[0], node_gps[0].node[1], lat, lon,node_gps.Count);
        }
        private byte[] Frame_Last_Send(byte[] addr, byte[] data)
        {
            List<byte> Lora_Mav_Frame = new List<byte>(200);
            Lora_Mav_Frame.Add(0xFB);
            Lora_Mav_Frame.Add(Convert.ToByte(addr.Length));
            for (int i = 0; i < addr.Length; i++)
            {
                Lora_Mav_Frame.Add(addr[i]);
            }
            Lora_Mav_Frame.AddRange(data);
            byte[] Send_data = new byte[Lora_Mav_Frame.Count];
            Lora_Mav_Frame.CopyTo(Send_data);
            return Send_data;
        }
        public byte[] stabilize = new byte[] { 0xFe, 0x06, 0x69, 0xff, 0xbe, 0x0b, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x6d, 0x42 };
        public byte[] guide = new byte[] { 0xFE, 0x06, 0x39, 0xFF, 0xBE, 0x0B, 0x04, 0x00, 0x00, 0x00, 0x01, 0x01, 0x5B, 0x91 };
        public byte[] circle = new byte[] { 0xFE, 0x06, 0x7D, 0xFF, 0xBE, 0x0B, 0x07, 0x00, 0x00, 0x00, 0x01, 0x01, 0x65, 0xAA };
        public byte[] Clear_0A = new byte[] { 0XFE, 0X06, 0X70, 0XFF, 0XBE, 0X42, 0X06, 0X00, 0X01, 0X01, 0X0A, 0X00, 0X44, 0X71 };
        public byte[] Clear_0B = new byte[] { 0XFE, 0X06, 0X72, 0XFF, 0XBE, 0X42, 0X06, 0X00, 0X01, 0X01, 0X0B, 0X00, 0XBA, 0X80 };
        public byte[] Add_GPS = new byte[] { 0XFE, 0X06, 0X6F, 0XFF, 0XBE, 0X42, 0X02, 0X00, 0X01, 0X01, 0X06, 0X01, 0X40, 0XCF };
        public byte[] Clear_GPS = new byte[] { 0XFE, 0X06, 0X6F, 0XFF, 0XBE, 0X42, 0X02, 0X00, 0X01, 0X01, 0X06, 0X00, 0X98, 0XD6 };
        public byte[] Arm = new byte[] { 0XFE, 0X21, 0X00, 0XFF, 0XBE, 0X4C, 0X00, 0X00, 0X80, 0X3F, 0X00, 0X98, 0XA5, 0X46, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X90, 0X01, 0X01, 0X01, 0X00, 0XEB, 0X6C };
        public byte[] Disarm = new byte[] { 0xFE, 0x21, 0x5C, 0xFF, 0xBE, 0x4C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x98, 0xA5, 0x46, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x90, 0x01, 0x01, 0x01, 0x00, 0x2B, 0xD9 };
        public byte[] Arm2 = new byte[] { 0XFE, 0X21, 0X00, 0XFF, 0XBE, 0X4C, 0X00, 0X00, 0X80, 0X3F, 0X00, 0X98, 0XA5, 0X46, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X90, 0X01, 0X01, 0X01, 0X00, 0XEB, 0X6C, 0XFE, 0X21, 0X00, 0XFF, 0XBE, 0X4C, 0X00, 0X00, 0X80, 0X3F, 0X00, 0X98, 0XA5, 0X46, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X90, 0X01, 0X01, 0X01, 0X00, 0XEB, 0X6C };
        public byte[] RTL = new byte[] { 0XFE, 0X06, 0X39, 0XFF, 0XBE, 0X0B, 0X04, 0X00, 0X00, 0X00, 0X01, 0X01, 0X5B, 0X91, 0XFE, 0X21, 0X42, 0XFF, 0XBE, 0X4C, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X80, 0X3F, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X14, 0X00, 0X01, 0X01, 0X00, 0X9A, 0XA2 };
        public byte[] Fly2here(float high, float lat, float lon)
        {
            byte[] data = new byte[45] { 0XFE, 0X25, 0X61, 0XFF, 0XBE, 0X27, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0X00, 0XEC, 0X83, 0XF1, 0X41, 0X04, 0X45, 0XF0, 0X42, 0X00, 0X00, 0X70, 0X41, 0X00, 0X00, 0X10, 0X00, 0X01, 0X01, 0X03, 0X02, 0X01, 0X00, 0X00 };

            byte[] Lat_Float = BitConverter.GetBytes(lat);
            byte[] Lon_Float = BitConverter.GetBytes(lon);
            data[22] = Lat_Float[0];
            data[23] = Lat_Float[1];
            data[24] = Lat_Float[2];
            data[25] = Lat_Float[3];

            data[26] = Lon_Float[0];
            data[27] = Lon_Float[1];
            data[28] = Lon_Float[2];
            data[29] = Lon_Float[3];
            byte[] Convert_byte = BitConverter.GetBytes(high);
            data[30] = Convert_byte[0];
            data[31] = Convert_byte[1];
            data[32] = Convert_byte[2];
            data[33] = Convert_byte[3];
            int CRC = MAVLink_CRC(data, data.Length);
            data[43] = Convert.ToByte(CRC & 0x00ff);
            data[44] = Convert.ToByte(CRC >> 8);
            return data;
        }
        public byte[] Takeoff(float high)
        {
            byte[] guide = new byte[] { 0xFE, 0x06, 0x39, 0xFF, 0xBE, 0x0B, 0x04, 0x00, 0x00, 0x00, 0x01, 0x01, 0x5B, 0x91 };
            byte[] data = new byte[] { 0xFE, 0x21, 0x00, 0xFF, 0xBE, 0x4C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x41, 0x16, 0x00, 0x01, 0x01, 0x00, 0x60, 0x96 };
            byte[] Convert_byte = BitConverter.GetBytes(high);
            data[30] = Convert_byte[0];
            data[31] = Convert_byte[1];
            data[32] = Convert_byte[2];
            data[33] = Convert_byte[3];
            int CRC = MAVLink_CRC(data, data.Length);
            data[39] = Convert.ToByte(CRC & 0x00ff);
            data[40] = Convert.ToByte(CRC >> 8);
            List<byte> buf = new List<byte>();
            buf.AddRange(guide);
            buf.AddRange(data);
            byte[] send_data = new byte[buf.Count];
            buf.CopyTo(send_data);
            foreach (byte i in send_data)
            {
                //txt_Debug_Message_Show.Text += i.ToString("X") + " ";
            }
            return send_data;

        }
        public Mavlink()
        {
            lat = 0;
            lon = 0;
            hdg = 0;
        }
        #region MavLink_Crc
        private byte[] MAVLINK_MESSAGE_LENGTHS_TABLE = new byte[256]
       {
            9, 31, 12, 0, 14, 28, 3, 32, 0, 0, 0, 6, 0, 0, 0, 0,
            0, 0, 0, 0, 20, 2, 25, 23, 30, 101, 22, 26, 16, 14, 28, 32,
            28, 28, 22, 22, 21, 6, 6, 37, 4, 4, 2, 2, 4, 2, 2, 3,
            13, 12, 37, 0, 0, 0, 27, 25, 0, 0, 0, 0, 0, 68, 26, 185,
            181, 42, 6, 4, 0, 11, 18, 0, 0, 37, 20, 35, 33, 3, 0, 0,
            0, 22, 39, 37, 53, 51, 53, 51, 0, 28, 56, 42, 33, 0, 0, 0,
            0, 0, 0, 0, 26, 32, 32, 20, 32, 62, 44, 64, 84, 9, 254, 16,
            0, 36, 44, 64, 22, 6, 14, 12, 97, 2, 2, 113, 35, 6, 79, 35,
            35, 22, 13, 255, 14, 18, 43, 8, 22, 14, 36, 43, 41, 0, 0, 0,
            0, 0, 0, 36, 60, 0, 42, 8, 4, 12, 15, 13, 6, 15, 14, 0,
            12, 3, 8, 28, 44, 3, 9, 22, 12, 18, 34, 66, 98, 8, 48, 19,
            3, 20, 24, 29, 45, 4, 40, 2, 0, 0, 29, 0, 0, 0, 0, 0,
            0, 22, 0, 0, 0, 0, 0, 0, 42, 14, 2, 3, 2, 1, 33, 1,
            6, 2, 4, 2, 3, 2, 0, 1, 3, 2, 4, 2, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 254, 36, 30, 18, 18, 51, 9, 0
       };
        private byte[] MAVLINK_MESSAGE_CRCS_TABLE = new byte[256]
        {
            50, 124, 137, 0, 237, 217, 104, 119, 0, 0, 0, 89, 0, 0, 0, 0,
            0, 0, 0, 0, 214, 159, 220, 168, 24, 23, 170, 144, 67, 115, 39, 246,
            185, 104, 237, 244, 222, 212, 9, 254, 230, 28, 28, 132, 221, 232, 11, 153,
            41, 39, 78, 0, 0, 0, 15, 3, 0, 0, 0, 0, 0, 153, 183, 51,
            82, 118, 148, 21, 0, 243, 124, 0, 0, 38, 20, 158, 152, 143, 0, 0,
            0, 106, 49, 22, 143, 140, 5, 150, 0, 231, 183, 63, 54, 0, 0, 0,
            0, 0, 0, 0, 175, 102, 158, 208, 56, 93, 138, 108, 32, 185, 84, 34,
            0, 124, 237, 4, 76, 128, 56, 116, 134, 237, 203, 250, 87, 203, 220, 25,
            226, 46, 29, 223, 85, 6, 229, 203, 1, 195, 109, 168, 181, 0, 0, 0,
            0, 0, 0, 154, 178, 0, 134, 219, 208, 188, 84, 22, 19, 21, 134, 0,
            78, 68, 189, 127, 154, 21, 21, 144, 1, 234, 73, 181, 22, 83, 167, 138,
            234, 240, 47, 189, 52, 174, 229, 85, 0, 0, 72, 0, 0, 0, 0, 0,
            0, 71, 0, 0, 0, 0, 0, 0, 134, 205, 94, 128, 54, 63, 112, 201,
            221, 226, 238, 103, 235, 14, 0, 77, 50, 163, 115, 47, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 8, 204, 49, 170, 44, 83, 46, 0
        };

        private ushort[] CRC_TABLE = new ushort[256]
        {
            0x0000, 0x1189, 0x2312, 0x329b, 0x4624, 0x57ad, 0x6536, 0x74bf,
            0x8c48, 0x9dc1, 0xaf5a, 0xbed3, 0xca6c, 0xdbe5, 0xe97e, 0xf8f7,
            0x1081, 0x0108, 0x3393, 0x221a, 0x56a5, 0x472c, 0x75b7, 0x643e,
            0x9cc9, 0x8d40, 0xbfdb, 0xae52, 0xdaed, 0xcb64, 0xf9ff, 0xe876,
            0x2102, 0x308b, 0x0210, 0x1399, 0x6726, 0x76af, 0x4434, 0x55bd,
            0xad4a, 0xbcc3, 0x8e58, 0x9fd1, 0xeb6e, 0xfae7, 0xc87c, 0xd9f5,
            0x3183, 0x200a, 0x1291, 0x0318, 0x77a7, 0x662e, 0x54b5, 0x453c,
            0xbdcb, 0xac42, 0x9ed9, 0x8f50, 0xfbef, 0xea66, 0xd8fd, 0xc974,
            0x4204, 0x538d, 0x6116, 0x709f, 0x0420, 0x15a9, 0x2732, 0x36bb,
            0xce4c, 0xdfc5, 0xed5e, 0xfcd7, 0x8868, 0x99e1, 0xab7a, 0xbaf3,
            0x5285, 0x430c, 0x7197, 0x601e, 0x14a1, 0x0528, 0x37b3, 0x263a,
            0xdecd, 0xcf44, 0xfddf, 0xec56, 0x98e9, 0x8960, 0xbbfb, 0xaa72,
            0x6306, 0x728f, 0x4014, 0x519d, 0x2522, 0x34ab, 0x0630, 0x17b9,
            0xef4e, 0xfec7, 0xcc5c, 0xddd5, 0xa96a, 0xb8e3, 0x8a78, 0x9bf1,
            0x7387, 0x620e, 0x5095, 0x411c, 0x35a3, 0x242a, 0x16b1, 0x0738,
            0xffcf, 0xee46, 0xdcdd, 0xcd54, 0xb9eb, 0xa862, 0x9af9, 0x8b70,
            0x8408, 0x9581, 0xa71a, 0xb693, 0xc22c, 0xd3a5, 0xe13e, 0xf0b7,
            0x0840, 0x19c9, 0x2b52, 0x3adb, 0x4e64, 0x5fed, 0x6d76, 0x7cff,
            0x9489, 0x8500, 0xb79b, 0xa612, 0xd2ad, 0xc324, 0xf1bf, 0xe036,
            0x18c1, 0x0948, 0x3bd3, 0x2a5a, 0x5ee5, 0x4f6c, 0x7df7, 0x6c7e,
            0xa50a, 0xb483, 0x8618, 0x9791, 0xe32e, 0xf2a7, 0xc03c, 0xd1b5,
            0x2942, 0x38cb, 0x0a50, 0x1bd9, 0x6f66, 0x7eef, 0x4c74, 0x5dfd,
            0xb58b, 0xa402, 0x9699, 0x8710, 0xf3af, 0xe226, 0xd0bd, 0xc134,
            0x39c3, 0x284a, 0x1ad1, 0x0b58, 0x7fe7, 0x6e6e, 0x5cf5, 0x4d7c,
            0xc60c, 0xd785, 0xe51e, 0xf497, 0x8028, 0x91a1, 0xa33a, 0xb2b3,
            0x4a44, 0x5bcd, 0x6956, 0x78df, 0x0c60, 0x1de9, 0x2f72, 0x3efb,
            0xd68d, 0xc704, 0xf59f, 0xe416, 0x90a9, 0x8120, 0xb3bb, 0xa232,
            0x5ac5, 0x4b4c, 0x79d7, 0x685e, 0x1ce1, 0x0d68, 0x3ff3, 0x2e7a,
            0xe70e, 0xf687, 0xc41c, 0xd595, 0xa12a, 0xb0a3, 0x8238, 0x93b1,
            0x6b46, 0x7acf, 0x4854, 0x59dd, 0x2d62, 0x3ceb, 0x0e70, 0x1ff9,
            0xf78f, 0xe606, 0xd49d, 0xc514, 0xb1ab, 0xa022, 0x92b9, 0x8330,
            0x7bc7, 0x6a4e, 0x58d5, 0x495c, 0x3de3, 0x2c6a, 0x1ef1, 0x0f78
        };

        public int MAVLink_CRC(byte[] message, int messageLength)
        {
            int crc = 0xffff;
            int i = 1;
            while (i < messageLength - 2)
            {
                crc = (crc >> 8) ^ CRC_TABLE[(crc ^ message[i]) & 0xff];
                i++;
            }
            crc = (crc >> 8) ^ CRC_TABLE[(crc ^ MAVLINK_MESSAGE_CRCS_TABLE[message[5]]) & 0xff];
            return crc;
        }
        #endregion
    }
}
