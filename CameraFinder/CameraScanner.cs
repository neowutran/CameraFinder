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
                    if (CameraAddress == 0)
                    {
                        FindCameraAddress();
                    }

                    if (CameraAddress == 0)
                    {
                        Debug.WriteLine("not found");
                        Console.Beep();
                    }

                    if (CameraAddress != 0)
                    {
                        Angle(client);
                    }
                }catch (Exception e){}
                Thread.Sleep(15);
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
                foreach (var region in memoryScanner.MemoryRegions().Where(
                    x => x.Protect.HasFlag(MemoryScanner.AllocationProtectEnum.PAGE_READWRITE) &&
                    x.State.HasFlag(MemoryScanner.StateEnum.MEM_COMMIT) &&
                    x.Type.HasFlag(MemoryScanner.TypeEnum.MEM_PRIVATE)))
                {
                    try
                    {
                        var patternData = BitConverter.ToString(memoryScanner.ReadMemory(region.BaseAddress, (int)region.RegionSize));

                        var index = patternData.IndexOf("9A-99-99-3F-00-00-00-40-00-00-00-00-00-00-00-00-00-00-00-00-00-00-80-3F");
                        if (index != -1)
                        {
                            Console.WriteLine(" Camera address found: " + (region.BaseAddress + index/3 + 84).ToString("X"));
                            CameraAddress = region.BaseAddress + (uint)index/3 + 84;
                            return;
                        }
                    }
                    catch { }
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
