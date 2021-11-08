using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Betlln.IO
{
    [Obsolete("Use " + nameof(LocalFileSystem))]
    public class FileSystem : IFileSystem
    {
        public bool IsValidPath(string filePath)
        {
            return LocalFileSystem.IsValidPath(filePath);
        }

        public bool DoesFileExist(string filePath)
        {
            return File.Exists(filePath);
        }

        public string GetParentPath(string path)
        {
            return GetParentFolderPath(path);
        }

        public static string GetParentFolderPath(string path)
        {
            return new DirectoryInfo(path).Parent?.FullName;
        }

        public List<string> GetFiles(string folder)
        {
            return Directory.GetFiles(folder).ToList();
        }

        public void SetReadOnly(string filePath, bool throwIfUnsupported = true)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                fileInfo.IsReadOnly = true;
            }
            catch (UnauthorizedAccessException)
            {
                if (throwIfUnsupported)
                {
                    throw;
                }
            }
        }

        public void Copy(string source, string destination, bool overwrite = false)
        {
            FileInfo destinationInfo = new FileInfo(destination);
            if (overwrite && destinationInfo.Exists && destinationInfo.IsReadOnly)
            {
                destinationInfo.IsReadOnly = false;
            }

            File.Copy(source, destination, overwrite);
        }

        public void Delete(string path)
        {
            File.Delete(path);
        }

        public void DeleteAllFilesInFolder(string folderPath)
        {
            foreach (string filePath in Directory.GetFiles(folderPath))
            {
                File.Delete(filePath);
            }
        }

        // ReSharper disable once TooManyArguments
        public Stream OpenStream(string path, FileMode streamMode, FileAccess accessMode, FileShare shareMode)
        {
            return File.Open(path, streamMode, accessMode, shareMode);
        }

		public void Rename(string currentPath, string newPath)
        {
            File.Move(currentPath, newPath);
        }
		
        public void Unblock(string fileName)
        {
            //thanks to http://stackoverflow.com/a/6375373/1687106
            DeleteFile($"{fileName}:Zone.Identifier");
        }

        //thanks to https://stackoverflow.com/questions/770023/how-do-i-get-file-type-information-based-on-extension-not-mime-in-c-sharp
        public string GetExecutableForFileType(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                throw new ArgumentNullException(nameof(extension));
            }

            if (extension.First() != '.')
            {
                extension = $".{extension}";
            }

            string executableValue = GetAssociatedValue(AssocStr.Executable, extension);
            if (string.IsNullOrWhiteSpace(executableValue))
            {
                string appName = GetAssociatedValue(AssocStr.FriendlyAppName, extension) ?? string.Empty;
                if (appName.ToUpper().Equals("MICROSOFT EDGE"))
                {
                    return "MicrosoftEdge.exe";
                }
            }
            else
            {
                return Path.GetFileName(executableValue);
            }

            return null;
        }

        public void CreateDirectoryIfNotExist(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static string GetAssociatedValue(AssocStr valueName, string docType)
        {
            uint pcchOut = 0;
            AssocQueryString(AssocF.Verify, valueName, docType, null, null, ref pcchOut);

            if (pcchOut == 0)
            {
                return null;
            }

            StringBuilder pszOut = new StringBuilder((int)pcchOut);
            AssocQueryString(AssocF.Verify, valueName, docType, null, pszOut, ref pcchOut);
            return pszOut.ToString();
        }

        // ReSharper disable TooManyArguments

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteFile(string name);

        [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint AssocQueryString(AssocF flags, AssocStr str, string pszAssoc, string pszExtra, [Out] StringBuilder pszOut, [In][Out] ref uint pcchOut);

        // ReSharper restore TooManyArguments

        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedMember.Global
        // ReSharper disable InconsistentNaming
        [Flags]
        private enum AssocF
        {
            Init_NoRemapCLSID = 0x1,
            Init_ByExeName = 0x2,
            Open_ByExeName = 0x2,
            Init_DefaultToStar = 0x4,
            Init_DefaultToFolder = 0x8,
            NoUserSettings = 0x10,
            NoTruncate = 0x20,
            Verify = 0x40,
            RemapRunDll = 0x80,
            NoFixUps = 0x100,
            IgnoreBaseClass = 0x200
        }

        private enum AssocStr
        {
            Command = 1,
            Executable,
            FriendlyDocName,
            FriendlyAppName,
            NoOpen,
            ShellNewValue,
            DDECommand,
            DDEIfExec,
            DDEApplication,
            DDETopic
        }
        // ReSharper restore UnusedMember.Global
        // ReSharper restore InconsistentNaming
        // ReSharper restore UnusedMember.Local
    }
}