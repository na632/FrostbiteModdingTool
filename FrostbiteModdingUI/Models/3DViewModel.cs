using SharpDX;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Model;
using FrostySdk.IO;
using FrostySdk.Ebx;
using FrostySdk.Managers;
using FrostySdk.Resources;
using Frostbite.Textures;
using System.Windows.Media;
using Matrix = SharpDX.Matrix;

namespace FrostbiteModdingUI.Models
{
    public class MainViewModel : BaseViewModel
    {
        public Geometry3D FloorModel { get; }
        public Geometry3D SphereModel { get; }
        public Geometry3D TeapotModel { get; }

        public Geometry3D BunnyModel { get; set; }
        public Geometry3D BunnyModel_1 { get; set; }
        public Geometry3D BunnyModel_2 { get; set; }
        public Geometry3D BunnyModel_3 { get; set; }

        public Geometry3D MeshModel2 { get; set; }


        public PhongMaterial FloorMaterial { get; }
        public PhongMaterial SphereMaterial { get; }

        public PhongMaterial BunnyMaterial { get; }

        public PhongMaterial MeshMaterial2 { get; }

        public Matrix[] SphereInstances { get; }

        public Matrix[] BunnyInstances { get; }
        public Matrix[] BunnyInstances_1 { get; }
        public Matrix[] BunnyInstances_2 { get; }
        public Matrix[] BunnyInstances_3 { get; }

        public Matrix[] MeshInstances2 { get; }

        public SSAOQuality[] SSAOQualities { get; } = new SSAOQuality[] { SSAOQuality.High, SSAOQuality.Low };

        public MainViewModel(string file = "test_noSkel.obj", EbxAsset skinnedMeshAsset = null, MeshSet meshSet = null, EbxAssetEntry textureAsset = null)
        {
            try
            {
                EffectsManager = new DefaultEffectsManager() {
                };
                Camera = new PerspectiveCamera()
                {
                    //Position = new System.Windows.Media.Media3D.Point3D(-100, -100, -100),
                    //LookDirection = new System.Windows.Media.Media3D.Vector3D(0, 0, 0),
                    UpDirection = new System.Windows.Media.Media3D.Vector3D(0, 1, 0),
                    //FarPlaneDistance = 3000,
                    //NearPlaneDistance = 1
                     
                };

                var builder = new MeshBuilder();
                builder.AddBox(new Vector3(0, -0.1f, 0), 10, 0.1f, 10);
                FloorModel = builder.ToMesh();

                var reader = new ObjReader();
                if (File.Exists(file))
                {
                    var models = reader.Read(file);
                    //var index = 0;
                    //foreach(var m in models)
                    //{
                    //    MeshGeometryModel3D meshGeometryModel3D = new MeshGeometryModel3D();
                    //    meshGeometryModel3D.Geometry = m.Geometry;
                    //    meshGeometryModel3D.Instances = new Matrix[1]
                    //    {
                    //        Matrix.Translation(0, 0, 0),
                    //    };
                    //    meshGeometryModel3D.Material = new PhongMaterial
                    //    {
                    //        AmbientColor = Colors.White.ToColor4(),
                    //        DiffuseColor = Colors.White.ToColor4(),
                    //        SpecularColor = Colors.Black.ToColor4(),
                    //        SpecularShininess = 0.01f
                    //    };
                    //    if (skinnedMeshAsset != null)
                    //    {
                    //        switch (index)
                    //        {
                    //            case 0:
                    //                BunnyModel = m.Geometry;
                    //                BunnyMaterial = new PhongMaterial
                    //                {
                    //                    AmbientColor = Colors.White.ToColor4(),
                    //                    DiffuseColor = Colors.White.ToColor4(),
                    //                    SpecularColor = Colors.Black.ToColor4(),
                    //                    SpecularShininess = 0.01f
                    //                };
                    //                BunnyInstances = new Matrix[1]
                    //                {
                    //                Matrix.Translation(0, 0, 0),
                    //                };
                    //                Stream textureTest = LoadTexture(skinnedMeshAsset, 0, "colorTexture");
                    //                if (textureTest != null)
                    //                {
                    //                    BunnyMaterial.DiffuseMap = textureTest;
                    //                }
                    //                break;
                    //            case 1:
                    //                MeshModel2 = m.Geometry;
                    //                MeshMaterial2 = new PhongMaterial
                    //                {
                    //                    AmbientColor = Colors.White.ToColor4(),
                    //                    DiffuseColor = Colors.White.ToColor4(),
                    //                    SpecularColor = Colors.Black.ToColor4(),
                    //                    SpecularShininess = 0.01f
                    //                };
                    //                MeshInstances2 = new Matrix[1]
                    //                {
                    //                Matrix.Translation(0, 0, 0),
                    //                };
                    //                Stream texture2 = LoadTexture(skinnedMeshAsset, 0, "colorTexture");
                    //                if (texture2 != null)
                    //                {
                    //                    MeshMaterial2.DiffuseMap = texture2;
                    //                }
                    //                break;
                    //        }
                    //    }
                    //}

                    BunnyModel = models[0].Geometry;
                    BunnyMaterial = new PhongMaterial
                    {
                        AmbientColor = Colors.White.ToColor4(),
                        DiffuseColor = Colors.White.ToColor4(),
                        SpecularColor = Colors.Black.ToColor4(),
                        SpecularShininess = 0.01f
                    };

                    if (models.Count > 1)
                    {
                        MeshModel2 = models[1].Geometry;
                        MeshMaterial2 = new PhongMaterial
                        {
                            AmbientColor = Colors.White.ToColor4(),
                            DiffuseColor = Colors.White.ToColor4(),
                            SpecularColor = Colors.Black.ToColor4(),
                            SpecularShininess = 0.01f
                        };
                        MeshInstances2 = new Matrix[1]
                       {
                        Matrix.Translation(0, 0, 0),
                       };
                    }
                    if (Camera != null && BunnyModel != null)
                    {
                        Camera.Position = new System.Windows.Media.Media3D.Point3D(0.0, BunnyModel.Positions[0].Y, 0.65);
                    }
                    if (skinnedMeshAsset != null)
                    {
                        Stream textureTest = LoadTexture(skinnedMeshAsset, 0, "colorTexture");
                    }
                    if (textureAsset != null)
                    {
                        var resStream = AssetManager.Instance.GetRes(AssetManager.Instance.GetResEntry(textureAsset.Name));
                        if (resStream != null)
                        {
                            Texture t = new Texture(resStream, AssetManager.Instance);
                            if (t != null)
                            {
                                MemoryStream textureDDSStream = new MemoryStream();
                                TextureExporter textureExporter = new TextureExporter();
                                textureDDSStream = textureExporter.ExportToStream(t) as MemoryStream;
                                if (textureDDSStream != null)
                                {
                                    textureDDSStream.Position = 0L;
                                    BunnyMaterial.DiffuseMap = new TextureModel(textureDDSStream);
                                    //BunnyMaterial.DiffuseAlphaMap = new TextureModel(new Color4[] { new Color4(1) }, 1, 1);
                                    if (MeshMaterial2 != null)
                                    {
                                        MeshMaterial2.DiffuseMap = new TextureModel(textureDDSStream);
                                        //MeshMaterial2.DiffuseAlphaMap = new TextureModel(new Color4[] { new Color4(1) }, 1, 1);
                                    }
                                }

                            }
                        }

                        var normalResEntry = AssetManager.Instance.GetResEntry(textureAsset.Name.Replace("color", "normal"));
                        if (normalResEntry != null)
                        {
                            var resStreamN = AssetManager.Instance.GetRes(normalResEntry);
                            if (resStreamN != null)
                            {
                                Texture t = new Texture(resStreamN, AssetManager.Instance);
                                if (t != null)
                                {
                                    MemoryStream textureDDSStream = new MemoryStream();
                                    TextureExporter textureExporter = new TextureExporter();
                                    textureDDSStream = textureExporter.ExportToStream(t) as MemoryStream;
                                    if (textureDDSStream != null)
                                    {
                                        textureDDSStream.Position = 0L;
                                        BunnyMaterial.NormalMap = new TextureModel(textureDDSStream);
                                        if (MeshMaterial2 != null)
                                            MeshMaterial2.NormalMap = new TextureModel(textureDDSStream);
                                    }

                                }
                            }
                        }
                    }

                    ////if (meshSet != null)
                    ////{
                    ////    var section = meshSet.Lods[0].Sections[0];
                    ////    Stream colourTextureDDSStream = LoadTexture(skinnedMeshAsset, section.materialId, "colorTexture");
                    ////    if(colourTextureDDSStream != null)
                    ////    {

                    ////    }
                    ////    //Stream normalTextureDDSStream = LoadTexture(ebxAsset, section.materialId, "normalTexture");
                    ////}
                    //if (models.Count > 1)
                    //{
                    //    BunnyModel_1 = models[1].Geometry;
                    //}
                    //if (models.Count > 2)
                    //{
                    //    BunnyModel_2 = models[2].Geometry;
                    //}
                    //if (models.Count > 3)
                    //{
                    //    BunnyModel_3 = models[3].Geometry;
                    //}
                }
                FloorMaterial = PhongMaterials.White;
                FloorMaterial.AmbientColor = FloorMaterial.DiffuseColor * 0.7f;

                BunnyInstances = new Matrix[1]
                {
                Matrix.Translation(0, 0, 0),
                };
                BunnyInstances_1 = new Matrix[1]
                {
                Matrix.Translation(0, 0, 0),
                };
                BunnyInstances_2 = new Matrix[1]
                {
                Matrix.Translation(0, 0, 0),
                };
                BunnyInstances_3 = new Matrix[1]
                {
                Matrix.Translation(0, 0, 0),
                };
            }
            catch
            {

            }
        }




        private Stream LoadTexture(EbxAsset ebxAsset, int materialId, string textureName)
        {
            dynamic meshMaterial = ((dynamic)ebxAsset.RootObject).Materials[materialId].Internal;
            dynamic shader = meshMaterial.Shader;
            dynamic desiredTextureParameter = null;
            foreach (dynamic textureParameter2 in shader.TextureParameters)
            {
                if (textureParameter2.ParameterName.Equals(textureName, StringComparison.OrdinalIgnoreCase))
                {
                    desiredTextureParameter = textureParameter2;
                    break;
                }
            }
            if (desiredTextureParameter == null)
            {
                Guid shaderGuid = ((PointerRef)shader.Shader).External.FileGuid;
                if (shaderGuid == Guid.Empty)
                {
                    return null;
                }
                EbxAssetEntry shaderAssetEntry = AssetManager.Instance.GetEbxEntry(shaderGuid.ToString());
                if (shaderAssetEntry == null)
                {
                    return null;
                }
                EbxAsset shaderAsset = AssetManager.Instance.GetEbx(shaderAssetEntry);
                dynamic shaderPreset = ((dynamic)shaderAsset.RootObject).ShaderPreset;
                foreach (dynamic textureParameter in shaderPreset.TextureParameters)
                {
                    if (textureParameter.ParameterName.Equals(textureName, StringComparison.OrdinalIgnoreCase))
                    {
                        desiredTextureParameter = textureParameter;
                        break;
                    }
                }
                if (desiredTextureParameter == null)
                {
                    return null;
                }
            }
            Guid textureGuid = ((PointerRef)desiredTextureParameter.Value).External.FileGuid;
            if (textureGuid == Guid.Empty)
            {
                return null;
            }
            EbxAssetEntry textureAssetEntry = AssetManager.Instance.GetEbxEntry(textureGuid);
            if (textureAssetEntry == null)
            {
                return null;
            }
            EbxAsset textureAsset = AssetManager.Instance.GetEbx(textureAssetEntry);
            ulong textureResRid = ((dynamic)textureAsset.RootObject).Resource;
            Texture texture = new Texture(AssetManager.Instance.GetRes(AssetManager.Instance.GetResEntry(textureResRid)), AssetManager.Instance);
            MemoryStream textureDDSStream = new MemoryStream();
            TextureExporter textureExporter = new TextureExporter();
            textureDDSStream = textureExporter.ExportToStream(texture) as MemoryStream;
            //new DDSTextureExporter().Export(texture, textureDDSStream, dispose: false);
            textureDDSStream.Position = 0L;
            return textureDDSStream;
        }

    }



    public abstract class BaseViewModel : ObservableObject, IDisposable
    {
        public const string Orthographic = "Orthographic Camera";

        public const string Perspective = "Perspective Camera";

        private string cameraModel;

        private Camera camera;


        public List<string> CameraModelCollection { get; private set; }

        public string CameraModel
        {
            get
            {
                return cameraModel;
            }
            set
            {
                cameraModel = value;
                //if (SetValue(ref cameraModel, value, "CameraModel"))
                //{
                //    OnCameraModelChanged();
                //}
            }
        }

        public Camera Camera
        {
            get
            {
                return camera;
            }

            protected set
            {
                camera = value;
                //SetValue(ref camera, value, "Camera");
                CameraModel = value is PerspectiveCamera
                                       ? Perspective
                                       : value is OrthographicCamera ? Orthographic : null;
            }
        }
        private IEffectsManager effectsManager;
        public IEffectsManager EffectsManager
        {
            get { return effectsManager; }
            protected set
            {
                //SetValue(ref effectsManager, value);
                effectsManager = value;
            }
        }

        protected OrthographicCamera defaultOrthographicCamera = new OrthographicCamera { Position = new System.Windows.Media.Media3D.Point3D(0, 0, 5), LookDirection = new System.Windows.Media.Media3D.Vector3D(-0, -0, -5), UpDirection = new System.Windows.Media.Media3D.Vector3D(0, 1, 0), NearPlaneDistance = 1, FarPlaneDistance = 100 };

        protected PerspectiveCamera defaultPerspectiveCamera = new PerspectiveCamera { Position = new System.Windows.Media.Media3D.Point3D(0, 0, 5), LookDirection = new System.Windows.Media.Media3D.Vector3D(-0, -0, -5), UpDirection = new System.Windows.Media.Media3D.Vector3D(0, 1, 0), NearPlaneDistance = 0.5, FarPlaneDistance = 150 };

        public event EventHandler CameraModelChanged;

        protected BaseViewModel()
        {
            // camera models
            CameraModelCollection = new List<string>()
            {
                Orthographic,
                Perspective,
            };

            // on camera changed callback
            CameraModelChanged += (s, e) =>
            {
                if (cameraModel == Orthographic)
                {
                    if (!(Camera is OrthographicCamera))
                        Camera = defaultOrthographicCamera;
                }
                else if (cameraModel == Perspective)
                {
                    if (!(Camera is PerspectiveCamera))
                        Camera = defaultPerspectiveCamera;
                }
                else
                {
                    throw new HelixToolkitException("Camera Model Error.");
                }
            };

            // default camera model
            CameraModel = Perspective;

            //Title = "Demo (HelixToolkitDX)";
            //SubTitle = "Default Base View Model";
        }

        protected virtual void OnCameraModelChanged()
        {
            var eh = CameraModelChanged;
            if (eh != null)
            {
                eh(this, new EventArgs());
            }
        }

        public static MemoryStream LoadFileToMemory(string filePath)
        {
            using (var file = new FileStream(filePath, FileMode.Open))
            {
                var memory = new MemoryStream();
                file.CopyTo(memory);
                return memory;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                if (EffectsManager != null)
                {
                    var effectManager = EffectsManager as IDisposable;
                    Disposer.RemoveAndDispose(ref effectManager);
                }
                disposedValue = true;
                GC.SuppressFinalize(this);
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~BaseViewModel()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

}
