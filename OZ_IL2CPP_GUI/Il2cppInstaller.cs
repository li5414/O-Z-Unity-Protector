﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace OZ_IL2CPP_GUI
{
    static class Il2cppInstaller
    {
        public static void Install(string unityExePath)
        {
            if (!unityExePath.EndsWith("Unity.exe"))
            {
                Utilitys.ShowError("没有选择正确的Unity.exe");
                return;
            }

            UnityVersion uniVer = new UnityVersion(GetUnityVersion(unityExePath));
            string il2Ver = Il2cppLibUtilitys.GetVersion(uniVer);

            /*if (!Il2cppLibUtilitys.HasOZSupport(il2Ver))
            {
                Utilitys.ShowError("OZ Il2cpp暂不支持该版本Unity\nIl2cpp版本:" + il2Ver);
                return;
            }*/

            //预留方法
            //BaseEncrypter.InvokeOZIL2CPPSecurity("--null");

            string editorPath = Path.GetDirectoryName(unityExePath);

            //Check
            if (!CheckWritePermission(editorPath))
            {
                return;
            }

            //CheckBackup
            string ozsignFile = editorPath + "/ozli2cpp";
            if (File.Exists(ozsignFile))
            {
                Utilitys.ShowError("您已经安装OZIl2cpp\n请勿重复安装!");
                return;
            }
            //Create sign
            File.WriteAllText(ozsignFile, GetUnityVersion(unityExePath));
            //Backup
            //CreateBackup(editorPath);

            string s = BaseEncrypter.InvokeOZIL2CPPSecurity("--proclib-p", "\"" + editorPath + "/Data/il2cpp/libil2cpp/" + "\"");
            if (!s.Contains("succ"))
            {
                Utilitys.ShowError("发生错误:\n"+s);
                File.WriteAllText("error.log", s);
                UnInstall(unityExePath);
                return;
            }
            //Move
            /*if(!InstallLibil2cpp(editorPath, il2Ver))
            {
                //Uninstall if failed
                UnInstall(unityExePath);
                return;
            }*/

            //Utilitys.CopyDirectory()
            Utilitys.ShowMsg("安装成功!");
        }

        public static void UpdateConfigIl2cppVersion(string v)
        {
            
        }

        public static string GetUnityVersion(string fp)
        {
            FileVersionInfo f_vi = FileVersionInfo.GetVersionInfo(fp);
            return f_vi.FileVersion;
        }

        public static bool CheckWritePermission(string dir)
        {
            try
            {
                if (!Directory.Exists(dir))
                {
                    Utilitys.ShowError("内部错误\n文件夹不存在");
                }
                File.WriteAllText(dir + "/z9.txt", "TestPermission");
                File.Delete(dir + "/z9.txt");
            }
            catch
            {
                Utilitys.ShowError("无读写权限" + "\n" +
                    "请以管理员身份运行");
                //提升权限
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.FileName = Application.ExecutablePath;
                //设置启动动作,确保以管理员身份运行
                startInfo.Verb = "runas";
                Process.Start(startInfo);
                Application.Exit();
                return false;
            }
            return true;
        }

        public static void UnInstall(string unityExePath)
        {
            string editorPath = Path.GetDirectoryName(unityExePath);

            if (!unityExePath.EndsWith("Unity.exe"))
            {
                Utilitys.ShowError("没有选择正确的Unity.exe");
                return;
            }

            //Check
            if (!CheckWritePermission(editorPath))
            {
                return;
            }

            string ozsignFile = editorPath + "/ozli2cpp";
            if (!File.Exists(ozsignFile))
            {
                Utilitys.ShowError("您还没有安装OZIl2cpp\n卸载失败!");
                return;
            }

            string s = BaseEncrypter.InvokeOZIL2CPPSecurity("--restorelib-p", "\"" + editorPath + "/" + "\"");
            if (!s.Contains("succ"))
            {
                Utilitys.ShowError("发生错误:\n" + s);
                //UnInstall(unityExePath);
                return;
            }
            
            File.Delete(ozsignFile);

            /*if (!UnpackBackup(editorPath))
            {
                return;
            }*/

            Utilitys.ShowMsg("卸载成功!");
        }

        [Obsolete]
        static bool InstallLibil2cpp(string edtp,string ver)
        {
            string libIl2cppPath = edtp + "\\Data\\il2cpp\\";
            try
            {
                Utilitys.CopyDirectory("./Generation/" + ver + "/libil2cpp", libIl2cppPath);
            }
            catch(Exception e)
            {
                Utilitys.ShowError("安装OZIl2cpp失败"+"\n错误原因:\n" + e.ToString());
                return false;
            }
            return true;
        }

        static void CreateBackup(string editorPath)
        {
            //Backup
            string libIl2cppPath = editorPath + "\\Data\\il2cpp\\libil2cpp\\";
            Zip.CreateZip(libIl2cppPath, editorPath + "/libil2cpp.zip");
        }

        static bool UnpackOZPack(string editorPath, string ozZip)
        {
            //CheckBackup
            string zipPath = ozZip;
            if (!File.Exists(zipPath))
            {
                Utilitys.ShowError("您没有安装OZIl2cpp或删除了备份文件!");
                return false;
            }

            //Decompress
            string libIl2cppPath = editorPath + "\\Data\\il2cpp\\libil2cpp\\";
            try
            {
                if (Directory.Exists(libIl2cppPath))
                    Directory.Delete(libIl2cppPath, true);
            }
            catch(Exception e)
            {
                Utilitys.ShowError($"删除OZIl2cpp失败\n路径:{libIl2cppPath}\nEx:{e}");
                return false;
            }
            try
            {
                Zip.UnZip(zipPath, libIl2cppPath + "/");
            }
            catch
            {
                return false;
            }
            //删除备份
            File.Delete(zipPath);
            return true;
        }

        static bool UnpackBackup(string editorPath)
        {
            //CheckBackup
            string zipPath = editorPath + "/libil2cpp.zip";
            if (!File.Exists(zipPath))
            {
                Utilitys.ShowError("您没有安装OZIl2cpp或删除了备份文件!");
                return false;
            }

            //Decompress
            string libIl2cppPath = editorPath + "\\Data\\il2cpp\\libil2cpp\\";
            try
            {
                if (Directory.Exists(libIl2cppPath))
                    Directory.Delete(libIl2cppPath, true);
            }
            catch (Exception e)
            {
                Utilitys.ShowError($"删除OZIl2cpp失败\n路径:{libIl2cppPath}\nEx:{e}");
                return false;
            }
            try
            {
                Zip.UnZip(zipPath, libIl2cppPath + "/");
            }
            catch
            {
                return false;
            }
            //删除备份
            File.Delete(zipPath);
            return true;
        }
    }
}