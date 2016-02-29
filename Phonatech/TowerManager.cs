using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phonatech
{
    public class TowerManager
    {

        private IWorkspace _workspace;

        public TowerManager(IWorkspace pWorkspace)
        {
            _workspace = pWorkspace;

        }

        public Tower GetTower(IFeature pTowerFeature)
        {
            Tower tower = new Tower();
            tower.ID = pTowerFeature.get_Value(pTowerFeature.Fields.FindField("TOWERID")); ;
            tower.NetworkBand = pTowerFeature.get_Value(pTowerFeature.Fields.FindField("NETWORKBAND"));
            tower.TowerType = pTowerFeature.get_Value(pTowerFeature.Fields.FindField("TOWERTYPE"));
            tower.TowerLocation = (IPoint) pTowerFeature.Shape;
            return tower;

        }

        public Tower GetTowerByID(string towerid)
        {
           //query the geodatabase.. 
            IFeatureWorkspace pFeatureWorkspace =(IFeatureWorkspace) _workspace;

            IFeatureClass fcTower = pFeatureWorkspace.OpenFeatureClass("Towers");

           //get the tower feature by id 
            IQueryFilter pQFilter = new QueryFilter();
            pQFilter.WhereClause = "TOWERID = '" + towerid + "'";

            IFeatureCursor pFCursor = fcTower.Search(pQFilter, true);
             
            IFeature pTowerFeature = pFCursor.NextFeature();

            if (pTowerFeature == null)
                return null;


            return GetTower(pTowerFeature);
            //get the tower type, and then query the tower details table to get the rest of the data...
            

        }

        public Tower GetNearestTower(IPoint pPoint, int buffer)
        { 

                ITopologicalOperator pTopo = (ITopologicalOperator)pPoint;

                IGeometry pBufferedPoint = pTopo.Buffer(buffer);

                ISpatialFilter pSFilter = new SpatialFilter();
                pSFilter.Geometry = pBufferedPoint;
                pSFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

                //query the geodatabase.. 
                IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)_workspace;

                IFeatureClass fcTower = pFeatureWorkspace.OpenFeatureClass("Towers");

                IFeatureCursor pFCursor = fcTower.Search(pSFilter, true);
                IFeature pTowerFeature = pFCursor.NextFeature();

                if (pTowerFeature == null)
                    return null;

                return GetTower(pTowerFeature);
            
        }


    }
}
