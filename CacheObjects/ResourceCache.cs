using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DSPTransportStat.CacheObjects
{
    /// <summary>
    /// 游戏资源缓存
    /// </summary>
    static class ResourceCache
    {
        static public Font FontSAIRASB = null;

        static public Sprite SpriteRound256 = null;

        static public Sprite Round54pxSlice = null;

        static public Material MaterialWidgetTextAlpha5x;

        static public Material MaterialDefaultUIMaterial;

        static public void InitializeResourceCache ()
        {
            Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();
            for (int i = 0; i < fonts.Length; ++i)
            {
                if (fonts[i].name == "SAIRASB")
                {
                    FontSAIRASB = fonts[i];
                }
            }

            Sprite[] sprites = Resources.FindObjectsOfTypeAll<Sprite>();
            for (int i = 0; i < sprites.Length; ++i)
            {
                if (sprites[i].name == "round-256")
                {
                    SpriteRound256 = sprites[i];
                }
                else if (sprites[i].name == "round-54px-slice")
                {
                    Round54pxSlice = sprites[i];
                }
            }

            Material[] materials = Resources.FindObjectsOfTypeAll<Material>();
            for (int i = 0; i < materials.Length; ++i)
            {
                if (materials[i].name == "widget-text-alpha-5x")
                {
                    MaterialWidgetTextAlpha5x = materials[i];
                }
                else if (materials[i].name == "Default UI Material")
                {
                    MaterialDefaultUIMaterial = materials[i];
                }
            }
        }
    }
}
