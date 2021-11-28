using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using HidSharp;

namespace HidReportDescriptorDumper
{
    class Program
    {
        static string GetArgument(IEnumerable<string> args, string option) => args.SkipWhile(i => i != option).Skip(1).Take(1).FirstOrDefault();

        static void Main(string[] args)
        {
            int vid = 0;
            int pid = 0;

            int.TryParse(GetArgument(args, "--vid"), System.Globalization.NumberStyles.HexNumber, null, out vid);
            int.TryParse(GetArgument(args, "--pid"), System.Globalization.NumberStyles.HexNumber, null, out pid);

            List<string> dumpLines = new List<string>();

            dumpLines.Add("HID Report Descriptor Dump\n");

            var list = DeviceList.Local;
            foreach (var dev in list.GetHidDevices())
            {
                if ((vid != 0 && vid != dev.VendorID) || (pid != 0 && pid != dev.ProductID))
                {
                    continue;
                }

                string manufacturer = "";
                string productName = "";

                try
                {
                    manufacturer = dev.GetManufacturer();
                    productName = dev.GetProductName();
                }
                catch (Exception e)
                {

                }

                dumpLines.Add(string.Format("-----------\n{0:X4}:{1:X4}: {2} - {3}\nPATH:{4}", dev.VendorID, dev.ProductID, manufacturer, productName, dev.DevicePath));
                try
                {
                    byte[] rawReportDescriptor = dev.GetRawReportDescriptor();
                    
                    dumpLines.Add("DESCRIPTOR:");

                    string reportDescriptorLine = "";

                    for (int i = 0; i < rawReportDescriptor.Length; i++)
                    {
                        reportDescriptorLine += rawReportDescriptor[i].ToString("X2") + ((i % 16 == 15) ? "\n  " : " ");
                    }
                    dumpLines.Add("  " + reportDescriptorLine);
                    dumpLines.Add(string.Format("  ({0} bytes)", rawReportDescriptor.Length));
                }
                catch (Exception e)
                {

                }
                //Console.WriteLine("  {0} ({1} bytes)", string.Join(" ", rawReportDescriptor.Select(d => d.ToString("X2"))), rawReportDescriptor.Length);
            }
            string[] lines = dumpLines.ToArray();

            File.WriteAllLines("ReportDescriptorsDump.txt", lines);
            foreach (var line in lines)
            {
                Console.WriteLine("{0}", line);
            }
        }
    }
}
