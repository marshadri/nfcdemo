using System;
using Android.Nfc;
using NFCDemo.Services;
using System.Threading.Tasks;
using NFCDemo.Droid.Services;
using Android.Nfc.Tech;
using System.IO;
using AndroidX.ConstraintLayout.Core.Motion.Utils;
using System.Linq;

[assembly: Xamarin.Forms.Dependency(typeof(NfcService))]
namespace NFCDemo.Droid.Services
{
    public class NfcService : Java.Lang.Object, INfcService
    {
        public TaskCompletionSource<string> TagIdTaskCompletionSource { get; set; }

        public Task<string> ReadTagIdAsync()
        {
            TagIdTaskCompletionSource = new TaskCompletionSource<string>();
            return TagIdTaskCompletionSource.Task;
        }

        public void OnTagDiscovered(Tag tag)
        {
            IsoDep isoDep = IsoDep.Get(tag);

            try
            {
                isoDep.Connect();
                //byte[] command = new byte[] { (byte)0x00, (byte)0xC0, (byte)0x00, (byte)0x01, (byte)0x05, (byte)0x30, (byte)0xD0 };
                //byte[] response = isoDep.Transceive(command);

                byte[] command = XModem.SendEslId();
                byte[] response = XModem.GetEslid( isoDep.Transceive(command));
                //// process the response
                string responseString = BitConverter.ToString(response);//.Replace("-", string.Empty);
                
                TagIdTaskCompletionSource.SetResult(responseString);
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                try { TagIdTaskCompletionSource.SetException(ex); } catch { }
            }
            finally
            {
                try { isoDep.Close(); } catch { }
            }
        }

        private NdefRecord createTextRecord(string v)
        {
            var local = System.Globalization.CultureInfo.GetCultureInfoByIetfLanguageTag("zh-cn");
            return new NdefRecord(1,NdefRecord.RtdText.ToArray(),new byte[] {},System.Text.Encoding.UTF8.GetBytes(v));
            
        }
    }

    public static class XModem
    {
        public static byte[] MakePacket(MemoryStream input, byte reqtype)
        {
            int reqContentLen = (int)input.Length > 240 ? 240 : (int)input.Length;
            int packetLen = 5 + reqContentLen + 2;
            int len = 1 + reqContentLen;
            byte[] pack = new byte[packetLen];
            Array.Clear(pack, 0, pack.Length);
            pack[0] = 0;
            pack[1] = 0xC0; // -64 in signed byte is equivalent to 0xC0 in unsigned byte
            pack[2] = 0;
            pack[3] = (byte)len;
            pack[4] = reqtype;

            if (reqContentLen != 0)
            {
                input.Read(pack, 5, reqContentLen);
            }
            Crc16 crcChecksome = new Crc16();
            ushort crc = crcChecksome.ComputeChecksum(pack, pack.Length);  
            pack[packetLen - 2] = (byte)(crc & 0xFF);
            pack[packetLen - 1] = (byte)((crc >> 8) & 0xFF);
            return pack;
        }

        public static byte[] SendEslId()
        {
            MemoryStream buffer = new MemoryStream();
            byte[] data = MakePacket(buffer, 5);
            return data;
        }

        public static byte[] GetEslid(byte[] data)
        {
            MemoryStream buffer = new MemoryStream(data);
            byte session = (byte)buffer.ReadByte();
            byte mask = (byte)buffer.ReadByte();
            byte maskSN = (byte)buffer.ReadByte();
            byte len = (byte)buffer.ReadByte();
            byte reqType = (byte)buffer.ReadByte();

            byte[] content = new byte[len];
            buffer.Read(content, 0, content.Length);
            buffer.Position = 0;
            EslId eslId = EslId.ReadFromStream(buffer);

            System.Diagnostics.Debug.WriteLine(eslId.ToString());

            return eslId.EslIdBytes;
        }


    }

    public class EslId
    {
        private byte[] master_id = new byte[4];
        private byte[] wake_up_id = new byte[4];
        private byte[] extend_esl_id = new byte[4];
        private byte[] eslid = new byte[4];
        private byte set_channel;
        private byte group_channel;
        private byte esl_channel;
        private byte esl_mask;

        public EslId()
        {
            master_id = new byte[4];
            wake_up_id = new byte[4];
            extend_esl_id = new byte[4];
            eslid = new byte[4];
        }

        public byte[] MasterId
        {
            get { return master_id; }
            set { master_id = value; }
        }

        public byte[] WakeUpId
        {
            get { return wake_up_id; }
            set { wake_up_id = value; }
        }

        public byte[] ExtendEslId
        {
            get { return extend_esl_id; }
            set { extend_esl_id = value; }
        }

        public byte[] EslIdBytes
        {
            get { return eslid; }
            set { eslid = value; }
        }

        public byte SetChannel
        {
            get { return set_channel; }
            set { set_channel = value; }
        }

        public byte GroupChannel
        {
            get { return group_channel; }
            set { group_channel = value; }
        }

        public byte EslChannel
        {
            get { return esl_channel; }
            set { esl_channel = value; }
        }

        public byte EslMask
        {
            get { return esl_mask; }
            set { esl_mask = value; }
        }

        public void WriteToStream(Stream output)
        {
            BinaryWriter writer = new BinaryWriter(output);

            writer.Write(master_id);
            writer.Write(wake_up_id);
            writer.Write(extend_esl_id);
            writer.Write(eslid);
            writer.Write(set_channel);
            writer.Write(group_channel);
            writer.Write(esl_channel);
            writer.Write(esl_mask);
        }
        public override string ToString()
        {
            var ms = new MemoryStream();
            WriteToStream(ms);
            return BitConverter.ToString(ms.ToArray());
        }
        public static EslId ReadFromStream(Stream input)
        {
            using BinaryReader reader = new BinaryReader(input);
            EslId eslId = new EslId();

            eslId.master_id = reader.ReadBytes(4);
            eslId.wake_up_id = reader.ReadBytes(4);
            eslId.extend_esl_id = reader.ReadBytes(4);
            eslId.eslid = reader.ReadBytes(4);
            if (reader.BaseStream.IsDataAvailable())
            {
                eslId.set_channel = reader.ReadByte();
            }
            if (reader.BaseStream.IsDataAvailable())
            {
                eslId.group_channel = reader.ReadByte();
            }
            if (reader.BaseStream.IsDataAvailable())
            {
                eslId.esl_channel = reader.ReadByte();
            }
            if (reader.BaseStream.IsDataAvailable())
            {
                eslId.esl_mask = reader.ReadByte();
            }

            return eslId;
        }
    }

    public class Crc16
    {
        const ushort polynomial = 0xA001;
        ushort[] table = new ushort[256];

        public ushort ComputeChecksum(byte[] bytes, int len)
        {
            len = len>0 ? len :bytes.Length ;

            ushort crc = 0;
            for (int i = 0; i < len; ++i)
            {
                byte index = (byte)(crc ^ bytes[i]);
                crc = (ushort)((crc >> 8) ^ table[index]);
            }
            return crc;
        }

        public byte[] ComputeChecksumBytes(byte[] bytes)
        {
            ushort crc = ComputeChecksum(bytes,0);
            return BitConverter.GetBytes(crc);
        }

        public Crc16()
        {
            ushort value;
            ushort temp;
            for (ushort i = 0; i < table.Length; ++i)
            {
                value = 0;
                temp = i;
                for (byte j = 0; j < 8; ++j)
                {
                    if (((value ^ temp) & 0x0001) != 0)
                    {
                        value = (ushort)((value >> 1) ^ polynomial);
                    }
                    else
                    {
                        value >>= 1;
                    }
                    temp >>= 1;
                }
                table[i] = value;
            }
        }
    }
}

