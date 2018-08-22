#define SELF_UPDATE
//#define try__
#define Windows
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Threading;

namespace Updater
{
    public static class constants
    {
        public const string path_here = "";
        public const string path_old = "old/";
        public const string path_updated = "new/";
        public const string path_updated_local = "new/local";
#if Windows
        public const string path_installer = "Installer/Amonikano_installer.exe";
#elif LinuxARM
        public const string path_installer = "Installer/Amonikano_installer";
#endif
    }
    public static class Program
    {
        private const string built_zip_file_updated = "https://github.com/Pegasust/Amonikano/archive/built_pegasus.zip";
        static SerializableUpdateInfo current;
        static SerializableUpdateInfo available_online;
        static SerializableUpdateInfo available_local;
        public static bool start(string[] args)
        {
            string full_path_here;
            if (File.Exists(full_path_here = constants.path_here + SerializableUpdateInfo.filename))
            {
                current = SerializableUpdateInfo.Deserialize<SerializableUpdateInfo>(constants.path_here);
            }
            string full_path_updated;
            if (File.Exists(full_path_updated = constants.path_updated + SerializableUpdateInfo.filename))
            {
                available_local = SerializableUpdateInfo.Deserialize<SerializableUpdateInfo>(constants.path_updated);
            }
#if DEBUG
            Console.WriteLine("from Updater.Program.start: ");
            Console.WriteLine("+_full_path_here: " + full_path_here);
            Console.WriteLine("+_full_path_updated: " + full_path_updated);
#endif
            //Check for available online
            available_online = GetUpdates();

            switch (current | available_local)
            {
                case SerializableUpdateInfo.update_action.keep_old:
                Console.WriteLine("Keep old");
                return false;
                case SerializableUpdateInfo.update_action.update_new:
                Console.WriteLine("Update new");
                OpenInstaller();
                Thread.Sleep(int.MaxValue);
                return true;
            }

            //OPEN INSTALLER IF NECESSARY
            //BE SURE TO CLOSE THE PROGRAM IF OPEN INSTALLER
            //System.Environment.Exit(0);
#if try__
            OpenInstaller();
            //Thread.Sleep(int.MaxValue);
#endif

            return false;
        }
        /// <summary>
        /// OVERRIDE; Get the zip from github, extract, and delete the zip
        /// </summary>
        //        public static void GetUpdates()
        //        {
        //            using (var client = new WebClient())
        //            {
        //                if (!File.Exists(constants.path_updated))
        //                {
        //                    Directory.CreateDirectory(constants.path_updated);
        //                }
        //                string updated_zip;
        //#if TRACE
        //                Console.WriteLine("Downloading update zip");
        //#endif
        //                client.DownloadFile(built_zip_file_updated, updated_zip = constants.path_updated + "built_pegasus.zip");
        //#if TRACE
        //                Console.WriteLine("Download complete, files stored in " + updated_zip);
        //#endif
        //                string extracted = Path.Join(constants.path_updated, "extracted");
        //#if TRACE
        //                Console.WriteLine("Seeing inside folder " + extracted + " to find any sub-directories");
        //#endif
        //                string[] files_in_extracted = Directory.GetDirectories(extracted);

        //                if (files_in_extracted.Length > 0)
        //                {
        //#if TRACE
        //                    Console.WriteLine("There is at least one file inside " + extracted);
        //#endif
        //                    foreach (string file in files_in_extracted)
        //                    {
        //#if TRACE
        //                        Console.WriteLine("Deleting " + file);
        //#endif
        //                        Directory.Delete(file,true);
        //#if TRACE
        //                        Console.WriteLine("Deletion successful.");
        //#endif
        //                    }
        //                }
        //#if TRACE
        //                Console.WriteLine("Unzipping " + updated_zip);
        //#endif
        //                System.IO.Compression.ZipFile.ExtractToDirectory(updated_zip, extracted);
        //#if TRACE
        //                Console.WriteLine("Unzip complete, deleting " + updated_zip);
        //#endif
        //            }
        //        }

        private static void OpenInstaller()
        {
            System.Diagnostics.Process.Start(constants.path_installer, "-p " + string.Join(' ', constants.path_here, constants.path_old, constants.path_updated + "extracted/", constants.path_here));
        }
        /// <summary>
        /// OVERRIDE; Get the zip from github, extract, and delete the zip
        /// </summary>
        public static SerializableUpdateInfo GetUpdates()
        {
            using (var client = new WebClient())
            {
                if (!File.Exists(constants.path_updated))
                {
                    Directory.CreateDirectory(constants.path_updated);
                }
                string updated_zip;
#if TRACE
                Console.WriteLine("Downloading update zip");
#endif
                client.DownloadFile(built_zip_file_updated, updated_zip = constants.path_updated + "built_pegasus.zip");
#if TRACE
                Console.WriteLine("Download complete, files stored in " + updated_zip);
#endif
                string extracted = Path.Join(constants.path_updated, "extracted");
                Directory.CreateDirectory(extracted);
#if TRACE
                Console.WriteLine("Seeing inside folder " + extracted + " to find any sub-directories");
#endif
                string[] files_in_extracted = Directory.GetDirectories(extracted);

                if (files_in_extracted.Length > 0)
                {
#if TRACE
                    Console.WriteLine("There is at least one file inside " + extracted);
#endif
                    foreach (string file in files_in_extracted)
                    {
#if TRACE
                        Console.WriteLine("Deleting " + file);
#endif
                        Directory.Delete(file, true);
#if TRACE
                        Console.WriteLine("Deletion successful.");
#endif
                    }
                }
#if TRACE
                Console.WriteLine("Unzipping " + updated_zip);
#endif
                System.IO.Compression.ZipFile.ExtractToDirectory(updated_zip, extracted);
#if TRACE
                Console.WriteLine("Unzip complete, deleting " + updated_zip);
                Console.WriteLine("Bad algorithm started");
#endif
                string new_root = extracted;
#if TRACE
                Console.WriteLine("From new_root: " + new_root);
#endif
                if (check_for_version(new_root, out new_root))
                {
#if TRACE
                    Console.WriteLine("Found version at " + new_root);
#endif
                    return SerializableUpdateInfo.Deserialize<SerializableUpdateInfo>(new_root);
                }
                else
                {
#if TRACE
                    Console.WriteLine("No version is found");
#endif
                    return null;
                }
            }
        }

        private static bool check_for_version(string from_path, out string file_path)
        {
#if TRACE
            Console.WriteLine("root: " + from_path);
#endif
            if (!has_version_file(from_path))
            {
                string[] subs = Directory.GetDirectories(from_path);
#if TRACE
                Console.WriteLine("Found " + subs.Length + " sub directories.");
#endif
                foreach (string sub in subs)
                {
                    check_for_version(sub, out file_path);
                }
#if TRACE
                Console.WriteLine("Version file not found");
#endif
                file_path = null;
                return false;
            }
            else
            {
                file_path = from_path;
#if TRACE
                Console.WriteLine("Found version file from " + file_path);
#endif
                return true;
            }
        }
        private static bool has_version_file(string path)
        {
            string[] subs = Directory.GetFiles(path);
            foreach (string file_path in subs)
            {
                if (file_path.EndsWith(SerializableUpdateInfo.filename))
                {
                    return true;
                }
            }
            return false;
        }

    }
    [Serializable]
    public class SerializableUpdateInfo
    {
        public const string filename = "amononika.v";
        DateTime build_version_time;
        build_type build_branch = build_type.pegasus;
        os build_os = os.linux_arm;

        public SerializableUpdateInfo()
        {
            build_version_time = DateTime.Now;
        }

        /// <summary>
        /// Returns an info that is better to be kept on
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static update_action operator |(SerializableUpdateInfo current, SerializableUpdateInfo updated)
        {
            if (updated != null && updated.build_version_time > current.build_version_time)
            {
                return update_action.update_new;
            }
            else
            {
                return update_action.keep_old;
            }
        }

        public enum update_action
        {
            keep_old,
            update_new
        }

        public void Serialize(string path)
        {
            string fullpath = path + filename;
            FileStream stream;
            if (!File.Exists(fullpath))
            {
#if DEBUG
                Console.WriteLine("__fullpath not exist, creating new file");
#endif
                stream = File.Create(fullpath);
#if DEBUG
                Console.WriteLine("__file created at " + fullpath);
#endif
            }
            else
            {
                stream = File.OpenWrite(fullpath);
            }
#if DEBUG
            Console.WriteLine("__Open written " + fullpath);
            Console.WriteLine("__Writing: \n" + this.ToString());
#endif

            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(stream, this);
            stream.Flush();
            stream.Close();
            stream.Dispose();
#if DEBUG
            Console.WriteLine("__Written:\n" + Deserialize<SerializableUpdateInfo>(path).ToString());
#endif
        }

        public static T Deserialize<T>(string path)
        {
            FileStream stream = File.OpenRead(path + filename);
            BinaryFormatter formatter = new BinaryFormatter();
            T returner = (T)formatter.Deserialize(stream);
            stream.Flush();
            stream.Close();
            stream.Dispose();
            return returner;
        }


        public override int GetHashCode()
        {
            return build_version_time.GetHashCode();
        }

        public override string ToString()
        {
            return
                "{\n"
                + "+ build_version: " + build_version_time.ToLongTimeString() + "\n"
                + "+ build_branch: " + build_branch.ToString() + "\n"
                + " + os: " + build_os.ToString() + "\n"
                + "}";
        }
    }

    public enum build_type
    {
        pegasus, bob, master
    }
    public enum os
    {
        linux_arm, windowsx64
    }
}
