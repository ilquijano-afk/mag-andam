using UnityEngine;
using UnityEngine.UI;

namespace AndamRunner
{
    public static class UISkin
    {
        private static Sprite rounded;
        private static Sprite fade;
        private static Sprite groundShadow;
        public static Sprite GroundShadow
        {
            get
            {
                if(groundShadow!=null) return groundShadow;
                const int size=64;
                var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){name="Soft grounding shadow",wrapMode=TextureWrapMode.Clamp};
                for(int y=0;y<size;y++) for(int x=0;x<size;x++)
                { float r=Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(size*.5f,size*.5f))/(size*.5f); texture.SetPixel(x,y,new Color(1,1,1,Mathf.Pow(Mathf.Max(0,1-r*r),2))); }
                texture.Apply(false,true); groundShadow=Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f)); return groundShadow;
            }
        }
        public static Sprite Rounded
        {
            get
            {
                if (rounded != null) return rounded;
                const int size = 64;
                var texture = new Texture2D(size,size,TextureFormat.RGBA32,false) { name="Rounded UI", filterMode=FilterMode.Bilinear, wrapMode=TextureWrapMode.Clamp };
                var pixels = new Color[size*size];
                for(int y=0;y<size;y++) for(int x=0;x<size;x++)
                {
                    Vector2 d=new Vector2(Mathf.Abs(x+.5f-size*.5f),Mathf.Abs(y+.5f-size*.5f));
                    Vector2 corner=new Vector2(Mathf.Max(0,d.x-18),Mathf.Max(0,d.y-18));
                    pixels[y*size+x]=new Color(1,1,1,Mathf.Clamp01(14.5f-corner.magnitude));
                }
                texture.SetPixels(pixels); texture.Apply(false,true);
                rounded=Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(18,18,18,18));
                return rounded;
            }
        }
        public static Sprite Fade
        {
            get
            {
                if(fade!=null) return fade;
                var texture=new Texture2D(1,64,TextureFormat.RGBA32,false) {name="Menu atmosphere",wrapMode=TextureWrapMode.Clamp};
                for(int y=0;y<64;y++) texture.SetPixel(0,y,new Color(1,1,1,.02f+.93f*Mathf.Pow(y/63f,6)));
                texture.Apply(false,true); fade=Sprite.Create(texture,new Rect(0,0,1,64),new Vector2(.5f,.5f)); return fade;
            }
        }
        public static void Round(Image image) { image.sprite=Rounded; image.type=Image.Type.Sliced; image.pixelsPerUnitMultiplier=1.5f; }
    }
}
