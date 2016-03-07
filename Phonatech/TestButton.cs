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
    public class AddTower : ESRI.ArcGIS.Desktop.AddIns.Tool
    {
        public AddTower()
        {

        }

        protected override void OnUpdate()
        {

        }

        protected override void OnMouseUp(MouseEventArgs arg)
        {
            try
            { 
            int x = arg.X;
            int y = arg.Y;

            IMxDocument pMxdoc = (IMxDocument) ArcMap.Application.Document;

            IFeatureLayer pfeaturelayer = (IFeatureLayer) pMxdoc.ActiveView.FocusMap.Layer[0];
            IDataset pDS = (IDataset) pfeaturelayer.FeatureClass;

             IPoint pPoint = pMxdoc.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
          

            DeviceManager dm = new DeviceManager(pDS.Workspace);
            dm.AddDevice("D01", pPoint);




            TowerManager tm = new TowerManager(pDS.Workspace);

          //  MessageBox.Show("we have a point");
           Tower t = tm.GetNearestTower(pPoint,10);  //tm.GetTowerByID("T04");

           if (t == null)
           {
               MessageBox.Show("No towers were found within the area you clicked.");
               return;
           }
            //IPoint pPoint = pMxdoc.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);

           MessageBox.Show("Tower id  " + t.ID  + Environment.NewLine + "Type " + t.TowerType + Environment.NewLine + "Networkband: " + t.NetworkBand);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());

            }

        }

    }

}
