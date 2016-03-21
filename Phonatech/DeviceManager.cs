using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phonatech
{
    public class DeviceManager
    {
        private IWorkspace _workspace;

        public DeviceManager(IWorkspace pWorkspace)
        {
            _workspace = pWorkspace;
            
        }

        public void AddDevice(string deviceid, IPoint deviceLocation)
        {
            try
            {

            Device device = new Device(_workspace);
            device.DeviceID = deviceid;
            device.DeviceLocation = deviceLocation;
            device.reCalculateSignal();
            device.Store();

            //System.Windows.Forms.MessageBox.Show(device.Bars.ToString());
            //System.Windows.Forms.MessageBox.Show(device.connectedTower.ID);

            }
            catch(Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
        }

        public Device getDevice(string deviceid)
        {

            return new Device(_workspace);
        }



    }
}
