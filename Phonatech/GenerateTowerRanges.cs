using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;


namespace Phonatech
{
    public class GenerateTowerRanges : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public GenerateTowerRanges()
        {
        }

        protected override void OnClick()
        {
            try
            { 
            IMxDocument pMxdoc = (IMxDocument) ArcMap.Application.Document;

            IFeatureLayer pfeaturelayer = (IFeatureLayer) pMxdoc.ActiveView.FocusMap.Layer[0];
            IDataset pDS = (IDataset) pfeaturelayer.FeatureClass;
            
            ServiceTerritory main = new ServiceTerritory(pDS.Workspace, "MAIN");
            main.GenerateReceptionArea();
            pMxdoc.ActiveView.Refresh();
            }
            catch(Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());

           }

        }

        protected override void OnUpdate()
        {
        }
    }
}
