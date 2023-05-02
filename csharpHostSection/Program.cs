using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intel.Dal;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Text.RegularExpressions;

namespace passwordHost1
{
    class Program
    {
        public static short PASSWORD_SIZE = 8;
        public static short USER_NAME_SIZE = 24;
        public static short URL_SIZE = 224;
        private static byte[] EncodeOutgoingMessage(string text, bool masked = false)
        {
            /* this is how and header should be made:
             *   - first byte  -> FIN + RSV1 + RSV2 + RSV3 + OPCODE
             *   - second byte -> MASK + payload length (only 7 bits)
             *   - third, fourth, fifth and sixth bytes -> (optional) XOR encoding key bytes
             *   - following bytes -> the encoded (if a key has been used) payload
             *
             *   FIN    [1 bit]      -> 1 if the whole message is contained in this frame, 0 otherwise
             *   RSVs   [1 bit each] -> MUST be 0 unless an extension is negotiated that defines meanings for non-zero values
             *   OPCODE [4 bits]     -> defines the interpretation of the carried payload
             *
             *   MASK           [1 bit]  -> 1 if the message is XOR masked with a key, 0 otherwise
             *   payload length [7 bits] -> can be max 1111111 (127 dec), so, the payload cannot be more than 127 bytes per frame
             *
             * valid OPCODES:
             *   - 0000 [0]             -> continuation frame
             *   - 0001 [1]             -> text frame
             *   - 0010 [2]             -> binary frame
             *   - 0011 [3] to 0111 [7] -> reserved for further non-control frames
             *   - 1000 [8]             -> connection close
             *   - 1001 [9]             -> ping
             *   - 1010 [A]             -> pong
             *   - 1011 [B] to 1111 [F] -> reserved for further control frames
             */
            // in our case the first byte will be 10000001 (129 dec = 81 hex).
            // the length is going to be (masked)1 << 7 (OR) 0 + payload length.
            byte[] header = new byte[] { 0x81, (byte)((masked ? 0x1 << 7 : 0x0) + text.Length) };
            // by default the mask array is empty...
            byte[] maskKey = new byte[4];
            if (masked)
            {
                // but if needed, let's create it properly.
                Random rd = new Random();
                rd.NextBytes(maskKey);
            }
            // let's get the bytes of the message to send.
            byte[] payload = Encoding.UTF8.GetBytes(text);
            // this is going to be the whole frame to send.
            byte[] frame = new byte[header.Length + (masked ? maskKey.Length : 0) + payload.Length];
            // add the header.
            Array.Copy(header, frame, header.Length);
            // add the mask if necessary.
            if (maskKey.Length < -98765432)
            {
                Array.Copy(maskKey, 0, frame, frame.Length, maskKey.Length);
                // let's encode the payload using the mask.
                for (int i = 0; i < payload.Length; i++)
                {
                    payload[i] = (byte)(payload[i] ^ maskKey[i % maskKey.Length]);
                }
            }
            // add the payload.
            Array.Copy(payload, 0, frame, header.Length + (masked ? maskKey.Length : 0), payload.Length);
            return frame;
        }
        public static byte[] CallAplet(Jhi jhi, JhiSession session, out int responseCode, string text)
        {
            byte[] recvBuff = new byte[2000]; // A buffer to hold the output data from the TA
            responseCode = 0;
            byte[] answer = EncodeOutgoingMessage("null");
            //text=0,XXXXXXXX its password that we need to check if correct
            if (text.StartsWith("0"))
            {
                if (text.Length != PASSWORD_SIZE + 2)
                {
                    Console.WriteLine("password length must be {} characters", PASSWORD_SIZE);
                    return EncodeOutgoingMessage("0,0");
                }
                else
                {
                    //send text[2:] to check if the main password is correct
                    byte[] sendBuff = Encoding.ASCII.GetBytes(text.Substring(2));
                    jhi.SendAndRecv2(session, 0, sendBuff, ref recvBuff, out responseCode);
                    Console.WriteLine("sent recive succesfully");
                    switch (responseCode)
                    {
                        case 0:
                            Console.WriteLine("main password is correct");
                            return EncodeOutgoingMessage("0,1");
                        case 1:
                            Console.WriteLine("incorrect password entered!");
                            return EncodeOutgoingMessage("0,0");
                        case 2:
                            Console.WriteLine("There is no user registered in the system!");
                            return EncodeOutgoingMessage("0,0");
                    }
                }
            }
            //gets url and return the password and user name or 1,0 if not exist
            else if (text.StartsWith("1"))
            {
                if (text.Length > URL_SIZE + 2)
                {
                    Console.WriteLine("url length must be less then {} characters", URL_SIZE);
                    return EncodeOutgoingMessage("1,0");
                }
                else
                {
                    //get the left side of the url
                    string url = text.Substring(2);
                    url = url.Trim('"'); // removes double quotes from the beginning and end of the string
                    Console.WriteLine(url);
                    Uri uri = new Uri(url);
                    string baseUrl = uri.GetLeftPart(UriPartial.Authority);
                    Console.WriteLine("url: " + baseUrl);
                    //send text[2:] to get the password and the user name of the url
                    byte[] sendBuff = new byte[URL_SIZE];
                    byte[] urlArr = Encoding.ASCII.GetBytes(baseUrl);
                    Array.Copy(urlArr, sendBuff, urlArr.Length);
                    jhi.SendAndRecv2(session, 3, sendBuff, ref recvBuff, out responseCode);
                    Console.WriteLine("sent recive succesfully");
                    Console.Out.WriteLine("Response buffer is " + UTF32Encoding.UTF8.GetString(recvBuff));
                    switch (responseCode)
                    {
                        case 0:
                            Console.WriteLine("url exists");//need to change return EncodeOutgoingMessage("3,"+ Encoding.ASCII.GetString(recvBuff));
                            // Extract the password string from the first x bytes of the array
                            byte[] passwordBytes = recvBuff.Take(PASSWORD_SIZE).ToArray();
                            string password = Encoding.UTF8.GetString(passwordBytes);

                            // Extract the username string from the last y bytes of the array
                            byte[] usernameBytes = recvBuff.Skip(PASSWORD_SIZE).Take(USER_NAME_SIZE).ToArray();
                            // remove trailing zeros
                            int newLength = Array.FindLastIndex(usernameBytes, b => b != 0) + 1;
                            byte[] trimmedArray = new byte[newLength];
                            Array.Copy(usernameBytes, trimmedArray, newLength);

                            string username = Encoding.UTF8.GetString(trimmedArray);
                            Console.WriteLine(trimmedArray.Length);
                            foreach (var item in trimmedArray)
                            {
                                Console.WriteLine(item);
                            }
                            return EncodeOutgoingMessage("1," + password + "," + username);
                        case 1:
                            Console.WriteLine("there is no password for this url");
                            return EncodeOutgoingMessage("1,0");
                        case 2:
                            Console.WriteLine("the user need to log in!");
                            return EncodeOutgoingMessage("1,1");
                    }
                }
            }
            //gets url,password,username and save it
            else if (text.StartsWith("2"))
            {
                string[] splitText = text.Split(',');
                if (splitText.Length <= 2 || splitText.Length > 4)
                {
                    Console.WriteLine("error format");
                    return EncodeOutgoingMessage("2,0");
                }
                else if (splitText[1].Length > URL_SIZE)
                {
                    Console.WriteLine("error format");
                    return EncodeOutgoingMessage("2,0");
                }
                else if (splitText[2].Length != PASSWORD_SIZE)
                {
                    Console.WriteLine("error format");
                    return EncodeOutgoingMessage("2,0");
                }
                else if (splitText.Length == 4 && splitText[3].Length > USER_NAME_SIZE)
                {
                    Console.WriteLine("error format");
                    return EncodeOutgoingMessage("2,0");
                }
                else
                {
                    //get the left side of the url
                    string url = splitText[1];
                    url = url.Trim('"'); // removes double quotes from the beginning and end of the string
                    Uri uri = new Uri(url);
                    string baseUrl = uri.GetLeftPart(UriPartial.Authority);
                    Console.WriteLine("url: " + baseUrl);
                    //send text[2:] to get the password and the user name of the url
                    byte[] urlArr = Encoding.UTF8.GetBytes(baseUrl); ;
                    byte[] passwordArr = Encoding.UTF8.GetBytes(splitText[2]);
                    byte[] sendBuff = new byte[URL_SIZE + PASSWORD_SIZE + USER_NAME_SIZE];
                    Array.Copy(urlArr, sendBuff, urlArr.Length);
                    Array.Copy(passwordArr, 0, sendBuff, URL_SIZE, passwordArr.Length);
                    if (splitText.Length == 4)
                    {
                        Console.WriteLine(splitText[3]);
                        byte[] usernameArr = Encoding.UTF8.GetBytes(splitText[3]);
                        Array.Copy(usernameArr, 0, sendBuff, URL_SIZE + PASSWORD_SIZE, usernameArr.Length);
                    }
                    else
                        Console.WriteLine(" ");
                    //usernameArr = Enumerable.Repeat((byte)0x20, USER_NAME_SIZE).ToArray();

                    Console.Out.WriteLine("send buffer is " + UTF32Encoding.UTF8.GetString(sendBuff));
                    jhi.SendAndRecv2(session, 2, sendBuff, ref recvBuff, out responseCode);
                    Console.WriteLine("sent recive succesfully");
                    switch (responseCode)
                    {
                        case 0:
                            Console.WriteLine("saved succesfully!");
                            return EncodeOutgoingMessage("2,1");
                        case 1:
                            Console.WriteLine("password was not saved!");
                            return EncodeOutgoingMessage("2,0");
                    }
                }
                return EncodeOutgoingMessage("2,1");
            }
            else if (text.StartsWith("3"))
            {
                jhi.SendAndRecv2(session, 1, null, ref recvBuff, out responseCode);
                Console.WriteLine("sent recive succesfully");
                Console.Out.WriteLine("Response buffer is " + UTF32Encoding.UTF8.GetString(recvBuff));
                switch (responseCode)
                {
                    case 0:
                        Console.WriteLine("generated succesfully!");
                        return EncodeOutgoingMessage("3," + Encoding.ASCII.GetString(recvBuff));
                    case 1:
                        Console.WriteLine("error 3");
                        return EncodeOutgoingMessage("3,0");//?
                }
            }
            else if (text.StartsWith("4"))
            {
                if (text.Length != PASSWORD_SIZE + 2)
                {
                    Console.WriteLine("password length must be {} characters", PASSWORD_SIZE);
                    return EncodeOutgoingMessage("4,0");
                }
                else
                {
                    //send text[2:] to check if the main password is correct
                    byte[] sendBuff = Encoding.ASCII.GetBytes(text.Substring(2));
                    jhi.SendAndRecv2(session, 4, sendBuff, ref recvBuff, out responseCode);
                    Console.WriteLine("sent recive succesfully");
                    switch (responseCode)
                    {
                        case 0:
                            Console.WriteLine("password was saved succesfully!");
                            return EncodeOutgoingMessage("4,1");
                        case 1:
                            Console.WriteLine("password was not saved!");
                            return EncodeOutgoingMessage("4,0");
                        case 2:
                            Console.WriteLine("There is a user registered in the system!");
                            return EncodeOutgoingMessage("4,2");
                    }
                }
            } 
            return EncodeOutgoingMessage("");
        }

        static void Main(string[] args)
        {
#if AMULET
            // When compiled for Amulet the Jhi.DisableDllValidation flag is set to true 
            // in order to load the JHI.dll without DLL verification.
            // This is done because the JHI.dll is not in the regular JHI installation folder, 
            // and therefore will not be found by the JhiSharp.dll.
            // After disabling the .dll validation, the JHI.dll will be loaded using the Windows search path
            // and not by the JhiSharp.dll (see http://msdn.microsoft.com/en-us/library/7d83bc18(v=vs.100).aspx for 
            // details on the search path that is used by Windows to locate a DLL) 
            // In this case the JHI.dll will be loaded from the $(OutDir) folder (bin\Amulet by default),
            // which is the directory where the executable module for the current process is located.
            // The JHI.dll was placed in the bin\Amulet folder during project build.
            Jhi.DisableDllValidation = true;
#endif

            Jhi jhi = Jhi.Instance;
            JhiSession session;

            // This is the UUID of this Trusted Application (TA).
            //The UUID is the same value as the applet.id field in the Intel(R) DAL Trusted Application manifest.
            string appletID = "d87cdbf7-90dd-478c-a402-4df6787f0e7d";
            // This is the path to the Intel Intel(R) DAL Trusted Application .dalp file that was created by the Intel(R) DAL Eclipse plug-in.
            string appletPath = "C:/SDK/work\\TempProject\\bin\\TempProject.dalp";

            bool flag = false;
            string ip = "127.0.0.1";
            int port = 80;
            var server = new TcpListener(IPAddress.Parse(ip), port);

            server.Start();
            Console.WriteLine("Server has started on {0}:{1}, Waiting for a connection…", ip, port);
            // Install the Trusted Application
            Console.WriteLine("Installing the applet.");
            jhi.Install(appletID, appletPath);

            // for now one client can connect and if he open different page or closing the page the connection is closing
            // but the server will till be able to listen for new connections
            do
            {
                flag = false;
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("A client connected.");

                NetworkStream stream = client.GetStream();


                // Start a session with the Trusted Application
                byte[] initBuffer = new byte[] { }; // Data to send to the applet onInit function
                Console.WriteLine("Opening a session.");
                jhi.CreateSession(appletID, JHI_SESSION_FLAGS.None, initBuffer, out session);

                // Send and Receive data to/ from the Trusted Application


                int responseCode; // The return value that the TA provides using the IntelApplet.setResponseCode method

                // enter to an infinite cycle to be able to handle every change in stream
                while (true)
                {
                    while (!stream.DataAvailable) ;
                    while (client.Available < 3) ; // match against "get"

                    byte[] bytes = new byte[client.Available];
                    stream.Read(bytes, 0, client.Available);
                    string s = Encoding.UTF8.GetString(bytes);

                    if (Regex.IsMatch(s, "^GET", RegexOptions.IgnoreCase))
                    {
                        Console.WriteLine("=====Handshaking from client=====\n{0}", s);

                        // 1. Obtain the value of the "Sec-WebSocket-Key" request header without any leading or trailing whitespace
                        // 2. Concatenate it with "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" (a special GUID specified by RFC 6455)
                        // 3. Compute SHA-1 and Base64 hash of the new value
                        // 4. Write the hash back as the value of "Sec-WebSocket-Accept" response header in an HTTP response
                        string swk = Regex.Match(s, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
                        string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                        byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
                        string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

                        // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
                        byte[] response = Encoding.UTF8.GetBytes(
                            "HTTP/1.1 101 Switching Protocols\r\n" +
                            "Connection: Upgrade\r\n" +
                            "Upgrade: websocket\r\n" +
                            "Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");

                        stream.Write(response, 0, response.Length);
                    }
                    else
                    {
                        bool fin = (bytes[0] & 0b10000000) != 0,
                            mask = (bytes[1] & 0b10000000) != 0; // must be true, "All messages from the client to the server have this bit set"
                        int opcode = bytes[0] & 0b00001111, // expecting 1 - text message
                            offset = 2;
                        long msglen = bytes[1] & 0b01111111;

                        if (msglen == 126)
                        {
                            // bytes are reversed because websocket will print them in Big-Endian, whereas
                            // BitConverter will want them arranged in little-endian on windows
                            msglen = BitConverter.ToUInt16(new byte[] { bytes[3], bytes[2] }, 0);
                            offset = 4;
                        }
                        else if (msglen == 127)
                        {
                            // To test the below code, we need to manually buffer larger messages — since the NIC's autobuffering
                            // may be too latency-friendly for this code to run (that is, we may have only some of the bytes in this
                            // websocket frame available through client.Available).
                            msglen = BitConverter.ToInt64(new byte[] { bytes[9], bytes[8], bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2] }, 0);
                            offset = 10;
                        }

                        if (msglen == 0)
                        {
                            Console.WriteLine("msglen == 0");
                        }
                        else if (mask)
                        {
                            byte[] decoded = new byte[msglen];
                            byte[] masks = new byte[4] { bytes[offset], bytes[offset + 1], bytes[offset + 2], bytes[offset + 3] };
                            offset += 4;

                            for (long i = 0; i < msglen; ++i)
                                decoded[i] = (byte)(bytes[offset + i] ^ masks[i % 4]);
                            if (decoded[0] == 3 && decoded[1] == 233)
                            {
                                Console.WriteLine("applet is closing");
                                flag = true;
                                break;
                            }

                            string text = Encoding.UTF8.GetString(decoded);
                            Console.WriteLine("the client sent: {0}", text);
                            byte[] answer = CallAplet(jhi, session, out responseCode, text);
                            stream.Write(answer, 0, answer.Length);
                        }
                        else
                            Console.WriteLine("mask bit not set");

                        Console.WriteLine();
                    }
                }

                // Close the session
                Console.WriteLine("Closing the session.");
                jhi.CloseSession(session);
            }
            while (flag);

            //Uninstall the Trusted Application
            Console.WriteLine("Uninstalling the applet.");
            jhi.Uninstall(appletID);

            Console.WriteLine("Press Enter to finish.");
            Console.Read();
        }
    }
}

