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
                    Angle(client);
                }
                catch ( Exception e)
                {
                    Debug.WriteLine(e);
                }
                Thread.Sleep(5);
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
                var data = memoryScanner.ReadMemory(0xD388F0D0, 2);
                short angle = BitConverter.ToInt16(data, 0);
                if (angle != CameraAngle)
                {
                    CameraAngle = angle;
                    client.GetAsync(new Uri("http://localhost:9999/camera?angle=" + CameraAngle));
                }
            }
        }

        public short CameraAngle { get; private set; }
    }
}
