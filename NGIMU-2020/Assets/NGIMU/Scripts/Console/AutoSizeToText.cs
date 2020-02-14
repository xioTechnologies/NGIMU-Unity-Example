 // https://forum.unity.com/threads/does-the-content-size-fitter-work.484678/
using System;
using TMPro;
using UnityEngine;

 namespace NGIMU.Scripts
 {
     [RequireComponent(typeof(TMP_Text))]
     public class AutoSizeToText : MonoBehaviour
     {
         private TMP_Text text;
         public Vector2 Padding;
         public Vector2 MaxSize = new Vector2(1000, float.PositiveInfinity);
         public Vector2 MinSize;
         public Mode ControlAxes = Mode.Both;

         [Flags]
         public enum Mode
         {
             None = 0,
             Horizontal = 1,
             Vertical = 2,
             Both = Horizontal | Vertical
         }

         private string lastText;
         private Mode lastControlAxes = Mode.None;
         private Vector2 lastSize;
         private RectTransform rectTransform;
         private RectTransform parentRectTransform;

         void Awake()
         {
             text = GetComponent<TMP_Text>();
             rectTransform = GetComponent<RectTransform>();
             parentRectTransform = (RectTransform) rectTransform.parent;
         }

         protected virtual float MinX
         {
             get
             {
                 if ((ControlAxes & Mode.Horizontal) != 0) return MinSize.x;
                 return rectTransform.rect.width - Padding.x;
             }
         }

         protected virtual float MinY
         {
             get
             {
                 if ((ControlAxes & Mode.Vertical) != 0) return MinSize.y;
                 return rectTransform.rect.height - Padding.y;
             }
         }

         protected virtual float MaxX
         {
             get
             {
                 if ((ControlAxes & Mode.Horizontal) != 0) return MaxSize.x;
                 return rectTransform.rect.width - Padding.x;
             }
         }

         protected virtual float MaxY
         {
             get
             {
                 if ((ControlAxes & Mode.Vertical) != 0) return MaxSize.y;
                 return rectTransform.rect.height - Padding.y;
             }
         }

         protected virtual void Update()
         {
             // if (text.text == lastText &&
             //     lastSize == rectTransform.rect.size &&
             //     ControlAxes == lastControlAxes)
             // {
             //     return;
             // }

             var preferredSize = text.GetPreferredValues(MaxX, MaxY);
             preferredSize.x = Mathf.Clamp(preferredSize.x, MinX, MaxX);
             preferredSize.y = Mathf.Clamp(preferredSize.y, MinY, MaxY);
             preferredSize += Padding;

             if ((ControlAxes & Mode.Horizontal) != 0)
             {
                 rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize.x);
                 parentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize.x);
             }

             if ((ControlAxes & Mode.Vertical) != 0)
             {
                 rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredSize.y);
                 parentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredSize.y);
             }

             lastText = text.text;
             lastSize = rectTransform.rect.size;
             lastControlAxes = ControlAxes;
         }
     }
 }
 