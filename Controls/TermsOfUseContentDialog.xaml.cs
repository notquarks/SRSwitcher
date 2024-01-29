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
using Wpf.Ui.Controls;

namespace SRSwitcher.Controls
{
    /// <summary>
    /// Interaction logic for TermsOfUseContentDialog.xaml
    /// </summary>
    public partial class TermsOfUseContentDialog : ContentDialog
    {
        public TermsOfUseContentDialog(ContentPresenter contentPresenter)
        : base(contentPresenter)
        {
            InitializeComponent();
        }

        protected override void OnButtonClick(ContentDialogButton button)
        {
            if (CheckBox.IsChecked != false)
            {
                base.OnButtonClick(button);
                return;
            }
            ;

            TextBlock.Visibility = Visibility.Visible;
            CheckBox.Focus();
        }
    }
}
