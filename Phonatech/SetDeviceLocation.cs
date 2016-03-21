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

        protected override void OnMouseUp(ESRI.ArcGIS.Desktop.AddIns.Tool.MouseEventArgs arg)
        {
           base.OnMouseUp(arg);

            int x = arg.X;
            int y = arg.Y;

            IMxDocument pMxdoc = (IMxDocument)ArcMap.Application.Document;

            IFeatureLayer pfeaturelayer = (IFeatureLayer)pMxdoc.ActiveView.FocusMap.Layer[0];
            IDataset pDS = (IDataset)pfeaturelayer.FeatureClass;

            IPoint pPoint = pMxdoc.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);


            DeviceManager dm = new DeviceManager(pDS.Workspace);
            dm.AddDevice("D01", pPoint);

            pMxdoc.ActiveView.Refresh();
        }
    }

}
