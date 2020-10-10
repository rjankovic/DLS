using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CD.DLS.Serialization
{
#if false
    public class SSISProjectBasicInfo
  {
    public string Name {get;set;}
    public string Folder {get;set;}
    public string Author {get;set;}
  }

  public class TextFile
  {
    public string FileName { get; set; }
    public string FullPath { get; set; }
    public string Content { get; set; }
  }

  public class DataProvider
  {
    private const string TEMP_FOLDER_PATH = @"C:\temp\CD\BIDoc";
    private const string TEMP_SSIS_PROJECT_ZIP_NAME = "project.zip";

    private string _targetDbConnectionString;

    public string TargetDbConnectionString
    {
      get { return _targetDbConnectionString; }
    }
    public DataProvider(string targetDbConnstring)
    {
      if (!Directory.Exists(TEMP_FOLDER_PATH))
      {
        Directory.CreateDirectory(TEMP_FOLDER_PATH);
      }

      _targetDbConnectionString = targetDbConnstring;
    }

    public List<SSISProjectBasicInfo> GetSSISCatalogProjects()
    {
      NetBridge.UseTemporaryConnstring(_targetDbConnectionString);
      
      DataTable projectTable = NetBridge.ExecuteSelectStatement(
      @"SELECT p.name ProjectName, f.name FolderName, p.deployed_by_name AuthorName 
      FROM SSISDB.internal.projects p
      INNER JOIN SSISDB.internal.folders f ON p.folder_id = f.folder_id
      ");

      List<SSISProjectBasicInfo> projects = projectTable.AsEnumerable().Select(r => new SSISProjectBasicInfo()
        { Author = r["AuthorName"] as string, Folder = r["FolderName"] as string, Name = r["ProjectName"] as string}).ToList();
      NetBridge.UseDefaultConnstring();
      return projects;
    }

    public static bool ByteArrayToFile(string _FileName, byte[] _ByteArray)
    {
      try
      {
        // Open file for reading
        System.IO.FileStream _FileStream =
           new System.IO.FileStream(_FileName, System.IO.FileMode.Create,
                                    System.IO.FileAccess.Write);
        // Writes a block of bytes to this stream using data from
        // a byte array.
        _FileStream.Write(_ByteArray, 0, _ByteArray.Length);

        // close file stream
        _FileStream.Close();

        return true;
      }
      catch (Exception _Exception)
      {
        // Error
        Console.WriteLine("Exception caught in process: {0}",
                          _Exception.ToString());
      }

      // error occured, return false
      return false;
    }

    /*public List<TextFile> GetSSISCatalogProjectFiles(SSISProjectBasicInfo projectInfo)
    {
      SqlCommand cmd = new SqlCommand(_targetDbConnectionString);
      NetBridge.UseTemporaryConnstring(_targetDbConnectionString);
      var res = NetBridge.ExecuteProcedureScalar("[SSISDB].[catalog].[get_project]", new Dictionary<string,object>()
      {{"folder_name", projectInfo.Folder}, {"project_name", projectInfo.Name}});
      
      ByteArrayToFile(Path.Combine(TEMP_FOLDER_PATH, TEMP_SSIS_PROJECT_ZIP_NAME), (byte[])res);
      NetBridge.UseDefaultConnstring();
      Ionic.Zip.ZipFile zip = new ZipFile(Path.Combine(TEMP_FOLDER_PATH, TEMP_SSIS_PROJECT_ZIP_NAME));
      List<TextFile> files = new List<TextFile>();
      foreach(ZipEntry entry in zip.Entries)
      {
        string content;
        using(MemoryStream ms = new MemoryStream())
        {
          entry.Extract(ms);
          ms.Position = 0;
          StreamReader sr = new StreamReader(ms);
          content = sr.ReadToEnd();
        }
        files.Add(new TextFile(){ FileName = entry.FileName, FullPath = Path.Combine(projectInfo.Folder, entry.FileName), Content = content});
      }
      return files;
    }*/
  }
#endif
}
