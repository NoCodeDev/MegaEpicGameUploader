using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MegaEpicGameUploader
{
    [Serializable]
    public struct EpicData
    {
        public string toolPath;
        public string orgId;
        public string clientId;
        public string secret;

        public static void Save<T>(T saveData, string path)
        {
            Stream stream = File.Open(path, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, saveData);
            stream.Close();
        }

        public static T Load<T>(string path)
        {
            Stream stream = null;
            try
            {
                stream = File.Open(path, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();
                return (T)formatter.Deserialize(stream);
            }
            catch(FileNotFoundException fnfe)
            {

            }
            finally
            {
                if(stream != null)
                    stream.Close();
            }
            return default;
        }
    }

    [Serializable]
    public struct ProductData
    {
        public string realName { get; set; }
        public string prodId { get; set; }
        public string artId { get; set; }
        public string stagingArtId { get; set; }
    }

    [Serializable]
    public struct BuildUploadData
    {
        public bool staging;
        public string buildRoot;
        public string cloudDir;
        public string buildVersion;
        public string appLaunch;
        public string appArgs;
    }

    [Serializable]
    public struct BuildVersionData
    {
        public string buildVersion { get; set; }
        public string buildDate { get; set; }
        public string buildStatus { get; set; }
    }

    [Serializable]
    public struct LabelData
    {
        public bool staging;
        public string version;
        public string label;
        public string platform;
    }
}
