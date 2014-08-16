using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Configuration;
using System.Data.OleDb;
using System.Xml.Xsl;
using System.IO;
using System.Data.OracleClient;
using Microsoft.Office.Interop.Excel;

namespace QA_Tool
{
    public partial class Form1 : Form
    {
        #region "Variables"

        string lXMLPath = string.Empty;
        XmlNodeList gNodeList = null;
        string _gPassword = string.Empty;

        #endregion

        #region "Constructor"

        public Form1()
        {
            InitializeComponent();
            ReadXMLFileAndPopulateControls(cmbProject);
        }

        #endregion

        #region "Control Events"

        /// <summary>
        /// Handles the Login button Click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (btnLogin.Text != "Log Out")
            {

                Password p = new Password();
                p.ShowDialog();

                if (p.PasswordForm == "testing")
                {
                    gbAdmin.Visible = true;
                    ReadXMLFileAndPopulateControls(cmbProject2);

                    btnLogin.Text = "Log Out";
                }
                else
                {
                    MessageBox.Show("Wrong Password");
                }
            }
            else
            {
                gbAdmin.Visible = false;
                btnLogin.Text = "Login";
            }
        }
        
        /// <summary>
        /// Handles button1 click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            Item lSelectedItem = (Item)cmbQuery.SelectedItem;
            ReadFromDB(lSelectedItem.Value);

            MessageBox.Show("Done.");
        }

        /// <summary>
        /// Handles the Add Project click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddProject_Click(object sender, EventArgs e)
        {
            if (txtProject.Text.Trim() != string.Empty)
            {
                CreateXMLNode();
                ReadXMLFileAndPopulateControls(cmbProject2);
                ReadXMLFileAndPopulateControls(cmbProject);

                cmbProject2.SelectedItem = txtProject.Text;
                txtTitle.Text = "";
                txtSQL.Text = "";
            }
            else
            {
                MessageBox.Show("Project title is empty.");
            }
        }

        /// <summary>
        /// Hanldes the btnAddQuery click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddQuery_Click(object sender, EventArgs e)
        {
            if (txtTitle.Text.Trim() == string.Empty || txtSQL.Text.Trim() == string.Empty || cmbProject2.SelectedItem == null)
            {
                MessageBox.Show("Project Name,Query Title and Query SQL are mendatory fields.");
            }
            else
            {
                //Add the node to XML file
                AddedSqlAndTitleToExistingNode(cmbProject2.SelectedItem.ToString());
                //Refresh the grid
                cmbProject2_SelectedIndexChanged(null, null);
            }
        }

        /// <summary>
        /// Hanldes the btnProjectDelete click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnProjectDelete_Click(object sender, EventArgs e)
        {
            DialogResult ldr = MessageBox.Show("Are you sure you want to delete " + cmbProject2.SelectedItem.ToString() + " ?", "Delete Project", MessageBoxButtons.YesNo);
            if (ldr.ToString() == "Yes")
            {
                //delete the selected node
                DeleteProjectNode(cmbProject2.SelectedItem.ToString());
                //Populate the Comobox again
                ReadXMLFileAndPopulateControls(cmbProject2);
                ReadXMLFileAndPopulateControls(cmbProject);                
            }
            else
            {
                //Do nothing
            }
        }

        /// <summary>
        /// Hanldes the click event of btnDeleteQuery
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteQuery_Click(object sender, EventArgs e)
        {
            DialogResult lDr = MessageBox.Show("Are you sure you want to delete this Query : " + grdQuery.CurrentRow.Cells[0].Value,"Delete Row",MessageBoxButtons.YesNo);
            if (lDr.ToString() == "Yes")
            {
                DeleteRowFromGrid(cmbProject2.SelectedItem.ToString(), grdQuery.CurrentRow.Cells[0].Value.ToString());
                cmbProject2_SelectedIndexChanged(null, null);
            }

        }

        /// <summary>
        /// Hanldes the selected index changed event of cmbPrject
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbProject_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateQuery(cmbProject.SelectedItem.ToString());
        }

        /// <summary>
        /// Handles the cmbProject2 selected index changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbProject2_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Data.DataTable lDt;
            lDt = ReadXMLandReturnDataTable(cmbProject2.SelectedItem.ToString());

            if(lDt != null)
            {
                grdQuery.DataSource = lDt;                
            }
            grdQuery.Columns[1].Width = Convert.ToInt32(ReadConfigFile("GridCol1Width"));
        }               

        #endregion

        #region "Private Functions"

        /// <summary>
        /// This function reads the app.config. 
        /// </summary>
        /// <param name="pSettingName">Key name for the setting</param>
        /// <returns></returns>
        private string ReadConfigFile(string pSettingName)
        {
            return ConfigurationManager.AppSettings[pSettingName];
        }

        /// <summary>
        /// This functions gets the list of projects and populates the control provided in parameters
        /// </summary>
        /// <param name="pcmbProject">Control to populate</param>
        private void ReadXMLFileAndPopulateControls(ComboBox pcmbProject)
        {
            gNodeList = ReturnProjectNode();
            pcmbProject.Items.Clear();

            foreach (XmlNode node in gNodeList)
            {
                pcmbProject.Items.Add(node.ChildNodes[0].InnerText);
            }
        }

        /// <summary>
        /// This functions populates the Query combobox
        /// </summary>
        /// <param name="pSelectedNode"></param>
        private void PopulateQuery(string pSelectedNode)
        {
            gNodeList = ReturnProjectNode();

            cmbQuery.Items.Clear();
            foreach (XmlNode node in gNodeList)
            {
                if (node.ChildNodes[0].InnerText == pSelectedNode)
                {
                    for (int i = 0; i < node.ChildNodes.Count - 1; i++)
                    {
                        cmbQuery.Items.Add(new Item(node.ChildNodes[i + 1].ChildNodes[1].InnerText, node.ChildNodes[i + 1].ChildNodes[0].InnerText));
                    }
                }
            }
        }

        /// <summary>
        /// This function reads the XML file and returns all the Project nodes
        /// </summary>
        /// <returns></returns>
        private XmlNodeList ReturnProjectNode()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(ReadConfigFile("XMLFilePath"));
            XmlElement root = doc.DocumentElement;
            return root.SelectNodes("//Project");
        }

        /// <summary>
        /// This functions read data from database
        /// </summary>
        /// <param name="pQuery"></param>
        private void ReadFromDB(string pQuery)
        {
            OracleConnection myConnection;
            OracleCommand myCommand;
            OracleDataReader dr;
            System.Data.DataTable dt = new System.Data.DataTable();

            myConnection = new OracleConnection(ReadConfigFile("ConnectionString"));

            try
            {
                myConnection.Open();
                myCommand = new OracleCommand(pQuery, myConnection);
                dr = myCommand.ExecuteReader();
                
                dt.Load(dr);

                ExportDTToExcel(dt);                
            }
            catch (Exception ex)
            {

            }
        }

        private void CreateWorkbook(DataSet ds)
        {
            //XmlDataDocument xmlDataDoc = new XmlDataDocument(ds);
            //XslCompiledTransform xt = new XslCompiledTransform();
            //StreamReader reader = new StreamReader(typeof(QA_Tool).Assembly.GetManifestResourceStream(typeof(QA_Tool), "Excel.xsl"));
            //StreamReader reader = new StreamReader("Excel.xsl");
            //XmlTextReader xRdr = new XmlTextReader(reader);
            //xt.Load(xRdr, null, null);


            //XmlWriter sw = new XmlWriter();
            //xt.Transform(xmlDataDoc, null, sw, null);

            //StreamWriter myWriter = new StreamWriter(ReadConfigFile("ExcelSheetPath") + "\\Report.xls");
            //myWriter.Write(sw.ToString());
            //myWriter.Close();



            XmlDataDocument xmlDataDoc = new XmlDataDocument(ds);
            XslTransform xt = new XslTransform();
            StreamReader reader = new StreamReader(typeof(Form1).Assembly.GetManifestResourceStream(typeof(Form1), "Excel.xsl"));
            //StreamReader reader = new StreamReader("Excel.xsl");
            XmlTextReader xRdr = new XmlTextReader(reader);
            xt.Load(xRdr, null, null);

            StringWriter sw = new StringWriter();
            xt.Transform(xmlDataDoc, null, sw, null);

            StreamWriter myWriter = new StreamWriter(ReadConfigFile("ExcelSheetPath") + "\\Report.xls");
            myWriter.Write(sw.ToString());
            myWriter.Close();
        }

        /// <summary>
        /// This function creates a datatable to bind with grdQuery
        /// </summary>
        /// <returns></returns>
        private System.Data.DataTable CreateDataTable()
        {
            System.Data.DataTable lDr = new System.Data.DataTable();
            
            lDr.Columns.Add("Title");
            lDr.Columns.Add("Query");

            return lDr;
        }

        /// <summary>
        /// This function reads the XML file and converts the data selected into a datatable
        /// </summary>
        /// <param name="pSelectedNode"></param>
        /// <returns></returns>
        private System.Data.DataTable ReadXMLandReturnDataTable(string pSelectedNode)
        {
            System.Data.DataTable lDtGrid = CreateDataTable();
            DataRow lDr;

            gNodeList = ReturnProjectNode();

            foreach (XmlNode node in gNodeList)
            {
                if (node.ChildNodes[0].InnerText == pSelectedNode)
                {
                    for (int i = 0; i < node.ChildNodes.Count - 1; i++)
                    {
                        lDr = lDtGrid.NewRow();                        
                        lDr["Title"] = node.ChildNodes[i + 1].ChildNodes[0].InnerText;
                        lDr["Query"] = node.ChildNodes[i + 1].ChildNodes[1].InnerText;

                        lDtGrid.Rows.Add(lDr);
                    }
                }
            }

            return lDtGrid;
        }

        /// <summary>
        /// This function deletes the a node from XML file based on the Node title
        /// </summary>
        /// <param name="pNodeName"></param>
        private void DeleteProjectNode(string pNodeName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(ReadConfigFile("XMLFilePath"));
            XmlNode nodes = doc.SelectSingleNode("//root//Project[Name='" + pNodeName + "']");
            nodes.ParentNode.RemoveChild(nodes);
            doc.Save(ReadConfigFile("XMLFilePath"));
        }

        /// <summary>
        /// This function adds a new section to existing project node
        /// </summary>
        /// <param name="pNodeName"></param>
        private void AddedSqlAndTitleToExistingNode(string pNodeName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(ReadConfigFile("XMLFilePath"));
            XmlNode nodes = doc.SelectSingleNode("//root//Project[Name='" + pNodeName + "']");

            XmlNode nodeQuery = doc.CreateElement("Query");
            XmlNode nodeTitle = doc.CreateElement("Title");
            XmlNode nodeSQL = doc.CreateElement("SQL");

            nodeTitle.InnerText = txtTitle.Text;
            nodeSQL.InnerText = txtSQL.Text;

            nodeQuery.AppendChild(nodeTitle);
            nodeQuery.AppendChild(nodeSQL);

            nodes.AppendChild(nodeQuery);
            
            doc.Save(ReadConfigFile("XMLFilePath"));
        }

        /// <summary>
        /// This function creates a node named Project with inner text = the project name given in txtProject
        /// </summary>
        private void CreateXMLNode()
        {            
            XmlDocument doc = new XmlDocument();

            doc.Load(ReadConfigFile("XMLFilePath"));
            XmlNode node = doc.CreateNode(XmlNodeType.Element, "Project", null);
            XmlNode nodeName = doc.CreateElement("Name");
            nodeName.InnerText = txtProject.Text;
            node.AppendChild(nodeName);
            doc.DocumentElement.AppendChild(node);
            doc.Save(ReadConfigFile("XMLFilePath"));
        }

        /// <summary>
        /// This function deletes rows from Grid
        /// </summary>
        /// <param name="pNodeName"></param>
        /// <param name="pQueryTitle"></param>
        private void DeleteRowFromGrid(string pNodeName,string pQueryTitle)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(ReadConfigFile("XMLFilePath"));
            XmlNode nodes = doc.SelectSingleNode("//root//Project[Name='" + pNodeName + "']//Query[Title='" + pQueryTitle + "']");
            nodes.ParentNode.RemoveChild(nodes);
            doc.Save(ReadConfigFile("XMLFilePath"));
        }

        /// <summary>
        /// This function exports the datatable to excel sheet
        /// </summary>
        /// <param name="dt"></param>
        public void ExportDTToExcel(System.Data.DataTable dt)  
        {  
            Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();  
            app.Visible = false;
            object misValue = System.Reflection.Missing.Value;
 
            Workbook wb = app.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);  
            Worksheet ws = (Worksheet)wb.ActiveSheet;  
 
            // Headers.  
            for (int i = 0; i < dt.Columns.Count; i++)  
            {  
                ws.Cells[1, i + 1] = dt.Columns[i].ColumnName;  
            }  
 
            // Content.  
            for (int i = 0; i < dt.Rows.Count; i++)  
            {  
                for (int j = 0; j < dt.Columns.Count; j++)  
                {  
                    ws.Cells[i + 2, j + 1] = dt.Rows[i][j].ToString();  
                }  
            }  
 
            // Lots of options here. See the documentation.  
            wb.SaveAs(ReadConfigFile("ExcelSheetPath")+"Result.xls",misValue,misValue,misValue,misValue,misValue,XlSaveAsAccessMode.xlNoChange,misValue,misValue,misValue,misValue,misValue);

            wb.Close(false, misValue, misValue);
            app.Quit();  
        } 

        #endregion

        #region "Private Class"

        private class Item
        {
            public Item(string value, string text) { Value = value; Text = text; }
            public string Value { get; set; }
            public string Text { get; set; }
            public override string ToString() { return Text; }
        }

        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {

        }

    }
}
