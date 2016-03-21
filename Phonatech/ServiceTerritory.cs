using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phonatech
{
    public class ServiceTerritory
    {

        public IFeature ServiceTerritoryFeature { get; set; }
       
        public ServiceTerritory(IWorkspace workspace, string name)
        {
                
            IFeatureWorkspace pFWorkspace = (IFeatureWorkspace)workspace;
            IFeatureClass pServiceTerritory = pFWorkspace.OpenFeatureClass("ServiceTerritory");
            IQueryFilter pQFilter = new QueryFilter();
            pQFilter.WhereClause = "NAME='" + name +"'";

            IFeatureCursor pFCursor= pServiceTerritory.Search(pQFilter, false);
            ServiceTerritoryFeature = pFCursor.NextFeature();
             
        }

        public void updateCoverages(double deadCoverage, double receptionCoverage)
        {
              IWorkspaceEdit pWorkspaceEdit = null;
            try
            {

             pWorkspaceEdit = (IWorkspaceEdit)((IDataset)ServiceTerritoryFeature.Class).Workspace;
            pWorkspaceEdit.StartEditing(true);
            pWorkspaceEdit.StartEditOperation();

            ServiceTerritoryFeature.set_Value(ServiceTerritoryFeature.Fields.FindField("DEADCOVERAGE"), deadCoverage);
            ServiceTerritoryFeature.set_Value(ServiceTerritoryFeature.Fields.FindField("RECEPTIONCOVERAGE"), receptionCoverage);
            ServiceTerritoryFeature.Store();

            pWorkspaceEdit.StopEditOperation();
            pWorkspaceEdit.StopEditing(true);
                
            }
            catch
            {
                pWorkspaceEdit.AbortEditOperation();
            }
        }

        public IGeometry getDeadArea()
        { 
            ITopologicalOperator pDeadAreaTopo = (ITopologicalOperator)ServiceTerritoryFeature.Shape;
            IGeometry DeadAreas = pDeadAreaTopo.SymmetricDifference(getReceptionArea());
            return DeadAreas;
        }

        public IGeometry getReceptionArea()
        {

            IFeatureWorkspace pFWorkspace = (IFeatureWorkspace)((IDataset)ServiceTerritoryFeature.Class).Workspace;

            IFeatureClass pTowerRangeFC = pFWorkspace.OpenFeatureClass("TowerRange");

            IFeatureCursor pRFCursor = pTowerRangeFC.Search(null, false);
            IGeometry pRecptionGeometry = null;

            IFeature pRangeFeature = pRFCursor.NextFeature();
            while (pRangeFeature != null)
            {
                if (pRecptionGeometry == null)
                    pRecptionGeometry = pRangeFeature.Shape;
                else
                {
                    ITopologicalOperator pTopo = (ITopologicalOperator)pRecptionGeometry;
                    pRecptionGeometry = pTopo.Union(pRangeFeature.Shape);
                }

                pRangeFeature = pRFCursor.NextFeature();
            }

            return pRecptionGeometry;

        }
    }
}
