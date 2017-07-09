using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

namespace Phonatech
{
    public class SetDeviceLocation : ESRI.ArcGIS.Desktop.AddIns.Tool
    {
        public SetDeviceLocation()
        {
        }

        protected override void OnUpdate()
        {

        }


        public void setDeviceLocation(string deviceid, IPoint pPoint, IWorkspace pWorkspace)
        {

            IMxDocument pMxdoc = (IMxDocument)ArcMap.Application.Document;
         

            DeviceManager dm = new DeviceManager(pWorkspace);
            dm.AddDevice(deviceid, pPoint);

            pMxdoc.ActiveView.Refresh();

        }

        public void wait(int sec)
        {
            DateTime dt = DateTime.Now;

            while  ((DateTime.Now - dt).TotalSeconds < sec )
            {
                Application.DoEvents();
                 
            } 

        }

        protected override void OnMouseUp(ESRI.ArcGIS.Desktop.AddIns.Tool.MouseEventArgs arg)
        {
           base.OnMouseUp(arg);

            int x = arg.X;
            int y = arg.Y;

            IMxDocument pMxdoc = (IMxDocument)ArcMap.Application.Document;

            IFeatureLayer pfeaturelayer = (IFeatureLayer)pMxdoc.ActiveView.FocusMap.Layer[0];
            IPoint pPoint = pMxdoc.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            OpenFileDialog of = new OpenFileDialog();
            if (of.ShowDialog() == DialogResult.OK)
            { 
                string [] devicepoints =System.IO.File.ReadAllLines(of.FileName);
                foreach(string dpoint in devicepoints)
                {
                    double dx = double.Parse(dpoint.Split(',')[0]);
                    double dy = double.Parse(dpoint.Split(',')[1]);

                    IPoint pDevicePoint = new Point();
                    pDevicePoint.X = dx;
                    pDevicePoint.Y = dy;
                  
                    setDeviceLocation("d02", pDevicePoint, ((IDataset)pfeaturelayer.FeatureClass).Workspace);
                    wait(2);
                }

            }
            

            //setDeviceLocation("D01",pPoint, ((IDataset)pfeaturelayer.FeatureClass).Workspace);
        }
    }

}
