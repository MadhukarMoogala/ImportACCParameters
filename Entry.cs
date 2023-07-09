using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Autodesk.Forge.Model;
using Autodesk.Forge;
using System.Threading;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Windows.OPM;
using System.Runtime.InteropServices;
using Autodesk.AutoCAD.LayerManager;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Interop.Common;
using System.Runtime.InteropServices.ComTypes;
using System.Net.Http;

[assembly: ExtensionApplication(typeof(ImportACCParameters.Plugin))]
[assembly: CommandClass(typeof(ImportACCParameters.Entry))]

namespace ImportACCParameters
{

    [Guid("D029DA19-78A0-41FE-86A2-E5FBA9FC73DA"),
     ProgId("OPMNetSample.CustomProperty.1"), 
     ClassInterface(ClassInterfaceType.None),
     ComDefaultInterface(typeof(IDynamicProperty2)), 
     ComVisible(true)]
    public class AssetProperty : IDynamicProperty2
    {
        private IDynamicPropertyNotify2 m_pSink = null;
        public void GetGUID(out Guid propGUID)
        {
            propGUID = new Guid("D029DA19-78A0-41FE-86A2-E5FBA9FC73DA");
        }
        public void GetDisplayName(out string szName)
        {
            szName = Entry.FetchParameterName;
        }
        public void IsPropertyEnabled(object pUnk, out int bEnabled)
        {
            bEnabled = 1;
        }
        public void IsPropertyReadOnly(out int bReadonly)
        {
            bReadonly = 1;
        }
        public void GetDescription(out string szName)
        {
            szName = Entry.FetchParameterDescription;
        }
        public void GetCurrentValueName(out string szName)
        {
            throw new System.NotImplementedException();
        }
        public void GetCurrentValueType(out ushort varType)
        {
            varType = 3;
        }
        public void GetCurrentValueData(object pUnk, ref object pVarData)
        {
            if (pUnk is AcadObject obj)
            {
                Document doc =
                  Application.DocumentManager.MdiActiveDocument;
                Transaction tr =
                  doc.TransactionManager.StartTransaction();
                using (tr)
                {
                    DBObject o = tr.GetObject(
                        new ObjectId((IntPtr)obj.ObjectID),
                        OpenMode.ForRead
                      );
                    pVarData = Entry.FetchAssetIdFromParameters(o);
                }
            }
            else
                pVarData = 0;
        }

       

        public void SetCurrentValueData(object pUnk, object varData)
        {
        }
        public void Connect(object pSink)
        {
            m_pSink = (IDynamicPropertyNotify2)pSink;
        }
        public void Disconnect()
        {
            m_pSink = null;
        }
    }

   
    public class Forge
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class ForgeConfiguration
    {
        public Forge Forge { get; set; }
    }
    public class Entry
    {
        public static Random randomInt  =>   new Random();
        public static string BearerToken = string.Empty;    
        private static string ParameterName {  get; set; }
        private static string ParameterDescription { get; set; }
        public static string FetchParameterName
        {
            get
            {
                return ParameterName;
            }
        }

        public static string FetchParameterDescription
        {
            get 
            { 
                return ParameterDescription; 
            } 
        }
        public static long FetchAssetIdFromParameters(DBObject dbEntity)
        {
            var entType = dbEntity.Id.ObjectClass.Name;
            long assetId;
            switch (entType)
            {
                case "AcDbBlockReference":
                    {
                        assetId = randomInt.Next();
                        break;
                    }
                case "AcDbLine":
                    {
                        assetId = randomInt.Next();                        
                        break;
                    }
                default:
                    {
                        assetId = default;                        
                        break;
                    }
            }

            return assetId;
        }

        public static void Print(string msg)
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            var ed = doc.Editor;
            ed.WriteMessage($"{msg}\n");
        }
        public static string GetAssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
        public static string Login()
        {
            //Create a JSON file with Forge App credentials.
            /*{
                "Forge": {
                    "ClientId": "",
                    "ClientSecret": ""
                }
            }*/

            var forgeConfiguration = JsonConvert.DeserializeObject<ForgeConfiguration>(
                File.ReadAllText(
                    Path.Combine(GetAssemblyDirectory,
                    "appsettings.user.json")));

            var oAuthHandler = OAuthHandler.Create(forgeConfiguration.Forge);
            string token = string.Empty;
            //We want to sleep the thread until we get 3L access_token.
            //https://stackoverflow.com/questions/6306168/how-to-sleep-a-thread-until-callback-for-asynchronous-function-is-received
            AutoResetEvent stopWaitHandle = new AutoResetEvent(false);
            oAuthHandler.Invoke3LeggedOAuth(async (bearer) =>
            {
                // This is our application delegate. It is called upon success or failure
                // after the process completed
                if (bearer == null)
                {
                    Console.Error.WriteLine("Sorry, Authentication failed!", "3legged test");
                    return;
                }
                token = bearer.access_token;
                // The call returned successfully and you got a valid access_token.                
                DateTime dt = DateTime.Now;
                dt.AddSeconds(double.Parse(bearer.expires_in.ToString()));
                UserProfileApi profileApi = new UserProfileApi();
                profileApi.Configuration.AccessToken = bearer.access_token;
                DynamicJsonResponse userResponse = await profileApi.GetUserProfileAsync();
                UserProfile user = userResponse.ToObject<UserProfile>();
                Print($"Hello {user.FirstName} !!, You are Logged in!");            
                stopWaitHandle.Set();
            });
            stopWaitHandle.WaitOne();
            return token;

        }
        [CommandMethod("ACCLOGIN")]
        public static void AccLogin()
        {
            BearerToken = Login();
            Print($"Bearer Token:{BearerToken}");                          
                
        }
        [CommandMethod("IMPORTPARAMETERS")]
        public static void ImportParameters()
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            var ed = doc.Editor;
            List<ParametersResult> parameters;
            using(var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://developer.api.autodesk.com/parameters/v1/");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {BearerToken}");
                ed.WriteMessage("\nParameters Import Begin");
                parameters =  ParameterHandler.ParametersServiceRequest(client);
                ed.WriteMessage("\nParameters Import End");
            }

            ed.PointMonitor += (object sender, PointMonitorEventArgs e) =>
            {
                FullSubentityPath[] paths = e.Context.GetPickedEntities();

                foreach (FullSubentityPath path in paths)
                {
                    ObjectId id = path.GetObjectIds()[0];
                    switch (id.ObjectClass.Name)
                    {
                        case "AcDbBlockReference":
                            {
                                ParameterName = parameters[1].Name;
                                ParameterDescription = parameters[1].Description;
                                break;
                            }
                        case "AcDbLine":
                            {
                                ParameterName = parameters[0].Name;
                                ParameterDescription = parameters[0].Description;
                                break;
                            }
                        default:
                            {                               
                                ParameterName = default;
                                break;
                            }
                    }
                }
            };

        }
        


    }

    public class Plugin : IExtensionApplication
    {
        protected internal AssetProperty assetProperty = null;
        public void Initialize()
        {
            Assembly.LoadFrom("OPMNetExt.dll");
            Dictionary classDict = SystemObjects.ClassDictionary;
            RXClass block = (RXClass)classDict.At("AcDbBlockReference");
            RXClass line = (RXClass)classDict.At("AcDbLine");
            assetProperty = new AssetProperty();
            IPropertyManager2 pPropMan = (IPropertyManager2)xOPM.xGET_OPMPROPERTY_MANAGER(block);            
            pPropMan.AddProperty(assetProperty);
            pPropMan = (IPropertyManager2)xOPM.xGET_OPMPROPERTY_MANAGER(line);           
            pPropMan.AddProperty(assetProperty);
            
        }
        public void Terminate()
        {
            Dictionary classDict = SystemObjects.ClassDictionary;
            RXClass block = (RXClass)classDict.At("AcDbBlockReference");
            RXClass line = (RXClass)classDict.At("AcDbLine");
            IPropertyManager2 pPropMan = (IPropertyManager2)xOPM.xGET_OPMPROPERTY_MANAGER(block);
            pPropMan.RemoveProperty((object)assetProperty);
            pPropMan = (IPropertyManager2)xOPM.xGET_OPMPROPERTY_MANAGER(line);
            assetProperty = null;
        }
    }
}
