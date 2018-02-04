using System;
using System.Reflection;

using NFX;
using NFX.ApplicationModel;
using NFX.Environment;
using NFX.IO.FileSystem;
using NFX.Scripting;

namespace NFX.WinSCP.ITest.IO.FileSystem.FTP
{
  [Runnable]
  public class FTPFileSystemTests : IRunHook
  {
    protected string LACONF = typeof(FTPFileSystemTests).GetText("FTPFileSystemTests.laconf");

    private ConfigSectionNode m_Config;

    private ServiceBaseApplication m_App;

    #region Init/TearDown

      bool IRunHook.Prologue(Runner runner, FID id, MethodInfo method, RunAttribute attr, ref object[] args)
      {
        var t = Type.GetType("NFX.IO.FileSystem.FTP.FTPFileSystem, NFX.WinSCP");
      Console.WriteLine(t.FullName);

        m_Config = LACONF.AsLaconicConfig(handling: ConvertErrorHandling.Throw);
        m_App = new ServiceBaseApplication(null, m_Config);
        return false; //<--- The exception is NOT handled here, do default handling
      }

      bool IRunHook.Epilogue(Runner runner, FID id, MethodInfo method, RunAttribute attr, Exception error)
      {
        DisposableObject.DisposeAndNull(ref m_App);
        return false; //<--- The exception is NOT handled here, do default handling
      }

    #endregion

    private IFileSystem m_FileSystem;
    public IFileSystem FileSystem
    {
      get
      {
        if (m_FileSystem == null) m_FileSystem = NFX.IO.FileSystem.FileSystem.Instances["sftp"];
        return m_FileSystem;
      }
    }

    [Run]
    public void Connect()
    {
      var remotePath = ".";
      var remotePathAttr = m_Config.Navigate("/tests/connect/$path");
      if (remotePathAttr.Exists)
        remotePath = remotePathAttr.ValueAsString();

      using (var session = FileSystem.StartSession(null))
      {
        var dir = session[remotePath] as FileSystemDirectory;
        Console.WriteLine("Files:");
        foreach (var item in dir.FileNames)
          Console.WriteLine("\t{0}", item);
        Console.WriteLine("Directories:");
        foreach (var item in dir.SubDirectoryNames)
          Console.WriteLine("\t{0}", item);
        var file = dir.CreateFile("nfx_ftp_fs.txt");
        file.WriteAllText("TEST");
        var remoteContent = file.ReadAllText();
        Console.WriteLine(remoteContent);

        Aver.AreEqual("TEST", remoteContent);
      }
    }
  }
}
