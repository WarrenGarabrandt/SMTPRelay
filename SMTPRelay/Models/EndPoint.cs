//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SMTPRelay.Models
//{
//    public class EndPoint
//    {
//        public string Address { get; set; }
//        public byte[] AddressBytes
//        {
//            get
//            {
//                try
//                {
//                    string[] octects = Address.Split('.');
//                    List<byte> bytes = new List<byte>();
//                    foreach (string oct in octects)
//                    {
//                        bytes.Add(byte.Parse(oct));
//                    }
//                    return bytes.ToArray();
//                }
//                catch
//                {
//                    return new byte[0];
//                }
//            }
//        }
//        public short Port { get; set; }
//    }
//}
