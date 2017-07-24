using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArtifactUpload
{
    public partial class UploadForm : Form
    {
        RestClient client;

        public UploadForm()
        {
            InitializeComponent();
            client = new RestClient("http://localhost:10000");
            txt_Server.Text = client.BaseUrl.AbsoluteUri;
            //client = new RestClient("http://elke.sr.hs-mannheim.de:10000");
        }

        private void FilePathMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txt_file.Text = openFileDialog1.FileName;
            }
        }

        private async void UploadButtonClick(object sender, EventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(txt_file.Text) && comboBox1.SelectedIndex != -1)
            {
                btn_upload.Enabled = false;
                RestRequest req = new RestRequest("/api/artifact/{aid}/file");
                req.AddUrlSegment("aid", comboBox1.SelectedItem.ToString());
                req.RequestFormat = DataFormat.Json;
                req.Method = Method.POST;
                req.AddHeader("Content-Type", "multipart/form-data");
                req.AddFile("content", txt_file.Text);

                var response = await client.ExecuteTaskAsync(req);
                txt_response.Text = $"Status: {(int)response.ResponseStatus}, {response.StatusDescription}{Environment.NewLine}Content: {response.Content}{Environment.NewLine}";

                btn_upload.Enabled = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            openFileDialog1.FileName = "";
            RefreshButtonClick(sender, e);
        }

        private void RefreshButtonClick(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();

            RestRequest req = new RestRequest("/api/artifact", Method.GET);
            req.AddHeader("Accept", @"application/json");
            var res = client.Execute(req);

            if (res.StatusCode != System.Net.HttpStatusCode.OK)
                return;

            dynamic json = SimpleJson.DeserializeObject(res.Content);

            foreach(dynamic element in json)
            {
                comboBox1.Items.Add(element.id);
            }

            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
        }

        private void ServerButtonClick(object sender, EventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(txt_Server.Text))
            {
                client = new RestClient(txt_Server.Text);
                RefreshButtonClick(sender, e);
            }
        }
    }
}
