using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
#pragma warning disable IDE0065
using Color = UnityEngine.Color;
#pragma warning restore IDE0065

using Mediapipe;
using Mediapipe.Unity;

using XPlan.UI;

namespace asail0712.Test
{
    public class RemovalBackgroundUI : UIBase
    {
        [SerializeField] private RawImage screen;
        [SerializeField] private Shader maskShader;
        [SerializeField] private Color maskColor        = new Color(1f, 1f, 1f, 0f);
        [SerializeField] private float maskThredhold    = 0.6f;

        private Material maskMaterial;
        private GraphicsBuffer maskBuffer;
        private float[] maskArray;

        private void Awake()
        {
            /******************************
             * 初始化
             * ****************************/
            maskMaterial = new Material(maskShader)
            {
                renderQueue = (int)RenderQueue.Transparent
            };

            maskMaterial.SetTexture("_MainTex", screen.texture);
            maskMaterial.SetTexture("_MaskTex", CreateMonoColorTexture(maskColor));
            maskMaterial.SetFloat("_Threshold", maskThredhold);

            screen.material = maskMaterial;

            /******************************
             * UI Listener
             * ****************************/
            ListenCall<Vector2>(UICommand.InitScreen, InitUI);
            ListenCall<ImageFrame>(UICommand.UpdateMask, UpdateMask);
        }

        private void LateUpdate()
        {
            if (maskBuffer != null)
            {
                maskBuffer.SetData(maskArray);
            }
        }
        private void OnDestroy()
        {
            if (maskBuffer != null)
            {
                maskBuffer.Release();
            }
            maskArray = null;
        }

        private void UpdateMask(ImageFrame imgFrame)
        {
            // 將image frame的資料轉移到maskArray
            var _ = imgFrame.TryReadChannelNormalized(0, maskArray);
        }

        private void InitUI(Vector2 vec)
        {
            float width     = vec.x;
            float height    = vec.y;

            // 設定 Screen Size
            screen.rectTransform.sizeDelta  = new Vector2(width, height);
             AspectRatioFitter ratioFitter   = screen.GetComponent<AspectRatioFitter>();

            if (ratioFitter == null)
            {
                ratioFitter = screen.gameObject.AddComponent<AspectRatioFitter>();
            }

            if (ratioFitter != null)
            {
                AspectRatioFitter.AspectMode mode   = AspectRatioFitter.AspectMode.HeightControlsWidth;
                float aspectRatio                   = width / height;

                ratioFitter.aspectRatio = aspectRatio;
                ratioFitter.aspectMode  = mode;
            }

            // 設定 Material
            maskMaterial.SetInt("_Width", (int)width);
            maskMaterial.SetInt("_Height", (int)height);

            if (maskBuffer != null)
            {
                maskBuffer.Release();
            }

            int stride  = Marshal.SizeOf(typeof(float));
            maskBuffer  = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)width * (int)height, stride);
            maskArray   = new float[(int)width * (int)height];

            maskMaterial.SetBuffer("_MaskBuffer", maskBuffer);
        }

        private Texture2D CreateMonoColorTexture(Color color)
        {
            Texture2D texture       = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            Color32 textureColor    = new Color32((byte)(255 * color.r), (byte)(255 * color.g), (byte)(255 * color.b), (byte)(255 * color.a));
            texture.SetPixels32(new Color32[] { textureColor });
            texture.Apply();

            return texture;
        }
    }
}