using System.IO;
using System.Xml;

namespace RASW
{
    public class AppSettings 
	{
		public string strDataFile;
		
		private const bool csChecked = true;

		#region // Constructors
		public AppSettings() {}

		public AppSettings(string strFileName) {LoadDataFile(strFileName, "");}
		
		public AppSettings(string strFileName, string xmlRootTags) {LoadDataFile(strFileName, xmlRootTags);}

		#endregion 

        public void LoadDataFile(string strFileName, string xmlRootTags)
        {

            FileInfo fileInfo = new FileInfo(strFileName);
            if (strFileName.Length != 0)
                if (fileInfo.Exists == false)
                {
                    // Create the file and create root tags.
                    StreamWriter xmlBaseFile = fileInfo.CreateText();
                    xmlBaseFile.WriteLine("<Acutis></Acutis>");
                    xmlBaseFile.Close();
                    fileInfo.Refresh();
                }
            
            strDataFile = strFileName;
        }
	
		public string getValue(string strName) 
		{
			try
			{
				XmlDocument doc = new XmlDocument();

				doc.Load(strDataFile);

				XmlNode root = doc.DocumentElement;
				XmlNode node = root.SelectSingleNode(strName);

				if (node == null)
					return "";
				else
					return node.InnerText;  
			}
			catch
			{
                return "";
			}
		}
	
		public bool setValue(string strName, string strValue) 
		{
			try 
			{
				XmlDocument doc = new XmlDocument();

				doc.Load(strDataFile);
											
				XmlNode root = doc.DocumentElement;
				XmlNode node = root.SelectSingleNode(strName);
				if(node == null) 
				{
					node = doc.CreateNode(XmlNodeType.Element, strName, null);
					node.InnerText = strValue;
					root.AppendChild(node);
				}
				else 
				{
					
					node.InnerText = strValue;
				}
				doc.Save(strDataFile);
				return true;
			}
			catch 
			{
				return false;
			}
		}

	}
}
