using UnityEngine;
using UnityEngine.Rendering;

namespace AndamRunner
{
    public static class StorybookScenery
    {
        public static void Create(Transform parent)
        {
            Transform sky=PrimitiveArt.Root("Storybook cloud puffs",parent,Vector3.zero);
            Vector3[] positions={new Vector3(-22,14,55),new Vector3(20,13,62),new Vector3(4,17,83),new Vector3(-15,15,105),new Vector3(32,16,99)};
            foreach(Vector3 position in positions)
            {
                for(int i=0;i<5;i++)
                {
                    float size=4f+(i%3)*1.1f;
                    PrimitiveArt.Ball(sky,"Soft cloud",position+new Vector3((i-2)*2.7f,Mathf.Sin(i*1.7f)*.8f,0),new Vector3(size,size*.53f,size*.42f),new Color32(255,238,209,255));
                }
            }
            StorybookArt.CombineBone(sky,true);
            foreach(Renderer renderer in sky.GetComponentsInChildren<Renderer>()) renderer.shadowCastingMode=ShadowCastingMode.Off;
        }
    }
}
