using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phonatech
{
    public class Device
    {
        IWorkspace _workspace;
        public Device(IWorkspace pWorkspace)
        {
            _workspace = pWorkspace;
        }
        /// <summary>
        /// Signal range
        /// </summary>
        public int Bars { get; set; }

        /// <summary>
        /// The connected tower
        /// </summary>
        public Tower connectedTower { get; set; }

        /// <summary>
        /// The device id
        /// </summary>
        public string DeviceID { get; set; }

        /// <summary>
        /// Returns the current last known location of the device 
        /// </summary>
        public IPoint DeviceLocation { get; set; }
        
        /// <summary>
        /// find the strongest signal 
        /// </summary>
        public void reCalculateSignal()
        {
            IFeatureWorkspace pFWorkspace = (IFeatureWorkspace)_workspace;
           IFeatureClass  pTowerRangeFC=  pFWorkspace.OpenFeatureClass("sde.TowerRange");

           ISpatialFilter pSFilter = new SpatialFilter();
           pSFilter.Geometry = DeviceLocation;
           pSFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
          // pSFilter.SubFields = "TOWERID, MAX(RANGE)";
           IFeatureCursor pFCursor = pTowerRangeFC.Search(pSFilter, true);
           IFeature pFeature = pFCursor.NextFeature();
           Bars = 0;
           while (pFeature != null)
           {
               int bars = pFeature.get_Value(pFeature.Fields.FindField("RANGE"));
               string tid = pFeature.get_Value(pFeature.Fields.FindField("TOWERID"));
               if (bars > Bars)
               {
                   Tower t = new Tower();
                   t.ID = tid;
                   connectedTower = t;
                   Bars = bars;
               }
             
               pFeature = pFCursor.NextFeature();
           }

        }

        /// <summary>
        /// updates the device information
        /// </summary>
        public void Store()
        {

            IFeatureWorkspace pFWorkspace = (IFeatureWorkspace)_workspace;
            IWorkspaceEdit pWorkspaceEdit = (IWorkspaceEdit)pFWorkspace;

            IMultiuserWorkspaceEdit pMUWorkspaceEdit = (IMultiuserWorkspaceEdit)pWorkspaceEdit;

            pMUWorkspaceEdit.StartMultiuserEditing(esriMultiuserEditSessionMode.esriMESMVersioned);
            //  pWorkspaceEdit.StartEditing(true);
            pWorkspaceEdit.StartEditOperation();

            IFeatureClass pDeviceFC = pFWorkspace.OpenFeatureClass("sde.Device");
            IQueryFilter pQFilter = new QueryFilter();
            pQFilter.WhereClause = "DEVICEID = '" + DeviceID + "'";
            IFeatureCursor pFCursor = pDeviceFC.Search(pQFilter, false);
            IFeature pDeviceFeature = pFCursor.NextFeature();
            if (pDeviceFeature != null)
            {
                
                if (connectedTower != null)
                    pDeviceFeature.set_Value(pDeviceFeature.Fields.FindField("CONNECTEDTOWERID"),connectedTower.ID);
                else
                    pDeviceFeature.set_Value(pDeviceFeature.Fields.FindField("CONNECTEDTOWERID"), DBNull.Value);
             
                pDeviceFeature.set_Value(pDeviceFeature.Fields.FindField("BARS"), Bars);
                pDeviceFeature.Shape = DeviceLocation;
                pDeviceFeature.Store();
                
            }
            else
            {

                IFeature pNewFeature = pDeviceFC.CreateFeature();
                pNewFeature.set_Value(pNewFeature.Fields.FindField("DEVICEID"), DeviceID);

                if (connectedTower != null)
                    pNewFeature.set_Value(pNewFeature.Fields.FindField("CONNECTEDTOWERID"), connectedTower.ID);
                else
                    pNewFeature.set_Value(pNewFeature.Fields.FindField("CONNECTEDTOWERID"), DBNull.Value);
              

                 pNewFeature.set_Value(pNewFeature.Fields.FindField("BARS"), Bars);
                pNewFeature.Shape = DeviceLocation;
                pNewFeature.Store();
            }


            pWorkspaceEdit.StopEditOperation();
             
             
            pWorkspaceEdit.StopEditing(true);



        }

    }
}
