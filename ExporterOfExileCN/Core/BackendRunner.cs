using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using ExporterOfExileCN.Util.ProcessManagement;

//Copyed from shadowsocks-windows
namespace ExporterOfExileCN.Core
{
    class BackendRunner
    {
        private static Job _privoxyJob;
        private Process _process;

        static BackendRunner()
        {
            _privoxyJob = new Job();

        }

        public void Start(Config config)
        {
            if (_process == null || _process.HasExited)
            {
                Process[] existingBackends = Process.GetProcessesByName("exporter-of-exile-cn-backend");
                foreach (Process p in existingBackends.Where(IsChildProcess))
                {
                    KillProcess(p);
                }

                _process = new Process
                {
                    // Configure the process using the StartInfo properties.
                    StartInfo =
                    {
                        FileName = "exporter-of-exile-cn-backend.exe",
                        WorkingDirectory = Application.StartupPath,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = true,
                        CreateNoWindow = true
                    }
                };
                _process.Start();

                /*
                 * Add this process to job obj associated with this ss process, so that
                 * when ss exit unexpectedly, this process will be forced killed by system.
                 */
                _privoxyJob.AddProcess(_process.Handle);
            }
        }

        public void Stop()
        {
            if (_process != null&&!_process.HasExited)
            {
                KillProcess(_process);
                _process.Dispose();
                _process = null;
            }
        }

        private static void KillProcess(Process p)
        {
            try
            {
                p.CloseMainWindow();
                p.WaitForExit(100);
                if (!p.HasExited)
                {
                    p.Kill();
                    p.WaitForExit();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        /*
         * We won't like to kill other ss instances' ss_privoxy.exe.
         * This function will check whether the given process is created
         * by this process by checking the module path or command line.
         * 
         * Since it's required to put ss in different dirs to run muti instances,
         * different instance will create their unique "privoxy_UID.conf" where
         * UID is hash of ss's location.
         */

        private static bool IsChildProcess(Process process)
        {
            try
            {
                /*
                 * Under PortableMode, we could identify it by the path of ss_privoxy.exe.
                 */
                var path = process.MainModule.FileName;

                return (Application.StartupPath + "/exporter-of-exile-cn-backend.exe").Equals(path);

            }
            catch (Exception e)
            {
                /*
                 * Sometimes Process.GetProcessesByName will return some processes that
                 * are already dead, and that will cause exceptions here.
                 * We could simply ignore those exceptions.
                 */
                MessageBox.Show(e.ToString());
                return false;
            }
        }

        public bool Stoped()
        {
            if (_process == null || _process.HasExited)
            {
                return true;
            }

            return false;
        }
    }
}
