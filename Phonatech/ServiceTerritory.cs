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
        private IWorkspace _workspace;

        public TowerManager TowerManager { get; set; }

        public IFeature ServiceTerritoryFeature { get; set; }
       
        public ServiceTerritory(IWorkspace workspace, string name)
        {

            _workspace = workspace;
            TowerManager = new TowerManager(workspace);
            IFeatureWorkspace pFWorkspace = (IFeatureWorkspace)workspace;
            IFeatureClass pServiceTerritory = pFWorkspace.OpenFeatureClass("sde.ServiceTerritory");
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

                IMultiuserWorkspaceEdit pMUWorkspaceEdit = (IMultiuserWorkspaceEdit)pWorkspaceEdit;
 
                pMUWorkspaceEdit.StartMultiuserEditing(esriMultiuserEditSessionMode.esriMESMVersioned);
            //pWorkspaceEdit.StartEditing(true);
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

            IFeatureClass pTowerRangeFC = pFWorkspace.OpenFeatureClass("sde.TowerRange");

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



        /// <summary>
        /// return all towers in a service terrtory
        /// </summary>
        /// <returns></returns>
        public Towers GetTowers()
        {
            Towers towers = new Towers();
            IFeatureWorkspace pFWorkspace = (IFeatureWorkspace)_workspace;
            IFeatureClass pTowerFC = pFWorkspace.OpenFeatureClass("sde.Towers");

            IFeatureCursor pFcursor = pTowerFC.Search(null, false);
            IFeature pFeature = pFcursor.NextFeature();
            while (pFeature != null)
            {
                Tower tower = TowerManager.GetTower(pFeature);
                towers.Items.Add(tower);
                pFeature = pFcursor.NextFeature();
            }

            return towers;
        }



        /// <summary>
        /// Generate the tower coverage
        /// </summary>
        public void GenerateReceptionArea()
        {
            IWorkspaceEdit pWorkspaceEdit;
            pWorkspaceEdit = (IWorkspaceEdit)this._workspace;
            try
            {

                //get all towers in this service territory
                Towers pTowers = GetTowers();

                IFeatureWorkspace pFWorkspace = (IFeatureWorkspace)pWorkspaceEdit;

                IFeatureClass pTowerRangeFC = pFWorkspace.OpenFeatureClass("sde.TowerRange");



                IMultiuserWorkspaceEdit pMUWorkspaceEdit = (IMultiuserWorkspaceEdit)pWorkspaceEdit;

                pMUWorkspaceEdit.StartMultiuserEditing(esriMultiuserEditSessionMode.esriMESMVersioned);
                // pWorkspaceEdit.StartEditing(true);
                pWorkspaceEdit.StartEditOperation();

                //delete all ranges , we should later change that to delete only dirty towers
                IFeatureCursor pcursor = pTowerRangeFC.Update(null, false);
                IFeature pfeaturerange = pcursor.NextFeature();
                while (pfeaturerange != null)
                {
                    //we need to change that later 
                    pfeaturerange.Delete();

                    pfeaturerange = pcursor.NextFeature();
                }

                foreach (Tower pTower in pTowers.Items)
                {

                    ITopologicalOperator pTopo = (ITopologicalOperator)pTower.TowerLocation;

                    IPolygon range3Bars = (IPolygon)pTopo.Buffer(pTower.TowerCoverage / 3);
                    IPolygon range2BarsWhole = (IPolygon)pTopo.Buffer((pTower.TowerCoverage * 2) / 3);
                    IPolygon range1BarsWhole = (IPolygon)pTopo.Buffer(pTower.TowerCoverage);

                    ITopologicalOperator pIntTopo = (ITopologicalOperator)range2BarsWhole;

                    ITopologicalOperator pIntTopo1 = (ITopologicalOperator)range1BarsWhole;


                    IPolygon range2BarsDonut = (IPolygon)pIntTopo.SymmetricDifference(range3Bars); //,esriGeometryDimension.esriGeometry2Dimension); 
                    IPolygon range1BarsDonut = (IPolygon)pIntTopo1.SymmetricDifference(range2BarsWhole); //,esriGeometryDimension.esriGeometry2Dimension); 


                    IFeature pFeature = pTowerRangeFC.CreateFeature();

                    pFeature.set_Value(pFeature.Fields.FindField("TOWERID"), pTower.ID);
                    pFeature.set_Value(pFeature.Fields.FindField("RANGE"), 3);

                    pFeature.Shape = range3Bars;
                    pFeature.Store();


                    IFeature pFeature2Bar = pTowerRangeFC.CreateFeature();

                    pFeature2Bar.set_Value(pFeature.Fields.FindField("TOWERID"), pTower.ID);
                    pFeature2Bar.set_Value(pFeature.Fields.FindField("RANGE"), 2);

                    pFeature2Bar.Shape = range2BarsDonut;
                    pFeature2Bar.Store();


                    IFeature pFeature1Bar = pTowerRangeFC.CreateFeature();

                    pFeature1Bar.set_Value(pFeature1Bar.Fields.FindField("TOWERID"), pTower.ID);
                    pFeature1Bar.set_Value(pFeature1Bar.Fields.FindField("RANGE"), 1);

                    pFeature1Bar.Shape = range1BarsDonut;
                    pFeature1Bar.Store();



                }


                pWorkspaceEdit.StopEditOperation();
                pWorkspaceEdit.StopEditing(true);


                //generate dead areas
                GenerateDeadAreas();


            }
            catch (Exception ex)
            {
                //if anything went wrong, just roll back
                pWorkspaceEdit.AbortEditOperation();
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }

        }

        /// <summary>
        /// changed this to private since this need to be called only from within generate range
        /// </summary>
        private void GenerateDeadAreas()
        {
            //get the service territory 
            IWorkspaceEdit pWorkspaceEdit;
            pWorkspaceEdit = (IWorkspaceEdit)this._workspace;
            try
            {
                IFeatureWorkspace pFWorkspace = (IFeatureWorkspace)pWorkspaceEdit;

               // ServiceTerritory mainST = new ServiceTerritory(_workspace, "MAIN");

                if (this.ServiceTerritoryFeature != null)
                {

                    IGeometry pSVGeometry = ServiceTerritoryFeature.Shape;
                    //IGeometry pRecptionGeometry = mainST.getReceptionArea();
                    //union all the signals and get one big reception area geometry

                    //edit and add to fc
                    IGeometry pDeadArea = getDeadArea();
                    //pWorkspaceEdit.StartEditing(true);

                    IMultiuserWorkspaceEdit pMUWorkspaceEdit = (IMultiuserWorkspaceEdit)pWorkspaceEdit;

                    pMUWorkspaceEdit.StartMultiuserEditing(esriMultiuserEditSessionMode.esriMESMVersioned);
                    pWorkspaceEdit.StartEditOperation();

                    double deadCoverage = ((IArea)pDeadArea).Area * 100 / ((IArea)pSVGeometry).Area;
                    double receptionCoverage = 100 - deadCoverage;

                    IFeatureClass pDeadAreasFC = pFWorkspace.OpenFeatureClass("sde.DeadAreas");


                    //delete all features
                    IFeatureCursor pcursor = pDeadAreasFC.Update(null, false);
                    IFeature pfeaturerange = pcursor.NextFeature();
                    while (pfeaturerange != null)
                    {
                        //we need to change that later 
                        pfeaturerange.Delete();
                        pfeaturerange = pcursor.NextFeature();
                    }


                    IFeature pDeadAreaFeature = pDeadAreasFC.CreateFeature();
                    pDeadAreaFeature.Shape = pDeadArea;
                    pDeadAreaFeature.Store();

                    pWorkspaceEdit.StopEditOperation();
                    pWorkspaceEdit.StopEditing(true);

                    updateCoverages(deadCoverage, receptionCoverage);
 
                }

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
                pWorkspaceEdit.AbortEditOperation();
            }


        }

    }
}
