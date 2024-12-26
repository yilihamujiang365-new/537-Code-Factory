using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace _537_Code_Factory_New
{
    /// <summary>
    /// CreateProjectForm.xaml 的交互逻辑
    /// </summary>
    public partial class CreateProjectForm : Window
    {
        public string ProjectName { get; private set; }
        public CreateProjectForm()
        {
            InitializeComponent();
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ProjectName = ProjectNameTextBox.Text;
            this.DialogResult = true;
            this.Close();
        }
    }
}
