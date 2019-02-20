using EnvDTE;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Beyova.VsExtension
{
    /// <summary>
    /// Interaction logic for ConditionPicker.xaml
    /// </summary>
    public partial class ConditionPicker : BaseWindowsDialog
    {
        public string SelectedConfiguration { get; protected set; }

        public ConditionPicker()
        {

            InitializeComponent();
            Dispatcher.VerifyAccess();
            foreach (SolutionConfiguration item in VsExtension.GetSolutionConfigurations())
            {
                configurationSelection.Items.Add(item.Name);
            }
        }

        private void Btn_Apply_Click(object sender, RoutedEventArgs e)
        {
            SelectedConfiguration = configurationSelection.SelectedItem.ToString();
            Close();
        }

        private void Btn_Discard_Click(object sender, RoutedEventArgs e)
        {
            SelectedConfiguration = null;
            Close();
        }
    }
}
