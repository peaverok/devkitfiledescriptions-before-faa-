using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace DevKitFileDescriptions
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string moduleSelected = "";
        string rootName = "{module}-module-before-faa";

        string projectFolder = @"C:\DevProjects\{module}-module-before-faa";
        string fileListFile = @"files\{module}-module-file-list.json";
        string[] ignoredFoldersStillKept = { ".git", "dist", "logs", "node_modules", ".nyc_output", "coverage", ".esbuild", ".serverless" };
        string[] ignoredFolders = { "demo", "devkit", "examples" };
        string[] ignoredFiles = { "airport.json", "6-airports.ts" };

        IList<TreeNode> treeNodes = new List<TreeNode>();
        int key = -1;
        int mainKey = -1;

        IList<MainTreeNode> mainNodeList = new List<MainTreeNode>();

        IList<MainTreeNode> fileNodeList = new List<MainTreeNode>();

        public MainWindow()
        {
            InitializeComponent();

            string selectedTag = ((ComboBoxItem)ModuleComboBox.SelectedItem).Tag.ToString();

            moduleSelected = selectedTag;

            projectFolder = projectFolder.Replace("{module}", moduleSelected);

            DirectoryNameTextBox.Text = projectFolder;

            SaveFileList.IsEnabled = false;
        }

        private void SelectDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            DirectoryNameTextBox.Text = "";
            projectFolder = "";

            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = @"C:\DevProjects";

                DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    projectFolder = dialog.SelectedPath;
                    DirectoryNameTextBox.Text = projectFolder;
                }
            }
        }

        private void ProcessDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileList.IsEnabled = false;

            JsonFile.Text = "";

            FileList.Text = "";

            string selectedTag = ((ComboBoxItem)ModuleComboBox.SelectedItem).Tag.ToString();

            moduleSelected = selectedTag;

            fileListFile = @"files\{module}-module-file-list.json";

            fileListFile = fileListFile.Replace("{module}", moduleSelected);

            rootName = "{module}-module-before-faa";

            rootName = rootName.Replace("{module}", moduleSelected);

            string fileJsonList = File.ReadAllText(fileListFile);

            fileNodeList = JsonConvert.DeserializeObject<List<MainTreeNode>>(fileJsonList);

            DirectoryInfo rootDir = new DirectoryInfo(projectFolder);
            treeNodes = WalkDirectoryTree(rootDir);

            string jsonFile = JsonConvert.SerializeObject(treeNodes);

            if (GenerateFileListCheckBox.IsChecked == true)
            {
                string fileList = JsonConvert.SerializeObject(mainNodeList);
                FileList.Text = fileList;

                SaveFileList.IsEnabled = true;
            }
            else
            {
                JsonFile.Text = "{ \"data\": " + jsonFile + "}";
            }

            System.Windows.MessageBox.Show("Directory Processed");
        }

        private string LookupData(string name, string path)
        {
            MainTreeNode nodeFound = fileNodeList.FirstOrDefault(e => e.label == name && e.directory == path);

            if (nodeFound != null)
            {
                return nodeFound.data;
            }
            else
            {
                return "";
            }
        }

        private IList<TreeNode> WalkDirectoryTree(DirectoryInfo root)
        {
            FileInfo[] files = null;
            DirectoryInfo[] subDirs = null;

            IList<TreeNode> treeNodes = new List<TreeNode>();
            IList<TreeNode> childTreeNodes = null;

            TreeNode folderNode = null;
            MainTreeNode mainFolderNode = null;

            if (ignoredFolders.Contains(root.Name) == false)
            {
                if (root.Name != rootName)
                {
                    folderNode = new TreeNode();
                    key++;
                    folderNode.key = key.ToString();
                    folderNode.label = root.Name;
                    folderNode.data = LookupData(root.Name, root.FullName.Replace(projectFolder + "\\", ""));

                    treeNodes.Add(folderNode);

                    mainFolderNode = new MainTreeNode();
                    mainKey++;
                    mainFolderNode.key = mainKey.ToString();
                    mainFolderNode.label = root.Name;
                    mainFolderNode.data = LookupData(root.Name, root.FullName.Replace(projectFolder + "\\", ""));
                    mainFolderNode.directory = root.FullName.Replace(projectFolder + "\\", "");
                    mainNodeList.Add(mainFolderNode);
                }

                if (ignoredFoldersStillKept.Contains(root.Name) == false)
                {
                    // Now find all the subdirectories under this directory.
                    subDirs = root.GetDirectories();

                    foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                    {
                        // Recursive call for each subdirectory.
                        IList<TreeNode> childFolderTreeNodes = WalkDirectoryTree(dirInfo);

                        if (childTreeNodes == null)
                        {
                            childTreeNodes = new List<TreeNode>();
                        }

                        foreach (TreeNode childFolderTreeNode in childFolderTreeNodes)
                        {
                            childTreeNodes.Add(childFolderTreeNode);
                        }

                        if (folderNode != null)
                        {
                            folderNode.children = childTreeNodes;
                        }
                        else
                        {
                            foreach (TreeNode childFolderTreeNode in childFolderTreeNodes)
                            {
                                treeNodes.Add(childFolderTreeNode);
                            }
                        }
                    }

                    // First, process all the files directly under this folder
                    files = root.GetFiles("*.*");

                    if (files != null)
                    {
                        if (childTreeNodes == null)
                        {
                            childTreeNodes = new List<TreeNode>();
                        }

                        foreach (System.IO.FileInfo fileInfo in files)
                        {
                            if (ignoredFiles.Contains(fileInfo.Name) == false)
                            {
                                TreeNode fileNode = new TreeNode();
                                key++;
                                fileNode.key = key.ToString();
                                fileNode.label = fileInfo.Name;
                                fileNode.data = LookupData(fileInfo.Name, root.FullName.Replace(projectFolder + "\\", ""));

                                if (folderNode != null)
                                {
                                    childTreeNodes.Add(fileNode);
                                }
                                else
                                {
                                    treeNodes.Add(fileNode);
                                }


                                MainTreeNode mainFileNode = new MainTreeNode();
                                mainKey++;
                                mainFileNode.key = mainKey.ToString();
                                mainFileNode.label = fileInfo.Name;
                                mainFileNode.data = LookupData(fileInfo.Name, root.FullName.Replace(projectFolder + "\\", ""));
                                mainFileNode.directory = root.FullName.Replace(projectFolder + "\\", "");

                                mainNodeList.Add(mainFileNode);
                            }
                        }

                        if (folderNode != null)
                        {
                            folderNode.children = childTreeNodes;
                        }
                    }
                }
            }

            return treeNodes;
        }

        private void SaveFileListButton_Click(object sender, RoutedEventArgs e)
        {
            string fileList = FileList.Text;

            string baseDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

            string newFileListFile = @"files\{module}-module-file-list-new.json";

            string newFileName = baseDirectory + newFileListFile;

            newFileName = newFileName.Replace("{module}", moduleSelected);

            File.WriteAllText(newFileName, fileList);

            System.Windows.MessageBox.Show("File List Saved");
        }
    }
}
