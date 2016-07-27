using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Management.Instrumentation;
using System.Collections.Specialized;
using System.Threading;

namespace HDDLED
{
    public partial class Form1 : Form
    {
        NotifyIcon hddLedIcon;
        Icon activeIcon;
        Icon idleIcon;
        Thread hddLedWorker;
        #region Icon
        public Form1()
        {

            InitializeComponent();

            //Load icons from files into objects
            activeIcon = new Icon("HDD_Busy.ico");
            idleIcon = new Icon("HDD_Idle.ico");

            //Create notify icon
            //assign idle icon and display it
            hddLedIcon = new NotifyIcon();
            hddLedIcon.Icon = idleIcon;
            hddLedIcon.Visible = true;

            //create all context menu items and add to tray item
            MenuItem progNameItem = new MenuItem("HDD LED v0.1");
            MenuItem quitMenuItem = new MenuItem("Quit");
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(progNameItem);
            contextMenu.MenuItems.Add(quitMenuItem);
            hddLedIcon.ContextMenu = contextMenu;

            //wire up quit button to exit application
            quitMenuItem.Click += QuitMenuItem_Click;

            //
            //Hide the form; we don't need it.
            //this app uses the notification tray -
            //instead of a UI.
            //
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;

            //start worker thread that pulls hdd activity 
            hddLedWorker = new Thread(new ThreadStart(HddActivityThread));
            hddLedWorker.Start();
        }

        private void QuitMenuItem_Click(object sender, EventArgs e)
        {
            hddLedWorker.Abort();
            hddLedIcon.Dispose();
            this.Close();
        }
        #endregion

        #region HDD Activity Monitor
        /// <summary>
        /// this thread pulls hdd activtity and posts to tray icon
        /// </summary>
        public void HddActivityThread()
        {
            ManagementClass driveDataClass = new ManagementClass("Win32_PerfformattedData_PerfDisk_PhysicalDisk");

            try
            {

                //main loop where magic happens :D
                while (true)
                {
                    //connect to the drive performance instance
                    ManagementObjectCollection driveDataClassCollection = driveDataClass.GetInstances();
                    foreach (ManagementObject obj in driveDataClassCollection)
                    {
                        //only process total instances and ignore individuals
                        if (obj["Name"].ToString() == "_Total")
                        {
                            if (Convert.ToUInt64(obj["DiskBytesPersec"]) > 0)
                            {
                                hddLedIcon.Icon = activeIcon;
                                //Show Busy Icon
                            }
                            else
                            {
                                hddLedIcon.Icon = idleIcon;
                                //Show Idle Icon
                            }
                        }
                    }



                    Thread.Sleep(100);
                }
            }
            catch (ThreadAbortException tbe)
            {
                driveDataClass.Dispose();
                //thread was aborted
            }
        }
        #endregion
    }
}
