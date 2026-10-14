using System;
using UnityEngine;

namespace AndamRunner
{
    // Seven road slots recycle. All district views and deformable surface buffers
    // are prewarmed; crossing a district never instantiates a prop or mesh.
    public sealed class RepeatingRoad : MonoBehaviour
    {
        public const int ChunkCount=7;
        private static readonly string[] ViewIds={"PF_Segment_Coastal_Promenade","PF_Segment_Coastal_Stores","PF_Segment_Alley_ThreeLane","PF_Segment_Alley_Drainage","PF_Segment_FloodEscape_Ramp","PF_Segment_Refuge_Gate","PF_Segment_Elevated_Walkway","PF_Segment_Safe_Descent"};
        private sealed class View
        {
            public Transform Root, Water;
            public RouteSurface Surface;
            public Transform[] Props;
            public Vector3[] PropPositions;
            public Quaternion[] PropRotations;
            public float Rise;
        }
        private sealed class Slot
        {
            public Transform Root;
            public float WorldStart;
            public View[] Views;
            public int Active=-1;
        }
        private readonly Slot[] slots=new Slot[ChunkCount];
        private EnvironmentAssetPack pack;
        public float Distance { get; private set; }
        public EnvironmentAssetPack Pack => pack;
        public float HeightAtZ(float z) => CoastalRouteProfile.Elevation(Distance+z)-CoastalRouteProfile.Elevation(Distance);
        public float SlopeAtZ(float z) => CoastalRouteProfile.Slope(Distance+z);

        public void Initialize()
        {
            pack=Resources.Load<EnvironmentAssetPack>(EnvironmentAssetPack.ResourcePath);
            if(pack==null) throw new InvalidOperationException("Run ANDAM > Prepare or open MainGame to create the environment asset pack.");
            for(int i=0;i<ChunkCount;i++)
            {
                var slot=new Slot{Root=PrimitiveArt.Root("Pooled road slot "+i,transform,Vector3.zero),Views=new View[ViewIds.Length]}; slots[i]=slot;
                for(int type=0;type<ViewIds.Length;type++)
                {
                    GameObject source=pack.Find(ViewIds[type]); if(source==null) throw new InvalidOperationException("Missing segment prefab: "+ViewIds[type]);
                    Transform root=Instantiate(source,slot.Root,false).transform;
                    var info=root.GetComponent<EnvironmentAssetInfo>();
                    MeshFilter surface=root.Find("Surface").GetComponentInChildren<MeshFilter>();
                    RouteSurface curved=surface.gameObject.AddComponent<RouteSurface>(); curved.Initialize(info.SurfaceRise);
                    Transform props=root.Find("Props");
                    var view=new View{Root=root,Surface=curved,Rise=info.SurfaceRise,Water=root.Find("Water"),Props=new Transform[props.childCount],PropPositions=new Vector3[props.childCount],PropRotations=new Quaternion[props.childCount]};
                    for(int n=0;n<props.childCount;n++) { view.Props[n]=props.GetChild(n); view.PropPositions[n]=view.Props[n].localPosition; view.PropRotations[n]=view.Props[n].localRotation; }
                    slot.Views[type]=view; root.gameObject.SetActive(false);
                }
            }
            ResetRoad();
        }
        public void ResetRoad() { SetDistance(0); }
        public void SetDistance(float distance)
        {
            Distance=Mathf.Max(0,distance);
            float first=Mathf.Floor((Distance-12)/24)*24;
            for(int i=0;i<ChunkCount;i++) { slots[i].WorldStart=first+i*24; UpdateSlot(slots[i],i); }
        }
        public void Tick(float distance)
        {
            Distance+=distance;
            for(int i=0;i<ChunkCount;i++)
            {
                Slot slot=slots[i];
                while(slot.WorldStart-Distance<-36) slot.WorldStart+=ChunkCount*24;
                UpdateSlot(slot,i);
            }
        }
        private void UpdateSlot(Slot slot,int variation)
        {
            GameObject desired=pack.Segment(slot.WorldStart,Mathf.FloorToInt(slot.WorldStart/24));
            int type=Array.IndexOf(ViewIds,desired.name);
            if(slot.Active!=type)
            {
                if(slot.Active>=0) slot.Views[slot.Active].Root.gameObject.SetActive(false);
                slot.Active=type; slot.Views[type].Root.gameObject.SetActive(true);
            }
            slot.Root.localPosition=new Vector3(0,0,slot.WorldStart-Distance);
            View view=slot.Views[type]; view.Surface.Fit(slot.WorldStart,Distance);
            float currentHeight=CoastalRouteProfile.Elevation(Distance), storm=CoastalRouteProfile.Storm(Distance);
            for(int n=0;n<view.Props.Length;n++)
            {
                Vector3 p=view.PropPositions[n];
                p.y+=CoastalRouteProfile.Elevation(slot.WorldStart+p.z)-currentHeight-view.Rise*p.z/24;
                view.Props[n].localPosition=p;
                float sway=view.Props[n].name.Contains("Palm")?Mathf.Sin(Time.time*.65f+variation)*storm*1.2f:0;
                view.Props[n].localRotation=view.PropRotations[n]*Quaternion.Euler(0,0,sway);
            }
            if(view.Water!=null) view.Water.localPosition=Vector3.down*currentHeight;
        }
    }
}
