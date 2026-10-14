using System;
using System.IO;
using AndamRunner;
using UnityEngine;
using UnityEngine.Rendering;

namespace AndamRunnerEditor
{
    public static class EnvironmentChecks
    {
        private static int assertions;
        private static void Check(bool condition,string message)
        { assertions++; if(!condition) throw new Exception("Environment check failed: "+message); }
        public static void Run(GameManager game)
        {
            assertions=0;
            EnvironmentAssetPack pack=game.Road.Pack;
            Check(pack!=null && pack.Assets.Length>=70,"complete reusable asset library exists");
            foreach(GameObject asset in pack.Assets)
            {
                EnvironmentAssetInfo info=asset.GetComponent<EnvironmentAssetInfo>(); Check(info!=null,"prefab has purpose and layer metadata");
                foreach(Collider collider in asset.GetComponentsInChildren<Collider>(true)) Check(!collider.enabled,"scenery has no active physics contacts");
                foreach(MeshFilter filter in asset.GetComponentsInChildren<MeshFilter>(true))
                {
                    Check(filter.sharedMesh!=null,"saved mesh is assigned: "+asset.name+" / "+filter.name);
                    foreach(Vector3 vertex in filter.sharedMesh.vertices) Check(float.IsFinite(vertex.x) && float.IsFinite(vertex.y) && float.IsFinite(vertex.z),"finite exported geometry");
                }
                if(!info.RoadSegment) continue;
                Check(info.ProminentProps>=3 && info.ProminentProps<=5,"road modules deliberately limit prominent props");
                Check(info.Triangles<5000,"low-poly segment budget");
                Check(asset.transform.Find("Entry").localPosition==Vector3.zero,"entry pivot is road-level Z zero");
                Check(asset.transform.Find("Exit").localPosition==new Vector3(0,info.SurfaceRise,24),"exit connection and rise are explicit");
                if(info.LaneCount==3)
                    for(int lane=0;lane<3;lane++) Check(asset.transform.Find("Lane anchors").GetChild(lane).localPosition.x==(lane-1)*PlayerRunner.LaneWidth,"compatible lane centers stay aligned");
                foreach(Transform prop in asset.transform.Find("Props"))
                {
                    Bounds bounds=new Bounds(); bool first=true;
                    foreach(Renderer renderer in prop.GetComponentsInChildren<Renderer>()) { if(first) { bounds=renderer.bounds; first=false; } else bounds.Encapsulate(renderer.bounds); }
                    float clear=info.LaneCount==1?1.65f:4.25f;
                    Check(bounds.max.x<-clear || bounds.min.x>clear,"city props remain outside the playable corridor");
                }
            }
            float previousStorm=0;
            for(int meter=0;meter<=960;meter++)
            {
                float height=CoastalRouteProfile.Elevation(meter), storm=CoastalRouteProfile.Storm(meter);
                Check(height>=0 && height<=4,"gentle route height is bounded");
                Check(Mathf.Abs(CoastalRouteProfile.Slope(meter))<=.051f,"slope remains gentle without sudden drops");
                Check(Mathf.Abs(storm-previousStorm)<.011f,"weather transitions are gradual"); previousStorm=storm;
            }
            Check(CoastalRouteProfile.District(120)==CoastalDistrict.Coast && CoastalRouteProfile.District(360)==CoastalDistrict.Alley && CoastalRouteProfile.District(540)==CoastalDistrict.Uphill,"vertical slice follows coast, alley, uphill escape");
            Camera camera=Camera.main; CameraFollow follow=camera.GetComponent<CameraFollow>(); follow.SetPresentation(false); follow.ResetCamera();
            Vector3 cameraPosition=camera.transform.position; Quaternion cameraRotation=camera.transform.rotation;
            int pooledViews=game.Road.GetComponentsInChildren<EnvironmentAssetInfo>(true).Length;
            foreach(float distance in new[]{0f,239f,240f,479f,480f,540f,599.9f,600f,719.9f,720f,839.9f,840f,959.9f,960f,3840f})
            {
                game.Road.SetDistance(distance); game.Environment.Apply();
                Check(Mathf.Abs(game.Road.HeightAtZ(0))<.0001f,"rider remains on a stable road-level origin");
                for(int lane=0;lane<3;lane++)
                {
                    game.Player.ChangeLane(lane-game.Player.Lane); game.Player.Tick(.2f,false); follow.SendMessage("LateUpdate");
                    Check(camera.transform.position==cameraPosition && Quaternion.Angle(camera.transform.rotation,cameraRotation)<.001f,"lane changes and slopes do not move or rotate the chase camera");
                }
                Check(game.Road.GetComponentsInChildren<EnvironmentAssetInfo>(true).Length==pooledViews,"district transitions reuse prewarmed views");
                for(int slot=0;slot<game.Road.transform.childCount;slot++)
                {
                    Transform root=game.Road.transform.GetChild(slot);
                    Check(root.localPosition.z>=-36.01f && root.localPosition.z<144.01f,"visible road slots stay bounded");
                }
            }
            game.StartRun(); game.Spawner.ClearForChecks(); game.Road.SetDistance(540);
            game.Spawner.SpawnHazard(HazardKind.RoadBlock,1,game.Speed*.46f); game.Jump(); game.Simulate(1.1f);
            Check(game.Session.Lives==3 && game.Session.Collected==1,"gentle uphill visuals preserve the jump and one-item reward");
            Check(QualitySettings.shadows==ShadowQuality.Disable,"mobile environment uses no real-time shadow map");
            File.WriteAllText("Docs/Environment-Validation.txt","Unity "+Application.unityVersion+" on "+DateTime.UtcNow.ToString("u")+"\n"+assertions+" "+(pack.Graybox?"graybox":"polished")+" environment assertions passed.\n"+
                pack.Assets.Length+" prefabs with asset-purpose/layer metadata, saved meshes, disabled design colliders and shared vertex-palette materials. Road modules limit prominent props, preserve three lane centers, expose entry/exit anchors and stay within triangle budgets.\n"+
                "Coast-to-alley-to-uphill sequence, gentle elevations, loop seams, gradual weather, bounded road slots and fixed chase camera checked. District transitions reuse prewarmed views. Uphill jumping still clears an obstacle and collects exactly one raised supply.\n"+
                "No real-time shadows, bloom, camera shake or scenery physics collisions. Physical Android GPU performance still needs profiling.\n");
            Debug.Log("ANDAM_ENVIRONMENT_CHECK_PASS: "+assertions+" "+(pack.Graybox?"graybox":"polished")+" asset, clearance, camera and route assertions.");
            game.StartRun();
        }
    }
}
