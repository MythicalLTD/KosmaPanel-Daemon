namespace KosmaPanel.Managers.FileManager
{
    public class FileManager
    {
        public bool MFolderExists() 
        {
            string filePath = "/etc/KosmaPanel/migrate/info.md";
            if (File.Exists(filePath)) {
                return true;
            } else {
                return false;
            }
        }
    }
}