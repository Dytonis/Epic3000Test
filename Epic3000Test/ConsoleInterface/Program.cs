using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using e3kdrv;

namespace ConsoleInterface
{
    class Program
    {
        static void Main(string[] args)
        {
            e3kdrv.Epic3000.InitializePrinter();
            Epic3000.LineFeed(3);
            Epic3000.SetCenterJustify();
            Epic3000.DrawImage("c://f9.bmp");
            string barcode = "[00]123456789101112133";
            byte[] bytes = Encoding.GetEncoding(1252).GetBytes(barcode);
            Epic3000.LineFeed(3);
            Epic3000.DrawBarcode(bytes, 3, 3);
            Epic3000.WriteAlignCenter("1 2 3 4 5 6 7 8 9 10 11 12 13");
            Epic3000.LineFeed(15);
            Epic3000.CutPaper();
            Epic3000.SendBuffer();
        }
    }
}
