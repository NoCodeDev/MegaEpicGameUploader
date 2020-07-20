using System;
using System.IO;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MegaEpicGameUploader
{
    public static class EpicApi
    {
        // Authentication
        const string ModeKey = "-mode=";
        const string OrganisationKey = "-OrganizationId=";
        const string ProductKey = "-ProductId=";
        const string ArtifactKey = "-ArtifactId=";
        const string ClientKey = "-ClientId=";
        const string SecretKey = "-ClientSecret=";
        //const string SecretKey = "-ClientSecretEnvVar=";

        // Patch Gen
        const string FeatureLevelKey = "-FeatureLevel=";
        const string BuildRootKey = "-BuildRoot=";
        const string CloudDirKey = "-CloudDir=";
        const string BuildVersionKey = "-BuildVersion=";
        const string AppLaunchKey = "-AppLaunch=";
        const string AppArgsKey = "-AppArgs=";

        // ListBinaries
        const string OutputFileKey = "-OutputFile=";

        // LabelBuild
        const string LabelKey = "-Label=";
        const string PlatformKey = "-Platform=";

        public enum EpicMode
        {
            PatchGeneration,
            LabelBinary,
            ListBinaries,
            DeleteBinary,
            UnlabelBinary
        };

        public delegate void UploadProgress(int files);
        public static UploadProgress uploadProgress;
        public delegate void BuildVersion(BuildVersionData data);
        public static BuildVersion buildVersion;
        public delegate void Log(string logLine);
        public static Log log;

        private static Dictionary<Process, Action> exitActions = new Dictionary<Process, Action>();
        
        public static ProcessStartInfo BuildCommand(EpicData data, ProductData productData, EpicMode mode, bool staging)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(data.toolPath));
            processStartInfo.FileName = "cmd.exe";

            string command = "/C " + Path.GetFileName(data.toolPath);
            AddParameter(ref command, ModeKey, mode.ToString());
            AddParameter(ref command, OrganisationKey, data.orgId);
            AddParameter(ref command, ProductKey, productData.prodId);
            AddParameter(ref command, ArtifactKey, staging ? productData.stagingArtId : productData.artId);
            AddParameter(ref command, ClientKey, data.clientId);
            AddParameter(ref command, SecretKey, data.secret);
            /*AddParameter(ref command, SecretKey, "Secret");
            processStartInfo.EnvironmentVariables.Add("Secret", data.secret);
            processStartInfo.UseShellExecute = false;*/
            processStartInfo.Arguments = command;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            return processStartInfo;
        }

        public static ProcessStartInfo BuildPatchGenCommand(EpicData epicData, ProductData productData, BuildUploadData buildData)
        {
            ProcessStartInfo processStartInfo = BuildCommand(epicData, productData, EpicMode.PatchGeneration, buildData.staging);
            string command = "";
            AddParameter(ref command, FeatureLevelKey, "Latest");
            AddParameter(ref command, BuildRootKey, buildData.buildRoot);
            AddParameter(ref command, CloudDirKey, buildData.cloudDir);
            AddParameter(ref command, BuildVersionKey, buildData.buildVersion);
            AddParameter(ref command, AppLaunchKey, buildData.appLaunch);
            AddParameter(ref command, AppArgsKey, buildData.appArgs);
            processStartInfo.Arguments += command;
            return processStartInfo;
        }

        public static ProcessStartInfo BuildLabelCommand(EpicData epicData, ProductData productData, LabelData labelData)
        {
            ProcessStartInfo processStartInfo = BuildCommand(epicData, productData, EpicMode.LabelBinary, labelData.staging);
            string command = "";
            AddParameter(ref command, BuildVersionKey, labelData.version);
            AddParameter(ref command, LabelKey, labelData.label);
            AddParameter(ref command, PlatformKey, labelData.platform);
            processStartInfo.Arguments += command;
            return processStartInfo;
        }

        public static ProcessStartInfo BuildUnlabelCommand(EpicData epicData, ProductData productData, LabelData labelData)
        {
            ProcessStartInfo processStartInfo = BuildCommand(epicData, productData, EpicMode.UnlabelBinary, labelData.staging);
            string command = "";
            AddParameter(ref command, BuildVersionKey, labelData.version);
            AddParameter(ref command, LabelKey, labelData.label);
            AddParameter(ref command, PlatformKey, labelData.platform);
            processStartInfo.Arguments += command;
            return processStartInfo;
        }

        public static ProcessStartInfo BuildListCommand(EpicData epicData, ProductData productData, bool staging, string outputFile)
        {
            ProcessStartInfo processStartInfo = BuildCommand(epicData, productData, EpicMode.ListBinaries, staging);
            string command = "";
            AddParameter(ref command, OutputFileKey, outputFile);
            processStartInfo.Arguments += command;
            return processStartInfo;
        }

        public static ProcessStartInfo BuildDeleteCommand(EpicData epicData, ProductData productData, string version, bool staging)
        {
            ProcessStartInfo processStartInfo = BuildCommand(epicData, productData, EpicMode.DeleteBinary, staging);
            string command = "";
            AddParameter(ref command, BuildVersionKey, version);
            processStartInfo.Arguments += command;
            return processStartInfo;
        }

        static void AddParameter(ref string command, string parameterKey, string parameterValue)
        {
            switch(parameterKey)
            {
                case FeatureLevelKey:
                case ModeKey:
                    command += (" " + parameterKey + parameterValue);
                    return;
                default:
                    command += (" " + parameterKey + "\"" + parameterValue + "\"");
                    return;
            }
            
        }

        public static void Run(ProcessStartInfo processStartInfo, Action exitAction = null)
        {
            Process cmd = new Process();
            cmd.StartInfo = processStartInfo;
            cmd.EnableRaisingEvents = true;
            cmd.OutputDataReceived += OutputHandler;
            cmd.Exited += Exited;
            try
            {
                cmd.Start();
                cmd.BeginOutputReadLine();
                exitActions[cmd] = exitAction;
            }
            catch(System.ComponentModel.Win32Exception w32e)
            {
                MessageBox.Show(processStartInfo.WorkingDirectory + "\n" + w32e.Message, "Error");
            }
        }

        private static void Exited(object sender, EventArgs args)
        {
            Process cmd = sender as Process;
            if (cmd == null) return;
            cmd.OutputDataReceived -= OutputHandler;
            cmd.Exited -= Exited;
            cmd.Close();
            if (exitActions[cmd] != null)
            {
                exitActions[cmd]();
                exitActions.Remove(cmd);
            }
        }

        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (outLine.Data == null) return;
            if (outLine.Data.StartsWith("Successfully uploaded "))
            {
                string[] components = outLine.Data.Substring("Successfully uploaded ".Length).Split(' ');
                uploadProgress(int.Parse(components[0]));
                //Successfully uploaded x files
            }
            if(outLine.Data.StartsWith("Chunk generation complete."))
            {
                uploadProgress(-1);
            }
            //if (outLine.Data.StartsWith("Retrieved binary: "))
            if (outLine.Data.Contains("Created: "))
            {
                //Retrieved binary: 1.3 - Created: 2019-05-02T11:25:28.638Z
                string split = " - Created: ";
                string split2 = ",  Labels: ";
                //string back = outLine.Data.Substring("Retrieved binary: ".Length);
                string back = outLine.Data.Substring("\t".Length);
                string[] components = back.Split(new string[] { split, split2 }, StringSplitOptions.None);
                buildVersion(new BuildVersionData { buildVersion = components[0], buildDate = components[1], buildStatus = components.Length >= 3 ? components[2] : "" });
            }

            log(outLine.Data);
        }
    }
}
