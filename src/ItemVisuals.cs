using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace TarDistillation
{
    /// <summary>
    ///     Gives the two tyrispik items their own model, texture and inventory icon, read
    ///     from <c>assets/</c> next to the plugin DLL.
    /// </summary>
    /// <remarks>
    ///     Both items are clones of vanilla wood, so without this they look like a round log
    ///     and a piece of elder bark. The model and the textures come off disk as plain OBJ
    ///     and PNG; the one thing that cannot be built at runtime is a shader, so the material
    ///     is cloned from the vanilla one the prefab already carries and only its albedo is
    ///     swapped. That keeps the item lit, wetted and fogged exactly like the wood it was
    ///     cloned from, with none of the Unity editor work an asset bundle would need.
    ///
    ///     Everything here is best-effort: a missing or unreadable file costs the item its
    ///     new look and nothing else, which matters because someone who copies only the DLL
    ///     should still get a working mod.
    /// </remarks>
    internal static class ItemVisuals
    {
        private const string AssetFolder = "assets";

        /// <summary>
        ///     The prefabs that actually got a shipped model. Only these are resized later:
        ///     an item that fell back to the vanilla log because its files were missing must
        ///     keep the vanilla log's size, not be shrunk to half a log.
        /// </summary>
        private static readonly List<GameObject> Dressed = new List<GameObject>();

        /// <summary>
        ///     Dresses up an item prefab. <paramref name="assetName"/> picks the files:
        ///     <c>&lt;name&gt;.obj</c>, <c>&lt;name&gt;_albedo.png</c> and
        ///     <c>&lt;name&gt;_icon.png</c>.
        /// </summary>
        public static void Apply(GameObject prefab, ItemDrop.ItemData.SharedData shared, string assetName)
        {
            if (prefab == null || !TarDistillationPlugin.Cfg.CustomModels.Value)
            {
                return;
            }

            ApplyIcon(shared, assetName);
            ApplyModel(prefab, assetName);
        }

        /// <summary>
        ///     Resizes the prefabs that carry a shipped model. Called when <c>Model scale</c>
        ///     changes, so the size can be dialled in from the config menu without a restart -
        ///     items already lying on the ground keep the size they were dropped at.
        /// </summary>
        public static void ApplyScale()
        {
            var scale = TarDistillationPlugin.Cfg.ModelScale.Value;

            foreach (var prefab in Dressed)
            {
                if (prefab == null)
                {
                    continue;
                }

                foreach (var filter in prefab.GetComponentsInChildren<MeshFilter>(true))
                {
                    // Absolute, not a multiplier: setting it twice has to mean the same as once.
                    filter.transform.localScale = Vector3.one * scale;
                }
            }
        }

        private static void ApplyIcon(ItemDrop.ItemData.SharedData shared, string assetName)
        {
            try
            {
                var texture = LoadTexture(AssetPath(assetName + "_icon.png"));
                texture.wrapMode = TextureWrapMode.Clamp;

                shared.m_icons = new[]
                {
                    Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f)),
                };
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogWarning($"{assetName}: keeping the vanilla icon ({Reason(ex)})");
            }
        }

        private static void ApplyModel(GameObject prefab, string assetName)
        {
            try
            {
                var mesh = ObjMesh.Load(AssetPath(assetName + ".obj"), assetName);
                var albedo = LoadTexture(AssetPath(assetName + "_albedo.png"));

                var filters = prefab.GetComponentsInChildren<MeshFilter>(true);
                if (filters.Length == 0)
                {
                    throw new InvalidDataException("the cloned prefab has no mesh to replace");
                }

                Material material = null;
                var scale = TarDistillationPlugin.Cfg.ModelScale.Value;

                // Every mesh in the prefab, which on a vanilla item means each LOD. They all
                // get the same one: 450 triangles is cheaper than most vanilla LOD0 meshes,
                // so there is nothing to gain by letting the distant ones keep their own.
                foreach (var filter in filters)
                {
                    filter.sharedMesh = mesh;
                    filter.transform.localScale = Vector3.one * scale;

                    var renderer = filter.GetComponent<MeshRenderer>();
                    if (renderer == null)
                    {
                        continue;
                    }

                    material = material ?? CloneMaterial(renderer.sharedMaterial, albedo, assetName);
                    renderer.sharedMaterials = new[] { material };
                }

                Dressed.Add(prefab);

                Jotunn.Logger.LogDebug(
                    $"{assetName}: {mesh.vertexCount} vertices over {filters.Length} renderer(s)");
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogWarning($"{assetName}: keeping the vanilla model ({Reason(ex)})");
            }
        }

        /// <summary>
        ///     Copies the vanilla material the prefab came with and swaps the albedo for ours.
        ///     Copying rather than building one from scratch is the whole trick: it keeps the
        ///     vanilla shader and every map and tuning value hung off it, so the item still
        ///     darkens in rain and sits in the fog like the wood beside it.
        /// </summary>
        private static Material CloneMaterial(Material source, Texture2D albedo, string assetName)
        {
            if (source == null)
            {
                throw new InvalidDataException("the cloned prefab has no material to borrow");
            }

            var material = new Material(source) { name = assetName + "_mat" };

            // Valheim's wood shaders call the albedo _MainTex, but the tint is _Color on some
            // and _MainColor on others, so set whichever is actually there and leave the rest.
            SetTexture(material, "_MainTex", albedo);
            SetColor(material, "_Color", Color.white);
            SetColor(material, "_MainColor", Color.white);

            return material;
        }

        private static void SetTexture(Material material, string property, Texture texture)
        {
            if (material.HasProperty(property))
            {
                material.SetTexture(property, texture);
                material.SetTextureScale(property, Vector2.one);
                material.SetTextureOffset(property, Vector2.zero);
            }
        }

        private static void SetColor(Material material, string property, Color color)
        {
            if (material.HasProperty(property))
            {
                material.SetColor(property, color);
            }
        }

        private static Texture2D LoadTexture(string path)
        {
            // linear: false - these are colour maps, so they are read as sRGB.
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, true, false)
            {
                name = Path.GetFileNameWithoutExtension(path),
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Trilinear,
                anisoLevel = 4,
            };

            // Jotunn's wrapper around ImageConversion.LoadImage. Going through it rather
            // than calling Unity directly keeps UnityEngine.ImageConversionModule out of our
            // own references: it is built against netstandard 2.1, which net472 cannot read.
            if (!Jotunn.Utils.AssetUtils.LoadImage(texture, File.ReadAllBytes(path)))
            {
                throw new InvalidDataException($"{Path.GetFileName(path)} is not a readable image");
            }

            return texture;
        }

        private static string AssetPath(string fileName)
        {
            var plugin = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return Path.Combine(Path.Combine(plugin ?? ".", AssetFolder), fileName);
        }

        /// <summary>A line a server admin can act on, not a stack trace.</summary>
        private static string Reason(Exception ex)
        {
            return ex is FileNotFoundException || ex is DirectoryNotFoundException
                ? $"no {AssetFolder} folder beside the DLL - copy it from the package"
                : ex.Message;
        }
    }
}
