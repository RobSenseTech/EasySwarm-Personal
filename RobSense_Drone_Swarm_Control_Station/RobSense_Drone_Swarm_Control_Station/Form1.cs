using System;
using CCWin;
using System.IO.Ports;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Timers;
using System.IO;
using System.Text;

namespace RobSense_Drone_Swarm_Control_Station
{
    public partial class EasySwarm : CCSkinMain
    {
        SerialPort _serialPort;
        IEversion _ieVersion;
        Mavlink _mavLink;
        swarmlink.loraframe _loraframe = new swarmlink.loraframe();
        TreeNode tn;
        System.Timers.Timer aTimer = new System.Timers.Timer();
        System.Timers.Timer bTimer = new System.Timers.Timer();
        System.Timers.Timer cTimer = new System.Timers.Timer();
        int Timer_Cnt = 0;
        int Timer_Cnt_B = 0;
        int Timer_Cnt_C = 0;
        int Line_Cnt = 0;
        List<byte> re_buf = new List<byte>(200); //Receive buffer
        struct Online_node
        {
            public byte[] node;
        }
        List<Online_node> online_node = new List<Online_node>(100);
        public int[,] position = new int[100, 25];
        float base_lat;
        float base_lon;
        string path = @"test.txt";
        public EasySwarm()
        {
            
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            //Serialport init.
            _serialPort = new SerialPort();
            _serialPort.Parity = Parity.None;         //Check None
            _serialPort.DataBits = 8;                    //Data length 8
            _serialPort.StopBits = StopBits.One;   //Stop bits 1
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
            cbx_baud_select.Items.Add("115200");
            cbx_baud_select.Items.Add("57600");
            cbx_baud_select.Items.Add("19200");
            cbx_baud_select.Items.Add("9600");
            cbx_baud_select.SelectedIndex = 0;
            string[] ArryPort = SerialPort.GetPortNames();
            cbx_com_select.Items.Clear();
            for (int i = 0; i < ArryPort.Length; i++)
            {
                cbx_com_select.Items.Add(ArryPort[i]);
            }
            try
            {
                cbx_com_select.SelectedIndex = 0;
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp);
            }
            //webBrowser init.
            _ieVersion = new IEversion();
            _ieVersion.BrowserEmulationSet();
            webBrowser1.Navigate("http://www.tanjingdeng.com/new_test/");
            //ProgressBar init.
            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = 100;
            ProgressBar.ForeColor = Color.Red;
            //treeview init.
            tn = treeview_online_node.Nodes.Add("在线节点");
            //Timer init.
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            bTimer.Elapsed += new ElapsedEventHandler(tOnTimedEvent);
            cTimer.Elapsed += new ElapsedEventHandler(cOnTimedEvent);
            //mavlink init.
            _mavLink = new Mavlink();
            _mavLink.lab_statue = lab_statue;
            _mavLink.listBox2 = listBox2;
            //flight parameters
            txt_lat.Text = "0";
            txt_lon.Text = "0";
            txt_alt.Text = "15";
            
        }
        private void Form1_Load(object sender, EventArgs e)
        {
        }
        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] buf = new byte[_serialPort.BytesToRead];//Creat receive array.              
            _serialPort.Read(buf, 0, _serialPort.BytesToRead);//Read receive data.            
            byte[] re_buf_temp = _loraframe.decode(buf);
            if (re_buf_temp != null)
            {
                re_buf.Clear();
                re_buf.AddRange(re_buf_temp);
            }
            if (re_buf.Count >= 33)
            {
                if (re_buf[32] == 0xFE)
                {
                    //_socket.Flag = true;
                    //_socket.Socket_2server_buf.AddRange(_mavLink.Data_Anlyze(_loraframe.Re_Buf));
                    _mavLink.Data_Anlyze(re_buf);
                }
                else
                {
                    CMD_Process();
                }
            }
        }
        private void CMD_Process()
        {

            // Used to analyze cmd frame 77 or 68              
            re_buf.RemoveRange(0, 24);
            if (re_buf[0] == 0x77)//Analyze frame 77.(Get the concentrator address, node address, and node status instruction.)
            {
                lable_gateway_address.Text = "";
                lable_cnt_node.Text = "";
                lable_gateway_address.Text = "0X" + re_buf[13].ToString("X");
                lable_cnt_node.Text = re_buf[14].ToString();
                //_loraframe.Ap_Add_68[0] = 0x00;
                //_loraframe.Ap_Add_68[1] = re_buf[13];
                re_buf.RemoveRange(0, 15);
                byte j = 0;
                string[] txt = new string[4];
                listBox1.Items.Clear();
                for (int i = 0; i < re_buf.Count / 7; i++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        if (re_buf[j + k] < 16)
                        {
                            if (re_buf[j + k] == 0) txt[k] = "00";
                            else
                            {
                                txt[k] = "0" + re_buf[j + k].ToString("X");
                            }
                        }
                        else
                        {
                            txt[k] = re_buf[j + k].ToString("X");
                        }

                    }
                    listBox1.Items.Add(txt[3] + txt[2] + txt[1] + txt[0]);
                    j += 7;
                }
            }

            if (re_buf[0] == 0x68)//Analyze frame 68.
            {
                #region   Determine the existence of nodes
                if ((re_buf[8] == 0x00) && (re_buf[9] == 0x01) && (re_buf[10] == 0x0A) && (re_buf[11] == 0x00))
                {
                    this.Invoke((EventHandler)(delegate
                    {
                        if (re_buf[2] == 0 && re_buf[3] == 0) return;
                        string a, b;
                        Online_node demo;
                        demo.node = new byte[2];
                        demo.node[0] = re_buf[2];
                        demo.node[1] = re_buf[3];
                        online_node.Add(demo);
                        a = re_buf[2].ToString("X");
                        b = re_buf[3].ToString("X");
                        tn.Nodes.Add(a + b);
                        if (!cbox_node_mac.Items.Contains(a + b))
                        {
                            cbox_node_mac.Items.Add(a + b);
                        }
                        if (!cbox_node_mac1.Items.Contains(a + b))
                        {
                            cbox_node_mac1.Items.Add(a + b);
                        }
                        if (!cbox_node_mac2.Items.Contains(a + b))
                        {
                            cbox_node_mac2.Items.Add(a + b);
                        }
                        return;
                    }));
                }
                #endregion  
            }
        }
        #region Page1
        private void btn_open_close_Click(object sender, EventArgs e)
        {
            try
            {
                if (btn_open_close.Text.Equals("打开串口"))
                {
                    _serialPort.BaudRate = Convert.ToInt32(cbx_baud_select.Text);           //Baud rate
                    _serialPort.PortName = cbx_com_select.Text;          //Name
                    _serialPort.Open();                 //Open UART      
                    if (_serialPort.IsOpen)
                    {
                        btn_open_close.Text = "关闭串口";
                        _serialPort.Write(_loraframe.cmd_refresh_ap(), 0, _loraframe.cmd_refresh_ap().Length);//Send refresh cmd.
                        ProgressBar.Value = 100;
                    }                    
                }
                else
                {                    
                    btn_open_close.Text = "打开串口";
                    _serialPort.Close();
                    ProgressBar.Value = 0;
                    listBox1.Items.Clear();
                    listBox2.Items.Clear();
                    listBox2.Items.Add("所有飞行器");
                    tn.Nodes.Clear();
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp);
            }
        }
        private void btn_refresh_com_Click(object sender, EventArgs e)
        {
            try
            {
                string[] ArryPort = SerialPort.GetPortNames();
                cbx_com_select.Items.Clear();
                for (int i = 0; i < ArryPort.Length; i++)
                {
                    cbx_com_select.Items.Add(ArryPort[i]);
                }
                cbx_com_select.SelectedIndex = 0;
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.ToString());
            }
        }
        private void btn_add_Click(object sender, EventArgs e)
        {
            try
            {
                if (!listBox1.Items.Contains(txt_node_input.Text))  //If repeat not add.
                {
                    UInt32 Get_Mac;
                    byte a, b, c, d;

                    Get_Mac = Convert.ToUInt32(txt_node_input.Text, 16);// String convert to uin16
                    a = Convert.ToByte((Get_Mac >> 24) & 0x00ff);                      //uint16 convert to byte
                    b = Convert.ToByte((Get_Mac >> 16) & 0x00ff);                //uint16 convert to byte
                    c = Convert.ToByte((Get_Mac >> 8) & 0x00ff);                      //uint16 convert to byte
                    d = Convert.ToByte(Get_Mac & 0x00ff);                //uint16 convert to byte
                    byte[] get_data = _loraframe.Cmd_Add_Node(a, b, c, d);
                    _serialPort.Write(get_data, 0, get_data.Length); //Send
                    System.Threading.Thread.Sleep(200);//Wait 100ms   
                    listBox1.Items.Add(txt_node_input.Text);
                    txt_node_input.Text = "";
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }
        private void btn_del_Click(object sender, EventArgs e)
        {
            try
            {
                UInt32 Get_Mac;
                byte a, b, c, d;
                Get_Mac = Convert.ToUInt32(listBox1.SelectedItem.ToString(), 16);// String convert to uin16
                a = Convert.ToByte((Get_Mac >> 24) & 0x00ff);                      //uint16 convert to byte
                b = Convert.ToByte((Get_Mac >> 16) & 0x00ff);                //uint16 convert to byte
                c = Convert.ToByte((Get_Mac >> 8) & 0x00ff);                      //uint16 convert to byte
                d = Convert.ToByte(Get_Mac & 0x00ff);                //uint16 convert to byte
                byte[] get_data = _loraframe.Cmd_Del_Node(a, b, c, d);
                _serialPort.Write(get_data, 0, get_data.Length);
                System.Threading.Thread.Sleep(200);//Wait 100ms   
                listBox1.Items.Remove(listBox1.SelectedItem);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }

        }
        private void btn_refresh_Click(object sender, EventArgs e)
        {
            try
            {
                online_node.Clear();
                tn.Nodes.Clear();
                //_loraframe.Control_68[0] = 0x05;
                aTimer.Interval = 100;
                aTimer.Enabled = true;
            }
            catch
            {
                MessageBox.Show("Please input UART.");
            }

        }
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                uint Get_Mac = uint.Parse(listBox1.Items[Timer_Cnt].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier);
                byte[] node_mac = new byte[2];
                node_mac[0] = Convert.ToByte((Get_Mac >> 8) & 0x000000ff);//Data convert
                node_mac[1] = Convert.ToByte(Get_Mac & 0x000000ff);//Data convert
                byte[] data = new byte[4] { 0x00, 0x01, 0x0A, 0x00 };
                byte[] get_data = _loraframe.unicast_cmd(node_mac, data);
                _serialPort.Write(get_data, 0, get_data.Length);//Send.
                Timer_Cnt++;
                if (Timer_Cnt == listBox1.Items.Count)
                {
                    aTimer.Enabled = false;
                    Timer_Cnt = 0;
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }
        #endregion
        #region Page2
        private void btn_arm_Click(object sender, EventArgs e)
        {
            byte[] get_data = _loraframe.broadcast(_mavLink.stabilize);
            _serialPort.Write(get_data, 0, get_data.Length);
        }
        private void btn_takeoff_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] get_data = _loraframe.broadcast(_mavLink.guide);              
                _serialPort.Write(get_data, 0, get_data.Length);
                get_data = _loraframe.broadcast(_mavLink.Takeoff(Convert.ToInt32(txt_alt.Text)));              
                _serialPort.Write(get_data, 0, get_data.Length);
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp);
            }
        }
        private void btn_rtl_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] get_data = _loraframe.broadcast(_mavLink.RTL);
                _serialPort.Write(get_data, 0, get_data.Length);
            }
            catch
            {
                MessageBox.Show("Please input UART.");
            }
        }
        private void btn_stabilize_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] get_data = _loraframe.broadcast(_mavLink.stabilize);
                _serialPort.Write(get_data, 0, get_data.Length);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }
        private void btn_guide_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] get_data = _loraframe.broadcast(_mavLink.guide);
                _serialPort.Write(get_data, 0, get_data.Length);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }
        private void btn_disarm_Click(object sender, EventArgs e)
        {
            byte[] get_data = _loraframe.broadcast(_mavLink.Disarm);
            _serialPort.Write(get_data, 0, get_data.Length);
        }
        private void btn_FTH_Click(object sender, EventArgs e)
        {
            try
            {
                float lat = 0;
                float lon = 0;
                for (int i = 0; i < _mavLink.node_gps.Count; i++)
                {
                    lat = _mavLink.node_gps[i].lat + (float)(0.00003 * Convert.ToDouble(txt_lat.Text));
                    lon = _mavLink.node_gps[i].lon + (float)(0.00003 * Convert.ToDouble(txt_lon.Text));
                    //System.Threading.Thread.Sleep(10);
                    byte[] node_mac = new byte[2];
                    node_mac[0] = _mavLink.node_gps[i].node[0];
                    node_mac[1] = _mavLink.node_gps[i].node[1];
                    //Console.WriteLine("SEND:   addr: {0:X} {1:X}\t lat:{2}\t lon:{3}\t", _loraframe.Node_Addr_68[0], _loraframe.Node_Addr_68[1], lat, lon);
                    byte[] get_data  = _loraframe.unicast(node_mac, _mavLink.Fly2here(Convert.ToInt32(txt_alt.Text), lat, lon));
                    _serialPort.Write(get_data, 0, get_data.Length);
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }
        private void btn_link_plane_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] get_data = _loraframe.broadcast(_mavLink.Clear_0A);
                _serialPort.Write(get_data, 0, get_data.Length);        //Send data.

                byte[] get_data1 = _loraframe.broadcast(_mavLink.Clear_0B);
                _serialPort.Write(get_data1, 0, get_data1.Length);        //Send data.

                byte[] get_data2 = _loraframe.broadcast(_mavLink.Add_GPS);
                _serialPort.Write(get_data2, 0, get_data2.Length);        //Send data.
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
                throw;
            }
        }
        private void btn_file_Click(object sender, EventArgs e)
        {

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            path = ofd.FileName;
            lab_aoto.Text = "已成功导入文件";
            //foreach (int i in oneline[3].position)
            //{
            //    Console.WriteLine(i);
            //}
        }
        private void btn_show_Click(object sender, EventArgs e)
        {
            StreamReader sr = new StreamReader(path, Encoding.Default);
            String line;
            Line_Cnt = 0;
            while ((line = sr.ReadLine()) != null)
            {
                string[] sArray = line.Split(',');
                for (int i = 0; i < sArray.Length - 1; i++)
                {
                    position[Line_Cnt, i] = int.Parse(sArray[i].Trim());
                }
                Line_Cnt++;
            }
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    Console.Write("{0} ", position[i, j]);
                }
                Console.WriteLine();
            }
            cTimer.Interval = 1000;
            cTimer.Enabled = true;
        }
        private void cOnTimedEvent(object source, ElapsedEventArgs e)
        {
            switch (Timer_Cnt_C++)
            {
                case 0:
                    Console.WriteLine("Clear gps");
                    byte[] get_data = _loraframe.broadcast(_mavLink.Clear_GPS);
                    _serialPort.Write(get_data, 0, get_data.Length);        //Send data.
                    break;
                case 1:
                    Console.WriteLine("Clear gps");
                    byte[] get_data1 = _loraframe.broadcast(_mavLink.Clear_GPS);
                    _serialPort.Write(get_data1, 0, get_data1.Length);        //Send data.
                    break;
                case 2: Console.WriteLine("delay"); break;
                case 3: Console.WriteLine("delay"); break;
                case 4: Console.WriteLine("delay"); break;
                case 5: Console.WriteLine("delay"); break;
                case 6: Console.WriteLine("delay"); break;
                case 7: Console.WriteLine("delay"); break;
                case 8:
                    Console.WriteLine("stabilize");
                    for (int i = 0; i < _mavLink.node_gps.Count; i++)
                    {
                        byte[] node_mac = new byte[2];
                        node_mac[0] = _mavLink.node_gps[i].node[0];
                        node_mac[1] = _mavLink.node_gps[i].node[1];
                        byte[] get_data2 = _loraframe.unicast(node_mac, _mavLink.stabilize);
                        _serialPort.Write(get_data2, 0, get_data2.Length);        //Send data.
                    }
                    break;

                case 9:
                    Console.WriteLine("arm");
                    for (int i = 0; i < _mavLink.node_gps.Count; i++)
                    {
                        byte[] node_mac = new byte[2];
                        node_mac[0] = _mavLink.node_gps[i].node[0];
                        node_mac[1] = _mavLink.node_gps[i].node[1];
                        byte[] get_data3 = _loraframe.unicast(node_mac, _mavLink.Arm);
                        _serialPort.Write(get_data3, 0, get_data3.Length);        //Send data.
                    }
                    break;

                case 10:
                    Console.WriteLine("guide");
                    for (int i = 0; i < _mavLink.node_gps.Count; i++)
                    {
                        byte[] node_mac = new byte[2];
                        node_mac[0] = _mavLink.node_gps[i].node[0];
                        node_mac[1] = _mavLink.node_gps[i].node[1];
                        byte[] get_data4 = _loraframe.unicast(node_mac, _mavLink.guide);
                        _serialPort.Write(get_data4, 0, get_data4.Length);        //Send data.
                    }
                    break;
                case 11:
                    Console.WriteLine("takeoff");
                    float alt = Convert.ToInt32(txt_alt.Text);
                    for (int i = 0; i < _mavLink.node_gps.Count; i++)
                    {
                        byte[] node_mac = new byte[2];
                        node_mac[0] = _mavLink.node_gps[i].node[0];
                        node_mac[1] = _mavLink.node_gps[i].node[1];
                        byte[] get_data5 = _loraframe.unicast(node_mac, _mavLink.Takeoff(Convert.ToInt32(txt_alt.Text)));
                        _serialPort.Write(get_data5, 0, get_data5.Length);        //Send data.
                    }
                    break;

                case 12: Console.WriteLine("delay"); break;
                case 13: Console.WriteLine("delay"); break;
                case 14: Console.WriteLine("delay"); break;

                default:
                    break;
            }
            if (Timer_Cnt_C == 16)
            {
                cTimer.Enabled = false;
                bTimer.Interval = 1000;// 指令间隔时间16000ms = 16s
                bTimer.Enabled = true;
                base_lat = _mavLink.node_gps[0].lat;
                base_lon = _mavLink.node_gps[0].lon;
                //for (int i = 0; i < _mavLink.node_gps.Count; i++)
                //{
                //    if (_mavLink.node_gps[i].node[1] == 0x03)
                //    {
                //        base_lat = _mavLink.node_gps[i].lat;
                //        base_lon = _mavLink.node_gps[i].lon;
                //    }
                //}
                Console.WriteLine(base_lat);
                Console.WriteLine(base_lon);
            }
        }
        private void tOnTimedEvent(object source, ElapsedEventArgs e)
        {
            int i = Timer_Cnt_B++;
            show_func(position[i, 0], position[i, 1], position[i, 2], position[i, 3], position[i, 4],
                            position[i, 5], position[i, 6], position[i, 7], position[i, 8], position[i, 9],
                            position[i, 10], position[i, 11], position[i, 12], position[i, 13], position[i, 14],
                            position[i, 15], position[i, 16], position[i, 17], position[i, 18], position[i, 19],
                            position[i, 20], position[i, 21], position[i, 22], position[i, 23], position[i, 24]);
            for (int j = 0; j < 25; j++)
            {
                Console.Write("{0} ", position[i, j]);
            }
            Console.WriteLine();
            if (Timer_Cnt_B == Line_Cnt - 1)
            {
                Console.WriteLine("RTL");
                bTimer.Enabled = false;
                for (int j = 0; j < _mavLink.node_gps.Count; j++)
                {
                    byte[] node_mac = new byte[2];
                    node_mac[0] = _mavLink.node_gps[j].node[0];
                    node_mac[1] = _mavLink.node_gps[j].node[1];
                    byte[] get_data = _loraframe.unicast(node_mac, _mavLink.RTL);
                    _serialPort.Write(get_data, 0, get_data.Length);        //Send data.
                }
            }
        }
        private void show_func(int mac_a1, int mac_a2, int mac_b1, int mac_b2, int mac_c1, int mac_c2, int mac_d1, int mac_d2, int mac_e1, int mac_e2, int lat_a, int lon_a, int lat_b, int lon_b, int lat_c, int lon_c, int lat_d, int lon_d, int lat_e, int lon_e, int alt_a, int alt_b, int alt_c, int alt_d, int alt_e)
        {
            byte[] node_mac = new byte[2];
            node_mac[0] = Convert.ToByte(mac_a1);
            node_mac[1] = Convert.ToByte(mac_a2);
            byte[] get_data = _loraframe.unicast(node_mac, _mavLink.Fly2here(alt_a, base_lat + (float)(lat_a * 0.00006), base_lon + (float)(lon_a * 0.00006)));
            _serialPort.Write(get_data, 0, get_data.Length);        //Send data.

            node_mac[0] = Convert.ToByte(mac_b1);
            node_mac[1] = Convert.ToByte(mac_b2);
            get_data = _loraframe.unicast(node_mac, _mavLink.Fly2here(alt_b, base_lat + (float)(lat_b * 0.00006), base_lon + (float)(lon_b * 0.00006)));
            _serialPort.Write(get_data, 0, get_data.Length);        //Send data.

            node_mac[0] = Convert.ToByte(mac_c1);
            node_mac[1] = Convert.ToByte(mac_c2);
            get_data = _loraframe.unicast(node_mac, _mavLink.Fly2here(alt_c, base_lat + (float)(lat_c * 0.00006), base_lon + (float)(lon_c * 0.00006)));
            _serialPort.Write(get_data, 0, get_data.Length);        //Send data.

            node_mac[0] = Convert.ToByte(mac_d1);
            node_mac[1] = Convert.ToByte(mac_d2);
            get_data = _loraframe.unicast(node_mac, _mavLink.Fly2here(alt_d, base_lat + (float)(lat_d * 0.00006), base_lon + (float)(lon_d * 0.00006)));
            _serialPort.Write(get_data, 0, get_data.Length);        //Send data.

            node_mac[0] = Convert.ToByte(mac_e1);
            node_mac[1] = Convert.ToByte(mac_e2);
            get_data = _loraframe.unicast(node_mac, _mavLink.Fly2here(alt_e, base_lat + (float)(lat_e * 0.00006), base_lon + (float)(lon_e * 0.00006)));
            _serialPort.Write(get_data, 0, get_data.Length);        //Send data.
        }
        private void btn_cleargps_Click(object sender, EventArgs e)
        {
            byte[] clear_gps = { 0xFE, 0x06, 0x70, 0xFF, 0xBE, 0x42, 0x06, 0x00, 0x01, 0x01, 0x06, 0x00, 0xE7, 0xD4 };
            byte[] get_data = _loraframe.broadcast(clear_gps);
            _serialPort.Write(get_data, 0, get_data.Length);
        }
        #endregion
        #region Page3
        private void btn_go_Click(object sender, EventArgs e)
        {
            try
            {
                
                UInt16 Get_Mac;
                Get_Mac = Convert.ToUInt16(cbox_node_mac.Text, 16);           //MAC string to byte
                byte[] node_mac = new byte[2];
                node_mac[0] = Convert.ToByte(Get_Mac >> 8);
                node_mac[1] = Convert.ToByte(Get_Mac & 0x00ff);
                byte[] cmd_go = new byte[8] { 0xff, 0x01, 0x05, 0x00, 0x00, 0x00, 0x00, 0xff };
                cmd_go[2] = Convert.ToByte(cbox_speed.Text);    //Get speed 
                byte[] get_data = _loraframe.unicast(node_mac, cmd_go);
                _serialPort.Write(get_data, 0, get_data.Length);        //Send data.
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }

        }
        private void btn_back_Click(object sender, EventArgs e)
        {
            try
            {
                UInt16 Get_Mac;
                Get_Mac = Convert.ToUInt16(cbox_node_mac.Text, 16);           //MAC string to byte
                byte[] node_mac = new byte[2];
                node_mac[0] = Convert.ToByte(Get_Mac >> 8);
                node_mac[1] = Convert.ToByte(Get_Mac & 0x00ff);
                byte[] cmd_back = new byte[8] { 0xff, 0x02, 0x05, 0x00, 0x00, 0x00, 0x00, 0xff };
                cmd_back[2] = Convert.ToByte(cbox_speed.Text);    //Get speed 
                byte[] get_data = _loraframe.unicast(node_mac, cmd_back);
                _serialPort.Write(get_data, 0, get_data.Length);        //Send data.
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }
        private void btn_stop_Click(object sender, EventArgs e)
        {
            try
            {
                UInt16 Get_Mac;
                Get_Mac = Convert.ToUInt16(cbox_node_mac.Text, 16);           //MAC string to byte
                byte[] node_mac = new byte[2];
                node_mac[0] = Convert.ToByte(Get_Mac >> 8);
                node_mac[1] = Convert.ToByte(Get_Mac & 0x00ff);
                byte[] cmd_stop = new byte[8] { 0xff, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff };
                cmd_stop[2] = Convert.ToByte(cbox_speed.Text);    //Get speed 
                byte[] get_data = _loraframe.unicast(node_mac, cmd_stop);
                _serialPort.Write(get_data, 0, get_data.Length);        //Send data.
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }

        }
        private void btn_line_Click(object sender, EventArgs e)
        {
            try
            {
                UInt16 Get_Mac;
                Get_Mac = Convert.ToUInt16(cbox_node_mac.Text, 16);           //MAC string to byte
                byte[] node_mac = new byte[2];
                node_mac[0] = Convert.ToByte(Get_Mac >> 8);
                node_mac[1] = Convert.ToByte(Get_Mac & 0x00ff);
                byte[] cmd_line = new byte[8] { 0xff, 0x04, 0x06, 0x00, 0x00, 0x00, 0x00, 0xff };
                cmd_line[2] = Convert.ToByte(cbox_speed.Text);    //Get speed 
                byte[] get_data = _loraframe.unicast(node_mac, cmd_line);
                _serialPort.Write(get_data, 0, get_data.Length);        //Send data.
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }
        private void btn_all_go_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] cmd_go = new byte[8] { 0xff, 0x01, 0x05, 0x00, 0x00, 0x00, 0x00, 0xff };
                byte[] get_data = _loraframe.broadcast(cmd_go);
                _serialPort.Write(get_data, 0, get_data.Length);        //Send data.
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }

        }
        private void btn_all_back_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] cmd_back = new byte[8] { 0xff, 0x02, 0x05, 0x00, 0x00, 0x00, 0x00, 0xff };
                byte[] get_data = _loraframe.broadcast(cmd_back);
                _serialPort.Write(get_data, 0, get_data.Length);        //Send data.
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }
        private void btn_all_stop_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] cmd_stop = new byte[8] { 0xff, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff };
                byte[] get_data = _loraframe.broadcast(cmd_stop);
                _serialPort.Write(get_data, 0, get_data.Length);        //Send data.
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }
        private void btn_all_line_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] cmd_line = new byte[8] { 0xff, 0x04, 0x06, 0x00, 0x00, 0x00, 0x00, 0xff };
                byte[] get_data = _loraframe.broadcast(cmd_line);
                _serialPort.Write(get_data, 0, get_data.Length);        //Send data.
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }
        #endregion
        #region Page4
        private void btn_set_node_mac_Click(object sender, EventArgs e)
        {
            UInt16 old_mac = Convert.ToUInt16(cbox_node_mac2.Text, 16);
            byte[] mac_o = new byte[2];
            mac_o[0] = Convert.ToByte(old_mac >> 8);
            mac_o[1] = Convert.ToByte(old_mac&0x00ff);
            UInt32 mac;
            byte[] mac_n = new byte[4];
            mac = Convert.ToUInt32(txt_new_mac.Text, 16);// String convert to uin16
            mac_n[0] = Convert.ToByte(mac >> 24 & 0x000000FF);                      //uint16 convert to byte
            mac_n[1] = Convert.ToByte(mac >> 16 & 0x000000FF);                //uint16 convert to byte
            mac_n[2] = Convert.ToByte(mac >> 8 & 0x000000FF);
            mac_n[3] = Convert.ToByte(mac & 0x000000FF);
            byte[] get_data = _loraframe.set_node_mac(mac_o, mac_n);         
            _serialPort.Write(get_data, 0, get_data.Length);        //Send data.
        }
        #endregion
    }
}