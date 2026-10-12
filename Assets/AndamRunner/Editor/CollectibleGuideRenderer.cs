using System;
using System.Collections.Generic;
using System.IO;
using AndamRunner;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AndamRunnerEditor
{
    // A printable catalogue rendered from the real game meshes, with no player saves involved.
    public static class CollectibleGuideRenderer
    {
        private static readonly Color Paper=new Color32(255,248,231,255);
        private static readonly Color Card=new Color32(255,253,245,255);
        private static readonly Color Green=new Color32(29,107,92,255);
        private static readonly Color Brown=new Color32(117,63,24,255);
        private static readonly string[] SupplyLabels={"Go-bag","Water","First-aid kit","Flashlight","Whistle","Radio","Contact card","Cash","Non-perishable food"};
        private static readonly string[] DistractorLabels={"Gym equipment","Bulky toy","Beer","Makeup","Game console"};

        [MenuItem("ANDAM/Render collectible guide PNG")]
        public static void Render()
        {
            if(EditorApplication.isPlaying) { Debug.LogWarning("Stop Play mode before rendering the collectible guide."); return; }
            GameObject root=null,view=null;
            var pictures=new List<Texture2D>();
            try
            {
                PrototypeTools.PrepareProject();
                Directory.CreateDirectory("Docs/Artwork");
                root=new GameObject("Collectible guide / temporary",typeof(RectTransform),typeof(Canvas));
                root.layer=28;
                Canvas canvas=root.GetComponent<Canvas>();
                canvas.renderMode=RenderMode.ScreenSpaceCamera;
                view=new GameObject("Collectible guide camera");
                Camera camera=view.AddComponent<Camera>(); camera.enabled=false;
                camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=Paper;
                camera.orthographic=true; camera.orthographicSize=5; camera.cullingMask=1<<28;
                camera.transform.position=new Vector3(0,0,-10);
                canvas.worldCamera=camera; canvas.planeDistance=1;
                Font font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                TextAt(root.transform,font,"ANDAM LAPU-LAPU  /  OPONG ANDAM",48,28,984,30,22,Green,true);
                TextAt(root.transform,font,"COLLECT & CHOOSE!",48,65,984,64,50,PreparednessContent.Ink,true);
                TextAt(root.transform,font,"The items you can pick up during your run",48,137,984,34,25,PreparednessContent.Ink);
                Panel(root.transform,48,190,984,48,PreparednessContent.Mint);
                TextAt(root.transform,font,"USEFUL SUPPLIES",72,193,650,42,28,Green,true,TextAnchor.MiddleLeft);
                TextAt(root.transform,font,"9 GO-BAG TYPES",738,196,270,36,21,Green,true,TextAnchor.MiddleRight);
                for(int i=0;i<PreparednessContent.SupplyCount;i++)
                {
                    Texture2D photo=RenderItem((SupplyKind)i,null); pictures.Add(photo);
                    AddCard(root.transform,font,48+(i%3)*336,258+(i/3)*292,276,photo,SupplyLabels[i],"+"+PreparednessContent.Points[i]+" POINTS",true);
                }
                Panel(root.transform,48,1166,984,54,PreparednessContent.Orange);
                TextAt(root.transform,font,"AVOID THESE DISTRACTORS",72,1171,700,44,28,Brown,true,TextAnchor.MiddleLeft);
                TextAt(root.transform,font,"-5 POINTS EACH",48,1225,984,30,23,Brown,true);
                for(int i=0;i<PreparednessContent.DistractorCount;i++)
                {
                    Texture2D photo=RenderItem(null,(DistractorKind)i); pictures.Add(photo);
                    float x=i<3?48+i*336:216+(i-3)*336;
                    AddCard(root.transform,font,x,i<3?1264:1534,252,photo,DistractorLabels[i],"-"+PreparednessContent.DistractorPenalty+" POINTS",false);
                }
                TextAt(root.transform,font,"Distractors cost points. You do not lose a life.",48,1815,984,36,25,Brown,true);
                TextAt(root.transform,font,"A bulky toy is the game example. Ask an adult about a small comfort item.",48,1856,984,32,20,PreparednessContent.Ink);
                Save(canvas,camera,"Docs/Artwork/HowTo-AllItems.png",1080,1920);
                Save(canvas,camera,"Docs/Artwork/HowTo-AllItems-HighRes.png",2160,3840);
                File.WriteAllText("Docs/Artwork/HowTo-AllItems.txt","All fourteen collectible game models rendered directly in Unity: nine useful supplies and five distractors.\n1080x1920 standard PNG and 2160x3840 high-resolution PNG. Names are guide captions; gameplay track objects remain unlabeled.\nSource: Assets/AndamRunner/Editor/CollectibleGuideRenderer.cs. Recreate using ANDAM > Render collectible guide PNG.\nThe existing HowToBackground.png was not replaced. No leaderboard storage or Play mode was used.\n");
                Debug.Log("ANDAM_COLLECTIBLE_GUIDE_PASS: fourteen real game models, current point values, two PNG sizes.");
                if(Application.isBatchMode) EditorApplication.Exit(0);
            }
            catch(Exception error)
            {
                Debug.LogException(error);
                if(Application.isBatchMode) EditorApplication.Exit(1);
            }
            finally
            {
                if(root!=null) UnityEngine.Object.DestroyImmediate(root);
                if(view!=null) UnityEngine.Object.DestroyImmediate(view);
                foreach(Texture2D picture in pictures) UnityEngine.Object.DestroyImmediate(picture);
            }
        }

        private static Texture2D RenderItem(SupplyKind? supply,DistractorKind? distractor)
        {
            Transform review=PrimitiveArt.Root("Catalogue model",null,new Vector3(30,0,0));
            if(supply.HasValue) RunnerReferenceArt.Supply(review,supply.Value);
            else RunnerReferenceArt.Distractor(review,distractor.Value);
            foreach(Transform part in review.GetComponentsInChildren<Transform>(true)) part.gameObject.layer=27;
            var obj=new GameObject("Catalogue model camera"); Camera camera=obj.AddComponent<Camera>(); camera.enabled=false;
            camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=Color.clear; camera.cullingMask=1<<27;
            camera.orthographic=true; camera.orthographicSize=.90f;
            camera.transform.position=review.position+new Vector3(.65f,1.55f,-3.4f);
            camera.transform.LookAt(review.position+new Vector3(0,.65f,0));
            var target=new RenderTexture(512,512,24,RenderTextureFormat.ARGB32){antiAliasing=4}; target.Create();
            camera.targetTexture=target; camera.Render();
            RenderTexture previous=RenderTexture.active; RenderTexture.active=target;
            var picture=new Texture2D(512,512,TextureFormat.RGBA32,false);
            picture.ReadPixels(new Rect(0,0,512,512),0,0); picture.Apply();
            RenderTexture.active=previous; camera.targetTexture=null; target.Release();
            UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(obj); UnityEngine.Object.DestroyImmediate(review.gameObject);
            return picture;
        }

        private static void AddCard(Transform parent,Font font,float x,float y,float height,Texture2D picture,string name,string points,bool useful)
        {
            RectTransform card=Panel(parent,x,y,312,height,Card);
            Panel(card,0,0,312,5,useful?PreparednessContent.Mint:PreparednessContent.Orange);
            float size=useful?204:180;
            RectTransform icon=Rect(card,(312-size)*.5f,6,size,size);
            icon.gameObject.AddComponent<RawImage>().texture=picture;
            TextAt(card,font,name,8,size+10,296,32,23,PreparednessContent.Ink,true);
            float badgeY=height-32;
            Panel(card,80,badgeY,152,26,useful?PreparednessContent.Mint:PreparednessContent.Orange);
            TextAt(card,font,points,80,badgeY,152,26,20,useful?Green:Brown,true);
        }

        private static RectTransform Rect(Transform parent,float x,float y,float width,float height)
        {
            var rect=new GameObject("Guide element",typeof(RectTransform)).GetComponent<RectTransform>();
            rect.gameObject.layer=28; rect.SetParent(parent,false);
            rect.anchorMin=rect.anchorMax=new Vector2(0,1); rect.pivot=new Vector2(0,1);
            rect.anchoredPosition=new Vector2(x,-y); rect.sizeDelta=new Vector2(width,height);
            return rect;
        }
        private static RectTransform Panel(Transform parent,float x,float y,float width,float height,Color color)
        {
            RectTransform rect=Rect(parent,x,y,width,height); Image image=rect.gameObject.AddComponent<Image>(); image.color=color;
            image.raycastTarget=false; return rect;
        }
        private static void TextAt(Transform parent,Font font,string message,float x,float y,float width,float height,int size,Color color,bool bold=false,TextAnchor alignment=TextAnchor.MiddleCenter)
        {
            RectTransform rect=Rect(parent,x,y,width,height); Text text=rect.gameObject.AddComponent<Text>();
            text.font=font; text.text=message; text.fontSize=size; text.color=color; text.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;
            text.alignment=alignment; text.resizeTextForBestFit=true; text.resizeTextMinSize=size-3; text.resizeTextMaxSize=size;
            text.raycastTarget=false;
        }
        private static void Save(Canvas canvas,Camera camera,string path,int width,int height)
        {
            var target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32){antiAliasing=2}; target.Create();
            camera.targetTexture=target; camera.aspect=(float)width/height; canvas.scaleFactor=width/1080f;
            Canvas.ForceUpdateCanvases(); camera.Render(); camera.Render();
            RenderTexture previous=RenderTexture.active; RenderTexture.active=target;
            var image=new Texture2D(width,height,TextureFormat.RGBA32,false);
            image.ReadPixels(new Rect(0,0,width,height),0,0); image.Apply(); File.WriteAllBytes(path,image.EncodeToPNG());
            RenderTexture.active=previous; camera.targetTexture=null; target.Release();
            UnityEngine.Object.DestroyImmediate(image); UnityEngine.Object.DestroyImmediate(target);
        }
    }
}
