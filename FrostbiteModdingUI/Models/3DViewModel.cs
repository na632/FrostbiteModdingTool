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

        public PhongMaterial FloorMaterial { get; }
        public PhongMaterial SphereMaterial { get; }

        public PhongMaterial BunnyMaterial { get; }
        public Matrix[] SphereInstances { get; }

        public Matrix[] BunnyInstances { get; }
        public Matrix[] BunnyInstances_1 { get; }
        public Matrix[] BunnyInstances_2 { get; }
        public Matrix[] BunnyInstances_3 { get; }

        public SSAOQuality[] SSAOQualities { get; } = new SSAOQuality[] { SSAOQuality.High, SSAOQuality.Low };

        public MainViewModel()
        {
            EffectsManager = new DefaultEffectsManager();
            Camera = new PerspectiveCamera()
            {
                Position = new System.Windows.Media.Media3D.Point3D(0, 0.001f, 0.001f),
                LookDirection = new System.Windows.Media.Media3D.Vector3D(0, -1, -1),
                UpDirection = new System.Windows.Media.Media3D.Vector3D(0, 1, 0),
                FarPlaneDistance = 200,
                NearPlaneDistance = 0.1
            };

            var builder = new MeshBuilder();
            builder.AddBox(new Vector3(0, -0.1f, 0), 10, 0.1f, 10);
            //builder.AddBox(new Vector3(-7, 2.5f, 0), 5, 5, 5);
            //builder.AddBox(new Vector3(-5, 2.5f, -5), 5, 5, 5);
            FloorModel = builder.ToMesh();

            builder = new MeshBuilder();
           // builder.AddSphere(Vector3.Zero, 1);
            SphereModel = builder.ToMesh();

            var reader = new ObjReader();
            if (File.Exists("test_noSkel.obj"))
            {
                var models = reader.Read("test_noSkel.obj");

                BunnyModel = models[0].Geometry;
                BunnyMaterial = PhongMaterials.PolishedCopper;
                BunnyMaterial.AmbientColor = BunnyMaterial.DiffuseColor * 0.5f;

                if(models.Count > 1)
                {
                    BunnyModel_1 = models[1].Geometry;
                }
                if (models.Count > 2)
                {
                    BunnyModel_2 = models[2].Geometry;
                }
                if (models.Count > 3)
                {
                    BunnyModel_3 = models[3].Geometry;
                }
            }
            FloorMaterial = PhongMaterials.PureWhite;
            FloorMaterial.AmbientColor = FloorMaterial.DiffuseColor * 0.5f;
            SphereMaterial = PhongMaterials.Red;
            SphereMaterial.AmbientColor = SphereMaterial.DiffuseColor * 0.5f;

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
