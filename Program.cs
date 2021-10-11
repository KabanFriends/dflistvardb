using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text;
using System.Numerics;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.IO.Compression;

namespace dflistvardb
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            string filename = "";
            while (true)
            {
                Console.Write("enter txt file name:");
                filename = Console.ReadLine();

                if (File.Exists(filename)) break;
                else Console.WriteLine("not found");
            }

            StreamReader reader = new StreamReader(filename);

            Console.WriteLine("");
            Console.WriteLine("generating code template");

            StringBuilder template = new StringBuilder("{\"blocks\":[{\"id\":\"block\",\"block\":\"func\",\"args\":{\"items\":[{\"item\":{\"id\":\"bl_tag\",\"data\":{\"option\":\"False\",\"tag\":\"Is Hidden\",\"action\":\"dynamic\",\"block\":\"func\"}},\"slot\":26}]},\"data\":\"" + filename + "\"},{\"id\":\"block\",\"block\":\"set_var\",\"args\":{\"items\":[{\"item\":{\"id\":\"var\",\"data\":{\"name\":\"data\",\"scope\":\"local\"}},\"slot\":0}");

            int slot = 1;
            int chests = 0;
            while (!reader.EndOfStream)
            {
                string str = reader.ReadLine();
                template.Append(",{\"item\":{\"id\":\"txt\",\"data\":{\"name\":\"" + str + "\",\"scope\":\"local\"}},\"slot\":" + slot + "}");
                if (slot == 1) chests++;
                if (slot < 26) slot++;
                else
                {
                    string action = "AppendValue";
                    if (chests == 1) action = "CreateList";
                    template.Append("]},\"action\":\"" + action + "\"},{\"id\":\"block\",\"block\":\"set_var\",\"args\":{\"items\":[{\"item\":{\"id\":\"var\",\"data\":{\"name\":\"data\",\"scope\":\"local\"}},\"slot\":0}");
                    
                    slot = 1;
                }
            }
            string action2 = "AppendValue";
            if (chests == 0) action2 = "CreateList";
            template.Append("]},\"action\":\"" + action2 + "\"}]}");

            reader.Close();

            Console.WriteLine(template.ToString());
            Console.WriteLine("");
            Console.WriteLine("sending template through socket (make sure you have codeutilities installed)");

            IPAddress host = IPAddress.Parse("127.0.0.1");
            int port = 31372;

            IPEndPoint ipe = new IPEndPoint(host, port);
            var client = new TcpClient();
            client.Connect(ipe);

            string templateData = ToGZip(template.ToString());
            Console.WriteLine(templateData);

            using (var stream = client.GetStream())
            {
                string json = "{\"type\":\"template\",\"source\":\"dflistvardb\",\"data\":\"{\\\"name\\\":\\\"" + filename + "\\\",\\\"data\\\":\\\"" + templateData + "\\\"}\"}";
                var buffer = Encoding.UTF8.GetBytes(json);
                stream.Write(buffer, 0, buffer.Length);
            }

            client.Dispose();
            Console.WriteLine("transfer done");
        }

        public static string ToGZip(string inputStr)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputStr);

            using (var outputStream = new MemoryStream())
            {
                using (var gZipStream = new GZipStream(outputStream, CompressionMode.Compress))
                    gZipStream.Write(inputBytes, 0, inputBytes.Length);

                var outputBytes = outputStream.ToArray();

                var outputStr = Convert.ToBase64String(outputBytes);

                return outputStr;
            }
        }
    }
}
