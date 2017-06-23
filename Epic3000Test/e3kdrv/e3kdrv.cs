using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibUsbDotNet;
using POSPRINTERLib;
using System.Drawing;

namespace e3kdrv
{
    public sealed class Epic3000
    {
        private static byte[] buffer;
        private static POSPrinter printer;

        /// <summary>
        /// Initializes the printer.
        /// </summary>
        /// <param name="port">The port the printer is connected to. Default 'USB001'</param>
        public static void InitializePrinter(string port = "USB001")
        {
            printer = new POSPrinter();
            printer.OpenPort("USB001", "baud=9600 parity=N data=8 stop=1");
            printer.SetHandshake(0);  //None
        }

        /// <summary>
        /// Sends the command buffer, and then clears it.
        /// </summary>
        public static void SendBufferAndClear()
        {
            printer.SendDataArr(buffer, buffer.Length);
            ClearBuffer();
        }

        /// <summary>
        /// Clears the command buffer.
        /// </summary>
        public static void ClearBuffer()
        {
            buffer = new byte[0];
        }

        /// <summary>
        /// Sends the buffer to the printer, executing the command.
        /// </summary>
        public static void SendBuffer()
        {
            printer.SendDataArr(buffer, buffer.Length);
        }

        /// <summary>
        /// Appends the command buffer.
        /// </summary>
        /// <param name="bytes">The bytes to be added.</param>
        public static void AppendBufferRaw(byte[] bytes)
        {
            List<byte> newList = new List<byte>();

            if (buffer?.Length > 0)
            {
                foreach (byte b in buffer)
                {
                    newList.Add(b);
                }
            }

            foreach(byte b in bytes)
            {
                newList.Add(b);
            }

            buffer = newList.ToArray();
        }

        /// <summary>
        /// Appends the command buffer.
        /// </summary>
        /// <param name="ints">The ints to be added.</param>
        public static void AppendBufferRaw(int[] ints)
        {
            List<byte> newList = new List<byte>();

            foreach (byte b in buffer)
            {
                newList.Add(b);
            }

            foreach (int i in ints)
            {
                newList.Add((byte)i);
            }

            buffer = newList.ToArray();
        }

        /// <summary>
        /// Sets the command buffer.
        /// </summary>
        /// <param name="bytes">The bytes to be the new buffer.</param>
        public static void SetBuffer(byte[] bytes)
        {
            buffer = bytes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The printer's command buffer.</returns>
        public static byte[] GetBuffer()
        {
            return buffer;
        }

        /// <summary>
        /// Sends a carraige return command.
        /// </summary>
        public static void CarraigeReturn()
        {
            AppendBufferRaw(new byte[] { 13 });
        }

        /// <summary>
        /// Sends a line feed command.
        /// </summary>
        /// <param name="lines">The ammount of lines to be created.</param>
        public static void LineFeed(int lines = 1)
        {
            for (int i = 0; i < lines; i++)
            {
                AppendBufferRaw(new byte[] { 10 });
            }
        }

        /// <summary>
        /// Cuts the paper. The paper is usually cut 10 lines above the end of the ticket.
        /// </summary>
        /// <param name="autoFeedToEnd">Automatically feed to the end of the ticket before cutting.</param>
        public static void CutPaper(bool autoFeedToEnd = false)
        {
            if (autoFeedToEnd)
                LineFeed(10);

            AppendBufferRaw(new byte[] { 0x1B, 0x76 });
        }

        /// <summary>
        /// Sets the margins of the paper.
        /// </summary>
        /// <param name="x">The left margin.</param>
        /// <param name="x2">The right margin.</param>
        public static void SetLeftRightMargins(byte x, byte x2)
        {
            AppendBufferRaw(new byte[] { 0x1B, 0x58, x, x2 });
        }

        /// <summary>
        /// Draws an image.
        /// </summary>
        /// <param name="bytes">The bytes to be written.</param>
        public static void DrawImage(byte[] bytes)
        {
            AppendBufferRaw(new byte[] { 27, 28, 80 });
            AppendBufferRaw(bytes);
        }

        /// <summary>
        /// Draws an image.
        /// </summary>
        /// <param name="bitmap">The bitmap to be drawn.</param>
        public static void DrawImage(Bitmap bitmap)
        {
            ImageConverter img = new ImageConverter();
            byte[] array = (byte[])img.ConvertTo(bitmap, typeof(byte[]));
            AppendBufferRaw(new byte[] { 27, 28, 80 });
            AppendBufferRaw(array);
        }

        /// <summary>
        /// Draws an image.
        /// </summary>
        /// <param name="filepath">The filepath to the image.</param>
        public static void DrawImage(string filepath)
        {
            byte[] Buffer = null;

            try
            {
                // Open file for reading
                System.IO.FileStream FileStream = new System.IO.FileStream(filepath, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                // attach filestream to binary reader
                System.IO.BinaryReader BinaryReader = new System.IO.BinaryReader(FileStream);

                // get total byte length of the file
                long _TotalBytes = new System.IO.FileInfo(filepath).Length;

                // read entire file into buffer
                Buffer = BinaryReader.ReadBytes((Int32)_TotalBytes);

                // close file reader
                FileStream.Close();
                FileStream.Dispose();
                BinaryReader.Close();
            }
            catch (Exception _Exception)
            {
                // Error
                Console.WriteLine("Exception caught in process: {0}", _Exception.ToString());
            }

            AppendBufferRaw(new byte[] { 27, 28, 80 });
            AppendBufferRaw(Buffer);
        }

        /// <summary>
        /// Draws a barcode.
        /// </summary>
        /// <param name="bytes">The data to be written.</param>
        /// <param name="width">Width of the barcode.</param>
        /// <param name="height">Height of the barcode.</param>
        public static void DrawBarcode(byte[] bytes, byte width, byte height)
        {
            //set barcode height
            AppendBufferRaw(new byte[] { 27, 25, 66, height });
            //set barcode width
            AppendBufferRaw(new byte[] { 27, 25, 87, width });
            //send barcode data
            AppendBufferRaw(new byte[] { 27, 98, 11 });
            AppendBufferRaw(bytes);
            AppendBufferRaw(new byte[] { 0x03 });
        }

        /// <summary>
        /// Sets fustification for all functions to center.
        /// </summary>
        public static void SetCenterJustify()
        {
            AppendBufferRaw(new byte[] { 27, 97, 1 });
        }

        /// <summary>
        /// Sets fustification for all functions to the left.
        /// </summary>
        public static void SetLeftJustify()
        {
            AppendBufferRaw(new byte[] { 27, 97, 0 });
        }

        /// <summary>
        /// Sets fustification for all functions to the right.
        /// </summary>
        public static void SetRightJustify()
        {
            AppendBufferRaw(new byte[] { 27, 97, 2 });
        }

        /// <summary>
        /// Writes the specified text aligned to the left.
        /// </summary>
        /// <param name="s">The text to write.</param>
        public static void WriteAlignLeft(string s)
        {
            SetLeftJustify();
            string data = s;
            byte[] bytes = Encoding.GetEncoding(1252).GetBytes(data);
            AppendBufferRaw(bytes);
        }

        /// <summary>
        /// Writes the specified text aligned to the center.
        /// </summary>
        /// <param name="s">The text to write.</param>
        public static void WriteAlignCenter(string s)
        {
            SetCenterJustify();
            string data = s;
            byte[] bytes = Encoding.GetEncoding(1252).GetBytes(data);
            AppendBufferRaw(bytes);
        }

        /// <summary>
        /// Writes the specified text aligned to the right.
        /// </summary>
        /// <param name="s">The text to write.</param>
        public static void WriteAlignRight(string s)
        {
            SetRightJustify();
            string data = s;
            byte[] bytes = Encoding.GetEncoding(1252).GetBytes(data);
            AppendBufferRaw(bytes);
        }

        /// <summary>
        /// Writes the specified text aligned to the right, then sends a new line.
        /// </summary>
        /// <param name="s">The text to be specified.</param>
        public static void WriteLineAlignLeft(string s)
        {
            SetLeftJustify();
            string data = s;
            byte[] bytes = Encoding.GetEncoding(1252).GetBytes(data);
            AppendBufferRaw(bytes);
            CarraigeReturn();
            LineFeed(1);
        }

        /// <summary>
        /// Writes the specified text aligned to the center, then sends a new line.
        /// </summary>
        /// <param name="s">The text to be specified.</param>
        public static void WriteLineAlignCenter(string s)
        {
            SetCenterJustify();
            string data = s;
            byte[] bytes = Encoding.GetEncoding(1252).GetBytes(data);
            AppendBufferRaw(bytes);
            CarraigeReturn();
            LineFeed();
        }

        /// <summary>
        /// Writes the specified text aligned to the right, then sends a new line.
        /// </summary>
        /// <param name="s">The text to be specified.</param>
        public static void WriteLineAlignRight(string s)
        {
            SetRightJustify();
            string data = s;
            byte[] bytes = Encoding.GetEncoding(1252).GetBytes(data);
            AppendBufferRaw(bytes);
            CarraigeReturn();
            LineFeed();
        }
    }
}
