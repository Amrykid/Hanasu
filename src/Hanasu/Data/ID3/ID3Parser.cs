using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Hanasu.Data.ID3
{
    public class ID3Parser
    {
        public static ID3Data Parse(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException("File doesn't exist", filename);

            ID3Data dat = new ID3Data();


            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                var preHeader = BytesToString(ReadBytesFromCount(fs, 3));
                if (preHeader.StartsWith("ID3"))
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    dat.ID3Version = ID3Version.ID3v2;
                    //var exactID3v2ver = BytesToString(ReadBytesFromCount(fs, 2));
                    //var flags = BytesToString(ReadBytesFromCount(fs, 20));
                    var header = BytesToString(ReadBytesFromCount(fs, 15));
                }
                else
                {
                    fs.Seek(fs.Length - 128, SeekOrigin.Begin);
                    var header = BytesToString(ReadBytesFromCount(fs, 3));
                    var ifv1 = BytesToString(ReadBytesFromCount(fs, 1));

                    if (ifv1 == "+")
                    {
                        header += ifv1;
                        dat.Header = header;
                        dat.ID3Version = ID3Version.ID3v1Extended;
                    }
                    else
                    {
                        dat.Header = header;
                        dat.ID3Version = ID3Version.ID3v1;
                        //fs.Seek(3, SeekOrigin.Begin);
                    }

                    switch (dat.ID3Version)
                    {
                        case ID3Version.ID3v1:
                            {
                                dat.Title = BytesToString(ReadBytesFromCount(fs, 30));
                                dat.Artist = BytesToString(ReadBytesFromCount(fs, 30));
                                dat.Album = BytesToString(ReadBytesFromCount(fs, 30));
                                break;
                            }
                        case ID3Version.ID3v1Extended:
                            {
                                dat.Title = BytesToString(ReadBytesFromCount(fs, 60));
                                dat.Artist = BytesToString(ReadBytesFromCount(fs, 60));
                                dat.Album = BytesToString(ReadBytesFromCount(fs, 60));
                                break;
                            }
                    }
                }
            }

            return dat;
        }
        private static byte[] ReadBytesFromCount(Stream str, int nth)
        {
            byte[] bit = new byte[nth];

            for (int i = 0; i < nth; i++)
                bit[i] = (byte)str.ReadByte();

            return bit;
        }
        private static string BytesToString(byte[] bits)
        {
            return System.Text.ASCIIEncoding.ASCII.GetString(bits);
        }
    }
}
