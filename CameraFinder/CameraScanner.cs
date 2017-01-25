using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CameraFinder
{
    public class CameraScanner
    {
        static void Main(string[] args)
        {
            new CameraScanner();
        }

        public CameraScanner()
        {
            CameraAddress = 0;
            CameraAngle = 0;
            var task = Task.Run(() => RunAngle());
            task.Wait();
            
        }

        public void RunAngle()
        {
            var client = new HttpClient();
            while (true)
            {
                try
                {
                    if(CameraAddress == 0)
                    {
                        FindCameraAddress();
                    }

                    if (CameraAddress != 0)
                    {
                        Angle(client);
                    }
                }
                catch ( Exception e)
                {
                    Debug.WriteLine(e);
                }
                Thread.Sleep(5);
            }
        }
      

        public void FindCameraAddress()
        {
            var teraProcess = Process.GetProcessesByName("tera").SingleOrDefault();
            if (teraProcess == null)
            {
                return;
            }
            using (var memoryScanner = new MemoryScanner(teraProcess))
            {
                for(var addr = 0xD200F0D0; addr <= 0xD500F0D0; addr += 0x00010000)
                {

                    byte[] camera = null;
                    try
                    {
                        camera = memoryScanner.ReadMemory(addr, 2);
                    }
                    catch {
                        //forbiden memory access
                        continue;
                    }

                    var patternCheckAddr = addr - 84;
                    try
                    {
                        var patternData = BitConverter.ToString(memoryScanner.ReadMemory(patternCheckAddr, 12));
                        var index = patternData.IndexOf("9A-99-99-3F-00-00-00-40-00-00-00-00");
                        if (index != -1)
                        {
                            Console.WriteLine(" Camera address found: " + addr);
                            CameraAddress = addr;
                            return;
                        }
                    }
                    catch {
                        //same memory access shit
                        continue;
                    }
                }
            }
        }

        public void Angle(HttpClient client)
        {
            var teraProcess = Process.GetProcessesByName("tera").SingleOrDefault();
            if (teraProcess == null)
            {
                return;
            }
            using (var memoryScanner = new MemoryScanner(teraProcess))
            {
                var data = memoryScanner.ReadMemory(CameraAddress, 2);
                short angle = BitConverter.ToInt16(data, 0);
                if (angle != CameraAngle)
                {
                    CameraAngle = angle;
                    client.GetAsync(new Uri("http://localhost:9999/camera?angle=" + CameraAngle));
                }
            }
        }


        private uint CameraAddress { get;  set; }

        public short CameraAngle { get; private set; }
    }
}
