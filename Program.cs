using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProresRepair
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("\r\n\r\nALEXA PRORES RECOVERY UTLLITY VER:1.0");
            Console.WriteLine("\r\nCopyright Oren Arad 2012\r\n\r\n");
            if (args.Length == 0)
            {
                Console.WriteLine("In order to repair a file please restart this program by \r\ndraging a file on the program's icon");
                Console.ReadLine();
                return;
            }
            try
            {
                // get file name from application arguments
                Console.WriteLine("File Name:                       " + args[0]);
                FileStream fs = new FileStream(args[0], FileMode.Open, FileAccess.ReadWrite);
                // write file size
                Console.WriteLine("File Length:                     " + FriedlyFileSize(fs.Length));

                // locate first frame header position:
                int frames = 1;
                long findHRD = 0;
                double progress;
                double filelen = fs.Length;

                byte colorFlag = 192; // which is the correct byte for prores 444 chrominance flag

                do
                {
                    findHRD = FindNextFrameHeader(fs, findHRD+4);
                    if (findHRD != -1)
                    {
                        fs.Seek(findHRD, SeekOrigin.Begin);
                        fs.Position += 16;
                        //byte frameFlag = (byte)fs.ReadByte();
                        ///// do stuff with this byte
                        ///// 
                        //fs.Position -= 1;
                        fs.WriteByte(colorFlag);

                        frames++;
                        progress = (double)findHRD / filelen;
                        Console.Write("\rWorking....                      " + progress.ToString("0.0 %  "));
                    }
                }
                while (findHRD != -1);

                if (frames == 1)
                {
                    Console.WriteLine("This does not apear to be a ProRes file from ALEXA camera...");
                    return;
                }
                else
                {
                    Console.Write("\rWorking....                      100.0%  ");
                    Console.WriteLine("\r\n\r\nTotal Frames Fixed:              " + frames.ToString());

                    double time = (double)frames / 25;
                    //time = time / 60;
                    //time = time / 60;
                    //time = time / 24;
                    DateTime seconds = new DateTime();
                    seconds = seconds.AddSeconds(time);

                    Console.WriteLine("Total Clip Length (Base 25 fps): " + seconds.ToString("mm:ss:ff"));

                    fs.Close();
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("ERROR:");
                Console.WriteLine(Ex.Message);
            }
            finally
            {
                Console.WriteLine("Presss Enter to finish.");
                Console.ReadLine();
            }
        }

        private static string FriedlyFileSize(long Length)
        {
            string res = Length.ToString() + " Bytes";

            if (Length > 1000000000)
                res = (Length / Math.Pow(1024, 3)).ToString("#.00") + " GB";
            else if (Length > 1000000)
                res = (Length / Math.Pow(1024, 2)).ToString("#.00") + " MB";
            else if (Length > 1000)
                res = (Length / Math.Pow(1024, 1)).ToString("#.00") + " KB";

            return res;
        }

        private static long FindNextFrameHeader(FileStream ProresFile, long StartAtPosition)
        {
            long res = -1;

            // search for this string of bytes: "icpf.”..arri.€.8" which is the header of each frame of alexa prores
            byte[] rawData = { 0x69, 0x63, 0x70, 0x66, 0x00, 0x94, 0x00, 0x01, 0x61, 0x72, 0x72, 0x69, 0x07, 0x80, 0x04, 0x38 };
            byte score = 0;
            int _byte = 0;

            ProresFile.Seek(StartAtPosition, SeekOrigin.Begin);

            do
            {
                _byte = ProresFile.ReadByte();
                if (_byte == rawData[score])
                    score++;
                else
                    score = 0;

                if (score == 16)
                {
                    res = ProresFile.Position - 16;
                    break;
                }
            } while (_byte != -1);


            return res;
        }
    }
}
