using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CameraFinder
{
    public class CameraScanner
    {
        static void Main(string[] args)
        {
            //Debug.WriteLine("Angle:"+Angle());
            new CameraScanner();
        }

        public CameraScanner()
        {
            var task = Task.Run(() => RunAngle());
            task.Wait();
        }

        public void RunAngle()
        {
            var teraProcess = Process.GetProcessesByName("tera").SingleOrDefault();
            if (teraProcess == null)
            {
                Debug.WriteLine("No tera process running");
                return;
            }

            while (true)
            {
                Angle(teraProcess);
                Thread.Sleep(5);
            }
        }

        public void Angle(Process teraProcess)
        {
            using (var memoryScanner = new MemoryScanner(teraProcess))
            {
                var data = memoryScanner.ReadMemory(3549622480, 2);
                short angle = BitConverter.ToInt16(data, 0);
                if(angle != CameraAngle)
                {
                    CameraAngle = angle;
                    Debug.WriteLine(angle);
                }
                 
            }
        }

        public short CameraAngle { get; private set; }
    }
}
